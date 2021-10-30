using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace nxPinterest.Web.Models
{
    public class FileInfo
    {
        public string PrimaryImagePath { get; set; }
        public string SecondaryImagePath { get; set; }
    }
}
