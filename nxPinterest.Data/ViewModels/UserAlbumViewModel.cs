using nxPinterest.Data.Enums;
using System;

namespace nxPinterest.Data.ViewModels
{
    public class UserAlbumViewModel
    {
        public int AlbumId { get; set; }

        public string AlbumName { get; set; }

        public string UserId { get; set; }

        public int ContainerId { get; set; }

        public string AlbumUrl { get; set; }

        public int ImageCount { get; set; } 

        public DateTime? AlbumCreatedat { get; set; }

        public string AlbumThumbnailUrl { get; set; }
        public DateTime? AlbumExpireDate { get; set; }

        public int AlbumType { get; set; }

        public string Comment { get; set; }
    }
}
