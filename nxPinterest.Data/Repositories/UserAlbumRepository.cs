using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
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

            UserAlbum result = await Context.UserAlbums.SingleOrDefaultAsync(n => n.AlbumId == albumId);

            if (result is null)
            {
                return false;
            }

            DateTime? expiryDate = result.AlbumExpireDate;
            TimeSpan diff = (TimeSpan)(DateTime.UtcNow - expiryDate);

            // if day >30 has expired
            return diff.Days > 30;
        }

        public async Task<IEnumerable<UserAlbum>> GetAlbumByUser(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<UserAlbum>();

            var result = await Context.UserAlbums.Select(n => new UserAlbum
            {
                UserId = n.UserId,
                AlbumName = n.AlbumName,
                AlbumId = n.AlbumId,
                AlbumCreatedat = n.AlbumCreatedat,
                AlbumUrl = n.AlbumUrl
            }).Where(n => n.UserId == userId).OrderByDescending(n => n.AlbumCreatedat).ToListAsync();

            return result != null ? result : new List<UserAlbum>();
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
