using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteFixedListCommand : ICommand { }

    public class DeleteFixedListCommand : IDeleteFixedListCommand
    {
        private ITreeModel treeModel;
        public DeleteFixedListCommand(ITreeModel treeModel)
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

            //get the parent
            var parent = selectedFixedList.Parent as ResultPromptTreeViewModel;
            if (parent == null) { return; }

            //determine if we are purging the fixed list
            if (selectedFixedList.IsDeleted)
            {
                PurgeFixedList(selectedFixedList, parent);
            }
            else
            {
                //set the fixed list as deleted
                selectedFixedList.FixedListViewModel.DeletedDate = DateTime.Now;
                foreach (var flv in selectedFixedList.FixedListViewModel.FixedListValues)
                {
                    flv.DeletedDate = DateTime.Now;
                    flv.LastModifiedDate = DateTime.Now;
                }

                parent.ResultPromptRuleViewModel.ResultPromptViewModel.LastModifiedDate = DateTime.Now;
            }

            //reset the copied item
            selectedFixedList.PendingCopyModel = null;
        }

        private void PurgeFixedList(FixedListTreeViewModel selectedFixedList, ResultPromptTreeViewModel parent)
        {
            //remove fixed list from its parent
            parent.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList = null;

            //determine if the fixed is soon to be an orphan by looking for other parents
            var otherParents = treeModel.AllResultPromptViewModel.GetOtherParents(selectedFixedList);
            if (otherParents.Count == 0)
            {
                //tidy up the data provider to remove the soon to be orphan
                treeModel.AllFixedListViewModel.FixedLists.RemoveAll(x => x.UUID == selectedFixedList.FixedListViewModel.UUID);

                //Remove the data = only when the data is draft.  When not draft then the data must remain for the data patch
                if (selectedFixedList.FixedListViewModel.PublishedStatus == DataLib.PublishedStatus.Draft)
                {
                    treeModel.AllData.FixedLists.RemoveAll(x => x.UUID == selectedFixedList.FixedListViewModel.UUID);
                }
            }

            int index;
            //find the selected fixed list in the tree of children
            for (index = 0; index < parent.Children.Count; index++)
            {
                var fixedListChild = parent.Children[index] as FixedListTreeViewModel;
                if (fixedListChild != null && fixedListChild.FixedListViewModel.UUID == selectedFixedList.FixedListViewModel.UUID)
                {
                    //index is the location of the fixed list for removal
                    parent.Children.RemoveAt(index);
                    break;
                }
            }

            //select the next node
            if (parent.Children.Count - 1 <= index && index > 0)
            {
                index = index - 1;
                parent.Children[index].IsSelected = true;
            }
            else
            {
                parent.IsSelected = true;
            }            
        }
    }
}
