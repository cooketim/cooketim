using Models.ViewModels;
using System;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TimsTool.Tree.BindingValidation
{
    public class UniqueRDCJSCodeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var cjsCode = string.Empty;

            try
            {
                if (value == null || string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(true, null);
                }

                cjsCode = ((string)value).Trim();
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //check that code is unique
            IServiceProvider serviceProvider = ((App)Application.Current).ServiceProvider;
            var treeModel = serviceProvider.GetRequiredService<ITreeModel>();
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
            var matchedRD = treeModel.AllResultDefinitionsViewModel.Definitions.Find(x => x.DeletedDate == null && x.CJSCode != null &&
                                                                                   x.CJSCode.Trim().ToLowerInvariant() == cjsCode.ToLowerInvariant() &&
                                                                                   x.MasterUUID != selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
            if (matchedRD != null)
            {
                return new ValidationResult(false, string.Format("Please enter a unique cjs code. '{0}' already exists for '{1}'", cjsCode, matchedRD.Label));
            }

            return new ValidationResult(true, null);
        }
    }
}
