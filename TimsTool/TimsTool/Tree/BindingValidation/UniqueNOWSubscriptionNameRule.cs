using Models.ViewModels;
using System;
using System.Windows;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TimsTool.Tree.BindingValidation
{
    public class UniqueNOWSubscriptionNameRule : ValidationRule
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
            var selectedNowSubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            var matchedNowSubscription = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FirstOrDefault(x => x.DeletedDate == null && !string.IsNullOrEmpty(x.Name)
                                                                                            && x.Name.Trim().ToLowerInvariant() == name.ToLowerInvariant()
                                                                                            && x.MasterUUID != selectedNowSubscription.NowSubscriptionViewModel.MasterUUID
                                                                                            && x.IsEDT == selectedNowSubscription.NowSubscriptionViewModel.IsEDT);

            if (matchedNowSubscription != null)
            {
                return new ValidationResult(false, string.Format("Please enter a unique Name. '{0}' already exists", name));
            }

            return new ValidationResult(true, null);
        }
    }
}
