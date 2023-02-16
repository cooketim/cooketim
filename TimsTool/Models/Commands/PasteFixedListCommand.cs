using Models.ViewModels;
using System;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IPasteFixedListCommand : ICommand { }

    public class PasteFixedListCommand : IPasteFixedListCommand
    {
        private ITreeModel treeModel;
        public PasteFixedListCommand(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            // I intentionally left these empty because
            // this command never raises the event, and
            // not using the WeakEvent pattern here can
            // cause memory leaks.  WeakEvent pattern is
            // not simple to implement, so why bother.
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            //Determine the selected item
            var selectedFixedList = treeModel.SelectedItem as FixedListTreeViewModel;

            if (selectedFixedList == null)
            {
                //parent result prompt selected as target of paste
                PasteTarget(treeModel.SelectedItem as ResultPromptTreeViewModel);
            }
            else
            {
                //fixedList selected as target of paste
                PasteTarget((ResultPromptTreeViewModel)selectedFixedList.Parent);
            }           

            //reset the copied item
            PendingCopyModel().PendingCopyModel = null;
        }

        private FixedListTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as FixedListTreeViewModel;
        }

        private void PasteTarget(ResultPromptTreeViewModel targetParent)
        {
            //only paste when the target parent does not already have a fixed list, only one fixed list is allowed
            if (targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList == null)
            {
                //set the data
                targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList = PendingCopyModel().FixedListViewModel;

                //make a shallow clone for the new tree item
                var newItem = PendingCopyModel().Clone();
                newItem.ResetParentage(targetParent);

                //add the new tree item to the children collection at position 0
                targetParent.Children.Add(newItem);
                targetParent.Children.Move(targetParent.Children.Count - 1, 0);

                //ensure that the pasted item is selected and the parent is expanded
                targetParent.IsExpanded = true;
                newItem.IsSelected = true;
            }
        }
    }
}
