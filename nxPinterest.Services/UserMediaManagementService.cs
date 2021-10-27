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
                                     .Include(c => c.UserMediaThumbnails)
                                     .Where(c => c.UserId.Equals(userId)));


            IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId)
                                                                    .ToListAsync();


            return userMediaList;
        }

        public async Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, string userId) {
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

                if (response != null) {

                    foreach (SearchResult<SearchIndexUserMediaTable> result in response.GetResults()) {
                        string key = string.Format("{0}|{1}", result.Document.PartitionKey, result.Document.RowKey);

                        var userMediaQueryResult = (from usm in this._context.UserMedia
                                                  join mid in this._context.MediaId
                                                  on usm.MediaId equals mid.Sql_id
                                                  where mid.Storage_table == key
                                                  select usm)
                                                  .FirstOrDefault();

                        if (userMediaQueryResult != null)
                            searchResult.Add(userMediaQueryResult);

                    }
                }


                return searchResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Data.Models.UserMedia> GetUserMediaDetailsByIDAsync(int media_id) {

            Data.Models.UserMedia userMedia = await (this._context.UserMedia.AsNoTracking()
                                             .Include(c => c.UserMediaThumbnails)
                                             .FirstOrDefaultAsync(c => c.MediaId.Equals(media_id)));
            return userMedia;

        }

}
}
