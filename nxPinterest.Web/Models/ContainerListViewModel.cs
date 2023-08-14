using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nxPinterest.Services.Models;
using nxPinterest.Utils;

namespace nxPinterest.Web.Models
{
    public class ContainerListViewModel : BaseViewModel
    {
        public IList<Data.Models.UserContainer> containerList { get; set; } = new List<Data.Models.UserContainer>();
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public String CurrentPageName { get; set; }
    }
}
