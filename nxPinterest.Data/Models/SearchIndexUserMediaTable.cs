using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace nxPinterest.Data.Models
{
    public class SearchIndexUserMediaTable
    {
        [JsonPropertyName("Key")]
        [SearchableField(IsKey = true, IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string Key { get; set; }

        [JsonPropertyName("PartitionKey")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string PartitionKey { get; set; }

        [JsonPropertyName("RowKey")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string RowKey { get; set; }

        [JsonPropertyName("DateTimeUploaded")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public DateTime DateTimeUploaded { get; set; }

        [JsonPropertyName("MediaFileName")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string MediaFileName { get; set; }

        [JsonPropertyName("MediaFileType")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string MediaFileType { get; set; }

        [JsonPropertyName("MediaUrl")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string MediaUrl { get; set; }

        [JsonPropertyName("Tags")]
        [SearchableField(IsFacetable = true, IsFilterable = true, IsSortable = false)]
        public string Tags { get; set; }

        [JsonPropertyName("UserId")]
        [SimpleField(IsFacetable = true, IsFilterable = true, IsSortable = true)]
        public string UserId { get; set; }

        [JsonPropertyName("search_score")]
        public string search_score { get; set; }
    }
}
