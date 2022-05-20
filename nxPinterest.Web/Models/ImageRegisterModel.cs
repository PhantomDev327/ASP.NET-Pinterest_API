using System.Collections.Generic;

namespace nxPinterest.Web.Models
{
    public class ImageRegisterModel
    {
        public Services.Models.Request.IndividualImageRegistrationRequests ImageRegistrationRequests { get; set; } = 
            new Services.Models.Request.IndividualImageRegistrationRequests();
        public IList<Data.Models.UserMedia> UserMediaList { get; set; } = new List<Data.Models.UserMedia>();
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
