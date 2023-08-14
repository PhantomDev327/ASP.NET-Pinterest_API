using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories
{
    public class UserAlbumRepository : BaseRepository<UserAlbum>, IUserAlbumRepository
    {
        private IUserRepository _userRepository;
        private IUserAlbumMediaRepository _userAlbumMediaRepository;
        public UserAlbumRepository(ApplicationDbContext context, IUserRepository userRepository, IUserAlbumMediaRepository userAlbumMediaRepository) : base(context)
        {
            _userRepository = userRepository;
            _userAlbumMediaRepository = userAlbumMediaRepository;
        }

        public bool CheckExpiryDayAlbum(int albumId, DateTime? albumExpireDate)
        {
            if (albumId == 0)
            {
                return false;
            }
          
            DateTime? expiryDate = albumExpireDate;
            TimeSpan diff = (TimeSpan)(DateTime.UtcNow - expiryDate);

            // if day >0 has expired
            return diff.Days > 0;
        }

        public async Task<IEnumerable<UserAlbumViewModel>> GetAlbumUserByContainer(int container_id, string searchKey)
        {

            if (container_id == 0) return new List<UserAlbumViewModel>();

            var result = await Context.UserAlbums.Where(a => a.AlbumVisibility == true).Select(n => new UserAlbumViewModel
            {
                ContainerId = n.ContainerId,
                AlbumName = n.AlbumName,
                AlbumId = n.AlbumId,
                AlbumCreatedat = n.AlbumCreatedat,
                AlbumUrl = n.AlbumUrl,
                AlbumType = (int)n.AlbumType,
                AlbumThumbnailUrl = n.AlbumThumbnailUrl
            }).Where(n => n.ContainerId == container_id && n.AlbumType == (int)Data.Enums.AlbumType.Album).OrderBy(n => n.AlbumName).ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchKey)) {
                result = await Context.UserAlbums.Where(a => a.AlbumVisibility == true).Select(n => new UserAlbumViewModel
                {
                    ContainerId = n.ContainerId,
                    AlbumName = n.AlbumName,
                    AlbumId = n.AlbumId,
                    AlbumCreatedat = n.AlbumCreatedat,
                    AlbumUrl = n.AlbumUrl,
                    AlbumType = (int)n.AlbumType,
                    AlbumThumbnailUrl = n.AlbumThumbnailUrl
                }).Where(n => n.ContainerId == container_id && n.AlbumType == (int)Data.Enums.AlbumType.Album && n.AlbumName.Contains(searchKey)).OrderBy(n => n.AlbumName).ToListAsync();
            }
            foreach ( var item in result )
            {
                item.ImageCount = await _userAlbumMediaRepository.GetAlmubMediaCount(item.AlbumId);
            }

            return result != null ? result : new List<UserAlbumViewModel>();
        }

        public async Task<int> GetAlbumIdByPathUrl(string pathUrl)
        {
            int albumId = 0;

            if (string.IsNullOrWhiteSpace(pathUrl)) return albumId;

            var result = await Context.UserAlbums.Where(a => a.AlbumVisibility == true).Select(n => new UserAlbum
            {
                AlbumId = n.AlbumId,
                AlbumUrl = n.AlbumUrl,
                AlbumExpireDate = n.AlbumExpireDate
            }).SingleOrDefaultAsync(n => n.AlbumUrl == pathUrl);

            if(result == null) return albumId;

            //if day > 0 has expired are return 0;
            if (CheckExpiryDayAlbum(result.AlbumId, result.AlbumExpireDate)) return albumId;

            return result.AlbumId;
        }

        public (int albumId, string albumName) IsUserAlbumAlreadyExists(string albumName)
        {
            if (string.IsNullOrEmpty(albumName)) return (0, null);

            var result = Context.UserAlbums.Where(a => a.AlbumVisibility == true).Select(n => new
            {
                n.AlbumName,
                n.AlbumId
            }).SingleOrDefault(n => n.AlbumName == albumName);

            return result != null ? (result.AlbumId, result.AlbumName) : (0, null);
        }

        public async Task<int> GetAlbumIdByName(string albumName)
        {
            if (string.IsNullOrEmpty(albumName)) return 0;

            var result = await Context.UserAlbums.Where(a => a.AlbumVisibility == true).Select(n => new
            {
                n.AlbumName,
                n.AlbumId
            }).SingleOrDefaultAsync(n => n.AlbumName == albumName);

            return result != null ? result.AlbumId : 0;
        }

        public async Task<DateTime?> GetCreateDateAlbumName(int albumId)
        {
            if (albumId ==0 ) return null;

            var result = await Context.UserAlbums.Select(n => new
            {
                n.AlbumId,
                n.AlbumCreatedat
            }).SingleOrDefaultAsync(n => n.AlbumId == albumId);

            return result != null ? result.AlbumCreatedat : null;
        }

        public async Task<bool> IsAlbumNameExist(string albumName)
        {
            return await Context.UserAlbums
                .Where(x => x.AlbumName == albumName && x.AlbumVisibility == true)
                .AnyAsync();
        }

        public async Task<IEnumerable<UserAlbumViewModel>> GetSharedAlbumByUser(string userId, string role, int containerId)
        {

            if (string.IsNullOrEmpty(userId)) return new List<UserAlbumViewModel>();

            var user = _userRepository.GetSingleById(userId);

            if (user is null) return new List<UserAlbumViewModel>();

            var result = await Context.UserAlbums.Where(a => a.AlbumVisibility == true).Select(n => new UserAlbumViewModel
            {
                ContainerId = n.ContainerId,
                AlbumName = n.AlbumName,
                AlbumId = n.AlbumId,
                AlbumCreatedat = n.AlbumCreatedat,
                AlbumUrl = n.AlbumUrl,
                AlbumType = (int)n.AlbumType,
                AlbumThumbnailUrl = n.AlbumThumbnailUrl,
                AlbumExpireDate = n.AlbumExpireDate,
                UserId = n.UserId,
                Comment = n.AlbumComment
            }).Where(n => n.AlbumType == (int)Data.Enums.AlbumType.AlbumShare && n.UserId.Equals(userId) && n.ContainerId == containerId).OrderByDescending(n => n.AlbumCreatedat).ToListAsync();

            if (role == "SysAdmin" || role == "ContainerAdmin") {
                result = await Context.UserAlbums.Where(a => a.AlbumVisibility == true).Select(n => new UserAlbumViewModel
                {
                    ContainerId = n.ContainerId,
                    AlbumName = n.AlbumName,
                    AlbumId = n.AlbumId,
                    AlbumCreatedat = n.AlbumCreatedat,
                    AlbumUrl = n.AlbumUrl,
                    AlbumType = (int)n.AlbumType,
                    AlbumThumbnailUrl = n.AlbumThumbnailUrl,
                    AlbumExpireDate = n.AlbumExpireDate,
                    UserId = n.UserId,
                    Comment = n.AlbumComment
                }).Where(n => n.AlbumType == (int)Data.Enums.AlbumType.AlbumShare && n.ContainerId == containerId).OrderByDescending(n => n.AlbumCreatedat).ToListAsync();
            }

            foreach (var item in result)
            {
                item.ImageCount = await _userAlbumMediaRepository.GetAlmubMediaCount(item.AlbumId);
            }

            return result != null ? result : new List<UserAlbumViewModel>();
        }

        public async Task<int> DeleteAlbumIdByID(int albumId)
        {

            var album = await Context.UserAlbums.FindAsync(albumId);
            album.AlbumVisibility = false;
            album.AlbumDeletedat = DateTime.Now;
            this.Context.UserAlbums.Update(album);
            await Context.SaveChangesAsync();

            return album.AlbumId;
        }
    }
}
