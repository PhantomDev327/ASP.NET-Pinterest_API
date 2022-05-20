using Newtonsoft.Json;

namespace nxPinterest.Data.Models
{
    [JsonObject]
    public class UserMediaCosmosJSON : UserMediaJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("mediaid")]
        public int MediaId { get; set; }

        [JsonProperty("mediatitle")]
        public string MediaTitle { get; set; }

        [JsonProperty("mediadescription")]
        public string MediaDescription { get; set; }

        [JsonProperty("projecttags")]
        public string ProjectTags { get; set; }
    }
}
