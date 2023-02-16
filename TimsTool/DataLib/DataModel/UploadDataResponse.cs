using Identity;

namespace DataLib
{
    public class UploadDataResponse
    {
        /// <summary>
        /// Current checked out user 
        /// </summary>
        public User CheckedoutUser { get; set; }

        /// <summary>
        /// The response status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The response message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The length of the saved bytes
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// The name of teh source file
        /// </summary>
        public string Name { get; set; }
    }
}
