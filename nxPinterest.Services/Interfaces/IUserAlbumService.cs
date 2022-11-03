using nxPinterest.Data.Models;
using nxPinterest.Services.Models.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nxPinterest.Services.Interfaces
{
    public interface IUserAlbumService
    {
        Task<bool> Create(CreateUserAlbumRequest model, string userId);

        Task<IEnumerable<UserAlbum>> GetAlbumByUser(string userId);
    }
}
