using System.Collections.Generic;
using nxPinterest.Data.Models;
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
        public IList<UserContainer> UserContainers { get; set; }

        public int currentContainer { get; set; }

        public bool AlbumMode { get; set; } = false;


    }
}
