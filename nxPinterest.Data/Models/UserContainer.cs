using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace nxPinterest.Data.Models
{
    public class UserContainer
    {
        [System.ComponentModel.DataAnnotations.Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int container_id { get; set; }
        public string container_name { get; set; }
        public Boolean container_visibility { get; set; }
    }
}
