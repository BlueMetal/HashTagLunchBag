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
    public class SentimentUpdatedConsumer : IConsumer<ISentimentsUpdatedMessage>
    {
        private readonly ISignalsRestService _signalsRestService;
        private readonly ILogger<SentimentUpdatedConsumer> _logger;

        public SentimentUpdatedConsumer(ISignalsRestService signalsRestService, ILogger<SentimentUpdatedConsumer> logger)
        {
            _logger = logger;
            _signalsRestService = signalsRestService;
        }

        public async Task Consume(ConsumeContext<ISentimentsUpdatedMessage> context)
        {
            _logger.LogDebug($"SentimentUpdatedConsumer: Retrieved message {JsonConvert.SerializeObject(context.Message)}");

            var result = await _signalsRestService.SentimentUpdated(context.Message);
            if (result == HttpStatusCode.OK)
                _logger.LogDebug($"SentimentUpdatedConsumer: Message sent successfully.");
            else
                _logger.LogError($"SentimentUpdatedConsumer: Error while sending a message: {result}");
        }
    }
}
