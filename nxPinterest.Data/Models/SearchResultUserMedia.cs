using System;
using System.Collections.Generic;
using System.Text;

namespace nxPinterest.Data.Models
{
    public class SearchResultUserMedia
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string MediaFileName { get; set; }
        public string MediaFileType { get; set; }
        public string MediaUrl { get; set; }
        public string Tags { get; set; }
        public DateTime DateTimeUploaded { get; set; }
        public string SearchScore { get; set; }
    }
}
