using Configuration;
using Models.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Linq;
using Models.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using TimsTool.DataService;
using System.Threading.Tasks;
using Identity;
using Serilog;

namespace TimsTool.Tree
{
    public interface INowSubscriptionControl
    {
        void InitializeComponent();
    }

    public partial class NowSubscriptionControl : UserControl, INowSubscriptionControl
    {
        private IAppSettings appSettings;
        private ITreeModel treeModel;
        private readonly IServiceProvider serviceProvider;
        private readonly IResultsDataClient resultsDataClient;
        public NowSubscriptionControl()
        {
            serviceProvider = ((App)Application.Current).ServiceProvider;
            treeModel = serviceProvider.GetRequiredService<ITreeModel>();
            appSettings = serviceProvider.GetRequiredService<IAppSettings>();
            resultsDataClient = serviceProvider.GetRequiredService<IResultsDataClient>();
            InitializeComponent();

            // Let the UI bind to the view-model.
            DataContext = treeModel;
        }

        private void MenuItem_PromptUsages_Click(object sender, RoutedEventArgs e)
        {
            //Determine the selected item
            var selectedPromptDefinition = treeModel.SelectedItem as ResultPromptTreeViewModel;
            if (selectedPromptDefinition == null)
            {
                return;
            }

            var cmd = treeModel.GenerateReportCommand as GenerateReportCommand;
            var report = cmd.PromptUsagesReport(selectedPromptDefinition.ResultPromptRuleViewModel.ResultPromptViewModel.UUID);
            var promptLabel = selectedPromptDefinition.ResultPromptRuleViewModel.ResultPromptViewModel.ResultPrompt.Label;

            OnOpenErrorDialog(report, null, "Result Prompt Usages ...", promptLabel, false);
        }

        private void MenuItem_ResultUsages_Click(object sender, RoutedEventArgs e)
        {
            //Determine the selected item
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

            var cmd = treeModel.GenerateReportCommand as GenerateReportCommand;
            var report = cmd.ResultUsagesReport(selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            var resultLabel = selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.Label;

            OnOpenErrorDialog(report, null, "Result Definition Usages ...", resultLabel, false);
        }

        private void MenuItem_NOWUsages_Click(object sender, RoutedEventArgs e)
        {
            //Determine the selected item
            var selectedNOW = treeModel.SelectedItem as NowTreeViewModel;

            var cmd = treeModel.GenerateReportCommand as GenerateReportCommand;
            var report = cmd.NowUsagesReport(selectedNOW.NowViewModel.Now.UUID);
            var resultLabel = selectedNOW.NowViewModel.Now.Name;

            OnOpenErrorDialog(report, null, "NOW Usages ...", resultLabel, false);
        }

        #region publish and draft menu actions
        private async void MenuItem_PublishSubscription_Click(object sender, RoutedEventArgs e)
        {
            //Determine the selected item
            var selectedNOWSub = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            if (selectedNOWSub == null) { return; }

            //first validate
            treeModel.ValidateCommand.Execute(null);
            //show any errors
            if (!string.IsNullOrEmpty(treeModel.ValidateCommand.ErrorMessages))
            {
                OnOpenErrorDialog(treeModel.ValidateCommand.ErrorMessages, null, "Data validation failures prevent publish action!", "Publish NOW Subscription", false);
                return;
            }

            //get the draft nows
            var publishCmd = treeModel.PublishDraftsCommand as PublishDraftsCommand;
            var publishModel = publishCmd.CollectDrafts(selectedNOWSub, true);
            if (publishModel == null)
            {
                return;
            }

            //check for errors that will prevent the publication of the now subscription
            if (publishModel.HasErrors)
            {
                OnOpenErrorDialog(publishModel.ErrorReport, null, "Errors preventing publishing draft NOW Subscription!", "Publish NOW Subscription", false);
                return;
            }

            if (string.IsNullOrEmpty(publishModel.SummaryPublicationReport))
            {
                return;
            }

            //confirm the action to publish
            if (OnOpenErrorDialog(publishModel.SummaryPublicationReport, publishModel.DetailedPublicationReport, "The following items will be published, all changes will be saved and you will not be able to reverse this action. Do you wish to continue?", "Publish NOW Subscription"))
            {
                publishCmd.Execute(publishModel);

                //save the data changes made from the action to publish
                var errorMessage = await SaveData();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OnOpenErrorDialog(errorMessage, null, "Publish action changes not saved. Fatal errors ocurred saving to Azure", "Publish NOW Subscription", false);
                    return;
                }
            }
        }

        private async Task<string> SaveData()
        {
            //save to file
            treeModel.SaveToFileCommand.Execute(appSettings.DataFilePath);

            //only save to azure when required
            if (treeModel.IsLocalChangeMode)
            {
                return null;
            }

            try
            {
                var resp = await resultsDataClient.SaveDataAsync(IdentityHelper.SignedInUser, appSettings.DataFilePath);
                treeModel.CheckOutUser = resp.CheckedoutUser;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception whilst saving changes to Azure");
                return ex.Message;
            }
            return null;
        }

        private void MenuItem_DraftSubscription_Click(object sender, RoutedEventArgs e)
        {
            var selectedNOWSubs = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            if (selectedNOWSubs == null) { return; }

            //confirm the draft action
            var pm = new PublicationTagsModel(treeModel, null);
            ConfirmDraftWindow win = new ConfirmDraftWindow(pm, string.Format("Please confirm the draft action and select Publication Tags for NOW Subscription: '{0}'", selectedNOWSubs.Label));
            win.Owner = Window.GetWindow(this);

            var confirmed = win.ShowDialog();
            if (confirmed.HasValue && confirmed.Value)
            {
                treeModel.DraftNowSubscriptionCommand.Execute(pm);
            }
        }

        private void MenuItem_AddNewNowSubscription_Click(object sender, RoutedEventArgs e)
        {
            var commandParameter = ((MenuItem)sender).CommandParameter as string;

            //confirm the add new action
            var pm = new PublicationTagsModel(treeModel, null);
            ConfirmDraftWindow win = new ConfirmDraftWindow(pm, "Please confirm the add new action and select Publication Tags");
            win.Owner = Window.GetWindow(this);

            var confirmed = win.ShowDialog();
            if (confirmed.HasValue && confirmed.Value)
            {
                treeModel.AddNewNowSubscriptionCommand.Execute(new Tuple<string, PublicationTagsModel>(commandParameter, pm));
            }
        }

        #endregion publish and draft menu actions

        private bool OnOpenErrorDialog(string messages, string detailedMessages, string title, string winTitle, bool withCancel = true)
        {
            var msgModel = new MessagesModel()
            {
                Title = title,
                Messages = messages,
                DetailedMessages = detailedMessages,
                CancelVisible = withCancel
            };
            MessageWindow win = new MessageWindow(msgModel);
            win.Owner = Window.GetWindow(this);
            win.Title = winTitle;

            var res = win.ShowDialog();
            return res.HasValue ? res.Value : false;
        }

        private void btnSampleNow_Click(object sender, RoutedEventArgs e)
        {
            //first generate the sample
            var menu = sender as MenuItem;

            if (menu == null) { return; }

            var selection = menu.CommandParameter as string;
            if (selection != null && selection.ToLowerInvariant() == "application")
            {
                ApplicationNowWindow win = new ApplicationNowWindow(true);
                win.Owner = Window.GetWindow(this);

                if (win.ShowDialog() == true)
                {
                    treeModel.SampleNowCommand.Execute(win.DataContext);
                    OpenFiles();
                }
            }
            else
            {
                treeModel.SampleNowCommand.Execute(menu.CommandParameter as string);
                //open the sample
                OpenFiles();
            }
        }

        private void OpenFiles()
        {
            var dataFileDirectory = appSettings.DataFileDirectory;
            var directoryPath = Path.Combine(dataFileDirectory, "nowContent");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Now files (*.pdf;*.json;)|*.pdf;*.json;|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = directoryPath;
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName.EndsWith("pdf") || openFileDialog.FileName.EndsWith("json"))
                {
                    var process = new Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = openFileDialog.FileName;
                    process.Start();
                }
            }
        }

        private void btnCascadeNRNowText_Click(object sender, RoutedEventArgs e)
        {
            var selectedNowRequirement = treeModel.SelectedItem as NowRequirementTreeViewModel;
            var cmd = treeModel.GenerateReportCommand as GenerateReportCommand;
            var usages = cmd.OtherNowRequirementsReport(selectedNowRequirement.NowRequirementViewModel.NowRequirement);

            if (usages.Item1 != null && usages.Item1.Count > 0)
            {
                //has other usages
                if (OnOpenErrorDialog(usages.Item2, null, "The following usages will be updated.  Do you wish to continue?", selectedNowRequirement.NowRequirementViewModel.ResultDefinition.Label))
                {
                    treeModel.CascadeNowTextCommand.Execute(usages.Item1);
                }
            }
        }

        private void ddlUserGroups_TextChanged(object sender, TextChangedEventArgs e)
        {
            NowSubscriptionTreeViewModel treeItem = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            ddlUserGroups.ItemsSource = treeItem.NowSubscriptionViewModel.AllUserGroups.Where(x => x.ValueString.ToLowerInvariant().StartsWith(ddlUserGroups.Text.ToLowerInvariant().Trim()));
        }

        private void chkUserGroup_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            NowSubscriptionTreeViewModel treeItem = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            treeItem.NowSubscriptionViewModel.SelectedUserGroupVariants.Add((string)checkBox.CommandParameter);
        }

        private void chkUserGroup_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            NowSubscriptionTreeViewModel treeItem = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            var itemToRemove = treeItem.NowSubscriptionViewModel.SelectedUserGroupVariants.FirstOrDefault(x => x == (string)checkBox.CommandParameter);
            if (itemToRemove != null)
            {
                treeItem.NowSubscriptionViewModel.SelectedUserGroupVariants.Remove(itemToRemove);
            }
        }

        private void TreeView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            InvalidateVisual();
            UpdateLayout();
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (treeModel.SelectedAvailablePromptRuleUserGroup != null)
            {
                var selectedResultPrompt = treeModel.SelectedItem as ResultPromptTreeViewModel;
                if (!selectedResultPrompt.ResultPromptRuleViewModel.UserGroups.Contains(treeModel.SelectedAvailablePromptRuleUserGroup))
                {
                    selectedResultPrompt.ResultPromptRuleViewModel.UserGroups.Add(treeModel.SelectedAvailablePromptRuleUserGroup);
                }
            }
        }

        private void btnRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            if (treeModel.SelectedPromptRuleUserGroup != null)
            {
                var selectedResultPrompt = treeModel.SelectedItem as ResultPromptTreeViewModel;
                selectedResultPrompt.ResultPromptRuleViewModel.UserGroups.Remove(treeModel.SelectedPromptRuleUserGroup);
            }
        }

        private void btnRDAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (treeModel.SelectedAvailableResultDefinitionUserGroup != null)
            {
                var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
                if (!selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UserGroups.Contains(treeModel.SelectedAvailableResultDefinitionUserGroup))
                {
                    selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UserGroups.Add(treeModel.SelectedAvailableResultDefinitionUserGroup);
                }
            }
        }

        private void btnRDRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            if (treeModel.SelectedResultDefinitionUserGroup != null)
            {
                var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
                selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UserGroups.Remove(treeModel.SelectedResultDefinitionUserGroup);
            }
        }

        private void DurationSequenceTextBox(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                Regex regex = new Regex("[^1-3]+");
                e.Handled = regex.IsMatch(e.Text);
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            TreeViewItem item = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }

        private TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        // stop editing on lost focus
        private void userEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            treeModel.SelectedEditTreeViewModel = null;
        }
    }
}