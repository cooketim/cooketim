using DataLib;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteNowRequirementCommand : ICommand { }

    public class DeleteNowRequirementCommand : IDeleteNowRequirementCommand
    {
        private ITreeModel treeModel;
        public DeleteNowRequirementCommand(ITreeModel treeModel)
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
            var selectedNowRequirement = treeModel.SelectedItem as NowRequirementTreeViewModel;

            //get the parent
            var parentNow = selectedNowRequirement.Parent as NowTreeViewModel;
            if (parentNow != null)
            {
                RemoveFromParentNow(parentNow, selectedNowRequirement);
            }
            else
            {
                var parentNowRequirement = selectedNowRequirement.Parent as NowRequirementTreeViewModel;
                if (parentNowRequirement != null)
                {
                    RemoveFromParentNowRequirement(parentNowRequirement, selectedNowRequirement);
                }
            }         

            //reset the copied item
            selectedNowRequirement.PendingCopyModel = null;
        }

        private void RemoveFromParentNowRequirement(NowRequirementTreeViewModel parent, NowRequirementTreeViewModel selectedNowRequirement)
        {
            //remove now requirement view model from its parent
            parent.NowRequirementViewModel.NowRequirements.RemoveAll(x => ReferenceEquals(x, selectedNowRequirement.NowRequirementViewModel));

            //remove the data from the Now Requirement
            parent.NowRequirementViewModel.NowRequirement.NowRequirements.RemoveAll(x => ReferenceEquals(x, selectedNowRequirement.NowRequirementViewModel.NowRequirement));

            //set the parent NOW Requirement as modified
            parent.NowRequirementViewModel.LastModifiedDate = DateTime.Now;

            DeleteAndCleanTree(parent, selectedNowRequirement);
        }

        private void RemoveFromParentNow(NowTreeViewModel parent, NowRequirementTreeViewModel selectedNowRequirement)
        {
            //remove now requirement view model from its parent
            parent.NowViewModel.NowRequirements.RemoveAll(x => ReferenceEquals(x, selectedNowRequirement.NowRequirementViewModel));

            //remove the data from the Now
            parent.NowViewModel.Now.NowRequirements.RemoveAll(x => ReferenceEquals(x, selectedNowRequirement.NowRequirementViewModel.NowRequirement));

            //set the parent NOW as modified
            parent.NowViewModel.LastModifiedDate = DateTime.Now;

            DeleteAndCleanTree(parent, selectedNowRequirement);

            //tidy up the child result definitions
            //qwerty
        }

        private void DeleteAndCleanTree(TreeViewItemViewModel parent, NowRequirementTreeViewModel selectedNowRequirement)
        {
            if (selectedNowRequirement.NowRequirementViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                treeModel.AllData.NowRequirements.RemoveAll(x => x.UUID == selectedNowRequirement.NowRequirementViewModel.UUID);
            }
            else
            {
                //logically delete the data in readiness for the next data patch
                selectedNowRequirement.NowRequirementViewModel.DeletedDate = DateTime.Now;
            }                

            //remove from the all view models
            treeModel.AllNowRequirementsViewModel.NowRequirements.Remove(selectedNowRequirement.NowRequirementViewModel);

            int index;
            //find the selected now requirement in the tree of children
            for (index = 0; index < parent.Children.Count; index++)
            {
                var nowReqChild = parent.Children[index] as NowRequirementTreeViewModel;
                if (ReferenceEquals(nowReqChild, selectedNowRequirement))
                {
                    //index is the location of the prompt for removal
                    parent.Children.RemoveAt(index);
                    break;
                }
            }

            //select the next node
            if (parent.Children.Count > 0)
            {
                if (parent.Children.Count - 1 <= index && index > 0)
                {
                    index = index - 1;
                }
                parent.Children[index].IsSelected = true;
            }
            else
            {
                parent.IsSelected = true;
            }
        }
    }
}
