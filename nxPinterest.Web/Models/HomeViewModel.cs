using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nxPinterest.Services.Models;
using nxPinterest.Utils;

namespace nxPinterest.Web.Models
{
    public class HomeViewModel
    {
        public string SearchKey { get; set; }
        public Services.Models.Request.ImageRegistrationRequests ImageRegistrationRequests { get; set; } = new Services.Models.Request.ImageRegistrationRequests();
        // OLD : SQL DB
        //public IList<Data.Models.UserMedia> UserMediaList { get; set; } = new List<Data.Models.UserMedia>();

        // NEW : Cosmos DB
        public IList<Data.Models.UserMediaCosmosJSON> UserMediaList { get; set; } = new List<Data.Models.UserMediaCosmosJSON>();
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public virtual string Discriminator { get; set; }
    }
}
