using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace nxPinterest.Data.Models
{
    public class UserMedia
    {
        public int MediaId { get; set; }
        public string UserId { get; set; }
        public string MediaTitle { get; set; }
        public string MediaDescription { get; set; }
        public string MediaFileName { get; set; }
        public string MediaFileType { get; set; }
        public string MediaUrl { get; set; }
        public string Tags { get; set; } // generated photo tags
        public string ProjectTags { get; set; } // generated ProjectTags
        public string MediaSmallUrl { get; set; }
        public string MediaThumbnailUrl { get; set; }
        public int ContainerId { get; set; }

        public List<UserMediaTags> TagsList { get; set; } // generated ProjectTags

        [NotMapped]
        public string tagsJson { get; set; }

        public DateTime DateTimeUploaded { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
