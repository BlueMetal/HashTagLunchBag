using LunchBag.Common.Test;
using LunchBag.WebPortal.TransportService.Config;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;
using Moq;
using LunchBag.Common.IoTMessages;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LunchBag.WebPortal.MicroServices.Test
{
    public class MessageServiceFixture : MicroServicesFixtureBase
    {
        protected Mock<IOptions<MessageServiceOptions>> _mockMessageOptionsTTL;
        protected Mock<IOptions<MessageServiceOptions>> _mockMessageOptionsNoTTL;

        public MessageServiceFixture()
        {
            MessageServiceOptions buttonOptionsTTL = new MessageServiceOptions(){ PushToQueueTimeToLive = 60};
            MessageServiceOptions buttonOptionsNoTTL = new MessageServiceOptions() { PushToQueueTimeToLive = 0 };
            _mockMessageOptionsTTL = new Mock<IOptions<MessageServiceOptions>>(MockBehavior.Strict);
            _mockMessageOptionsNoTTL = new Mock<IOptions<MessageServiceOptions>>(MockBehavior.Strict);
            _mockMessageOptionsTTL.Setup(ap => ap.Value).Returns(buttonOptionsTTL);
            _mockMessageOptionsNoTTL.Setup(ap => ap.Value).Returns(buttonOptionsNoTTL);
        }

        private MessageService.MessageService CreateMessageService()
        {
            return
                new MessageService.MessageService(
                    _mockMessageOptionsNoTTL.Object,
                    _mockEventRest.Object,
                    _mockNoteRest.Object,
                    _mockAzureServiceBus.Object, 
                    _mockServiceBus.Object, 
                    CreateLogger<MessageService.MessageService>());
        }

        [Fact]
        public async void SendNewIoTNoteMessageEventExists()
        {
            DBMock.Init();
            var service = CreateMessageService();
            var result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1", "This is a note about hope"));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal(1, eventObj.EventSentiments.Find(p => p.Name == "Hope").Value);
        }

        [Fact]
        public async void SendNewIoTNoteMessageEventNotExists()
        {
            DBMock.Init();
            var service = CreateMessageService();
            var result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("EventFake", "This is a note about hope"));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal(0, eventObj.EventSentiments.Find(p => p.Name == "Hope").Value);
        }

        [Fact]
        public async void SendNewIoTNoteMessageTypeButton()
        {
            DBMock.Init();
            var service = CreateMessageService();
            var result = await service.HandleNoteTriggerInput(CreateSerializedButtonTriggerIoTMessage("Event1", "Location1"));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal(0, eventObj.EventSentiments.Find(p => p.Name == "Hope").Value);
        }

        [Fact]
        public async void SendNewIoTNoteMessagesTestSentimentValues()
        {
            DBMock.Init();
            var service = CreateMessageService();
            var result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1", "This is a note about hope"));
            Assert.True(result);
            result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1", "This is a nete about hape")); //Duplicate should be ignored.
            Assert.True(result);
            result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1", "We all want love"));
            Assert.True(result);
            result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1", "this is a nete about hapy"));
            Assert.True(result);
            result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1", "we care about you!"));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal(2, eventObj.EventSentiments.Find(p => p.Name == "Hope").Value);
            Assert.Equal(50, eventObj.EventSentiments.Find(p => p.Name == "Hope").Percentage);
            Assert.Equal(1, eventObj.EventSentiments.Find(p => p.Name == "Love").Value);
            Assert.Equal(25, eventObj.EventSentiments.Find(p => p.Name == "Love").Percentage);
            Assert.Equal(1, eventObj.EventSentiments.Find(p => p.Name == "Care").Value);
            Assert.Equal(25, eventObj.EventSentiments.Find(p => p.Name == "Care").Percentage);

            eventObj = await _mockEventRest.Object.GetEvent("Event2");
            Assert.Equal(0, eventObj.EventSentiments.Find(p => p.Name == "Brave").Value);
            Assert.Equal(0, eventObj.EventSentiments.Find(p => p.Name == "Brave").Percentage);
            Assert.Equal(0, eventObj.EventSentiments.Find(p => p.Name == "Courage").Value);
            Assert.Equal(0, eventObj.EventSentiments.Find(p => p.Name == "Courage").Percentage);
            Assert.Equal(0, eventObj.EventSentiments.Find(p => p.Name == "Love").Value);
            Assert.Equal(0, eventObj.EventSentiments.Find(p => p.Name == "Love").Percentage);
        }

        [Fact]
        public async void SendNewIoTNoteMessageCreateNote()
        {
            DBMock.Init();
            var service = CreateMessageService();
            var result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1", "We all want love"));
            Assert.True(result);
            result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1", "we care about you!"));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal("We care about you!", eventObj.LastNote);
            Assert.Equal(2, DBMock.GetNotes("Event1").ToList().Count);

            eventObj = await _mockEventRest.Object.GetEvent("Event2");
            Assert.Equal(string.Empty, eventObj.LastNote);
            result = await service.HandleNoteTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event2", "You want courage"));
            Assert.True(result);
            eventObj = await _mockEventRest.Object.GetEvent("Event2");
            Assert.Equal("You need courage", eventObj.LastNote);
            Assert.Equal(1, DBMock.GetNotes("Event2").ToList().Count);
        }
    }
}
