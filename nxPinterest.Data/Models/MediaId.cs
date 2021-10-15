using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nxPinterest.Data.Models
{
    public partial class MediaId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Sql_id { get; set; }
        public string Cosmos_db { get; set; }
        public string Storage_table { get; set; }
        public string Storage_blob { get; set; }
    }
}
