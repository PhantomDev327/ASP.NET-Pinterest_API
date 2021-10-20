using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace nxPinterest.Data.Models
{
    public class UserMediaThumbnails
    {
        public int Id { get; set; }
        public int MediaId { get; set; }
        public string MediaFileName { get; set; }   
        public string MediaFileType { get; set; }
        public string MediaUrl { get; set; }
        public string Tags { get; set; }
        public DateTime DateTimeUploaded { get; set; }
        public UserMedia UserMedia { get; set; }
    }
}
