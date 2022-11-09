using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories
{
    public class UserAlbumRepository : BaseRepository<UserAlbum>, IUserAlbumRepository
    {
        public UserAlbumRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> CheckExpiryDayAlbum(int albumId)
        {
            if (albumId == 0)
            {
                return false;
            }

            UserAlbum result = await Context.UserAlbums.Select(n=>new UserAlbum
            {
                AlbumId = albumId,
                AlbumExpireDate = n.AlbumExpireDate
            }).SingleOrDefaultAsync(n => n.AlbumId == albumId);

            if (result is null)
            {
                return false;
            }

            DateTime? expiryDate = result.AlbumExpireDate;
            TimeSpan diff = (TimeSpan)(DateTime.UtcNow - expiryDate);

            // if day >0 has expired
            return diff.Days > 0;
        }

        public async Task<IEnumerable<UserAlbumViewModel>> GetAlbumByUser(string userId)
        {

            if (string.IsNullOrEmpty(userId)) return new List<UserAlbumViewModel>();

            var result = await Context.UserAlbums.Select(n => new UserAlbumViewModel
            {
                UserId = n.UserId,
                AlbumName = n.AlbumName,
                AlbumId = n.AlbumId,
                AlbumCreatedat = n.AlbumCreatedat,
                AlbumUrl = n.AlbumUrl,
                AlbumType = (int)n.AlbumType,
                AlbumThumbnailUrl = n.AlbumThumbnailUrl
            }).Where(n => n.UserId == userId && n.AlbumType == (int)Data.Enums.AlbumType.Album).OrderByDescending(n => n.AlbumCreatedat).ToListAsync();

            return result!=null ? result : new List<UserAlbumViewModel>();
        }

        public async Task<int> GetAlbumIdByUrl(string url)
        {
            int albumId = 0;

            if (string.IsNullOrWhiteSpace(url)) return albumId;

            var result = await Context.UserAlbums.Select(n => new UserAlbum
            {
                AlbumId = n.AlbumId,
                AlbumUrl = n.AlbumUrl
            }).SingleOrDefaultAsync(n=>n.AlbumUrl.Contains(url));

            //if day > 0 has expired are return 0;
            if (await CheckExpiryDayAlbum(albumId)) albumId = 0;

            return albumId;
        }

        public (int albumId, string albumName) IsUserAlbumAlreadyExists(string albumName)
        {
            if (string.IsNullOrEmpty(albumName)) return (0, null);

            var result = Context.UserAlbums.Select(n => new
            {
                n.AlbumName,
                n.AlbumId
            }).SingleOrDefault(n => n.AlbumName == albumName);

            return result != null ? (result.AlbumId, result.AlbumName) : (0, null);
        }
    }
}
