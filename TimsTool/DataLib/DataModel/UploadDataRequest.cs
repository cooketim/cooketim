using Identity;
using Microsoft.AspNetCore.Http;

namespace DataLib
{
    public class UploadDataRequest
    {
        /// <summary>
        /// The user that is checked out and requesting the data upload 
        /// </summary>
        public User RequestUser { get; set; }

        /// <summary>
        /// The source file with the data for update
        /// </summary>
        public IFormFile SourceFile { get; set; }
    }

}
