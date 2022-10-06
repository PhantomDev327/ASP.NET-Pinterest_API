using nxPinterest.Data.Models;
using nxPinterest.Services.Models.Request;
using nxPinterest.Services.Models.Response;
using nxPinterest.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace nxPinterest.Services.Interfaces
{
    public interface IUserMediaManagementService
    {
        Task<IList<Data.Models.UserMedia>> ListUserMediaAsyc(string userId = "");
        Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, int containerId);
        Task<IList<Data.Models.UserMedia>> SearchSimilarImagesAsync(UserMedia userMedia, int container_id);
        Task<UserMedia> GetUserMediaAsync(int mediaId);
        Task<UserMediaDetailModel> GetUserMediaDetailsByIDAsync(int mediaId);
        Task DeleteFromUserMedia(UserMedia userMedia);
        //Task DeleteFromUserMedia(string media_id);
        public void UploadMediaFile(ImageRegistrationRequests request, string UserId);
        public void UploadImageFile(IndividualImageRegistrationRequests request, string UserId);
    }
}
