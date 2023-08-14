using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Services.Models.Request
{
    public class ImageRegistrationRequests
    {
        public int MediaId { get; set; }
        [Required(ErrorMessage = "タイトルを入力してください")]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "写真をアップロードしてください")]
        [MinLength(1)]
        public IList<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string OriginalTags { get; set; }
        public string AITags { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateTimeUploaded { get; set; } = DateTime.Now;
        public string BtnName { get; set; }
        public bool SaveMode { get; set; } = false;
    }
}
