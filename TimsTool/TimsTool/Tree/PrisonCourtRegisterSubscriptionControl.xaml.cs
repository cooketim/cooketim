using Models.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Models.Commands;
using Configuration;
using TimsTool.DataService;
using System.Threading.Tasks;
using Identity;
using Serilog;

namespace TimsTool.Tree
{
    public interface IPrisonCourtRegisterSubscriptionControl
    {
        void InitializeComponent();
    }

    public partial class PrisonCourtRegisterSubscriptionControl : UserControl, IPrisonCourtRegisterSubscriptionControl
    {
        private ITreeModel treeModel;
        private readonly IAppSettings appSettings;
        private readonly IServiceProvider serviceProvider;
        private readonly IResultsDataClient resultsDataClient;
        public PrisonCourtRegisterSubscriptionControl()
        {
            serviceProvider = ((App)Application.Current).ServiceProvider;
            treeModel = serviceProvider.GetRequiredService<ITreeModel>();
            appSettings = serviceProvider.GetRequiredService<IAppSettings>();
            resultsDataClient = serviceProvider.GetRequiredService<IResultsDataClient>();
            InitializeComponent();

            // Let the UI bind to the view-model.
            DataContext = treeModel;
        }

        private void TreeView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            InvalidateVisual();
            UpdateLayout();
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
                OnOpenErrorDialog(treeModel.ValidateCommand.ErrorMessages, null, "Data validation failures prevent publish action!", "Publish Prison Court Register Subscription", false);
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
                OnOpenErrorDialog(publishModel.ErrorReport, null, "Errors preventing publishing draft Prison Court Register Subscription!", "Publish Prison Court Register Subscription", false);
                return;
            }

            if (string.IsNullOrEmpty(publishModel.SummaryPublicationReport))
            {
                return;
            }

            //confirm the action to publish
            if (OnOpenErrorDialog(publishModel.SummaryPublicationReport, publishModel.DetailedPublicationReport, "The following items will be published, all changes will be saved and you will not be able to reverse this action. Do you wish to continue?", "Publish Prison Court Register Subscription"))
            {
                publishCmd.Execute(publishModel);

                //save the data changes made from the action to publish
                var errorMessage = await SaveData();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OnOpenErrorDialog(errorMessage, null, "Publish action changes not saved. Fatal errors ocurred saving to Azure", "Publish Prison Court Register Subscription", false);
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
            ConfirmDraftWindow win = new ConfirmDraftWindow(pm, string.Format("Please confirm the draft action and select Publication Tags for Prison Court Register Subscription: '{0}'", selectedNOWSubs.Label));
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
    }
}