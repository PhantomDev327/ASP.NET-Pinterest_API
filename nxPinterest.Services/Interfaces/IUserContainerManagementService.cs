using nxPinterest.Data.Models;
//using nxPinterest.Services.Models.Request;
//using nxPinterest.Services.Models.Response;
//using nxPinterest.Utils;
//using System;
using System.Collections.Generic;
//using System.Threading;
//using System.Text;
using System.Threading.Tasks;
//using System.Security.Claims;

namespace nxPinterest.Services.Interfaces
{
    public interface IUserContainerManagementService
    {
        public Task<IList<Data.Models.UserContainer>> ListContainerAsyc();

        public void InsertUserContainer(UserContainer userContainer);
        public void UpdateUserContainer(UserContainer userContainer);

        public virtual Task<UserContainer> FindByContainerIdAsync(int ContainerId)
        {
            throw null;
        }
    }
}
