using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.WebPortal.Api.Config;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.Api.Helpers
{
    public class EventDataManager
    {
        private WebPortalApiConfig _config;
        private Dictionary<string, EventModel> _dictEvents;
        private ICosmosDBRepository<EventModel> _repoEvents;

        public EventDataManager(IOptions<WebPortalApiConfig> config,
            ICosmosDBRepository<EventModel> repoEvents)
        {
            _config = config.Value;
            _repoEvents = repoEvents;
            _dictEvents = new Dictionary<string, EventModel>();
        }

        public async Task<EventModel> GetEvent(string eventId, bool useCache = false)
        {
            EventModel eventModel = null;
            if (!useCache || !_config.UseCache || !_dictEvents.ContainsKey(eventId) )
            {
                eventModel = await _repoEvents.GetItemAsync(eventId);
                _dictEvents[eventId] = eventModel;
            }
            else
                eventModel = _dictEvents[eventId];
            return eventModel;
        }

        public async Task<IEnumerable<EventModel>> GetEvents()
        {
            return await _repoEvents.GetItemsAsync();
        }

        public async Task UpdateEvent(EventModel eventObj)
        {
            await _repoEvents.UpdateItemAsync(eventObj.Id, eventObj);
            if (_config.UseCache)
                await GetEvent(eventObj.Id, true);
        }

        public async Task<string> CreateEvent(EventModel eventObj)
        {
            eventObj.EventLocations = new List<EventLocationModel>()
            {
                new EventLocationModel(){ Id = "Location1", Goal = 1000, LocationName = "Room 1" }
            };
            eventObj.EventSentiments = new List<EventSentimentModel>()
            {
                new EventSentimentModel(){ Name = "Sentiment1" },
                new EventSentimentModel(){ Name = "Sentiment2" },
                new EventSentimentModel(){ Name = "Sentiment3" }
            };
            eventObj.EventViews = new List<EventViewModel>();
            eventObj.PayPalApi = new EventPayPalApiModel();
            eventObj.PayPalApi.MerchantInfo = new PayPalMerchantInfo();

            string result = await _repoEvents.CreateItemAsync(eventObj);
            if (_config.UseCache)
                await GetEvent(result, true);
            return result;
        }

        public async Task<bool> DeleteEvent(string eventId)
        {
            bool result = await _repoEvents.DeleteItemAsync(eventId);
            if (result)
                _dictEvents?.Remove(eventId);
            return result;
        }
    }
}
