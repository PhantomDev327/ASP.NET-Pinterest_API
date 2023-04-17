using System.Collections.Generic;
using nxPinterest.Data.ViewModels;

namespace nxPinterest.Web.Models
{
    public class BaseViewModel
    {
        public string SearchKey { get; set; }
        public IList<string> TagList { get; set; }
        public IList<UserAlbumViewModel> AlbumList { get; set; }

        public string Discriminator { get; set; }
        public string UserDispName { get; set; }
        public int SizeRange { get; set; }


    }
}
