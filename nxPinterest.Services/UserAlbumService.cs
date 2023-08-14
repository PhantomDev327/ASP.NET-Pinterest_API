using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.ViewModels;
using nxPinterest.Services.Interfaces;
using nxPinterest.Services.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        public async Task<bool> Create(CreateUserAlbumRequest model, string userId, int container_id)
        {
            try
            {
                if (string.IsNullOrEmpty(model.AlbumName) || model.UserAlbumMedias.Count == 0
                                                          || string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                var user = _userRepository.GetSingleByCondition(n => n.Id == userId);
                
                var userAlbum = new UserAlbum
                {
                    AlbumName = model.AlbumName,
                    ContainerId = container_id,
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
                    await _unitOfWork.SaveChangesAsync();
                }

                foreach (var item in model.UserAlbumMedias)
                {

                    if (await _userAlbumMediaRepository.IsMediaExistAsync(userAlbum.AlbumId, item.UserMediaId)) continue;

                    var userAlbumMedia = new UserAlbumMedia
                    {
                        AlbumId = userAlbum.AlbumId,
                        ContainerId = container_id,
                        UserMediaId = item.UserMediaId,
                        UserMediaName = item.MediaFileName,
                        AlbumMediaCreatedat = DateTime.Now
                    };

                    await _userAlbumMediaRepository.Add(userAlbumMedia);
                }

                await _unitOfWork.SaveChangesAsync();
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
                    AlbumExpireDate = model.AlbumExpireDate,
                    AlbumComment = model.AlbumComment
                };

                await _userAlbumRepository.Add(userAlbum);
                await _unitOfWork.SaveChangesAsync();

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

                await _unitOfWork.SaveChangesAsync();

                return !string.IsNullOrEmpty(userAlbum.AlbumUrl) ? userAlbum.AlbumUrl : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<IEnumerable<UserAlbumViewModel>> GetAlbumUserByContainer(int container, string searchKey="")
        {
            return await _userAlbumRepository.GetAlbumUserByContainer(container, searchKey);
        }

        public async Task<int> GetAlbumIdByPathUrlAsync(string pathUrl)
        {
            return await _userAlbumRepository.GetAlbumIdByPathUrl(pathUrl);
        }

        public async Task<int> GetAlbumIdByNameAsync(string albumName)
        {
            return await _userAlbumRepository.GetAlbumIdByName(albumName);
        }

        public async Task<DateTime?> GetCreateDateAlbumNameAsync(int albumId)
        {
            return await _userAlbumRepository.GetCreateDateAlbumName(albumId);
        }

        public UserAlbum UpdateAlbumAsync(int albumId, UserAlbum model)
        {
            var updateProps = new List<Expression<Func<UserAlbum, object>>>
            {
                x => x.AlbumUpdatedat,
                x => x.AlbumName
            };

            var result = _userAlbumRepository.Update(model, updateProps);

            _unitOfWork.SaveChanges();

            return result;
        }

        public async Task<bool> IsAlbumNameExistAsync(string albumName)
        {
            return await _userAlbumRepository.IsAlbumNameExist(albumName);
        }


        public async Task<UserAlbum> RemoveAlbum(string albumName)
        {
            var albumId = _userAlbumRepository.GetAlbumIdByName(albumName).Result;
            await _userAlbumMediaRepository.DeleteUserAlbumMediaAsyncById(albumId);
            var album = _userAlbumRepository.Delete(albumId);
            this._unitOfWork.SaveChanges();
            return album;
        }

        public async Task<bool> DeleteAlbum(int albumId)
        {
            // await _userAlbumMediaRepository.DeleteUserAlbumMediaAsyncById(albumId);
            int album = await _userAlbumRepository.DeleteAlbumIdByID(albumId);
            return album != 0;
        }


        public async Task<IEnumerable<SharedLinkAlbumMediaViewModel>> GetListAlbumById(int albumId, int pageIndex)
        {
            return await _userAlbumMediaRepository.GetListAlbumByIdAsync(albumId, pageIndex, dev_Settings.displayMaxItems_search);
        }


        public async Task<bool> RemoveMediaFromAlbum(int albumId, List<int> mediaIdList)
        {
            try
            {
                await _userAlbumMediaRepository.DeleteUserAlbumMediaAsync(albumId, mediaIdList);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<UserAlbumViewModel>> GetSharedAlbumByUser(string user_id, string role, int containerId)
        {
            return await _userAlbumRepository.GetSharedAlbumByUser(user_id, role,containerId);
        }

        public bool UpdateAlbumThumbnail(int albumId, UserAlbum model)
        {
            try
            {
                var updateProps = new List<Expression<Func<UserAlbum, object>>>
                {
                    x => x.AlbumUpdatedat,
                    x => x.AlbumThumbnailUrl
                };  

                var result = _userAlbumRepository.Update(model, updateProps);
                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IList<UserMediaAlbumViewModel>> GetMediaAlbumsAsync(int mediaId, Data.Enums.AlbumType albumType)
        {
            return await _userAlbumMediaRepository.GetMediaAlbumsAsync(mediaId, albumType);
        }
    }
}
