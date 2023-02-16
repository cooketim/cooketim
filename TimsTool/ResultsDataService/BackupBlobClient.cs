using Azure.Storage.Blobs;

namespace ResultsDataService
{
    public class BackupBlobClient : BlobServiceClient
    {
        public BackupBlobClient(string connectionString) : base(connectionString) { }
    }
}
