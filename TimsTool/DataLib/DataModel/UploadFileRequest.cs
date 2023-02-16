using Microsoft.AspNetCore.Http;

namespace DataLib
{
    public class UploadFileRequest
    {
        public BlobTypeFileName BlobTypeFileName { get; set; }

        public IFormFile SourceFile { get; set; }
    }
}
