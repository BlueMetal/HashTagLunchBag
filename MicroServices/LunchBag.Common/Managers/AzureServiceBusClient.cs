﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LunchBag.Common.Config;
using LunchBag.Common.Managers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LunchBag.Common.Managers
{
    public class AzureServiceBusClient : IDisposable, IAzureServiceBusClient
    {
        private readonly AzureServiceBusOptions _config;
        private readonly ILogger<AzureServiceBusClient> _logger;
        private IQueueClient _queueClient;
        private Func<object, Task<bool>> _messageHandler;

        public AzureServiceBusClient(IOptions<AzureServiceBusOptions> config,
            ILogger<AzureServiceBusClient> logger)
        {
            _logger = logger;
            _config = config.Value;
        }

        public void Start(Func<object, Task<bool>> messageHandler)
        {
            try
            {
                _messageHandler = messageHandler;
                _queueClient = new QueueClient(_config.ConnectionString, _config.QueueName);
                RegisterOnMessageHandlerAndReceiveMessages();
            }
            catch (Exception e)
            {
                _logger.LogError($"AzureServiceBusManager: {e.Message}");
            }
        }

        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that processes messages.
            _queueClient.PrefetchCount = _config.PrefetchCount;
            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            if (_queueClient.IsClosedOrClosing)
                token.ThrowIfCancellationRequested();

            // Process the message.
            _logger.LogDebug($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Retrieve the type
            object objectMessage = null;
            if (message.UserProperties.ContainsKey("messageType"))
            {
                Type messageType = Type.GetType(message.UserProperties["messageType"].ToString());
                objectMessage = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body), messageType);
            }
            else
            {
                objectMessage = Encoding.UTF8.GetString(message.Body);
            }

            try
            {
                bool result = await _messageHandler(objectMessage);
                if (!result)
                {
                    _logger.LogDebug($"ProcessMessagesAsync: Error from the messageHandler. The queue item will not be completed.");
                    return;
                }
            }
            catch(Exception e)
            {
                _logger.LogDebug($"ProcessMessagesAsync: Error while treating message: {e.Message}. The queue item will not be completed.");
                return;
            }

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _logger.LogDebug("Exception context for troubleshooting:");
            _logger.LogDebug($"- Endpoint: {context.Endpoint}");
            _logger.LogDebug($"- Entity Path: {context.EntityPath}");
            _logger.LogDebug($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_queueClient != null)
            {
                _queueClient.CloseAsync();
            }
        }
    }
}
