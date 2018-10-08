using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.Common.Config
{
    public class AzureServiceBusOptions
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public int PrefetchCount { get; set; }
    }
}
