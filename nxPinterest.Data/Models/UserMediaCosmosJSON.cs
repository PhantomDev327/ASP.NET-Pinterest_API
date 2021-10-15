using Newtonsoft.Json;

namespace nxPinterest.Data.Models
{
    [JsonObject]
    public class UserMediaCosmosJSON : UserMediaJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
