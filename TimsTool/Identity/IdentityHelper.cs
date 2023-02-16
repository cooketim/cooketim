using System.Linq;

namespace Identity
{    public static class IdentityHelper
    {
        private static User signedInUser = new User(null, null, new string[] { });
        public static User SignedInUser
        {
            get
            {
                return signedInUser;
            }
            set
            {
                signedInUser = value;
            }
        }

        public static bool IsSignedInUserAnAdministrator()
        {
            return SignedInUser.Roles.Contains("administrator");
        }

        public static bool IsSignedInUserAnEdditor()
        {
            return SignedInUser.Roles.Contains("edittor");
        }

        public static bool IsSignedInUserALocalEdditor()
        {
            return SignedInUser.Roles.Contains("local_edittor");
        }

        public static bool IsSignedInUserALocalResultsEdittor()
        {
            return SignedInUser.Roles.Contains("local_resultsedittor");
        }

        public static bool IsSignedInUserAReader()
        {
            return SignedInUser.Roles.Contains("reader");
        }
    }
}
