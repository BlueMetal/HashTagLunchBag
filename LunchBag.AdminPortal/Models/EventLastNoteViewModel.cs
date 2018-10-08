using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.AdminPortal.Models
{
    public class EventLastNoteViewModel
    {
        [Required(ErrorMessage = "An EventID is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(30, MinimumLength = 6)]
        public string EventId { get; set; }

        public string LastNote { get; set; }
    }
}
