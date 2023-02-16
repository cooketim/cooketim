namespace ResultsDataService
{
    public class BackupBlobService : BlobService, IBackupBlobService
    {
        public BackupBlobService(BackupBlobClient blobServiceClient) : base(blobServiceClient) { }
    }
}
