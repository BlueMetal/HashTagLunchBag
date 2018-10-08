using LunchBag.Common.Config;
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
    public interface IEventRestService
    {
        Task<EventModel> GetEvent(string eventId);
        Task<IEnumerable<EventModel>> GetEvents();
        Task<HttpStatusCode> UpdateEvent(EventModel eventObj);
        Task<string> CreateEvent(EventModel eventObj);
        Task<bool> DeleteEvent(string eventId);
    }
}
