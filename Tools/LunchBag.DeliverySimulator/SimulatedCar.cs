// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace LunchBag.DeliverySimulator
{
    class SimulatedCar
    {
        private static DeviceClient s_deviceClient;
        private string tripId;
        private string vehicleId;
        private string userId;
        private string vehicleReg;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private readonly static string s_connectionString = "HostName=LBFleetIoTHub.azure-devices.net;DeviceId=Dongle_Sample4;SharedAccessKey=iSgV1Rhd4fYQ6gYrvOuNAUYxfbKs9yBNK2oxNmFrGg8=";

        // Async method to send simulated telemetry
        private static async void SendDeviceToCloudMessagesAsync()
        {
            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                // Create JSON message
                var telemetryDataPoint = new
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the tlemetry message
                await s_deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }
        }

        private static async Task SendNextMessageAsync(TripUpdate currentUpdate)
        {
            TripUpdateData rawData = currentUpdate.data; // This is what gets sent to iot hub (I hope)

            string raw = JsonConvert.SerializeObject(rawData);
            raw = "{\"Parameters\":" + "\"" + raw.Replace("\"", "\\\"") + "\"" + "}";

            var message = new Message(Encoding.ASCII.GetBytes(raw));

            // Send the tlemetry message
            await s_deviceClient.SendEventAsync(message);

            Console.WriteLine("Message Sent...hopefully");
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("IoT Hub Quickstarts #1 - Simulated device. Ctrl-C to exit.\n");
            try
            {
                s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Mqtt);
                int iMessage = 0;

                // read in the sample json
                Trip trip = null;
                string tripId = "";

                using (StreamReader reader = new StreamReader("./sample-trip.json"))
                {
                    Task<string> ts = reader.ReadToEndAsync();
                    string s = ts.Result;
                    s = "{\"updates\":" + s + "}";

                    trip = JsonConvert.DeserializeObject<Trip>(s);
                    tripId = Guid.NewGuid().ToString();
                }

                while (true)
                {
                    Console.WriteLine("Press [space] to send the next message; Press Q to quit");
                    char c = Console.ReadKey().KeyChar;

                    if (c == 'q' || c == 'Q')
                    {
                        Console.WriteLine("Done.");
                        break;
                    }

                    if (c == ' ')
                    {
                        Console.WriteLine($"Sending message {(iMessage + 1)}...");

                        trip.updates[iMessage].data.ID = (iMessage + 1); // not sure if this is ok
                        trip.updates[iMessage].data.ParameterDateTime = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                        trip.updates[iMessage].data.VehicleId = "472f4bff-82e4-4138-9b5f-cbaea516a949";
                        trip.updates[iMessage].data.VehicleRegNumber = "SMPL0004";
                        trip.updates[iMessage].data.UserId = "617ca829-cd30-4a34-903f-dab14aada9f3";

                        if (iMessage == trip.updates.Count)
                            break;

                        if (iMessage == (trip.updates.Count - 1))
                            trip.updates[iMessage].data.TripId = "NA";
                        else
                            trip.updates[iMessage].data.TripId = tripId;

                        SendNextMessageAsync(trip.updates[iMessage++]).Wait();
                        continue;
                    }

                    Console.WriteLine("Invalid Choice...");
                }
                
                //SendDeviceToCloudMessagesAsync();
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
            }
        }

        private class Trip
        {
            public List<TripUpdate> updates { get; set; }
        }

        private class TripUpdate
        {
            public string _id { get; set; }
            public string vehicleId { get; set; }
            public TripUpdateData data { get; set; }
            public Epoch createdAt { get; set; }
            public bool isDeleted { get; set; }
            public int __v { get; set; }
        }

        private class TripUpdateData
        {
            public int ID { get; set; }
            public string rawPGNData { get; set; }
            public string isConnected { get; set; }
            public string WidebandAirFuelRatio { get; set; }
            public int VehicleSpeed { get; set; }
            public string VehicleRegNumber { get; set; }
            public string VehicleIdentificationNumber { get; set; }
            public string VehicleId { get; set; }
            public string VIN { get; set; }
            public string UserId { get; set; }
            public string UnitNo { get; set; }
            public string TroubleCodes { get; set; }
            public string TripId { get; set; }
            public int TransTemp { get; set; }
            public int TotalNoOfPassiveRegenerationsRaw { get; set; }
            public string TotalNoOfPassiveRegenerationsHex { get; set; }
            public int TotalNoOfPassiveRegenerations { get; set; }
            public int TotalNoOfActiveRegenerationsRaw { get; set; }
            public string TotalNoOfActiveRegenerationsHex { get; set; }
            public int TotalNoOfActiveRegenerations { get; set; }
            public int TotalHours { get; set; }
            public string TorqueMode { get; set; }
            public string TimingAdvance { get; set; }
            public string Timeout { get; set; }
            public string ThrottlePosition { get; set; }
            public int ThrottlePos { get; set; }
            public string TenantId { get; set; }
            public string TempPGNData { get; set; }
            public int Speed { get; set; }
            public string ShortTermFuelTrimBank2 { get; set; }
            public string ShortTermFuelTrimBank1 { get; set; }
            public string SerialNo { get; set; }
            public int SelectedGear { get; set; }
            public string SelectProtocolAUTO { get; set; }
            public int SecondaryFuelLevel { get; set; }
            public int SCROutletNoxRaw { get; set; }
            public string SCROutletNoxHex { get; set; }
            public string SCROutletNox { get; set; }
            public int SCRInletNoxRaw { get; set; }
            public string SCRInletNoxHex { get; set; }
            public string SCRInletNox { get; set; }
            public string ResetOBD { get; set; }
            public int RPM { get; set; }
            public int PrimaryFuelLevel { get; set; }
            public int PctTorque { get; set; }
            public int PctLoad { get; set; }
            public string ParkBrakeSwitch { get; set; }
            public string ParameterDateTime { get; set; }
            public string PGNRawValue { get; set; }
            public string PGNParameterName { get; set; }
            public string PGNHexValue { get; set; }
            public string PGNActualValue { get; set; }
            public int PGN { get; set; }
            public int OilTemp { get; set; }
            public int OilPressure { get; set; }
            public int Odometer { get; set; }
            public string Model { get; set; }
            public int MaxSpeed { get; set; }
            public string MassAirFlow { get; set; }
            public string Make { get; set; }
            public string Longitude { get; set; }
            public string LongTermFuelTrimBank2 { get; set; }
            public string LongTermFuelTrimBank1 { get; set; }
            public int LoResOdometer { get; set; }
            public int LoResDistance { get; set; }
            public string LineFeedOff { get; set; }
            public int LedBrightness { get; set; }
            public string Latitude { get; set; }
            public bool IsKeyOn { get; set; }
            public int IntakeTemp { get; set; }
            public int IntakePressure { get; set; }
            public string IntakeMainfoldPressure { get; set; }
            public int InstFuelEcon { get; set; }
            public int IdleHours { get; set; }
            public int IdleFuelUsed { get; set; }
            public int HiResOdometer { get; set; }
            public int HiResMaxSpeed { get; set; }
            public int HiResFuelUsed { get; set; }
            public int HiResDistance { get; set; }
            public int FuelUsed { get; set; }
            public string FuelType { get; set; }
            public int FuelTempRaw { get; set; }
            public string FuelTempHex { get; set; }
            public int FuelTemp { get; set; }
            public string FuelRation { get; set; }
            public int FuelRate { get; set; }
            public string FuelRailPressure { get; set; }
            public string FuelPressure { get; set; }
            public string FuelLevel { get; set; }
            public string FuelConsumptionRate { get; set; }
            public string FaultSource { get; set; }
            public string FaultSPN { get; set; }
            public string FaultOccurrence { get; set; }
            public string FaultFMI { get; set; }
            public string FaultDescription { get; set; }
            public string FaultConversion { get; set; }
            public int FanStateRaw { get; set; }
            public string FanStateHex { get; set; }
            public string FanState { get; set; }
            public string EngineVIN { get; set; }
            public string EngineUnitNo { get; set; }
            public int EngineTurbochargerSpeedRaw { get; set; }
            public string EngineTurbochargerSpeedHex { get; set; }
            public int EngineTurbochargerSpeed { get; set; }
            public string EngineSerialNo { get; set; }
            public string EngineRuntime { get; set; }
            public string EngineRPM { get; set; }
            public string EngineOilTempreture { get; set; }
            public string EngineModel { get; set; }
            public string EngineMake { get; set; }
            public string EngineLoad { get; set; }
            public int EngineIntakeManifoldTempRaw { get; set; }
            public string EngineIntakeManifoldTempHex { get; set; }
            public int EngineIntakeManifoldTemp { get; set; }
            public int EngineIntakeManifoldPressureRaw { get; set; }
            public string EngineIntakeManifoldPressureHex { get; set; }
            public int EngineIntakeManifoldPressure { get; set; }
            public int EngineCrankcasePressureRaw { get; set; }
            public string EngineCrankcasePressureHex { get; set; }
            public int EngineCrankcasePressure { get; set; }
            public string EngineCoolantTemperature { get; set; }
            public string EchoOff { get; set; }
            public int DrvPctTorque { get; set; }
            public string DistanceTraveledWithMILOn { get; set; }
            public string DistanceSincecodescleared { get; set; }
            public decimal Distance { get; set; }
            public string DiagonosticTroubleCodes { get; set; }
            public int DPFSootLoadRaw { get; set; }
            public string DPFSootLoadHex { get; set; }
            public string DPFSootLoad { get; set; }
            public int DPFPressureDifferentialRaw { get; set; }
            public string DPFPressureDifferentialHex { get; set; }
            public int DPFPressureDifferential { get; set; }
            public int DPFOutletTempRaw { get; set; }
            public string DPFOutletTempHex { get; set; }
            public int DPFOutletTemp { get; set; }
            public int DPFInletTempRaw { get; set; }
            public string DPFInletTempHex { get; set; }
            public int DPFInletTemp { get; set; }
            public int DPFAshLoadRaw { get; set; }
            public string DPFAshLoadHex { get; set; }
            public string DPFAshLoad { get; set; }
            public int CurrentGear { get; set; }
            public string CruiseState { get; set; }
            public int CruiseSetSpeed { get; set; }
            public string CruiseSet { get; set; }
            public string CruiseResume { get; set; }
            public string CruiseOnOff { get; set; }
            public string CruiseCoast { get; set; }
            public string CruiseActive { get; set; }
            public string CruiseAccel { get; set; }
            public int CoolantTemp { get; set; }
            public int CoolantPressure { get; set; }
            public int CoolantLevel { get; set; }
            public string ControlModulePowerSupply { get; set; }
            public string CommandEquivalanceRatio { get; set; }
            public string ClutchSwitch { get; set; }
            public string BrakeSwitch { get; set; }
            public int BrakeAppPressure { get; set; }
            public int Brake2AirPressure { get; set; }
            public int Brake1AirPressure { get; set; }
            public int BatteryPotential { get; set; }
            public int BarometricPressureRaw { get; set; }
            public string BarometricPressureHex { get; set; }
            public int BarometricPressure { get; set; }
            public int AvgFuelEcon { get; set; }
            public int AmbientTempRaw { get; set; }
            public string AmbientTempHex { get; set; }
            public int AmbientTemp { get; set; }
            public string AmbientAirTempreture { get; set; }
            public string AirIntakeTemperature { get; set; }
            public string AirFuelRatio { get; set; }
            public int AccelPedal { get; set; }
        }

        private class Epoch
        {
            [JsonProperty("$date")]
            public long date { get; set; }
        }
    }
}
