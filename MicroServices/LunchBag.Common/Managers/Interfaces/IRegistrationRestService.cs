﻿using LunchBag.Common.Config;
using LunchBag.Common.Models;
using LunchBag.Common.Models.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public interface IRegistrationRestService
    {
        Task<byte[]> GetRegistrationsExtract();
    }
}
