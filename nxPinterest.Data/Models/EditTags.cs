using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Data.Models
{
    public class EditTags
    {
        [Key]
        public string id { get; set; }
        public List<EditTag> tags { get; set; }
    }

    public class EditTag
    {
        [Key]
        public string Name { get; set; }
        public double Confidence { get; set; }
        public int Type { get; set; }
    }
}
