using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using nxPinterest.Data.Models;
using nxPinterest.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace nxPinterest.Services
{
    public class StorageTableService
    {
        public CloudStorageAccount storageAccount;

        public string inserted_id;    // [TEST] for Corresponding IDs

        public StorageTableService()
        {
            string UserConnectionString = string.Format(dev_Settings.storage_connectionString, dev_Settings.storage_accountName, dev_Settings.storage_accountKey);
            storageAccount = CloudStorageAccount.Parse(UserConnectionString);
        }


        public async Task<UserMediaStorageTableEntity> SelectEntityAsync(string partitionKey, string rowKey)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(dev_Settings.storage_tableName);

            try
            {
                TableOperation retrieveData = TableOperation.Retrieve<UserMediaStorageTableEntity>(partitionKey, rowKey);
                TableResult tableResult = await table.ExecuteAsync(retrieveData);

                return tableResult.Result as UserMediaStorageTableEntity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<bool> InsertOrMerageEntityAsync(UserMediaStorageTableEntity entity)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(dev_Settings.storage_tableName);

            try
            {
                entity.PartitionKey = entity.UserId;

                Base64stringUtility encode = new Base64stringUtility("UTF-8");
                entity.RowKey = (encode.Encode(DateTime.Now.ToString("yyyyMMddHHmmssfff_") + entity.MediaFileName)).Replace("+", "==");

                await table.CreateIfNotExistsAsync();
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                UserMediaStorageTableEntity insertedEntity = result.Result as UserMediaStorageTableEntity;

                inserted_id = entity.PartitionKey + "|" + entity.RowKey;  // for Corresponding IDs

                return true;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        public async Task<bool> ReplaceEntityAsync(UserMediaStorageTableEntity entity)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(dev_Settings.storage_tableName);

            try
            {
                TableOperation replaceOperation = TableOperation.Replace(entity);
                TableResult result = await table.ExecuteAsync(replaceOperation);
                UserMediaStorageTableEntity relacedEntity = result.Result as UserMediaStorageTableEntity;

                return true;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }


        public async Task<bool> DeleteItemAsync(string partitionKey, string rowKey)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(dev_Settings.storage_tableName);

            try
            {
                TableOperation retrieveData = TableOperation.Retrieve<UserMediaStorageTableEntity>(partitionKey, rowKey);
                TableResult tableResult = table.ExecuteAsync(retrieveData).Result;
                TableOperation deleteOperation = TableOperation.Delete(tableResult.Result as UserMediaStorageTableEntity);
                await table.ExecuteAsync(deleteOperation);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
