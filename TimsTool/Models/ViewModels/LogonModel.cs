using Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ViewModels
{
    public class LogonModel : ViewModelBase
    {
        string messages;
        public string Messages
        {
            get => messages;
            set
            {
                SetProperty(ref messages, value);
            }
        }

        public bool IsConnected { get; set; }

        public User SignedInUser { get; set; }

        public User CheckedOutUser { get; set; }

        public bool IsAuthorised { get; set; }

        public bool IsCheckedOutUser 
        {
            get => IsCheckedOut && SignedInUser != null ? CheckedOutUser.Email == SignedInUser.Email : false;
        }

        public bool IsCheckedOut 
        {
            get => CheckedOutUser != null && !string.IsNullOrEmpty(CheckedOutUser.Email);
        }

        public bool TestVisibility { get; set; }
    }
}
