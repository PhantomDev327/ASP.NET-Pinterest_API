using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.Models;

namespace nxPinterest.Data.Repositories
{
    public class UserRepository : BaseRepository<ApplicationUser>,IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
