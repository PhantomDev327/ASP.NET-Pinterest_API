using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;

namespace nxPinterest.Data.Repositories
{
    public class UserAlbumMediaRepository : BaseRepository<UserAlbumMedia>, IUserAlbumMediaRepository
    {
        public UserAlbumMediaRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
