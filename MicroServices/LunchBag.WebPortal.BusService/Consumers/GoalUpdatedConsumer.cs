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
    public class GoalUpdatedConsumer : IConsumer<IGoalUpdatedMessage>
    {
        private readonly ISignalsRestService _signalsRestService;
        private readonly ILogger<GoalUpdatedConsumer> _logger;

        public GoalUpdatedConsumer(ISignalsRestService signalsRestService, ILogger<GoalUpdatedConsumer> logger)
        {
            _logger = logger;
            _signalsRestService = signalsRestService;
        }

        public async Task Consume(ConsumeContext<IGoalUpdatedMessage> context)
        {
            _logger.LogDebug($"GoalUpdatedConsumer: Retrieved message {JsonConvert.SerializeObject(context.Message)}");

            var result = await _signalsRestService.GoalUpdated(context.Message);
            if (result == HttpStatusCode.OK)
                _logger.LogDebug($"GoalUpdatedConsumer: Message sent successfully.");
            else
                _logger.LogError($"GoalUpdatedConsumer: Error while sending a message: {result}");
        }
    }
}
