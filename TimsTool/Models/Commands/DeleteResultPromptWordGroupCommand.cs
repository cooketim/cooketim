using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteResultPromptWordGroupCommand : ICommand { }

    public class DeleteResultPromptWordGroupCommand : IDeleteResultPromptWordGroupCommand
    {
        private ITreeModel treeModel;
        public DeleteResultPromptWordGroupCommand(ITreeModel treeModel)
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
            var selectedResultPromptWordGroup = treeModel.SelectedItem as ResultPromptWordGroupTreeViewModel;

            //get the parent
            var parent = selectedResultPromptWordGroup.Parent as ResultPromptTreeViewModel;
            if (parent == null) { return; }

            //determine if we are purging the word group
            if (selectedResultPromptWordGroup.IsDeleted)
            {
                PurgeWordGroup(selectedResultPromptWordGroup, parent);
            }
            else
            {
                //set the word group as deleted
                selectedResultPromptWordGroup.ResultPromptWordGroupViewModel.DeletedDate = DateTime.Now;
                foreach (var synonym in selectedResultPromptWordGroup.ResultPromptWordGroupViewModel.Synonyms)
                {
                    synonym.DeletedDate = DateTime.Now;
                    synonym.LastModifiedDate = DateTime.Now;
                }

                parent.ResultPromptRuleViewModel.ResultPromptViewModel.LastModifiedDate = DateTime.Now;
            }

            //reset the copied item
            selectedResultPromptWordGroup.PendingCopyModel = null;
        }

        private void PurgeWordGroup(ResultPromptWordGroupTreeViewModel selectedWordGroup, ResultPromptTreeViewModel parent)
        {
            //remove Word Group from its parent
            parent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups.RemoveAll(x=>x.UUID == selectedWordGroup.ResultPromptWordGroupViewModel.UUID);

            //determine if the word group is soon to be an orphan by looking for other parents
            var otherParents = treeModel.AllResultPromptViewModel.GetOtherParents(selectedWordGroup);
            if (otherParents.Count == 0)
            {
                //tidy up the data provider to remove the soon to be orphan
                treeModel.AllResultPromptWordGroupViewModel.WordGroups.RemoveAll(x => x.UUID == selectedWordGroup.ResultPromptWordGroupViewModel.UUID);

                //Remove the data = only when the data is draft.  When not draft then the data must remain for the data patch
                if (selectedWordGroup.ResultPromptWordGroupViewModel.PublishedStatus == DataLib.PublishedStatus.Draft)
                {
                    treeModel.AllData.ResultPromptWordGroups.RemoveAll(x => x.UUID == selectedWordGroup.ResultPromptWordGroupViewModel.UUID);
                }
            }

            int index;
            //find the selected word group in the tree of children
            for (index = 0; index < parent.Children.Count; index++)
            {
                var wordGroupChild = parent.Children[index] as ResultPromptWordGroupTreeViewModel;
                if (wordGroupChild != null && wordGroupChild.ResultPromptWordGroupViewModel.UUID == selectedWordGroup.ResultPromptWordGroupViewModel.UUID)
                {
                    //index is the location of the word for removal
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
