using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace nxPinterest.Web.Models
{
    public class ImageRegisterViewModel
    {
        //public Services.Models.Request.IndividualImageRegistrationRequests ImageRegistrationRequests { get; set; } = 
        //    new Services.Models.Request.IndividualImageRegistrationRequests();
        //public IList<Data.Models.UserMedia> UserMediaList { get; set; } = new List<Data.Models.UserMedia>();
        //public int PageIndex { get; set; } = 1;
        //public int TotalPages { get; set; }
        //public int TotalRecords { get; set; }

        //public int MediaId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        [MinLength(1)]
        public IList<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string ProjectTags { get; set; }
        //public string PhotoTags { get; set; }
        //public string Created { get; set; }
        //public DateTime Uploaded { get; set; } = DateTime.Now;

        public string BtnName { get; set; }
        public IList<string> SuggestTagsList { get; set; }
    }
}
