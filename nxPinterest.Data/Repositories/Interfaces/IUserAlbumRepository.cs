using System.Collections.Generic;
using nxPinterest.Data.Models;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IUserAlbumRepository : IBaseRepository<UserAlbum>
    {
        (int albumId, string albumName) IsUserAlbumAlreadyExists(string albumName);

        Task<bool> CheckExpiryDayAlbum(int albumId);

        Task<IEnumerable<UserAlbum>> GetAlbumByUser(string userId);
    }
}
