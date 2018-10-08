using System;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using LunchBag.Common.Config;
using LunchBag.Common.EventMessages;
using LunchBag.Common.IoTMessages;
using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using MassTransit;
using MassTransit.Pipeline.Pipes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;

namespace LunchBag.Common.Test
{
    public abstract class MicroServicesFixtureBase
    {
        protected ILoggerFactory _loggerFactory;
        protected Mock<IEventRestService> _mockEventRest;
        protected Mock<INoteRestService> _mockNoteRest;
        protected Mock<IOptions<RestServiceOptions>> _mockRestServiceOptions;
        protected Mock<IAzureServiceBusClient> _mockAzureServiceBus;
        protected Mock<IServiceBusClient> _mockServiceBus;
        protected Mock<IBus> _mockBus;

        public MicroServicesFixtureBase()
        {
            //Logger
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            //Mock up
            _mockEventRest = new Mock<IEventRestService>(MockBehavior.Strict);
            _mockNoteRest = new Mock<INoteRestService>(MockBehavior.Strict);
            _mockServiceBus = new Mock<IServiceBusClient>(MockBehavior.Strict);
            _mockBus = new Mock<IBus>(MockBehavior.Strict);
            _mockAzureServiceBus = new Mock<IAzureServiceBusClient>(MockBehavior.Strict);

            SetupMocks();
        }

        protected void SetupMocks()
        {
            //Service Bus
            _mockServiceBus.SetupGet(p => p.BusAccess).Returns(_mockBus.Object);
            //_mockBus.Setup(p => p.Publish(It.IsAny<IMessage>(), It.IsAny<Action<PublishContext<IMessage>>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask); //Extension method, not working
            _mockBus.Setup(p => p.Publish(It.IsAny<IMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            //Event Rest
            _mockEventRest.Setup(p => p.GetEvent(It.IsAny<string>())).Returns<string>((p) => Task.FromResult(DBMock.GetEvent(p)));
            _mockEventRest.Setup(p => p.UpdateEvent(It.IsAny<EventModel>())).Returns<EventModel>((p) => Task.FromResult(DBMock.UpdateEvent(p)));

            //Note Reset
            _mockNoteRest.Setup(p => p.GetNoteTemplates(It.IsAny<string>())).Returns<string>((p) => Task.FromResult(DBMock.GetNoteTemplates(p)));
            _mockNoteRest.Setup(p => p.CreateNote(It.IsAny<NoteModel>())).Returns<NoteModel>((p) => Task.FromResult(DBMock.CreateNote(p)));
        }

        protected string CreateSerializedButtonTriggerIoTMessage(string eventId, string locationId, int capacity = 20)
        {
            return JsonConvert.SerializeObject(new ButtonTriggerIoTMessage()
            {
                EventId = eventId,
                Capacity = capacity,
                LocationId = locationId,
                MessageType = IoTMessageType.Button,
                UniqueId = Guid.NewGuid().ToString()
            });
        }

        protected string CreateSerializedNoteCreatedIoTMessage(string eventId, string note = "This is a note")
        {
            return JsonConvert.SerializeObject(new NoteCreatedIoTMessage()
            {
                EventId = eventId,
                MessageType = IoTMessageType.Note,
                Note = note
            });
        }

        //Todo
        protected string CreateSerializedDeliveryTriggerIoTMessage()
        {
            return JsonConvert.SerializeObject(new DeliveryTriggerIoTMessage()
            {
                MessageType = IoTMessageType.Unknown
            });
        }

        protected ILogger<T> CreateLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }
    }
}
