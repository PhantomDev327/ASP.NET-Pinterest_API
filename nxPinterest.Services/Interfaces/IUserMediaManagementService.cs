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
        //Task<IList<Data.Models.UserMedia>> ListUserMediaAsyc(string userId = "");
        Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, int containerId, int skip = 0, int take = dev_Settings.displayMaxItems_search);
        Task<IList<Data.Models.UserMedia>> SearchSimilarImagesAsync(UserMedia userMedia, int container_id);
        Task<UserMedia> GetUserMediaAsync(int mediaId);
        Task<UserMediaDetailModel> GetUserMediaDetailsByIDAsync(int mediaId);
        //Task DeleteFromUserMedia(UserMedia userMedia);
        Task DeleteFromUserMediaList(List<UserMedia> userMediaList);
        //Task DeleteFromUserMedia(string media_id);
        Task<IList<string>> GetOftenUseTagsAsyc(int containerId, string searchKey = "", int take = 100);
        public void UploadMediaFile(ImageRegistrationRequests request, string UserId);
        public void UploadIndividualMediaFile(IndividualImageRegistrationRequests request, string UserId);
    }
}
