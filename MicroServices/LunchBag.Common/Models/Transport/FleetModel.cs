using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport
{
    public class FleetModel
    {
        public string FleetId { get; set; }
        public int VehicleCount { get; set; }
        public IEnumerable<FleetVehicle> Vehicles { get; set; }
    }

    public class FleetVehicle
    {
        public string VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
    }
}
