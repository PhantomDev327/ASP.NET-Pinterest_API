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

        public async Task<IEnumerable<SharedLinkAlbumMediaViewModel>> GetListAlbumByIdAsync(int albumId,int pageIndex, int pageSize)
        {
            if (albumId == 0) return new List<SharedLinkAlbumMediaViewModel>();

            var data = await Context.UserAlbumMedias.OrderByDescending(x => x.AlbumMediaCreatedat)
                   .Join(Context.UserMedia, p => p.UserMediaId, i => i.MediaId, (p, i) => new SharedLinkAlbumMediaViewModel
                   {
                       AlbumId = p.AlbumId,
                       UserMediaName = p.UserMediaName,
                       MediaId = p.UserMediaId,
                       MediaUrl = i.MediaUrl,
                       MediaThumbnailUrl = i.MediaThumbnailUrl,
                       MediaSmallUrl = i.MediaSmallUrl,
                       MediaDescription =i.MediaDescription,
                       MediaTitle =i.MediaTitle
                   }).Where(n => n.AlbumId == albumId).ToListAsync();

            return data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}
