namespace ResultsDataService
{
    public class CoreBlobService : BlobService, ICoreBlobService
    {
        public CoreBlobService(CoreBlobClient blobServiceClient) : base(blobServiceClient) { }
    }
}
