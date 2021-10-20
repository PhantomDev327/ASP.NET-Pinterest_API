using nxPinterest.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace nxPinterest.Services.Interfaces
{
    public interface IUserMediaManagementService
    {
        Task<IList<Data.Models.UserMedia>> ListUserMediaAsyc(int pageIndex = 1, int pageSize = 5, string userId = "", string searchKey = "");
        Task<Data.Models.UserMedia> GetUserMediaDetailsByIDAsync(int media_id);
    }
}
