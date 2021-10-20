using System;
using System.Collections.Generic;

namespace nxPinterest.Data.Models
{
    public class UserMedia
    {

        public UserMedia() {
            UserMediaThumbnails = new List<UserMediaThumbnails>();
        }
        public int MediaId { get; set; }
        public string UserId { get; set; }
        public string MediaTitle { get; set; }
        public string MediaDescription { get; set; }
        public string MediaFileName { get; set; }
        public string MediaFileType { get; set; }
        public string MediaUrl { get; set; }
        public string Tags { get; set; }
        public string PhotoTags { get; set; }
        public DateTime DateTimeUploaded { get; set; }
        public virtual ApplicationUser User { get; set; }
        public IList<UserMediaThumbnails> UserMediaThumbnails { get; set; } 
    }
}
