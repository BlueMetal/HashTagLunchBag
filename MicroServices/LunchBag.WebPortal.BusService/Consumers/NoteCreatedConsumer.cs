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
    public class NoteCreatedConsumer : IConsumer<INoteCreatedMessage>
    {
        private readonly ISignalsRestService _signalsRestService;
        private readonly ILogger<GoalUpdatedConsumer> _logger;

        public NoteCreatedConsumer(ISignalsRestService signalsRestService, ILogger<GoalUpdatedConsumer> logger)
        {
            _logger = logger;
            _signalsRestService = signalsRestService;
        }

        public async Task Consume(ConsumeContext<INoteCreatedMessage> context)
        {
            _logger.LogDebug($"NoteCreatedConsumer: Retrieved message {JsonConvert.SerializeObject(context.Message)}");

            var result = await _signalsRestService.NoteCreated(context.Message);
            if (result == HttpStatusCode.OK)
                _logger.LogDebug($"NoteCreatedConsumer: Message sent successfully.");
            else
                _logger.LogError($"NoteCreatedConsumer: Error while sending a message: {result}");
        }
    }
}
