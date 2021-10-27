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
        Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, string userId);
        Task<Data.Models.UserMedia> GetUserMediaDetailsByIDAsync(int media_id);
    }
}
