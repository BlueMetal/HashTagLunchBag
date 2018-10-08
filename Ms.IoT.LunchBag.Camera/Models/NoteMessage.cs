using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ms.IoT.LunchBag.Camera.Models
{
    public sealed class NoteMessage
    {
        public string Note { get; set; }
        public string EventId { get; set; }
        public string Type { get; set; }
    }
}
