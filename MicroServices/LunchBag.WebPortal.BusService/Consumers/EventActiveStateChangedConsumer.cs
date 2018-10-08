using LunchBag.Common.EventMessages;
using LunchBag.Common.Managers;
using MassTransit;
using MassTransit.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.BusService
{
    public class EventActiveStateChangedConsumer : IConsumer<IEventActiveStateChangedMessage>
    {
        private readonly ISignalsRestService _signalsRestService;
        private readonly ILogger<EventActiveStateChangedConsumer> _logger;

        public EventActiveStateChangedConsumer(ISignalsRestService signalsRestService, ILogger<EventActiveStateChangedConsumer> logger)
        {
            _logger = logger;
            _signalsRestService = signalsRestService;
        }

        public async Task Consume(ConsumeContext<IEventActiveStateChangedMessage> context)
        {
            _logger.LogDebug($"EventActiveStateChangedConsumer: Retrieved message {JsonConvert.SerializeObject(context.Message)}");

            var result = await _signalsRestService.EventActiveStateChanged(context.Message);
            if (result == HttpStatusCode.OK)
                _logger.LogDebug($"EventActiveStateChangedConsumer: Message sent successfully.");
            else
                _logger.LogError($"EventActiveStateChangedConsumer: Error while sending a message: {result}");
        }
    }
}
