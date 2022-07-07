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

        [JsonProperty("media_title")]
        public string MediaTitle { get; set; }

        [JsonProperty("media_description")]
        public string MediaDescription { get; set; }

        [JsonProperty("ProjectTags")]
        public string ProjectTags { get; set; }

        [JsonProperty("media_thumbnail_url")]
        public string MediaThumbnailUrl { get; set; }

        [JsonProperty("container_id")]
        public string container_id { get; set; }
    }
}
