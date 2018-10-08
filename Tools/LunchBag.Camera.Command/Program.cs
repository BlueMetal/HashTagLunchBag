using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

namespace LunchBag.Camera.Command
{
    class Program
    {
        public const string METHOD_IOT_UPDATESUBSCRIPTIONKEY = "1";

        private static string _Action;
        private static string _IoTHubConnectionString;
        private static string _DeviceId;
        private static ServiceClient _ServiceClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Command Tool For Lunchbag OCR Camera");

            Console.WriteLine("Please enter IoT Hub Connection String:");
            _IoTHubConnectionString = Console.ReadLine();
            Console.WriteLine("Please enter DeviceId:");
            _DeviceId = Console.ReadLine();
            Console.WriteLine("Please enter Method index:");
            Console.WriteLine("[1] SetSubscriptionKey");
            _Action = Console.ReadLine();


            Console.WriteLine($"Method: {_Action}");
            Console.WriteLine($"DeviceId: {_DeviceId}");

            _ServiceClient = ServiceClient.CreateFromConnectionString(_IoTHubConnectionString);

            Console.WriteLine("Connected.");

            switch (_Action)
            {
                case METHOD_IOT_UPDATESUBSCRIPTIONKEY:
                    Console.WriteLine($"SetSubscriptionKey - Enter SubscriptionKey");
                    string subscriptionKey = Console.ReadLine();
                    
                    SetSubscriptionKey(subscriptionKey).Wait();
                    break;
                default:
                    Console.WriteLine("Method not found.");
                    break;
            }

            Console.WriteLine("");
            Console.WriteLine("Press Enter to exit.");
            Console.Read();
        }

        static async Task SetSubscriptionKey(string key)
        {
            CloudToDeviceMethod method = new CloudToDeviceMethod(METHOD_IOT_UPDATESUBSCRIPTIONKEY);
            method.SetPayloadJson("{\"key\":\""+key+"\"}");
            var result = await _ServiceClient.InvokeDeviceMethodAsync(_DeviceId, method);
            Console.WriteLine($"Result status: {result.Status}");
        }
    }
}
