using nxPinterest.Data.Models;
using nxPinterest.Data.ViewModels;
using nxPinterest.Services.Models.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nxPinterest.Services.Interfaces
{
    public interface IUserAlbumService
    {
        Task<bool> Create(CreateUserAlbumRequest model, string userId);

        Task<string> CreateAlbumShare(CreateUserAlbumSharedRequest model, string userId);

        Task<IEnumerable<UserAlbumViewModel>> GetAlbumByUser(string userId);

        Task<int> GetAlbumIdByUrl(string url);
    }
}
