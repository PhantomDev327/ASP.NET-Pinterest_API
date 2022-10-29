using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using System;
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

        public async Task<bool> IsUserAlbumAlreadyExists(string albumName)
        {
            if (string.IsNullOrEmpty(albumName)) return false;

            return await Context.UserAlbums.AnyAsync(n => n.AlbumName == albumName);
        }
    }
}
