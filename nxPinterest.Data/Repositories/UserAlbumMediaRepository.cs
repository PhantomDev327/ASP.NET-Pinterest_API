using System.Collections.Generic;
using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

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
                       MediaFileName = p.UserMediaName,
                       MediaId = p.UserMediaId,
                       MediaUrl = i.MediaUrl,
                       MediaThumbnailUrl = i.MediaThumbnailUrl,
                       MediaSmallUrl = i.MediaSmallUrl,
                       MediaDescription =i.MediaDescription,
                       MediaTitle =i.MediaTitle,
                       Deleted = i.Deleted,
                       Status = i.Status,
                       AlbumMediaDeletedat = p.AlbumMediaDeletedat
                   }).Where(n => n.AlbumId == albumId && n.Deleted == null && n.Status == 0 && n.AlbumMediaDeletedat == null).ToListAsync();

            return data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public async Task<int> GetAlmubMediaCount(int albumId)
        {
            return await Context.UserAlbumMedias.Where(x => x.AlbumId == albumId).CountAsync();
        }

        public async Task<bool> IsMediaExistAsync(int albumId, int mediaId)
        {
            return await Context.UserAlbumMedias
                .Where(x => x.AlbumId == albumId && x.UserMediaId == mediaId)
                .AnyAsync();
        }

        public Task<int> DeleteUserAlbumMediaAsync(int albumId, List<int> mediaIdList)
        {
            var ret = Context.UserAlbumMedias
                 .Where(x => x.AlbumId == albumId && mediaIdList.Contains(x.UserMediaId));

            Context.UserAlbumMedias.RemoveRange(ret);
            return Context.SaveChangesAsync();
        }

        public Task<int> DeleteUserAlbumMediaAsyncById(int albumId)
        {
            var ret = Context.UserAlbumMedias
                 .Where(x => x.AlbumId == albumId);

            Context.UserAlbumMedias.RemoveRange(ret);
            return Context.SaveChangesAsync();
        }
    }
}
