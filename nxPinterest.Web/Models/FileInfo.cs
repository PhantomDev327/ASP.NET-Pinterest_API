using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nxPinterest.Web.Models
{
    public class FileInfo
    {
        public IFormFile File { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public bool IsPrimary { get; set; }
    }
}
