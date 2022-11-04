using System;
using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Web.Models
{
    /// <summary>
    /// UserMedia EditページのViewModel
    /// </summary>
    public class EditViewModel
    {
        public int MediaId { get; set; }
        public string UserId { get; set; }
        public int ContainerId { get; set; }
        public string MediaFileName { get; set; }
        public string MediaFileType { get; set; }
        [Required(ErrorMessage = "タイトルを入力してください。")]
        public string MediaTitle { get; set; } = "";
        [MaxLength(1000, ErrorMessage = "1000文字以内で入力してください。")]
        public string MediaDescription { get; set; } = "";
        public string Tags { get; set; }
        public string OriginalTags { get; set; } = "";
        public string AITags { get; set; }
        public DateTime? Created { get; set; }
        public DateTime Uploaded { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? Deleted { get; set; }
        public byte Status { get; set; }

        public string MediaUrl { get; set; }
        public string MediaSmallUrl { get; set; }
        public string MediaThumbnailUrl { get; set; }
    }
}
