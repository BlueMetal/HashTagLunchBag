using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.AdminPortal.Models
{
    public class EventGeneralSettingsViewModel
    {
        [Required(ErrorMessage = "An EventID is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(30, MinimumLength = 6)]
        public string EventId { get; set; }

        [Required(ErrorMessage = "An Event Name is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(50, MinimumLength = 6)]
        public string EventName { get; set; }
        public bool IsEventActive { get; set; }
    }
}
