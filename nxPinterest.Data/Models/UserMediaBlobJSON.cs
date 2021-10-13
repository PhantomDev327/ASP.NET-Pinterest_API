using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace nxPinterest.Data.Models
{
    [JsonObject]
    public class UserMediaBlobJSON : UserMediaJSON
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
