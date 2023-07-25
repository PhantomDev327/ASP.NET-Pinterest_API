using System.Collections.Generic;
using System.Threading.Tasks;
using nxPinterest.Data.Models;
using nxPinterest.Data.ViewModels;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IUserAlbumMediaRepository : IBaseRepository<UserAlbumMedia>
    {
        Task<IEnumerable<SharedLinkAlbumMediaViewModel>> GetListAlbumByIdAsync(int albumId , int pageIndex, int pageSize);

        Task<bool> IsMediaExistAsync(int albumId,int mediaId);

        Task<int> DeleteUserAlbumMediaAsync(int albumId, List<int> mediaList);

        Task<int> GetAlmubMediaCount(int albumId);
        Task<int> DeleteUserAlbumMediaAsyncById(int albumId);
    }
}
