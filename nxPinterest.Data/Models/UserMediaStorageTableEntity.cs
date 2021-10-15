using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace nxPinterest.Data.Models
{
    public partial class UserMediaStorageTableEntity : TableEntity
    {
        public string UserId { get; set; }
        public string MediaFileName { get; set; }
        public string MediaFileType { get; set; }
        public string MediaUrl { get; set; }
        public string Tags { get; set; }
        public DateTime DateTimeUploaded { get; set; }
    }
}
