using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Services.Interfaces;
using System.Threading.Tasks;

namespace nxPinterest.Services
{
    public class UserManagementService : IUserManagementService
    {

        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;


        public UserManagementService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> UpdateUserContainerAsync(string UserId, int UserContainer)
        {
            return await _userRepository.UpdateContainerAsync(UserId, UserContainer);
        }
        
    }
}