using System.Security.Principal;

namespace Identity
{
    public class ResultsTreeIdentity : IIdentity
    {
        public ResultsTreeIdentity(User user, bool isAuthenticated)
        {
            User = user;
            IsAuthenticated = isAuthenticated;
        }

        public User User { get; set; }

        public string Name { get => User.Username; }
        public string Email { get => User.Email; }
        public string[] Roles { get => User.Roles; }

        #region IIdentity Members
        public string AuthenticationType { get { return "Custom authentication"; } }

        public bool IsAuthenticated { get; }
        #endregion

    }

    public class AnonymousIdentity : ResultsTreeIdentity
    {
        public AnonymousIdentity()
            : base(new User(string.Empty, string.Empty, new string[] { }), false)
        { }
    }

}
