using System.ComponentModel;

namespace DataLib
{
    /// <summary>
    /// The file names for each type of blob
    /// </summary>
    public enum BlobTypeFileName
    {
        /// <summary>
        /// Core tree data
        /// </summary>
        [Description("tree.bin")]
        TreeData,

        /// <summary>
        /// Checked out user
        /// </summary>
        [Description("checkout.json")]
        CheckoutData,

        /// <summary>
        /// List of authorised users
        /// </summary>
        [Description("authorisedUsers.json")]
        AuthorisedUsers,

        /// <summary>
        /// List of mappings of Ids to patch file names
        /// </summary>
        [Description("idMap.json")]
        IdMap
    }
}
