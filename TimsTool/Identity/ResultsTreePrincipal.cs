using System.Linq;
using System.Security.Principal;

namespace Identity
{
    public class ResultsTreePricipal : IPrincipal
    {
        public ResultsTreePricipal(IIdentity identity)
        {
            this.Identity = identity;
        }

        public IIdentity Identity { get; set; }

        #region IPrincipal Members
        IIdentity IPrincipal.Identity
        {
            get { return this.Identity; }
        }

        public bool IsInRole(string role)
        {
            if (Identity is ResultsTreeIdentity)
            {
                var resultsTreeIdentity = (ResultsTreeIdentity)Identity;
                return resultsTreeIdentity.Roles.Contains(role);
            }
            return false;
        }

        #endregion
    }
}
