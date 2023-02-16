using Models.ViewModels;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TimsTool.Tree.BindingValidation
{
    public class EmailAddressRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var email = string.Empty;

            try
            {
                if (value == null || string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(true, null);
                }

                email = ((string)value).Trim();

                //check that the address is not >254 charaters
                if (email.Length > 254)
                {
                    return new ValidationResult(false, "Maximum length is 254 characters");
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //check that post code is in valid format

            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            var emailRX = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = emailRX.Match(email);
            if (!match.Success)
            {
                return new ValidationResult(false, "Invalid format");
            }

            //check that the local part is <=64 characters
            var localPartEnd = email.LastIndexOf('@');
            if (localPartEnd >63)
            {
                return new ValidationResult(false, "Maximum length before '@' is 64 characters");
            }

            return new ValidationResult(true, null);
        }
    }
}
