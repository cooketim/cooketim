using Models.ViewModels;
using System;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TimsTool.Tree.BindingValidation
{
    public class UniqueRDLabelRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var label = string.Empty;

            try
            {
                if (value == null || string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(false, "Label cannot be empty");
                }

                label = ((string)value).Trim();
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //check that label is unique
            IServiceProvider serviceProvider = ((App)Application.Current).ServiceProvider;
            var treeModel = serviceProvider.GetRequiredService<ITreeModel>();
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
            var matchedRD = treeModel.AllResultDefinitionsViewModel.Definitions.Find(x => x.DeletedDate == null && x.Label != null &&
                                                                                   x.Label.Trim().ToLowerInvariant() == label.ToLowerInvariant() &&
                                                                                   x.MasterUUID != selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
            if (matchedRD != null)
            {
                return new ValidationResult(false, string.Format("Please enter a unique label. '{0}' already exists", label));
            }

            return new ValidationResult(true, null);
        }
    }
}
