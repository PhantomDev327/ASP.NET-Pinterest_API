using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace nxPinterest.Data.Models
{
    public class SearchIndexUserMediaSQL
    {

        [JsonPropertyName("media_id")]
        [SearchableField(IsKey = true, IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string media_id { get; set; }

        [JsonPropertyName("user_id")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string user_id { get; set; }

        [JsonPropertyName("media_file_name")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string media_file_name { get; set; }

        [JsonPropertyName("media_file_type")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string media_file_type { get; set; }

        [JsonPropertyName("media_url")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string media_url { get; set; }

        [JsonPropertyName("DateTimeUploaded")]
        [SimpleField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string DateTimeUploaded { get; set; }

        [JsonPropertyName("Tags")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = false)]
        public string Tags { get; set; }

        [JsonPropertyName("search_score")]
        public string search_score { get; set; }
    }
}
