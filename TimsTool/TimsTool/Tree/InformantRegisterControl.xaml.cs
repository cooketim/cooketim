using Models.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Models.Commands;
using TimsTool.DataService;
using Configuration;
using System.Threading.Tasks;
using Identity;
using Serilog;

namespace TimsTool.Tree
{
    public interface IInformantRegisterControl
    {
        void InitializeComponent();
    }

    public partial class InformantRegisterControl : UserControl, IInformantRegisterControl
    {
        private ITreeModel treeModel;
        private readonly IAppSettings appSettings;
        private readonly IServiceProvider serviceProvider;
        private readonly IResultsDataClient resultsDataClient;
        public InformantRegisterControl()
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
                OnOpenErrorDialog(publishModel.ErrorReport, null, "Errors preventing publishing draft Informant Register Subscription!", "Publish Informant Register Subscription", false);
                return;
            }

            if (string.IsNullOrEmpty(publishModel.SummaryPublicationReport))
            {
                return;
            }

            //confirm the action to publish
            if (OnOpenErrorDialog(publishModel.SummaryPublicationReport, publishModel.DetailedPublicationReport, "The following items will be published, all changes will be saved and you will not be able to reverse this action. Do you wish to continue?", "Publish Informant Register Subscription"))
            {
                publishCmd.Execute(publishModel);

                //save the data changes made from the action to publish
                var errorMessage = await SaveData();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OnOpenErrorDialog(errorMessage, null, "Publish action changes not saved. Fatal errors ocurred saving to Azure", "Publish Informant Register Subscription", false);
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
            ConfirmDraftWindow win = new ConfirmDraftWindow(pm, string.Format("Please confirm the draft action and select Publication Tags for Informant Register Subscription: '{0}'", selectedNOWSubs.Label));
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

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();

                //also bring the selected items children into view by scrolling vertically 
                item.UpdateLayout();

                ////find the scroll viewer
                //DependencyObject parent = VisualTreeHelper.GetParent(item);
                //while (parent != null && !(parent is ScrollViewer))
                //{
                //    parent = VisualTreeHelper.GetParent(parent);
                //}

                //if (parent is ScrollViewer)
                //{
                //    var scrollViewer = parent as ScrollViewer;
                //    Point offset = item.TransformToAncestor(scrollViewer).Transform(new Point(0, 0));
                //    scrollViewer.ScrollToVerticalOffset(offset.Y);
                //}

                e.Handled = true;
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
    }
}