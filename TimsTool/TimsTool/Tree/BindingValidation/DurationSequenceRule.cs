using System;
using System.Globalization;
using System.Windows.Controls;

namespace TimsTool.Tree.BindingValidation
{
    public class DurationSequenceRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value != null)
                {
                    var stringSeq = ((string)value).Trim();
                    int sequence = 0;

                    if (!string.IsNullOrEmpty(stringSeq))
                    {
                        if (!int.TryParse(stringSeq, out sequence))
                        {
                            return new ValidationResult(false, "Please enter a sequence in the range: " + Min + " - " + Max + ".");
                        }
                        if ((sequence < Min) || (sequence > Max))
                        {
                            return new ValidationResult(false,
                                "Please enter a sequence in the range: " + Min + " - " + Max + ".");
                        }
                    }
                }
               return new ValidationResult(true, null);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }
        }
    }
}
