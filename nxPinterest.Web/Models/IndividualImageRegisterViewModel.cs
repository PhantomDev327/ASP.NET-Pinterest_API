using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace nxPinterest.Web.Models
{
    public class IndividualImageRegisterViewModel : BaseViewModel
    {
        public IList<RegisterImageInfo> ImageInfoList { get; set; } = new List<RegisterImageInfo>();

        public int imageInfoListSize { get; set; }

        //public ImageInfo imageNew { get; set; }
    }

    public class RegisterImageInfo
    {
        //[Required(ErrorMessage = "タイトルを入力してください")]
        public string Title { get; set; }
        public string Description { get; set; }

        //[Required(ErrorMessage = "写真をアップロードしてください")]
        //[MinLength(1)]
        public IFormFile Images { get; set; }
        public string OriginalTags { get; set; }
        public string AITags { get; set; }
        public string url { get; set; }
        public string imgName { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateTimeUploaded { get; set; } = DateTime.Now;
    }
}
