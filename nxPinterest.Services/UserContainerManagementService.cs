using nxPinterest.Data;
using nxPinterest.Services.Interfaces;
using nxPinterest.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using nxPinterest.Data.Models;
//using nxPinterest.Services.Models.Response;
using nxPinterest.Services.Extensions;
using System.Text.RegularExpressions;
using nxPinterest.Services.Models.Request;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;

namespace nxPinterest.Services
{
    public class UserContainerManagementService : IUserContainerManagementService
    {
        #region Field
        public ApplicationDbContext _context;
        #endregion

        public UserContainerManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<Data.Models.UserContainer>> ListContainerAsyc()
        {
            var query = (this._context.UserContainer.AsNoTracking());

            IList<Data.Models.UserContainer> containerList = await query.OrderByDescending(c => c.container_name).ToListAsync();

            return containerList;
        }

        public async void InsertUserContainer(UserContainer userContainer)
        {
            if (userContainer != null)
            {
                try {
                    this._context.UserContainer.Add(userContainer);
                    this._context.SaveChanges();

                }
                catch (Exception)
                {
                    throw new Exception("SQL database への登録に失敗しました");
                }
            }
        }
        public async void UpdateUserContainer(UserContainer userContainer)
        {
            if (userContainer != null)
            {
                try
                {
                    this._context.UserContainer.Update(userContainer);
                    this._context.SaveChanges();

                }
                catch (Exception)
                {
                    throw new Exception("SQL database への変更に失敗しました");
                }
            }
        }
    }
}
