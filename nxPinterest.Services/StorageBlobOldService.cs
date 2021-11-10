using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace nxPinterest.Services
{
    public class StorageBlobOldService
    {
        public CloudStorageAccount storageAccount;
        public StorageBlobOldService()
        {
            string UserConnectionString = string.Format(dev_Settings.storage_connectionString, dev_Settings.storage_accountName, dev_Settings.storage_accountKey);
            storageAccount = CloudStorageAccount.Parse(UserConnectionString);
        }

        public async Task<string[]> GetBlobFileList(string ContainerName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);

            try
            {
                var token = default(BlobContinuationToken);
                var segment = await container.ListBlobsSegmentedAsync(token);
                string[] files = { };

                token = segment.ContinuationToken;

                foreach (var blob in segment.Results.OfType<CloudBlockBlob>())
                {
                    Array.Resize(ref files, files.Length + 1);
                    files[files.Length - 1] = blob.Name.ToString();
                }
                return files;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<string> GetBlobFileString(string BlobName, string ContainerName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);

            try
            {
                var blob = container.GetBlockBlobReference(BlobName);
                return await blob.DownloadTextAsync();
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<CloudBlockBlob> UploadImageBlobAsync(string BlobName, string ContainerName, string filePath)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName.ToLower());
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);

            try
            {
                await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, null, null);
                using (var ms = new MemoryStream())
                {
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    await fs.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    await blockBlob.UploadFromStreamAsync(ms);
                }
                return blockBlob;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public async Task<bool> StoreJsonBlobAsync(string fileName, string ContainerName, string json)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName.ToLower());
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            try
            {
                await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, null, null);
                using (var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
                {
                    await blockBlob.UploadFromStreamAsync(mem);
                }
                return true;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async void DeleteBlob(string BlobName, string ContainerName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);
            await blockBlob.DeleteAsync();
        }

        public CloudBlockBlob DownloadBlob(string BlobName, string ContainerName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);
            return blockBlob;
        }

        public async void CreateContainerIfNotExistsAsync(string ContainerName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName.ToLower());

            try
            {
                await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, null, null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
