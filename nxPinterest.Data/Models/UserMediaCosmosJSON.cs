using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace nxPinterest.Data.Models
{
    [JsonObject]
    public class UserMediaCosmosJSON : UserMediaJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
