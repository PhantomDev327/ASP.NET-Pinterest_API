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
using nxPinterest.Services.Models.Response;

namespace nxPinterest.Services
{
    public class UserMediaManagementService : IUserMediaManagementService
    {
        public ApplicationDbContext _context;

        public UserMediaManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<Data.Models.UserMedia>> ListUserMediaAsyc(string userId = "")
        {
            var query = (this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.UserId.Equals(userId) &&
                                                 c.IsPrimary.Equals(false)));


            IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId)
                                                                    .ToListAsync();


            return userMediaList;
        }

        public async Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, string userId)
        {
            try
            {
                Uri endpoint = new Uri(dev_Settings.cognitivesearch_endpoint);
                AzureKeyCredential credential = new AzureKeyCredential(dev_Settings.cognitivesearch_adminApiKey);
                SearchClient client = new SearchClient(endpoint, dev_Settings.cognitivesearch_index_Table, credential);

                SearchOptions options = new SearchOptions()
                {
                    Filter = "UserId eq '" + userId + "'",      // If wanna share images with all users, remove this filter.
                    SearchFields = { "Tags" }
                };

                SearchResults<SearchIndexUserMediaTable> response = client.Search<SearchIndexUserMediaTable>(searchKey, options);
                IList<Data.Models.UserMedia> searchResult = new List<Data.Models.UserMedia>();

                if (response != null)
                {

                    foreach (SearchResult<SearchIndexUserMediaTable> result in response.GetResults())
                    {
                        string key = string.Format("{0}|{1}", result.Document.PartitionKey, result.Document.RowKey);

                        var userMediaQueryResult = (from usm in this._context.UserMedia
                                                    join mid in this._context.MediaId
                                                    on usm.MediaId equals mid.Sql_id
                                                    where mid.Storage_table == key
                                                    select usm)
                                                  .FirstOrDefault();

                        if (userMediaQueryResult != null && !userMediaQueryResult.IsPrimary) {
                            searchResult.Add(userMediaQueryResult);
                        }
                            

                    }
                }


                return searchResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<UserMediaDetailViewModel> GetUserMediaDetailsByIDAsync(int media_id)
        {

            Data.Models.UserMedia userMedia = await (this._context.UserMedia.AsNoTracking()
                                             .FirstOrDefaultAsync(c => c.MediaId.Equals(media_id)));

            UserMediaDetailViewModel result = new UserMediaDetailViewModel();

            IList<UserMedia> mediaList = new List<UserMedia>();

            if (userMedia != null)
            {
                var query = await (this._context.UserMedia.AsNoTracking()
                             .Where(c => c.MediaTitle.Equals(userMedia.MediaTitle) &&
                                         c.MediaDescription.Equals(userMedia.MediaDescription) &&
                                         c.IsPrimary.Equals(true)))
                             .ToListAsync();

                mediaList = query;
            }

            result.UserMediaDetail = userMedia;
            result.UserMediaList = mediaList;

            return result;
        }

        public async Task DeleteFromUserMedia(UserMedia userMedia) {
            if (userMedia != null) {
                var userMediaList = await this._context.UserMedia.AsNoTracking()
                                         .Where(c => c.MediaFileName.Equals(userMedia.MediaFileName))
                                         .ToListAsync();
                                     

                this._context.UserMedia.RemoveRange(userMediaList);
                await this._context.SaveChangesAsync();
            }
        }

    }
}
