using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteResultDefinitionWordGroupCommand : ICommand { }

    public class DeleteResultDefinitionWordGroupCommand : IDeleteResultDefinitionWordGroupCommand
    {
        private ITreeModel treeModel;
        public DeleteResultDefinitionWordGroupCommand(ITreeModel treeModel)
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
            var selectedResultDefinitionWordGroup = treeModel.SelectedItem as ResultDefinitionWordGroupTreeViewModel;

            //get the parent
            var parent = selectedResultDefinitionWordGroup.Parent as ResultWordGroupTreeViewModel;
            if (parent == null) { return; }

            //remove word group from its parent and set the related result definition as modified
            parent.ResultWordGroupViewModel.ResultDefinitionWordGroups.RemoveAll(x=>x.UUID == selectedResultDefinitionWordGroup.ResultDefinitionWordGroupViewModel.UUID);
            parent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups.RemoveAll(x => x.UUID == selectedResultDefinitionWordGroup.ResultDefinitionWordGroupViewModel.UUID);
            var rdVM = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == parent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinition.UUID);
            if (rdVM != null) { rdVM.LastModifiedDate = DateTime.Now; }

            //maintain the parent Word group association for the result definition word group
            selectedResultDefinitionWordGroup.ResultDefinitionWordGroupViewModel.ParentWordGroups.Remove(parent.ResultWordGroupViewModel);

            //determine if the word group is soon to be an orphan by looking for other parents
            var otherParents = treeModel.AllResultDefinitionsViewModel.GetOtherParents(selectedResultDefinitionWordGroup);
            if (otherParents == null || otherParents.Count == 0)
            {
                //no other parents ...
                //tidy up the data provider to remove the soon to be orphan
                var vms = treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.FindAll(x => x.UUID == selectedResultDefinitionWordGroup.ResultDefinitionWordGroupViewModel.UUID);
                treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.RemoveAll(x => x.UUID == selectedResultDefinitionWordGroup.ResultDefinitionWordGroupViewModel.UUID);

                //logically delete the data so that it may be included in the next data patch upload;
                foreach(var rdWG in vms)
                {
                    rdWG.DeletedDate = DateTime.Now;

                    if (rdWG.Synonyms != null)
                    {
                        foreach(var item in rdWG.Synonyms)
                        {
                            item.DeletedDate = DateTime.Now;
                        }
                    }
                }
            }

            //inform the result definition that the word group has been removed so that it may determine if the short code needs to be edittable
            parent.ResultWordGroupViewModel.ParentResultDefinitionVM.SetWordGroupsChanged();

            int index;
            //find the selected word group in the tree of children
            for (index = 0; index < parent.Children.Count; index++)
            {
                var wordGroupChild = parent.Children[index] as ResultDefinitionWordGroupTreeViewModel;
                if (wordGroupChild != null && wordGroupChild.ResultDefinitionWordGroupViewModel.UUID == selectedResultDefinitionWordGroup.ResultDefinitionWordGroupViewModel.UUID)
                {
                    //index is the location of the word group for removal
                    parent.Children.RemoveAt(index);
                    break;
                }
            }

            //if the parent does not have any remaining children, purge it from the tree as we cannot have an empty group
            if (parent.Children.Count == 0)
            {
                parent.Parent.Children.Remove(parent);
                parent.Parent.IsSelected = true;
            }
            else
            {
                //select the next node
                if (parent.Children.Count - 1 <= index && index > 0)
                {
                    index = index - 1;
                }
                parent.Children[index].IsSelected = true;
            }

            //reset the copied item
            selectedResultDefinitionWordGroup.PendingCopyModel = null;
        }
    }
}
