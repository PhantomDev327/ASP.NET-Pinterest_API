using System;
using System.Collections.Generic;

namespace nxPinterest.Data.ViewModels
{
    public class HomeAlbumViewModel
    {
        public DateTime? AlbumCreateDate { get; set; }
        public IEnumerable<SharedLinkAlbumMediaViewModel> Albums { get; set; }
    }
}
