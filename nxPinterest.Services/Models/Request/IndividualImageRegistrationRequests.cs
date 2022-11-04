using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace nxPinterest.Services.Models.Request
{
    public class IndividualImageRegistrationRequests
    {
        public IList<ImageInfo> ImageInfoList { get; set; } = new List<ImageInfo>();

        public int imageInfoListSize { get; set; }

        //public ImageInfo imageNew { get; set; }
    }

    public class ImageInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public IFormFile Images { get; set; }
        public string OriginalTags { get; set; }
        public string AITags { get; set; }
        public string url { get; set; }
        public string imgName { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateTimeUploaded { get; set; } = DateTime.Now;
    }
}
