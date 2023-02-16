using Models.ViewModels;
using System;
using System.Windows;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TimsTool.Tree.BindingValidation
{
    public class ResultDefinitionGroupExistsRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var name = string.Empty;

            try
            {
                if (value == null || string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(true, null);
                }

                name = ((string)value).Trim();
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //check that name is a result definition group that exists
            IServiceProvider serviceProvider = ((App)Application.Current).ServiceProvider;
            var treeModel = serviceProvider.GetRequiredService<ITreeModel>();
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
            var matchedResultDefinitionGroup = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.DeletedDate == null 
                                        && !string.IsNullOrEmpty(x.ResultDefinitionGroup)
                                        && x.ResultDefinitionGroup.Trim().ToLowerInvariant() == name.ToLowerInvariant());

            if (matchedResultDefinitionGroup == null)
            {
                return new ValidationResult(false, string.Format("Result Definition Group '{0}' does not exist", name));
            }

            return new ValidationResult(true, null);
        }
    }
}
