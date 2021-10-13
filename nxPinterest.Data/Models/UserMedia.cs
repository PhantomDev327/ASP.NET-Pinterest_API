using System;
using System.Collections.Generic;
using System.Text;

namespace nxPinterest.Data.Models
{
    public class UserMedia
    {
        public int MediaId { get; set; }
        public string UserId { get; set; }
        public string MediaFileName { get; set; }
        public string MediaFileType { get; set; }
        public string MediaUrl { get; set; }
        public string Tags { get; set; }
        public DateTime DateTimeUploaded { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
