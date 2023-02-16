using Models.ViewModels;
using System;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TimsTool.Tree.BindingValidation
{
    public class UniqueNOWNameRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var name = string.Empty;

            try
            {
                if (value == null || string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(false, "Name cannot be empty");
                }

                name = ((string)value).Trim();
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //check that name is unique
            IServiceProvider serviceProvider = ((App)Application.Current).ServiceProvider;
            var treeModel = serviceProvider.GetRequiredService<ITreeModel>();
            var selectedNow = treeModel.SelectedItem as NowTreeViewModel;
            if (selectedNow.NowViewModel.IsEDT)
            {
                var matchedEDT = treeModel.AllEDTsViewModel.EDTs.Find(x => x.DeletedDate == null &&
                                                                        !string.IsNullOrEmpty(x.Name) &&
                                                                        x.Name.Trim().ToLowerInvariant() == name.ToLowerInvariant() &&
                                                                        x.MasterUUID != selectedNow.NowViewModel.MasterUUID
                                                                        );
                if (matchedEDT != null)
                {
                    return new ValidationResult(false, string.Format("Please enter a unique Name. '{0}' already exists", name));
                }
            }
            else
            {
                var matchedNow = treeModel.AllNowsViewModel.Nows.Find(x => x.DeletedDate == null &&
                                                                        !string.IsNullOrEmpty(x.Name) &&
                                                                        x.Name.Trim().ToLowerInvariant() == name.ToLowerInvariant() &&
                                                                        x.MasterUUID != selectedNow.NowViewModel.MasterUUID
                                                                        );
                if (matchedNow != null)
                {
                    return new ValidationResult(false, string.Format("Please enter a unique Name. '{0}' already exists", name));
                }
            }

            return new ValidationResult(true, null);
        }
    }
}
