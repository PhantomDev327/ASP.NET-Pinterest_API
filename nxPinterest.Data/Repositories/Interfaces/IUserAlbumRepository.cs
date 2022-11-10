using System.Collections.Generic;
using nxPinterest.Data.Models;
using System.Threading.Tasks;
using nxPinterest.Data.ViewModels;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IUserAlbumRepository : IBaseRepository<UserAlbum>
    {
        (int albumId, string albumName) IsUserAlbumAlreadyExists(string albumName);

        Task<bool> CheckExpiryDayAlbum(int albumId);

        Task<IEnumerable<UserAlbumViewModel>> GetAlbumUserByContainer(string userId);

        Task<int> GetAlbumIdByUrl(string url);
    }
}
