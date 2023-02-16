using Configuration;
using Models.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Models.Commands;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using TimsTool.DataService;
using Identity;
using System.Threading.Tasks;

namespace TimsTool.Tree
{
    public interface IEdtControl
    {
        void InitializeComponent();
    }

    public partial class EdtControl : UserControl, IEdtControl
    {
        private IAppSettings appSettings;
        private ITreeModel treeModel;
        private readonly IServiceProvider serviceProvider;
        private readonly IResultsDataClient resultsDataClient;
        public EdtControl()
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
            var report = cmd.ResultUsagesReport(selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
            var resultLabel = selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.Label;

            OnOpenErrorDialog(report, null, "Result Definition Usages ...", resultLabel, false);
        }

        private void MenuItem_EDTUsages_Click(object sender, RoutedEventArgs e)
        {
            //Determine the selected item
            var selectedEDT = treeModel.SelectedItem as NowTreeViewModel;

            var cmd = treeModel.GenerateReportCommand as GenerateReportCommand;
            var report = cmd.NowUsagesReport(selectedEDT.NowViewModel.Now.UUID);
            var resultLabel = selectedEDT.NowViewModel.Now.Name;

            OnOpenErrorDialog(report, null, "EDT Usages ...", resultLabel, false);
        }


        private void TreeView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            InvalidateVisual();
            UpdateLayout();
        }

        private void btnSampleEDT_Click(object sender, RoutedEventArgs e)
        {
            //first generate the sample
            var menu = sender as MenuItem;

            if (menu == null) { return; }

            var selection = menu.CommandParameter as string;
            if (selection != null && selection.ToLowerInvariant() == "application")
            {
                ApplicationNowWindow win = new ApplicationNowWindow(false);
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
            var directoryPath = Path.Combine(dataFileDirectory, "edtContent");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "EDT files (*.pdf;*.json;)|*.pdf;*.json;|All files (*.*)|*.*";
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

        #region publish and draft menu actions
        private async void MenuItem_PublishEDT_Click(object sender, RoutedEventArgs e)
        {
            //Determine the selected item
            var selectedEDT = treeModel.SelectedItem as NowTreeViewModel;
            if (selectedEDT == null) { return; }

            //first validate
            treeModel.ValidateCommand.Execute(null);
            //show any errors
            if (!string.IsNullOrEmpty(treeModel.ValidateCommand.ErrorMessages))
            {
                OnOpenErrorDialog(treeModel.ValidateCommand.ErrorMessages, null, "Data validation failures prevent publish action!", "Publish EDT", false);
                return;
            }

            //get the draft nows
            var publishCmd = treeModel.PublishDraftsCommand as PublishDraftsCommand;
            var publishModel = publishCmd.CollectDrafts(selectedEDT, true);
            if (publishModel == null)
            {
                return;
            }

            //check for errors that will prevent the publication of the now
            if (publishModel.HasErrors)
            {
                OnOpenErrorDialog(publishModel.ErrorReport, null, "Errors preventing publishing draft Edt!", "Publish EDT", false);
                return;
            }

            if (string.IsNullOrEmpty(publishModel.SummaryPublicationReport))
            {
                return;
            }

            //confirm the action to publish
            if (OnOpenErrorDialog(publishModel.SummaryPublicationReport, publishModel.DetailedPublicationReport, "The following items will be published, all changes will be saved and you will not be able to reverse this action. Do you wish to continue?", "Publish EDT"))
            {
                publishCmd.Execute(publishModel);

                //save the data changes made from the action to publish
                var errorMessage = await SaveData();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OnOpenErrorDialog(errorMessage, null, "Publish action changes not saved. Fatal errors ocurred saving to Azure", "Publish EDT", false);
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

        private void MenuItem_DraftEDT_Click(object sender, RoutedEventArgs e)
        {
            var selectedEDT = treeModel.SelectedItem as NowTreeViewModel;

            //confirm the draft action
            var pm = new PublicationTagsModel(treeModel, null);
            ConfirmDraftWindow win = new ConfirmDraftWindow(pm, string.Format("Please confirm the draft action and select Publication Tags for EDT: '{0}'", selectedEDT.Label));
            win.Owner = Window.GetWindow(this);

            var confirmed = win.ShowDialog();
            if (confirmed.HasValue && confirmed.Value)
            {
                treeModel.DraftNowCommand.Execute(pm);
            }
        }

        private void MenuItem_AddNewEDT_Click(object sender, RoutedEventArgs e)
        {
            //confirm the add new action
            var pm = new PublicationTagsModel(treeModel, null);
            ConfirmDraftWindow win = new ConfirmDraftWindow(pm, "Please confirm the add new action and select Publication Tags");
            win.Owner = Window.GetWindow(this);

            var confirmed = win.ShowDialog();
            if (confirmed.HasValue && confirmed.Value)
            {
                treeModel.AddNewEDTCommand.Execute(pm);
            }
        }       

        #endregion publish and draft menu actions

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

        Point startPoint;
        TreeViewItemViewModel sourceDragDataModel;
        //TreeViewItemViewModel targetDragDataModel;

        /// <summary>
        /// This event occurs when before the left mouse button is down. Save mouse position in a variable as the start point for the drag drop. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(tvEDTs);
        }

        /// <summary>
        /// This event occurs when mouse is moved. Here first we check whether left mouse button is pressed or not. 
        /// Then check the distance mouse moved if it moves outside the selected treeview item, then check the drop effect if it's dragged (move) and then dropped over a TreeViewItem (i.e. target is not null) then copy the selected item in dropped item. 
        /// In this event, you can put your desired condition for dropping treeviewItem. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && startPoint.X > 0 && startPoint.Y > 0)
                {
                    Point currentPosition = e.GetPosition(tvEDTs);

                    if ((Math.Abs(currentPosition.X - startPoint.X) > 50.0) ||
                        (Math.Abs(currentPosition.Y - startPoint.Y) > 50.0))
                    {
                        sourceDragDataModel = (TreeViewItemViewModel)tvEDTs.SelectedItem;
                        if (sourceDragDataModel != null && sourceDragDataModel.DragSupported)
                        {

                            // Create a brush which will paint the ListViewItem onto
                            // a visual in the adorner layer.

                            //AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(PART_DragDropAdorner);
                            //var dragAdorner = new DragAdorner(adornerLayer);
                            //dragAdorner.Text = sourceDragDataModel.DragName;
                            //adornerLayer.Add(dragAdorner);

                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(tvEDTs, tvEDTs.SelectedValue, DragDropEffects.Copy);
                            //Checking target is not null and item is dragging(moving)
                            if ((finalDropEffect == DragDropEffects.Copy))
                            //if ((finalDropEffect == DragDropEffects.Copy) && (targetDragDataModel != null))
                            {
                                // A Move drop was accepted
                                CopyItem(sourceDragDataModel);
                                //targetDragDataModel = null;
                                sourceDragDataModel = null;
                                startPoint = new Point(0, 0);
                            }

                            //adornerLayer.Remove(dragAdorner);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void CopyItem(TreeViewItemViewModel sourceItem)
        {
            //Asking user wether he want to drop the dragged TreeViewItem here or not
            if (MessageBox.Show(string.Format("Would you like to create a draft of {0}", sourceItem.DragName), "Create Draft Version", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                treeModel.DraftNowCommand.Execute(sourceItem);
                //to be replaced with the treeview making its own internal command call

                //var command = targetItem.DoDrop(sourceItem, targetItem);
                //var typeName = command == null ? "NULL" : (command.GetType()).Name;

                //var messageCheck = string.Format("Selected Paste Details...{0}Command - '{1}'{2}PendingCopyModel - '{3}'{4}TargetModel - {5}",
                //    Environment.NewLine,
                //    typeName,
                //    Environment.NewLine,
                //    treeModel.CopiedTreeViewModel == null ? "NULL" : treeModel.CopiedTreeViewModel.GetType().Name,
                //    Environment.NewLine,
                //    treeModel.SelectedItem == null ? "NULL" : treeModel.SelectedItem.GetType().Name
                //    );

                //MessageBox.Show(messageCheck, "", MessageBoxButton.OK);

                //try
                //{
                //    //adding dragged TreeViewItem in target TreeViewItem
                //    addChild(sourceItem, targetItem);

                //    //finding Parent TreeViewItem of dragged TreeViewItem 
                //    TreeViewItem ParentItem = FindVisualParent<TreeViewItem>(sourceItem);
                //    // if parent is null then remove from TreeView else remove from Parent TreeViewItem
                //    if (ParentItem == null)
                //    {
                //        tvResultDefinitions.Items.Remove(sourceItem);
                //    }
                //    else
                //    {
                //        ParentItem.Items.Remove(sourceItem);
                //    }
                //}
                //catch
                //{

                //}
            }
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