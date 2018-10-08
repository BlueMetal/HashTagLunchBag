using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.SimulatorApp.Config
{
    public class SimulationConfig
    {
        public int FleetUpdateSpan { get; set; }
        public int CarUpdateSpan { get; set; }
        public string ConnectionString { get; set; }
        public string BingUrl { get; set; }
        public string BingKey { get; set; }
        public double BingTolerance { get; set; }
    }
}
