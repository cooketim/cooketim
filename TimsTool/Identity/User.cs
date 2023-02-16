using System;
using System.Collections.Generic;
using System.Text;

namespace Identity
{
    [Serializable]
    public sealed class User
    {
        public User()
        { }
        public User(string username, string email, string[] roles)
        {
            Username = username;
            Email = email;
            Roles = roles;
        }
        public string Username { get; set; }

        public string Email { get; set; }

        public string[] Roles { get; set; }
    }
    [Serializable]
    public sealed class AllUsers
    {
        public List<User> AuthorisedUsers
        {
            get;
            set;
        }
    }
}
