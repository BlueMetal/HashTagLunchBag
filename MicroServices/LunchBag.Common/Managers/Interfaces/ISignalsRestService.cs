using LunchBag.Common.Config;
using LunchBag.Common.EventMessages;
using LunchBag.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public interface ISignalsRestService
    {
        Task<HttpStatusCode> GoalUpdated(IGoalUpdatedMessage message);
        Task<HttpStatusCode> NoteCreated(INoteCreatedMessage message);
        Task<HttpStatusCode> SentimentUpdated(ISentimentsUpdatedMessage message);
        Task<HttpStatusCode> ViewUpdatedUpdated(IViewUpdatedMessage message);
        Task<HttpStatusCode> EventActiveStateChanged(IEventActiveStateChangedMessage message);
        Task<HttpStatusCode> DeliveryUpdated(IDeliveryUpdatedMessage message);
    }
}
