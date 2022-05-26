using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace nxPinterest.Services.Models.Request
{
    public class ImageRegistrationRequests
    {
        public int MediaId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        [MinLength(1)]
        public IList<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string ProjectTags { get; set; }
        public string PhotoTags { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateTimeUploaded { get; set; } = DateTime.Now;
        public string BtnName { get; set; }
    }
}
