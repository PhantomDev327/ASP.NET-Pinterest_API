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
        public UserAlbumRepository(ApplicationDbContext context, IUserRepository userRepository) : base(context)
        {
            _userRepository = userRepository;
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

        public async Task<IEnumerable<UserAlbumViewModel>> GetAlbumUserByContainer(string userId)
        {

            if (string.IsNullOrEmpty(userId)) return new List<UserAlbumViewModel>();

            var user = _userRepository.GetSingleById(userId);

            if (user is null) return new List<UserAlbumViewModel>();
            
            var result = await Context.UserAlbums.Select(n => new UserAlbumViewModel
            {
                ContainerId = n.ContainerId,
                AlbumName = n.AlbumName,
                AlbumId = n.AlbumId,
                AlbumCreatedat = n.AlbumCreatedat,
                AlbumUrl = n.AlbumUrl,
                AlbumType = (int)n.AlbumType,
                AlbumThumbnailUrl = n.AlbumThumbnailUrl
            }).Where(n => n.ContainerId == user.container_id && n.AlbumType == (int)Data.Enums.AlbumType.Album).OrderByDescending(n => n.AlbumCreatedat).ToListAsync();

            return result != null ? result : new List<UserAlbumViewModel>();
        }

        public async Task<int> GetAlbumIdByPathUrl(string pathUrl)
        {
            int albumId = 0;

            if (string.IsNullOrWhiteSpace(pathUrl)) return albumId;

            var result = await Context.UserAlbums.Select(n => new UserAlbum
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

            var result = Context.UserAlbums.Select(n => new
            {
                n.AlbumName,
                n.AlbumId
            }).SingleOrDefault(n => n.AlbumName == albumName);

            return result != null ? (result.AlbumId, result.AlbumName) : (0, null);
        }
    }
}
