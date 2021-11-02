using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nxPinterest.Web.Models
{
    public class TagList
    {
        [JsonProperty("tags")]
        public IList<Tag> Tags { get; set; } = new List<Tag>();
        public string requestid { get; set; }
        public MetaData metadata { get; set; }
        public string modelversion { get; set; }
    }

    public class MetaData { 
        public int height { get; set; }
        public int width { get; set; }
        public string format { get; set; }
    }
}
