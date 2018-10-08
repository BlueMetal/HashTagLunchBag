using LunchBag.Common.EventMessages;
using LunchBag.Common.Models;
using LunchBag.WebPortal.Api.Config;
using LunchBag.WebPortal.Api.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace LunchBag.WebPortal.Api
{
    public class SignalRHub : Hub
    {
        private EventDataManager _eventCacheManager;
        private Dictionary<string, ConnectionObject> _dictConnections;

        public SignalRHub(IOptions<WebPortalApiConfig> config, EventDataManager eventCacheManager)
        {
            _eventCacheManager = eventCacheManager;
            _dictConnections = new Dictionary<string, ConnectionObject>();
        }

        public async Task InitConnection(string eventId, string viewId = "")
        {
            //If user previously initialized the connection, remove it from previous group.
            await PurgeCurrentClientGroup(Context.ConnectionId);

            //Add connectionId/EventId/ViewId to dictionary
            await AddToCurrentClientGroup(Context.ConnectionId, eventId, viewId);
        }

        public async Task PurgeCurrentClientGroup(string connectionId)
        {
            if (_dictConnections.ContainsKey(connectionId))
            {
                ConnectionObject connectionObject = _dictConnections[connectionId];
                if (connectionObject != null)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, connectionObject.EventId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{connectionObject.EventId}_{connectionObject.ViewId}");
                    _dictConnections[connectionId] = null;
                }
            }
        }

        public async Task AddToCurrentClientGroup(string connectionId, string eventId, string viewId)
        {
            _dictConnections[connectionId] = new ConnectionObject()
            {
                 EventId = eventId, ViewId = viewId
            };

            await Groups.AddToGroupAsync(connectionId, eventId);
            if (!string.IsNullOrEmpty(viewId))
                await Groups.AddToGroupAsync(connectionId, $"{eventId}_{viewId}");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //If user previously initialized the connection, remove it from previous group.
            await PurgeCurrentClientGroup(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
