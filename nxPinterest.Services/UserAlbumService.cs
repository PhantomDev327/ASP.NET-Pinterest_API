using nxPinterest.Data.Models;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Services.Interfaces;
using nxPinterest.Services.Models.Request;
using System;
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
                    AlbumType = Data.Enums.AlbumType.Album,
                    AlbumUrl = model.UserAlbumMedias[0].MediaUrl,
                    AlbumVisibility = true,
                    AlbumCreatedat = DateTime.Now,
                    AlbumExpireDate = DateTime.Now,

                };
                await _userAlbumRepository.Add(userAlbum);
                await _unitOfWork.CompleteAsync();

                foreach (UserAlbumMediaRequest item in model.UserAlbumMedias)
                {
                    var userAlbumMedia = new UserAlbumMedia
                    {
                        ContainerId = containerId,
                        UserMediaId = item.UserMediaId,
                        UserMediaName = model.AlbumName,
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
    }
}
