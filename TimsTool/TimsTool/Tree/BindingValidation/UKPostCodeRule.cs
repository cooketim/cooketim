using Models.ViewModels;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TimsTool.Tree.BindingValidation
{
    public class UKPostCodeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var postCode = string.Empty;

            try
            {
                if (value == null || string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(false, "Post Code cannot be empty");
                }

                postCode = ((string)value).Trim();
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //check that post code is in valid format
            var postCodeRX = new Regex(@"^([A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}|GIR ?0A{2})$");
            var match = postCodeRX.Match(postCode);
            if (!match.Success)
            {
                return new ValidationResult(false, "Invalid format.  Are there double spaces?");
            } 

            return new ValidationResult(true, null);
        }
    }
}
