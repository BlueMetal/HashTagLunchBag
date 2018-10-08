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
    public class DeliveryUpdatedConsumer : IConsumer<IDeliveryUpdatedMessage>
    {
        private readonly ISignalsRestService _signalsRestService;
        private readonly ILogger<DeliveryUpdatedConsumer> _logger;

        public DeliveryUpdatedConsumer(ISignalsRestService signalsRestService, ILogger<DeliveryUpdatedConsumer> logger)
        {
            _logger = logger;
            _signalsRestService = signalsRestService;
        }

        public async Task Consume(ConsumeContext<IDeliveryUpdatedMessage> context)
        {
            _logger.LogDebug($"DeliveryUpdatedConsumer: Retrieved message {JsonConvert.SerializeObject(context.Message)}");

            var result = await _signalsRestService.DeliveryUpdated(context.Message);
            if (result == HttpStatusCode.OK)
                _logger.LogDebug($"DeliveryUpdatedConsumer: Message sent successfully.");
            else
                _logger.LogError($"DeliveryUpdatedConsumer: Error while sending a message: {result}");
        }
    }
}
