using System.Threading.Tasks;

namespace nxPinterest.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<int> UpdateUserContainerAsync(string UserId, int UserContainer);
        Task<int> UpdateUserViewModeAsync(string UserId, int ViewMode);
    }
}