using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport
{
    public class DeliveryModel : IEntityModel
    {
        public static readonly int kDeliveryStatusNotStarted = 0;
        public static readonly int kDeliveryStatusInProgress = 1;
        public static readonly int kDeliveryStatusComplete = 2;

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string DeliveryId { get; set; }
        public string VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string Destination { get; set; }
        public LocationModel DestinationLocation { get; set; }
        public GeocodeModel DestinationGeocode { get; set; }
        public int LunchCount { get; set; }
        public string EventId { get; set; }
        public string LocationId { get; set; }
        public int Status { get; set; }
        public string RouteId { get; set; }
        public decimal MilesToDestination { get; set; }
        public decimal MilesTraveled { get; set; }
        public DateTime Date { get; set; }
        public string ETag { get; set; }
    }
}
