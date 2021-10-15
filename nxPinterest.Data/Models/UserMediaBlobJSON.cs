using Newtonsoft.Json;

namespace nxPinterest.Data.Models
{
    [JsonObject]
    public class UserMediaBlobJSON : UserMediaJSON
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
