using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public interface IAzureServiceBusClient
    {
        void Start(Func<object, Task<bool>> handlerMessage);
    }
}
