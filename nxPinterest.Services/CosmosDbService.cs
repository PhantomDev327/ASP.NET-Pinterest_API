using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using nxPinterest.Data.Models;
using nxPinterest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using nxPinterest.Services.Models.Response;

namespace nxPinterest.Services
{
    public class CosmosDbService
    {
        public string inserted_id;    // [TEST] for Corresponding IDs

        public async Task<UserMediaCosmosJSON> SelectOneItemAsync(string databaseName, string containerName, string id)
        {
            CosmosClient cosmosClient = new CosmosClient(dev_Settings.cosmos_endpointUri, dev_Settings.cosmos_accountKey);
            Database database = cosmosClient.GetDatabase(databaseName);
            Container container = database.GetContainer(containerName);

            try
            {
                ItemResponse<UserMediaCosmosJSON> result = await container.ReadItemAsync<UserMediaCosmosJSON>(id, new PartitionKey(id));
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Get Image By UserID
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="containerName"></param>
        /// <param name="searchKey"></param>
        /// <param name="containerid"></param>
        /// <returns></returns>
        public async Task<IList<Data.Models.UserMediaCosmosJSON>> SelectByUserIDAsync(string databaseName, string containerName, string searchKey, string containerid)
        {
            CosmosClient cosmosClient = new CosmosClient(dev_Settings.cosmos_endpointUri, dev_Settings.cosmos_accountKey);
            Database database = cosmosClient.GetDatabase(databaseName);
            Container container = database.GetContainer(containerName);


            //await container.GetItemLinqQueryable<UserMediaCosmosJSON>(true).Select(u => u.UserId == userid).ToListAsync();
            //IList<UserMediaCosmosJSON> testList = await container.GetItemLinqQueryable<UserMediaCosmosJSON>(true).Where(u => u.UserId == userid).ToListAsync();
            //Correct
            //IList<UserMediaCosmosJSON> testList = container.GetItemLinqQueryable<UserMediaCosmosJSON>(true).Where(u => u.UserId == userid).ToList<UserMediaCosmosJSON>();
            //Correct
            //IList<UserMediaCosmosJSON> testList = container.GetItemLinqQueryable<UserMediaCosmosJSON>(true).Where(u => u.UserId == userid).ToListAsync();

            var query = container.GetItemLinqQueryable<UserMediaCosmosJSON>(true).Where(u => u.container_id == containerid);

            if (!String.IsNullOrEmpty(searchKey))
            {
                query = query.Where(u => u.ProjectTags.ToLower().Contains(searchKey.Trim()) || u.MediaTitle.ToLower().Contains(searchKey.Trim()));
            }
            IList<UserMediaCosmosJSON> usermedialist = query.ToList<UserMediaCosmosJSON>();
            //IList<Data.Models.UserMediaCosmosJSON> userMediaList = await query.OrderByDescending(c => c.MediaId).AsAsyncEnumerable.ToListAsync();

            /*try
            {
                var query = container.GetItemLinqQueryable<UserMediaCosmosJSON>(true).Where(u => u.UserId == userid);

                if (!String.IsNullOrEmpty(searchKey))
                {
                    //string[] listSearchKey = Regex.Split(searchKey.Trim(), "[ 　]+", RegexOptions.IgnoreCase);

                    //if (listSearchKey.Count() > 1)
                    //{
                      //  query = query.Where(c => listSearchKey.Contains(c.Tags)
                        //        || listSearchKey.Contains(c.MediaTitle));
                    //}
                    //else
                    //{
                        query = query.Where(c => c.ProjectTags.Contains(searchKey) || c.MediaTitle.Contains(searchKey));
                    //}
                }

                IList<Data.Models.UserMediaCosmosJSON> userMediaList = await query.OrderByDescending(c => c.MediaId).ToListAsync();
                return userMediaList;
            }
            catch (Exception ex)
            {
                throw ex;
            }*/
            return usermedialist;
        }

        public async Task<bool> InsertOneItemAsync(string databaseName, string containerName, string json)
        {
            CosmosClient cosmosClient = new CosmosClient(dev_Settings.cosmos_endpointUri, dev_Settings.cosmos_accountKey);
            Database database = cosmosClient.GetDatabase(databaseName);
            Container container = database.GetContainer(containerName);

            try
            {
                database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
                container = await database.CreateContainerIfNotExistsAsync(containerName, "/id");

                UserMediaCosmosJSON userMedia = JsonConvert.DeserializeObject<UserMediaCosmosJSON>(json);
                Base64stringUtility encode = new Base64stringUtility("UTF-8");
                inserted_id = userMedia.Id = (encode.Encode(userMedia.UserId + DateTime.Now.ToString("_yyyyMMddHHmmssfff_") + userMedia.MediaFileName)).Replace("+", "==");
                await container.CreateItemAsync(userMedia);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateOneItemAsync(string databaseName, string containerName, string id, UserMediaCosmosJSON json)
        {
            CosmosClient cosmosClient = new CosmosClient(dev_Settings.cosmos_endpointUri, dev_Settings.cosmos_accountKey);
            Database database = cosmosClient.GetDatabase(databaseName);
            Container container = database.GetContainer(containerName);

            try
            {
                //await container.ReplaceItemAsync(json, id);
                await container.UpsertItemAsync<UserMediaCosmosJSON>(json, new PartitionKey(id));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> DeleteItemAsync(string databaseName, string containerName, string id)
        {
            CosmosClient cosmosClient = new CosmosClient(dev_Settings.cosmos_endpointUri, dev_Settings.cosmos_accountKey);
            Database database = cosmosClient.GetDatabase(databaseName);
            Container container = database.GetContainer(containerName);

            try
            {
                await container.DeleteItemAsync<UserMediaCosmosJSON>(id, new PartitionKey(id));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        ///// <summary>
        ///// Get Image Detail By MediaID
        ///// </summary>
        ///// <param name="databaseName"></param>
        ///// <param name="containerName"></param>
        ///// <param name="media_id"></param>
        ///// <returns></returns>
        //public async Task<UserMediaDetailViewModel> GetUserMediaDetailsCosmosByIDAsync(string databaseName, string containerName, int media_id)
        //{
        //    CosmosClient cosmosClient = new CosmosClient(dev_Settings.cosmos_endpointUri, dev_Settings.cosmos_accountKey);
        //    Database database = cosmosClient.GetDatabase(databaseName);
        //    Container container = database.GetContainer(containerName);

        //    Data.Models.UserMediaCosmosJSON userMedia = container.GetItemLinqQueryable<UserMediaCosmosJSON>(true)
        //                  .Where(x => x.MediaId == media_id)
        //                  .AsEnumerable()
        //                  .FirstOrDefault();

        //    UserMediaDetailViewModel result = new UserMediaDetailViewModel();

        //    IList<UserMediaCosmosJSON> mediaList = new List<UserMediaCosmosJSON>();

        //    if (userMedia != null)
        //    {
        //        var query = container.GetItemLinqQueryable<UserMediaCosmosJSON>(true).Where(u => u.container_id == userMedia.container_id).ToList();

        //        query = query.Select(c => new UserMediaCosmosJSON()
        //        {
        //            MediaId = c.MediaId,
        //            UserId = c.UserId,
        //            MediaTitle = c.MediaTitle,
        //            MediaDescription = c.MediaDescription,
        //            MediaFileName = c.MediaFileName,
        //            MediaFileType = c.MediaFileType,
        //            MediaUrl = c.MediaUrl,
        //            Tags = c.Tags,
        //            MediaThumbnailUrl = c.MediaThumbnailUrl
        //        })
        //        .Where(c => !string.IsNullOrEmpty(c.MediaTitle) && !string.IsNullOrEmpty(userMedia.MediaTitle) && c.MediaTitle.Equals(userMedia.MediaTitle)).ToList();

        //        mediaList = query;
        //    }

        //    result.UserMediaDetailCosmos = userMedia;
        //    result.UserMediaCosmosList = mediaList;

        //    return result;
        //}

        //public int GetLatestMediaId()
        //{
        //    int mediaid = 0;
        //    CosmosClient cosmosClient = new CosmosClient(dev_Settings.cosmos_endpointUri, dev_Settings.cosmos_accountKey);
        //    Database database = cosmosClient.GetDatabase(dev_Settings.cosmos_databaseName);
        //    Container container = database.GetContainer(dev_Settings.cosmos_containerName);
        //    var query = container.GetItemLinqQueryable<UserMediaCosmosJSON>(true).OrderByDescending(i => i.MediaId);
        //    IList<UserMediaCosmosJSON> usermedialist = query.ToList<UserMediaCosmosJSON>();

        //    if (usermedialist != null)
        //    {
        //        mediaid = usermedialist[0].MediaId + 1;
        //    }
        //    return mediaid;
        //}
    }


}
