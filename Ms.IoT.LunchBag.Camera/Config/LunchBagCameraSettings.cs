using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ms.IoT.LunchBag.Camera.Config
{
    public sealed class LunchBagCameraSettings
    {
        public string CognitiveVisionAPIKey { get; set; }
        public string EventId { get; set; }
        public int CameraIndex { get; set; }
        public string CameraResolution { get; set; }
        public int IntervalPhoto { get; set; }
    }
}
