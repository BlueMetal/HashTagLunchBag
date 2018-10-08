using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Models
{
    public class RegistrationCheckUserModel
    {
        public bool UserExist { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
