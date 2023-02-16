using Models.ViewModels;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TimsTool.Tree.BindingValidation
{
    public class EmailRule : ValidationRule
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
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //check that post code is in valid format



            var emailRX = new Regex(@"[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$");
            var match = emailRX.Match(email);
            if (!match.Success)
            {
                return new ValidationResult(false, "Invalid format");
            }

            return new ValidationResult(true, null);
        }
    }
}
