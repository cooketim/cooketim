using Models.ViewModels;
using System;
using System.Windows;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TimsTool.Tree.BindingValidation
{
    public class UniqueNRLabelRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var label = string.Empty;

            try
            {
                if (value == null || string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(false, "Text Label cannot be empty");
                }

                label = ((string)value).Trim();
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //check that label is in valid format
            var textLabelRX = new Regex(@"^[a-z]+((\d)|([A-Z0-9][a-z0-9]+))*([A-Z])?");
            var match = textLabelRX.Match(label);
            if (!match.Success)
            {
                return new ValidationResult(false, "Invalid format.  Text label must be in lower camel case format containing only alphanumeric characters");
            }

            //check that label is unique
            //SelectedItem.NowRequirementViewModel.SelectedTextValue.NowReference
            IServiceProvider serviceProvider = ((App)Application.Current).ServiceProvider;
            var treeModel = serviceProvider.GetRequiredService<ITreeModel>();
            var selectedNowRequirement = treeModel.SelectedItem as NowRequirementTreeViewModel;
            var nowRequirementTextList = selectedNowRequirement.NowRequirementViewModel.NowRequirement.NowRequirementTextList;
            if (nowRequirementTextList != null)
            {
                var matchedLabel = nowRequirementTextList.Find(x => x.NowReference.Trim().ToLowerInvariant() == label.ToLowerInvariant());
                if (matchedLabel != null)
                {
                    return new ValidationResult(false, string.Format("Please enter a unique label. '{0}' already exists", label));
                }
            }

            return new ValidationResult(true, null);
        }
    }
}
