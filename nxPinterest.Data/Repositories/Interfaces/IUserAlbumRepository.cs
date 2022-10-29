using nxPinterest.Data.Models;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IUserAlbumRepository : IBaseRepository<UserAlbum>
    {
        Task<bool> IsUserAlbumAlreadyExists(string albumName);

        Task<bool> CheckExpiryDayAlbum(int albumId);
    }
}
