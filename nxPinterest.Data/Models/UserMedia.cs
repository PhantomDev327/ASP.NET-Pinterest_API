using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace nxPinterest.Data.Models
{
    public class UserMedia
    {
        public int MediaId { get; set; }
        public string UserId { get; set; }
        public string MediaTitle { get; set; } = "";
        public string MediaDescription { get; set; } = "";
        public string MediaFileName { get; set; }
        public string MediaFileType { get; set; }
        public string MediaUrl { get; set; }
        public string Tags { get; set; }
        //public string ProjectTags { get; set; } = "";
        public string OriginalTags { get; set; } = "";
        public string AITags { get; set; }
        public string MediaSmallUrl { get; set; }
        public string MediaThumbnailUrl { get; set; }
        public string SearchText { get; set; } = "";
        public int ContainerId { get; set; }
        public DateTime? Created { get; set; }
        public DateTime Uploaded { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? Deleted { get; set; }
        public byte Status { get; set; }

        [NotMapped]
        public List<UserMediaTags> TagsList { get; set; } // New Tags

        [NotMapped]
        public string tagsJson { get; set; }

        public DateTime DateTimeUploaded { get; set; }
        public virtual ApplicationUser User { get; set; }

        public List<UserAlbumMedia> UserAlbumMedias { get; set; } = new List<UserAlbumMedia>();
    }
}
