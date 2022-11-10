using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Data.ViewModels;
using nxPinterest.Services.Interfaces;

namespace nxPinterest.Services;

public class UserAlbumMediaService : IUserAlbumMediaService
{
    private readonly IUserAlbumMediaRepository _userAlbumMediaRepository;

    public CloudStorageAccount _storageAccount;

    public UserAlbumMediaService(IUserAlbumMediaRepository userAlbumMediaRepository)
    {
        _userAlbumMediaRepository = userAlbumMediaRepository;

        var UserConnectionString = string.Format(dev_Settings.storage_connectionString,
            dev_Settings.storage_accountName, dev_Settings.storage_accountKey);
        _storageAccount = CloudStorageAccount.Parse(UserConnectionString);
    }

    public async Task<IEnumerable<SharedLinkAlbumMediaViewModel>> GetListAlbumById(int albumId, int pageIndex)
    {
        var blobClient = _storageAccount.CreateCloudBlobClient();

        return await _userAlbumMediaRepository.GetListAlbumByIdAsync(albumId, blobClient.BaseUri.ToString(),
            dev_Settings.blob_containerName_image, pageIndex, dev_Settings.displayMaxItems_search);
    }
}