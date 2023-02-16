using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Models.ViewModels;
using System.Windows.Media;
using System.Text.RegularExpressions;
using DataLib;
using Models.Commands;
using System;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using TimsTool.DataService;
using Configuration;
using System.Threading.Tasks;
using Identity;
using System.Linq;

namespace TimsTool.Tree
{
    public interface IResultDefinitionControl
    {
        void addChild(TreeViewItem _sourceItem, TreeViewItem _targetItem);
        void InitializeComponent();
    }

    public partial class ResultDefinitionControl : UserControl, IResultDefinitionControl
    {
        private ITreeModel treeModel;
        private readonly IAppSettings appSettings;
        private readonly IServiceProvider serviceProvider;
        private readonly IResultsDataClient resultsDataClient;
        public ResultDefinitionControl()
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

        #region delete menu actions
        private void MenuItem_DeleteResult_Click(object sender, RoutedEventArgs e)
        {
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
            var cmd = treeModel.GenerateReportCommand as GenerateReportCommand;
            var usages = cmd.ResultUsagesWithoutParentageReport(selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
            if (usages.Item1)
            {
                //has significant usages
                if (OnOpenErrorDialog(usages.Item2, null, "There are significant usages.  Do you wish to continue?", selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Label))
                {
                    treeModel.DeleteResultDefinitionCommand.Execute(null);
                }
            }
            else
            {
                treeModel.DeleteResultDefinitionCommand.Execute(null);
            }
        }

        private void MenuItem_DeletePrompt_Click(object sender, RoutedEventArgs e)
        {
            var selectedResultPrompt = treeModel.SelectedItem as ResultPromptTreeViewModel;
            var cmd = treeModel.GenerateReportCommand as GenerateReportCommand;
            var usages = cmd.PromptUsagesWithFlagReport(selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.MasterUUID);
            if (usages.Item1)
            {
                //has significant usages
                if (OnOpenErrorDialog(usages.Item2, null, "There are significant usages.  Do you wish to continue?", selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.Label))
                {
                    treeModel.DeleteResultPromptCommand.Execute(null);
                }
            }
            else
            {
                treeModel.DeleteResultPromptCommand.Execute(null);
            }
        }
        #endregion region delete menu actions        

        #region edit user groups
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

        #endregion edit user groups

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

        private void BringChildrenIntoView(TreeViewItem parent)
        {
            if (parent != null && parent.IsExpanded)
            {
                List<TreeViewItem> children = new List<TreeViewItem>();

                foreach (var item in parent.Items)
                {
                    TreeViewItem child = item as TreeViewItem;

                    if (child == null)
                    {
                        child = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
                    }

                    if (child != null)
                    {
                        children.Add(child);
                    }
                }

                if (children.Count > 0)
                {
                    var lastChild = children[children.Count - 1];
                    lastChild.BringIntoView();
                }
            }
        }

        #region Drag & Drop

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
            startPoint = e.GetPosition(tvResultDefinitions);
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
                    Point currentPosition = e.GetPosition(tvResultDefinitions);

                    if ((Math.Abs(currentPosition.X - startPoint.X) > 50.0) ||
                        (Math.Abs(currentPosition.Y - startPoint.Y) > 50.0))
                    {
                        sourceDragDataModel = (TreeViewItemViewModel)tvResultDefinitions.SelectedItem;
                        if (sourceDragDataModel != null && sourceDragDataModel.DragSupported)
                        {

                            // Create a brush which will paint the ListViewItem onto
                            // a visual in the adorner layer.

                            //AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(PART_DragDropAdorner);
                            //var dragAdorner = new DragAdorner(adornerLayer);
                            //dragAdorner.Text = sourceDragDataModel.DragName;
                            //adornerLayer.Add(dragAdorner);

                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(tvResultDefinitions, tvResultDefinitions.SelectedValue, DragDropEffects.Copy);
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

        /// <summary>
        /// This event occurs when an object is dragged (moves) within the drop target's boundary. 
        /// Here, we check whether the pointer is near a TreeViewItem or not; if near, then set Drop effect on it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                Point currentPosition = e.GetPosition(tvResultDefinitionsDraft);

                if ((Math.Abs(currentPosition.X - startPoint.X) > 50.0) ||
                    (Math.Abs(currentPosition.Y - startPoint.Y) > 50.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    TreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
                    if (item != null)
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// This event occurs when an object is dragged (moves) within the drop target's boundary. 
        /// Here, we check whether the pointer is near the target treeview or not; if near, then set Drop effect on it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                Point currentPosition = e.GetPosition(tvResultDefinitionsDraft);

                if ((Math.Abs(currentPosition.X - startPoint.X) > 50.0) ||
                    (Math.Abs(currentPosition.Y - startPoint.Y) > 50.0))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                e.Handled = true;
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
                var cmd = treeModel.DraftResultDefinitionCommand as DraftResultDefinitionCommand;
                cmd.Execute(sourceItem);
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

        public void addChild(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            // add item in target TreeViewItem 
            TreeViewItem item1 = new TreeViewItem();
            item1.Header = _sourceItem.Header;
            _targetItem.Items.Add(item1);
            foreach (TreeViewItem item in _sourceItem.Items)
            {
                addChild(item, item1);
            }
        }
        static TObject FindVisualParent<TObject>(UIElement child) where TObject : UIElement
        {
            if (child == null)
            {
                return null;
            }

            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

            while (parent != null)
            {
                TObject found = parent as TObject;
                if (found != null)
                {
                    return found;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }

            return null;
        }

        private TreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            return container;
        }

        #endregion Drag & Drop

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

        // stop editing user on lost focus
        private void userEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            treeModel.SelectedEditTreeViewModel = null;
        }

        #region usages
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
            var report = cmd.ResultUsagesReport(selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            var resultLabel = selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.Label;

            OnOpenErrorDialog(report, null, "Result Definition Usages ...", resultLabel, false);
        }
        #endregion usages

        #region publish and draft menu actions
        private async void MenuItem_PublishResultDefinition_Click(object sender, RoutedEventArgs e)
        {
            //Determine the selected item
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
            if (selectedResultDefinition == null) { return; }

            //first validate
            treeModel.ValidateCommand.Execute(null);
            //show any errors
            if (!string.IsNullOrEmpty(treeModel.ValidateCommand.ErrorMessages))
            {
                OnOpenErrorDialog(treeModel.ValidateCommand.ErrorMessages, null, "Data validation failures prevent publish action!", "Publish Result Definition", false);
                return;
            }

            //get the draft results
            var publishCmd = treeModel.PublishDraftsCommand as PublishDraftsCommand;
            var publishModel = publishCmd.CollectDrafts(selectedResultDefinition);

            if (publishModel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(publishModel.SummaryPublicationReport))
            {
                return;
            }

            //confirm the action to publish
            if (OnOpenErrorDialog(publishModel.SummaryPublicationReport, publishModel.DetailedPublicationReport, "The following items will be published, all changes will be saved and you will not be able to reverse this action. Do you wish to continue?", "Publish Result Definition"))
            {
                publishCmd.Execute(publishModel);

                //save the data changes made from the action to publish
                var errorMessage = await SaveData();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OnOpenErrorDialog(errorMessage, null, "Publish action changes not saved. Fatal errors ocurred saving to Azure", "Publish Result Definition", false);
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

        private void MenuItem_DraftResultDefinition_Click(object sender, RoutedEventArgs e)
        {
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
            if (selectedResultDefinition == null) { return; }

            //confirm the draft action
            var pm = new PublicationTagsModel(treeModel, null);
            ConfirmDraftWindow win = new ConfirmDraftWindow(pm, string.Format("Please confirm the draft action and select Publication Tags for Result Definition: '{0}'", selectedResultDefinition.Label));
            win.Owner = Window.GetWindow(this);

            var confirmed = win.ShowDialog();
            if (confirmed.HasValue && confirmed.Value)
            {
                treeModel.DraftResultDefinitionCommand.Execute(pm);
            }
        }
        private void MenuItem_AddNewResultDefinition_Click(object sender, RoutedEventArgs e)
        {
            //confirm the add new action
            var pm = new PublicationTagsModel(treeModel, null);
            ConfirmDraftWindow win = new ConfirmDraftWindow(pm, "Please confirm the add new action and select Publication Tags");
            win.Owner = Window.GetWindow(this);

            var confirmed = win.ShowDialog();
            if (confirmed.HasValue && confirmed.Value)
            {
                treeModel.AddNewResultDefinitionCommand.Execute(pm);
            }
        }

        #endregion publish and draft menu actions

        //check for circular references before proceeding with the paste
        private void MenuItem_PasteChildRD_Click(object sender, RoutedEventArgs e)
        {
            //Determine the selected item
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

            if (selectedResultDefinition == null)
            {
                return;
            }

            //check that creating this child relationship will not create a circular reference
            var parents = treeModel.AllData.ResultDefinitionRules.FindAll(x => x.ChildResultDefinitionUUID == selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            var pendingCopyModel = treeModel.CopiedTreeViewModel as ResultDefinitionTreeViewModel;
            var valid = true;
            foreach (var parent in parents)
            {
                if (FindCircularReference(parent, pendingCopyModel))
                {
                    //cannot continue as a circular reference will be created
                    valid = false;
                    var messages = string.Format("Cannot paste '{0}' as a child because a circular reference would be created", pendingCopyModel.ResultRuleViewModel.ChildResultDefinitionViewModel.Label);
                    OnOpenErrorDialog(messages, null, "Paste result definition error ...", "Validation Error", false);
                    break;
                }
            }

            if (valid)
            {
                treeModel.PasteResultDefinitionChildCommand.Execute(sender);
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

        private bool FindCircularReference(ResultDefinitionRule item, ResultDefinitionTreeViewModel pendingCopyModel)
        {
            //recursively check for a circular reference
            if (item.ParentUUID != null)
            {

                //check if the parent matches the copy item being pasted
                if (item.ParentUUID.Value == pendingCopyModel.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                {
                    return true;
                }

                //Get the next level of parents
                var parents = treeModel.AllData.ResultDefinitionRules.FindAll(x => x.ChildResultDefinitionUUID == item.ParentUUID);
                foreach (var parent in parents)
                {
                    if (FindCircularReference(parent, pendingCopyModel))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}