using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.Models;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories
{
    public class UserRepository : BaseRepository<ApplicationUser>,IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<int> UpdateContainerAsync(string userId, int containerId)
        {
            ApplicationUser user = await Context.Users.FindAsync(userId);
            user.container_id = containerId;
            this.Context.Users.Update(user);
            await Context.SaveChangesAsync();
            return user.container_id;
        }

        public async Task<int> UpdateUserViewModeAsync(string userId, int ViewMode)
        {
            ApplicationUser user = await Context.Users.FindAsync(userId);
            user.DisplayMode = ViewMode == 0 ? " " : "ALBUM";
            this.Context.Users.Update(user);
            await Context.SaveChangesAsync();
            return ViewMode;
        }
    }
}
