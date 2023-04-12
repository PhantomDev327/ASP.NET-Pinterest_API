using nxPinterest.Data.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nxPinterest.Services.Interfaces
{
    public interface IUserAlbumMediaService
    {
        Task<IEnumerable<SharedLinkAlbumMediaViewModel>> GetListAlbumById(int albumId, int pageIndex);

        Task<bool> RemoveMediaFromAlbum(int albumId, List<int> mediaList);

    }
}