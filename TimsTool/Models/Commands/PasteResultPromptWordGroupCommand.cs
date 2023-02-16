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
    public interface IPasteResultPromptWordGroupCommand : ICommand { }

    public class PasteResultPromptWordGroupCommand : IPasteResultPromptWordGroupCommand
    {
        private ITreeModel treeModel;
        public PasteResultPromptWordGroupCommand(ITreeModel treeModel)
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

            if (selectedResultPromptWordGroup == null)
            {
                //parent result prompt selected as target of paste
                PasteTarget(treeModel.SelectedItem as ResultPromptTreeViewModel);
            }
            else
            {
                //result prompt selected as target of paste
                PasteTarget((ResultPromptTreeViewModel)selectedResultPromptWordGroup.Parent, selectedResultPromptWordGroup);
            }           

            //reset the copied item
            PendingCopyModel().PendingCopyModel = null;
        }

        private ResultPromptWordGroupTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as ResultPromptWordGroupTreeViewModel;
        }

        private void PasteTarget(ResultPromptTreeViewModel targetParent, ResultPromptWordGroupTreeViewModel target =null)
        {
            //compare the target parent with the source parent to determine if they are siblings
            var sourceParent = PendingCopyModel().Parent as ResultPromptTreeViewModel;
            if (sourceParent == null) { return; }

            if (targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.UUID == sourceParent.ResultPromptRuleViewModel.ResultPromptViewModel.UUID)
            {
                //target and source are siblings, make a deep copy for the same parent
                CopySourceItem(targetParent);
            }
            else
            {
                //make a shallow clone for a different parent
                CloneSourceItem(targetParent);
            }
        }

        private void CloneSourceItem(ResultPromptTreeViewModel targetParent, ResultPromptWordGroupTreeViewModel target=null)
        {
            //determine if the target parent already has the source item requested to be copied
            var child = targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups == null ? null : targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups.FirstOrDefault(x => x.UUID == PendingCopyModel().ResultPromptWordGroupViewModel.UUID);

            if (child != null)
            {
                //deep copy is required for the target parent
                CopySourceItem(targetParent,target);
                return;
            }

            //Clone to make a shallow copy for new tree item
            var newItem = PendingCopyModel().Clone();

            PaseteNewItem(targetParent, target, newItem);
        }

        private void CopySourceItem(ResultPromptTreeViewModel targetParent, ResultPromptWordGroupTreeViewModel target =null)
        {
            //Copy and make a deep copy for a new item 
            var newItem = PendingCopyModel().Copy();

            //ensure that label is unique
            var duplicateLabel = false;
            int i = 2;
            {
                duplicateLabel = treeModel.AllResultPromptWordGroupViewModel.WordGroups.FirstOrDefault(x => x.ResultPromptWord.ToLowerInvariant() == newItem.ResultPromptWordGroupViewModel.ResultPromptWord.ToLowerInvariant()) != null;
                if (duplicateLabel)
                {
                    i++;
                    newItem.ResultPromptWordGroupViewModel.ResultPromptWord = string.Format("{0} ({1})", newItem.ResultPromptWordGroupViewModel.ResultPromptWord, i);
                }

            } while (duplicateLabel) ;

            PaseteNewItem(targetParent, target, newItem);

            //store the new data objects that have been created as part of the copy
            treeModel.AllResultPromptWordGroupViewModel.WordGroups.Add(newItem.ResultPromptWordGroupViewModel);
            treeModel.AllData.ResultPromptWordGroups.Add(newItem.ResultPromptWordGroupViewModel.ResultPromptWordGroup);
        }

        private void PaseteNewItem(ResultPromptTreeViewModel targetParent, ResultPromptWordGroupTreeViewModel target, ResultPromptWordGroupTreeViewModel toPaste)
        {
            //ensure correct parentage for the new item being pasted
            toPaste.ResetParentage(targetParent);

            //set the parentage on the VM
            toPaste.ResultPromptWordGroupViewModel.ParentPrompts.Add(targetParent.ResultPromptRuleViewModel.ResultPromptViewModel);

            //find the index of the selected node to determine where the new item should be inserted
            var index = target != null ? targetParent.Children.IndexOf(target) : -1;

            //add the word group and move it to the selected index
            if (targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups == null) { targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups = new List<ResultPromptWordGroupViewModel>(); }
            targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups.Add(toPaste.ResultPromptWordGroupViewModel);
            
            if (targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.ResultPrompt.ResultPromptWordGroups == null) { targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.ResultPrompt.ResultPromptWordGroups = new List<ResultPromptWordGroup>(); }
            targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.ResultPrompt.ResultPromptWordGroups.Add(toPaste.ResultPromptWordGroupViewModel.ResultPromptWordGroup);
            targetParent.ResultPromptRuleViewModel.ResultPromptViewModel.LastModifiedDate = DateTime.Now;

            if (!targetParent.HasDummyChild)
            {
                targetParent.Children.Add(toPaste);
                if (index >= 0)
                {
                    targetParent.Children.Move(targetParent.Children.Count - 1, index);
                }
                else
                {
                    targetParent.Children.OrderBy(x => x.GetType() == typeof(ResultPromptTreeViewModel)).ThenBy(x => x.GetType() == typeof(ResultPromptWordGroupTreeViewModel));
                }
            }           

            //ensure that the pasted item is selected and the parent is expanded
            targetParent.IsExpanded = true;
            toPaste.IsSelected = true;
        }
    }
}
