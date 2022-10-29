using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories
{
    public class UserAlbumRepository : BaseRepository<UserAlbum>, IUserAlbumRepository
    {
        public UserAlbumRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> IsUserAlbumAlreadyExists(string albumName)
        {
            if (string.IsNullOrEmpty(albumName)) return false;

            return await Context.UserAlbums.AnyAsync(n => n.AlbumName == albumName);
        }
    }
}
