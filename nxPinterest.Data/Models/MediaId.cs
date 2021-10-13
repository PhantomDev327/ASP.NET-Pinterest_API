using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace nxPinterest.Data.Models
{
    public partial class MediaId
    {
        [Key]
        public int Sql_id { get; set; }
        public string Cosmos_db { get; set; }
        public string Storage_table { get; set; }
        public string Storage_blob { get; set; }
    }
}
