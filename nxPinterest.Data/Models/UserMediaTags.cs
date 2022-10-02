using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Data.Models
{
    public class UserMediaTags
    {
        [Key]
        public string TagsMediaName { get; set; }
        public int TagsType { get; set; }
        public double Confidence { get; set; }
        public string Tag { get; set; }
    }
}
