using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace nxPinterest.Data.Models
{

    [JsonObject]
    public class UserMediaJSON
    {
        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("mediafilename")]
        public string MediaFileName { get; set; }

        [JsonProperty("mediafiletype")]
        public string MediaFileType { get; set; }

        [JsonProperty("mediaurl")]
        public string MediaUrl { get; set; }

        [JsonProperty("tags")]
        public List<ImageTagJSON> Tags { get; set; }

        [JsonProperty("datetimeuploaded")]
        public DateTime DateTimeUploaded { get; set; }
    }
    [JsonObject]
    public class ImageAnalysisJSON
    {
        [JsonProperty("tags")]
        public List<ImageTagJSON> Tags { get; set; }

        [JsonProperty("requestid")]
        public string RequestId { get; set; }

        [JsonProperty("metadata")]
        public ImageMetadataJSON MetaData { get; set; }

        [JsonProperty("modelversion")]
        public string ModelVersion { get; set; }
    }
    [JsonObject]
    public class ImageTagJSON
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }
    [JsonObject]
    public class ImageMetadataJSON
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }
}
