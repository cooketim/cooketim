using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DataLib;
using Identity;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ResultsDataService
{

    public interface IBlobService
    {
        Task<BlobInfo> GetBlobAsync(string fileName, string containerName);
        Task UploadFileBlobAsync(MemoryStream ms, string fileName, string containerName);
        Task UploadContentBlobAsync(string content, string fileName, string containerName);
        Task<IEnumerable<BlobItem>> ListBlobsAsync(string containerName);
        Task DeleteBlobAsync(string fileName, string containerName);
    }

    public abstract class BlobService : IBlobService
    {
        protected readonly BlobServiceClient blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        public async Task<IEnumerable<BlobItem>> ListBlobsAsync(string containerName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var items = new List<BlobItem>();
            await foreach(var blobItem in containerClient.GetBlobsAsync())
            {
                items.Add(blobItem);
            }
            return items;
        }

        public async Task DeleteBlobAsync(string fileName, string containerName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<BlobInfo> GetBlobAsync(string fileName, string containerName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var blobdownloadinfo = await blobClient.DownloadAsync();

            return new BlobInfo(blobdownloadinfo.Value.Content, blobdownloadinfo.Value.Details.ContentType);
        }

        public async Task UploadFileBlobAsync(MemoryStream ms, string fileName, string containerName)
        {
            //reset the memory stream to the start
            ms.Position = 0;
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = fileName.GetContentType() });
        }

        public async Task UploadContentBlobAsync(string content, string fileName, string containerName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var bytes = Encoding.UTF8.GetBytes(content);
            await using var memoryStream = new MemoryStream(bytes);
            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = fileName.GetContentType() });
        }
    }
}
