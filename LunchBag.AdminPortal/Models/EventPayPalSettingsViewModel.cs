using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.AdminPortal.Models
{
    public class EventPayPalSettingsViewModel
    {
        [Required(ErrorMessage = "An EventID is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(30, MinimumLength = 6)]
        public string EventId { get; set; }

        [Required(ErrorMessage = "An Client Id is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(100)]
        public string ClientId { get; set; }

        [Required(ErrorMessage = "An Secret is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(100)]
        public string Secret { get; set; }

        [Required(ErrorMessage = "An Email is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Business Name")]
        [Required(ErrorMessage = "An Business Name is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(100)]
        public string BusinessName { get; set; }

        [Display(Name = "Donation Name")]
        [Required(ErrorMessage = "An Donation Name is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(100)]
        public string DonationName { get; set; }

        [Display(Name = "Currency")]
        [Required(ErrorMessage = "An Currency is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(3, MinimumLength = 1)]
        public string Currency { get; set; }

        [Display(Name = "Thanks Note")]
        [Required(ErrorMessage = "An Thanks Note is required")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(500)]
        public string ThanksNote { get; set; }
    }
}
