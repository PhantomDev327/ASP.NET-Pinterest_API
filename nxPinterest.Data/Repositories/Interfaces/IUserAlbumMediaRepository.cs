using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nxPinterest.Data.Models;
using nxPinterest.Data.ViewModels;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IUserAlbumMediaRepository : IBaseRepository<UserAlbumMedia>
    {
        Task<IEnumerable<SharedLinkAlbumMediaViewModel>> GetListAlbumByIdAsync(int albumId , string baseUrl, string containerName, int pageIndex, int pageSize);
    }
}
