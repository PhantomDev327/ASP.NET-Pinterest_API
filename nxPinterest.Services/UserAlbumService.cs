using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.ViewModels;
using nxPinterest.Services.Interfaces;
using nxPinterest.Services.Models.Request;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nxPinterest.Services
{
    public class UserAlbumService : IUserAlbumService
    {
        private readonly IUserAlbumRepository _userAlbumRepository;
        private readonly IUserAlbumMediaRepository _userAlbumMediaRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;


        public UserAlbumService(IUserAlbumRepository userAlbumRepository, IUserAlbumMediaRepository userAlbumMediaRepository,
            IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userAlbumRepository = userAlbumRepository;
            _userAlbumMediaRepository = userAlbumMediaRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Create(CreateUserAlbumRequest model, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(model.AlbumName) || model.UserAlbumMedias.Count == 0
                                                          || string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                var user = _userRepository.GetSingleByCondition(n => n.Id == userId);
                var containerId = user.container_id;

                var userAlbum = new UserAlbum
                {
                    AlbumName = model.AlbumName,
                    ContainerId = containerId,
                    UserId = userId,
                    AlbumType = Data.Enums.AlbumType.Album,
                    AlbumUrl = model.AlbumUrl,
                    AlbumThumbnailUrl = model.UserAlbumMedias[0].MediaThumbnailUrl,
                    AlbumVisibility = true,
                    AlbumCreatedat = DateTime.Now,
                    AlbumExpireDate = new DateTime(2999,12,31)
                };

                var (albumId, albumName) = _userAlbumRepository.IsUserAlbumAlreadyExists(model.AlbumName);

                if (albumName is not null)
                {
                    userAlbum.AlbumId = albumId;
                }
                else
                {
                    await _userAlbumRepository.Add(userAlbum);
                    await _unitOfWork.CompleteAsync();
                }

                foreach (var item in model.UserAlbumMedias)
                {

                    if (await _userAlbumMediaRepository.IsMediaExistAsync(userAlbum.AlbumId, item.UserMediaId)) continue;

                    var userAlbumMedia = new UserAlbumMedia
                    {
                        AlbumId = userAlbum.AlbumId,
                        ContainerId = containerId,
                        UserMediaId = item.UserMediaId,
                        UserMediaName = item.MediaFileName,
                        AlbumMediaCreatedat = DateTime.Now
                    };

                    await _userAlbumMediaRepository.Add(userAlbumMedia);
                }

                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> CreateAlbumShare(CreateUserAlbumSharedRequest model, string userId)
        {
            try
            {
                if (model.UserAlbumMedias.Count == 0 || string.IsNullOrEmpty(userId))
                {
                    return string.Empty;
                }

                var user = _userRepository.GetSingleByCondition(n => n.Id == userId);
                var containerId = user.container_id;

                var userAlbum = new UserAlbum
                {
                    AlbumName = model.AlbumName,
                    ContainerId = containerId,
                    UserId = userId,
                    AlbumType = Data.Enums.AlbumType.AlbumShare,
                    AlbumUrl = model.AlbumUrl,
                    AlbumThumbnailUrl = model.UserAlbumMedias[0].MediaThumbnailUrl,
                    AlbumVisibility = true,
                    AlbumCreatedat = DateTime.Now,
                    AlbumExpireDate = model.AlbumExpireDate
                };

                await _userAlbumRepository.Add(userAlbum);
                await _unitOfWork.CompleteAsync();

                foreach (var item in model.UserAlbumMedias)
                {
                    var userAlbumMedia = new UserAlbumMedia
                    {
                        AlbumId = userAlbum.AlbumId,
                        ContainerId = containerId,
                        UserMediaId = item.UserMediaId,
                        UserMediaName = item.MediaFileName,
                        AlbumMediaCreatedat = DateTime.Now
                    };

                    await _userAlbumMediaRepository.Add(userAlbumMedia);
                }

                await _unitOfWork.CompleteAsync();

                return !string.IsNullOrEmpty(userAlbum.AlbumUrl) ? userAlbum.AlbumUrl : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<IEnumerable<UserAlbumViewModel>> GetAlbumUserByContainer(string userId)
        {
            return await _userAlbumRepository.GetAlbumUserByContainer(userId);
        }

        public async Task<int> GetAlbumIdByPathUrlAsync(string pathUrl)
        {
            return await _userAlbumRepository.GetAlbumIdByPathUrl(pathUrl);
        }
    }
}
