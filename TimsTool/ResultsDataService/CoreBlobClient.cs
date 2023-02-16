using Azure.Storage.Blobs;

namespace ResultsDataService
{
    public class CoreBlobClient : BlobServiceClient 
    {
        public CoreBlobClient(string connectionString) : base(connectionString) { }
    }
}
