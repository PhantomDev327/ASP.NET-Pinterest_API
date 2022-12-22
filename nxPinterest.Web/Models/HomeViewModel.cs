using System.Collections.Generic;

namespace nxPinterest.Web.Models
{
    public class HomeViewModel : BaseViewModel
    {
        //public Services.Models.Request.ImageRegistrationRequests ImageRegistrationRequests { get; set; } = new Services.Models.Request.ImageRegistrationRequests();
        public ImageRegisterViewModel ImageRegistrationVM { get; set; } = new ImageRegisterViewModel();
        
        public IList<Data.Models.UserMedia> UserMediaList { get; set; } = new List<Data.Models.UserMedia>();

        //// NEW : Cosmos DB
        //public IList<Data.Models.UserMediaCosmosJSON> UserMediaList { get; set; } = new List<Data.Models.UserMediaCosmosJSON>();
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }

}
