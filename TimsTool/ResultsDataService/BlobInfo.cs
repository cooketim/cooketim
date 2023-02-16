using System.IO;

namespace ResultsDataService
{
    public class BlobInfo
    {
        public BlobInfo(Stream content, string contentType)
        {
            Content = content;
            ContentType = contentType;
        }

        public Stream Content { get; set; }
        public string ContentType { get; set; }
    }
}
