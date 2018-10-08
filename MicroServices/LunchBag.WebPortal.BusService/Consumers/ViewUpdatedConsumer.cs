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
    public class ViewUpdatedConsumer : IConsumer<IViewUpdatedMessage>
    {
        private readonly ISignalsRestService _signalsRestService;
        private readonly ILogger<ViewUpdatedConsumer> _logger;

        public ViewUpdatedConsumer(ISignalsRestService signalsRestService, ILogger<ViewUpdatedConsumer> logger)
        {
            _logger = logger;
            _signalsRestService = signalsRestService;
        }

        public async Task Consume(ConsumeContext<IViewUpdatedMessage> context)
        {
            _logger.LogDebug($"ViewUpdatedConsumer: Retrieved message {JsonConvert.SerializeObject(context.Message)}");

            var result = await _signalsRestService.ViewUpdatedUpdated(context.Message);
            if (result == HttpStatusCode.OK)
                _logger.LogDebug($"ViewUpdatedConsumer: Message sent successfully.");
            else
                _logger.LogError($"ViewUpdatedConsumer: Error while sending a message: {result}");
        }
    }
}
