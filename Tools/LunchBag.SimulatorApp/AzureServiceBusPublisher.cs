using LunchBag.Common.Config;
using LunchBag.Common.Managers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public class AzureServiceBusPublisher : IDisposable
    {
        private readonly AzureServiceBusOptions _config;
        private IQueueClient _queueClient;
        private string _name;
        public string Name { get { return _name; } }

        public AzureServiceBusPublisher(string name, AzureServiceBusOptions config)
        {
            _config = config;
            _name = name;
        }

        public void Start()
        {
            try
            {
                _queueClient = new QueueClient(_config.ConnectionString, _config.QueueName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"EventPublishingManager: {e.Message}");
            }
        }

        public async Task Publish<TMessage>(TMessage message)
            where TMessage : class
        {
            try
            {
                string messageJson = JsonConvert.SerializeObject(message);
                Message newMessage = new Message(Encoding.UTF8.GetBytes(messageJson));

                // Write the body of the message to the console
                Console.WriteLine($"Sending message: {messageJson}");

                // Send the message to the topic
                await _queueClient.SendAsync(newMessage);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        public async Task Publish<TMessage>(TMessage message, TimeSpan ttl)
            where TMessage : class
        {
            try
            {
                string messageJson = JsonConvert.SerializeObject(message);
                Message newMessage = new Message(Encoding.UTF8.GetBytes(messageJson));
                newMessage.TimeToLive = ttl;
                newMessage.UserProperties["messageType"] = typeof(TMessage).FullName;

                // Write the body of the message to the console
                Console.WriteLine($"Sending message: {messageJson}");

                // Send the message to the topic
                await _queueClient.SendAsync(newMessage);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        public void Dispose()
        {
            if (_queueClient != null)
                _queueClient.CloseAsync();
        }

    }
}
