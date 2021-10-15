using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nxPinterest.Services.Models;

namespace nxPinterest.Web.Models
{
    public class HomeViewModel
    {
        public Services.Models.Request.ImageRegistrationRequests ImageRegistrationRequests { get; set; } = new Services.Models.Request.ImageRegistrationRequests();
    }
}
