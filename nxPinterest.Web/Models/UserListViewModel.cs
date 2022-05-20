using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nxPinterest.Services.Models;
using nxPinterest.Utils;

namespace nxPinterest.Web.Models
{
    public class UserListViewModel
    {
        public IList<Data.Models.ApplicationUser> ApplicationUserList { get; set; } = new List<Data.Models.ApplicationUser>();
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public string CurrentPageName { get; set; }

    }
}
