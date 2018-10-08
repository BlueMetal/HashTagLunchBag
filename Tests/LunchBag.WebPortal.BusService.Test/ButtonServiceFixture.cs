using LunchBag.Common.Test;
using LunchBag.WebPortal.TransportService.Config;
using Microsoft.Extensions.Options;
using System;
using Xunit;
using Moq;
using LunchBag.Common.IoTMessages;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LunchBag.WebPortal.MicroServices.Test
{
    public class ButtonServiceFixture : MicroServicesFixtureBase
    {
        protected Mock<IOptions<ButtonServiceOptions>> _mockButtonOptionsTTL;
        protected Mock<IOptions<ButtonServiceOptions>> _mockButtonOptionsNoTTL;

        public ButtonServiceFixture()
        {
            ButtonServiceOptions buttonOptionsTTL = new ButtonServiceOptions(){ PushToQueueTimeToLive = 60};
            ButtonServiceOptions buttonOptionsNoTTL = new ButtonServiceOptions() { PushToQueueTimeToLive = 0 };
            _mockButtonOptionsTTL = new Mock<IOptions<ButtonServiceOptions>>(MockBehavior.Strict);
            _mockButtonOptionsNoTTL = new Mock<IOptions<ButtonServiceOptions>>(MockBehavior.Strict);
            _mockButtonOptionsTTL.Setup(ap => ap.Value).Returns(buttonOptionsTTL);
            _mockButtonOptionsNoTTL.Setup(ap => ap.Value).Returns(buttonOptionsNoTTL);
        }

        private ButtonService.ButtonService CreateButtonService()
        {
            return
                new ButtonService.ButtonService(
                    _mockButtonOptionsNoTTL.Object,
                    _mockEventRest.Object, 
                    _mockAzureServiceBus.Object, 
                    _mockServiceBus.Object, 
                    CreateLogger<ButtonService.ButtonService>());
        }

        [Fact]
        public async void SendNewIoTButtonMessageEventExists()
        {
            var logger = CreateLogger<ButtonService.ButtonService>();

            DBMock.Init();
            var service = CreateButtonService();
            var result = await service.HandleButtonTriggerInput(CreateSerializedButtonTriggerIoTMessage("Event1", "Location1", 15));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal(15, eventObj.EventLocations.Find(p => p.Id == "Location1").GoalStatus);
        }

        [Fact]
        public async void SendNewIoTButtonMessageEventNotExists()
        {
            DBMock.Init();
            var service = CreateButtonService();
            var result = await service.HandleButtonTriggerInput(CreateSerializedButtonTriggerIoTMessage("EventFake", "Location1"));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal(0, eventObj.EventLocations.Find(p => p.Id == "Location1").GoalStatus);
        }

        [Fact]
        public async void SendNewIoTButtonMessageTypeNote()
        {
            DBMock.Init();
            var service = CreateButtonService();
            var result = await service.HandleButtonTriggerInput(CreateSerializedNoteCreatedIoTMessage("Event1"));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal(0, eventObj.EventLocations.Find(p => p.Id == "Location1").GoalStatus);
        }

        [Fact]
        public async void SendNewIoTButtonMessageCapacityAddition()
        {
            DBMock.Init();
            var service = CreateButtonService();
            var result = await service.HandleButtonTriggerInput(CreateSerializedButtonTriggerIoTMessage("Event1", "Location1", 10));
            Assert.True(result);
            result = await service.HandleButtonTriggerInput(CreateSerializedButtonTriggerIoTMessage("Event1", "Location2", 15));
            Assert.True(result);
            result = await service.HandleButtonTriggerInput(CreateSerializedButtonTriggerIoTMessage("Event1", "Location1", 20));
            Assert.True(result);
            result = await service.HandleButtonTriggerInput(CreateSerializedButtonTriggerIoTMessage("Event2", "Location1", 5));
            Assert.True(result);
            var eventObj = await _mockEventRest.Object.GetEvent("Event1");
            Assert.Equal(30, eventObj.EventLocations.Find(p => p.Id == "Location1").GoalStatus);
            Assert.Equal(15, eventObj.EventLocations.Find(p => p.Id == "Location2").GoalStatus);
            Assert.Null(eventObj.EventLocations.Find(p => p.Id == "Location3"));
            eventObj = await _mockEventRest.Object.GetEvent("Event2");
            Assert.Equal(5, eventObj.EventLocations.Find(p => p.Id == "Location1").GoalStatus);
            Assert.Equal(0, eventObj.EventLocations.Find(p => p.Id == "Location2").GoalStatus);
            Assert.Null(eventObj.EventLocations.Find(p => p.Id == "Location3"));
        }
    }
}
