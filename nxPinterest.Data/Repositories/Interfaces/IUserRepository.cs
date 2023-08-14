using nxPinterest.Data.Models;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IUserRepository :IBaseRepository<ApplicationUser>
    {
        Task<int> UpdateContainerAsync(string userId, int containerId);
        Task<int> UpdateUserViewModeAsync(string userId, int ViewMode);
    }
}
