using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LunchBag.Common.IoTMessages
{
    // Mobiliya's iot payload is a json object with a single string attribute, which 
    //  is actually a serialized json object itself

    public class DeliveryTriggerIoTMessage : IIoTMessage
    {
        public string Parameters { get; set; }

        [JsonProperty("Type")]
        public IoTMessageType MessageType { get; set; }
    }

    public class DeliveryIoTMessage
    {
        public decimal AccelPedal { get; set; }
        public decimal AmbientTemp { get; set; }
        public decimal AmbientTempRaw { get; set; }
        public decimal AvgFuelEcon { get; set; }
        public decimal BarometricPressure { get; set; } = -1.0M;
        public decimal BarometricPressureRaw { get; set; } = -1.0M;
        public decimal BatteryPotential { get; set; }
        public decimal Brake1AirPressure { get; set; }
        public decimal Brake2AirPressure { get; set; }
        public decimal BrakeAppPressure { get; set; }
        public decimal CoolantLevel { get; set; }
        public decimal CoolantPressure { get; set; }
        public decimal CoolantTemp { get; set; }
        public decimal CruiseSetSpeed { get; set; } = -1.0M;
        public decimal CurrentGear { get; set; }
        public decimal DPFAshLoadRaw { get; set; } = -1.0M;
        public decimal DPFInletTemp { get; set; }
        public decimal DPFInletTempRaw { get; set; }
        public decimal DPFOutletTemp { get; set; }
        public decimal DPFOutletTempRaw { get; set; }
        public decimal DPFPressureDifferential { get; set; }
        public decimal DPFPressureDifferentialRaw { get; set; }
        public decimal DPFSootLoadRaw { get; set; } = -1.0M;
        public decimal Distance { get; set; }
        public int DrvPctTorque { get; set; }
        public decimal EngineCrankcasePressure { get; set; } = -1.0M;
        public decimal EngineCrankcasePressureRaw { get; set; } = -1.0M;
        public decimal EngineIntakeManifoldPressure { get; set; }
        public decimal EngineIntakeManifoldPressureRaw { get; set; }
        public decimal EngineIntakeManifoldTemp { get; set; }
        public decimal EngineIntakeManifoldTempRaw { get; set; }
        public decimal EngineTurbochargerSpeed { get; set; } = -1.0M;
        public decimal EngineTurbochargerSpeedRaw { get; set; } = -1.0M;
        public decimal FanStateRaw { get; set; } = -1.0M;
        public string FaultConversion { get; set; }
        public string FaultDescription { get; set; }
        public string FaultFMI { get; set; }
        public string FaultOccurrence { get; set; }
        public string FaultSPN { get; set; }
        public string FaultSource { get; set; }
        public decimal FuelRate { get; set; }
        public decimal FuelTemp { get; set; }
        public decimal FuelTempRaw { get; set; }
        public decimal FuelUsed { get; set; }
        public decimal HiResDistance { get; set; }
        public decimal HiResFuelUsed { get; set; }
        public decimal HiResMaxSpeed { get; set; }
        public decimal HiResOdometer { get; set; }
        public decimal IdleFuelUsed { get; set; }
        public decimal IdleHours { get; set; }
        public decimal InstFuelEcon { get; set; }
        public decimal IntakePressure { get; set; }
        public decimal IntakeTemp { get; set; }
        public bool IsKeyOn { get; set; }
        public string Latitude { get; set; }
        public int LedBrightness { get; set; }
        public decimal LoResDistance { get; set; }
        public decimal LoResOdometer { get; set; }
        public string Longitude { get; set; }
        public int MaxSpeed { get; set; }
        public decimal Odometer { get; set; }
        public decimal OilPressure { get; set; }
        public decimal OilTemp { get; set; }
        public int PGN { get; set; }
        public string ParameterDateTime { get; set; }
        public int PctLoad { get; set; }
        public int PctTorque { get; set; }
        public decimal PrimaryFuelLevel { get; set; }
        public int RPM { get; set; }
        public decimal SCRInletNoxRaw { get; set; }
        public decimal SCROutletNoxRaw { get; set; } = -1.0M;
        public decimal SecondaryFuelLevel { get; set; }
        public int SelectedGear { get; set; }
        public decimal Speed { get; set; }
        public string TenantId { get; set; }
        public decimal ThrottlePos { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalNoOfActiveRegenerations { get; set; } = -1.0M;
        public decimal TotalNoOfActiveRegenerationsRaw { get; set; } = -1.0M;
        public decimal TotalNoOfPassiveRegenerations { get; set; } = -1.0M;
        public decimal TotalNoOfPassiveRegenerationsRaw { get; set; } = -1.0M;
        public decimal TransTemp { get; set; }
        public string TripId { get; set; }
        public string UserId { get; set; }
        public string VehicleId { get; set; }
        public string VehicleRegNumber { get; set; }
        public decimal VehicleSpeed { get; set; }
        public bool isConnected { get; set; }
        public int ID { get; set; }
    }
}
