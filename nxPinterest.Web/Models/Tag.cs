using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nxPinterest.Web.Models
{
    public class Tag
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("confidence")]
        public decimal Confidence { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
    }
}
