using System.Collections.Generic;
using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace nxPinterest.Data.Repositories
{
    public class UserAlbumMediaRepository : BaseRepository<UserAlbumMedia>, IUserAlbumMediaRepository
    {
        public UserAlbumMediaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SharedLinkAlbumMediaViewModel>> GetListAlbumByIdAsync(int albumId, string baseUrl,
            string containerName, int pageIndex, int pageSize)
        {
            if (albumId == 0) return new List<SharedLinkAlbumMediaViewModel>();

            baseUrl = "https://pinteresttest.blob.core.windows.net/";
            var data = await Context.UserAlbumMedias.Select(n => new SharedLinkAlbumMediaViewModel
            {
                AlbumId = n.AlbumId,
                MediaId = n.UserMediaId,
                ContainerId = n.ContainerId,
                MediaUrl = $"{baseUrl}{containerName}/{n.ContainerId}/original/{n.UserMediaName}.jpg",
                MediaSmallUrl = $"{baseUrl}{containerName}/{n.ContainerId}/small/{n.UserMediaName}.jpg",
                MediaThumbnailUrl = $"{baseUrl}{containerName}/{n.ContainerId}/thumb/{n.UserMediaName}.jpg"
            }).Where(n => n.AlbumId == albumId).ToListAsync();

            return data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}
