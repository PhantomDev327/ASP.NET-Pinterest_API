using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;

namespace nxPinterest.Data.Repositories
{
    public class UserAlbumRepository : BaseRepository<UserAlbum>, IUserAlbumRepository
    {
        public UserAlbumRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
