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
    public interface IPasteResultDefinitionWordGroupCommand : ICommand { }

    public class PasteResultDefinitionWordGroupCommand : IPasteResultDefinitionWordGroupCommand
    {
        private ITreeModel treeModel;
        public PasteResultDefinitionWordGroupCommand(ITreeModel treeModel)
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

            if (selectedResultDefinitionWordGroup == null)
            {
                //check if parent result definition selected as target of paste
                var parentResult = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
                if (parentResult != null)
                {
                    PasteTarget(parentResult);
                }
                else
                {
                    //sibling result word group selected as target of paste
                    PasteTarget(treeModel.SelectedItem as ResultWordGroupTreeViewModel);
                }                
            }
            else
            {
                //word group selected as target of paste
                PasteTarget((ResultWordGroupTreeViewModel)selectedResultDefinitionWordGroup.Parent);
            }

            //reset the copied item
            PendingCopyModel().PendingCopyModel = null;
        }

        private ResultDefinitionWordGroupTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as ResultDefinitionWordGroupTreeViewModel;
        }

        private void PasteTarget(ResultDefinitionTreeViewModel targetParent)
        {
            //determine if the targetParent has an existing word group as a child
            targetParent.PrepareChildren();
            var childRWGs = targetParent.Children.Where(x => x.GetType() == typeof(ResultWordGroupTreeViewModel)).Select(x=>x as ResultWordGroupTreeViewModel).ToList();

            if (childRWGs.Count>0)
            {
                //find the most appropriate word group for the target
                var multiWordChildRWGs = childRWGs.Where(x => x.ResultWordGroupViewModel.ResultDefinitionWordGroups != null && x.ResultWordGroupViewModel.ResultDefinitionWordGroups.Count > 1).ToList();
                if (multiWordChildRWGs.Count > 0)
                {
                    PasteTarget(multiWordChildRWGs.First());
                }
                else
                {
                    if (childRWGs.Count > 1)
                    {
                        //determine the first word group that is not a match to the short code
                        var nonShortCodeGroup = childRWGs.FirstOrDefault(
                                                                            x => x.ResultWordGroupViewModel.ResultDefinitionWordGroups != null &&
                                                                            x.ResultWordGroupViewModel.ResultDefinitionWordGroups.Count == 1 &&
                                                                            x.ResultWordGroupViewModel.ResultDefinitionWordGroups[0].ResultDefinitionWord != targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.ShortCode
                                                                        );
                        if (nonShortCodeGroup != null)
                        {
                            PasteTarget(nonShortCodeGroup);
                        }
                        else
                        {
                            PasteTarget(childRWGs.First());
                        }
                    }
                    else
                    {
                        PasteTarget(childRWGs.First());
                    }
                }
                return;
            }

            // new word groups are required ...

            // make a single word word group for the result short code
            MakeShortCodeWordGroup(targetParent);

            //make a new child result word group
            var data = new ResultWordGroup(targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition);
            var vm = new ResultWordGroupViewModel(data, treeModel.AllResultDefinitionWordGroupViewModel);
            vm.ParentResultDefinitionVM = targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel;
            var newChildRWG = new ResultWordGroupTreeViewModel(treeModel, vm, targetParent);

            //paste the source to the new child result word group
            PasteTarget(newChildRWG);

            //add the new child to the result definition tree view
            targetParent.Children.Add(newChildRWG);
        }

        private void MakeShortCodeWordGroup(ResultDefinitionTreeViewModel targetParent)
        {
            //make a new child result word group
            var rdWG = new ResultDefinitionWordGroup() { ResultDefinitionWord = targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.ShortCode, Synonyms = new List<ResultDefinitionWordSynonym>()};
            var rdWGVM = new ResultDefinitionWordGroupViewModel(treeModel, rdWG);

            treeModel.AllData.ResultDefinitionWordGroups.Add(rdWG);
            treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.Add(rdWGVM);

            var data = new ResultWordGroup(targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition);
            data.ResultDefinitionWordGroups.Add(rdWG);
            var vm = new ResultWordGroupViewModel(data, treeModel.AllResultDefinitionWordGroupViewModel);
            vm.ParentResultDefinitionVM = targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel;
            rdWGVM.ParentWordGroups.Add(vm);

            vm.ResultDefinitionWordGroups = new List<ResultDefinitionWordGroupViewModel>();
            vm.ResultDefinitionWordGroups.Add(rdWGVM);
            var newChildRWG = new ResultWordGroupTreeViewModel(treeModel, vm, targetParent);

            //add the new child to the result definition tree view
            targetParent.Children.Add(newChildRWG);

            //add the new child data to the result definition
            if (vm.ParentResultDefinitionVM.WordGroups == null) { vm.ParentResultDefinitionVM.WordGroups = new List<ResultWordGroupViewModel>(); }
            vm.ParentResultDefinitionVM.WordGroups.Add(vm);
            if (vm.ParentResultDefinitionVM.ResultDefinition.WordGroups == null) { vm.ParentResultDefinitionVM.ResultDefinition.WordGroups = new List<ResultWordGroup>(); }
            vm.ParentResultDefinitionVM.ResultDefinition.WordGroups.Add(data);

            //expand the new child
            newChildRWG.IsExpanded = true;
        }

        private void PasteTarget(ResultWordGroupTreeViewModel targetParent)
        {
            //compare the target parent with the source parent to determine if they are siblings
            var sourceParent = PendingCopyModel().Parent as ResultWordGroupTreeViewModel;
            if (sourceParent == null) { return; }

            if (targetParent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinition.UUID == sourceParent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinition.UUID)
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

        private void CloneSourceItem(ResultWordGroupTreeViewModel targetParent, ResultDefinitionWordGroupTreeViewModel target=null)
        {
            //determine if the target parent already has the source item requested to be copied
            var child = targetParent.ResultWordGroupViewModel.ResultDefinitionWordGroups == null ? null : targetParent.ResultWordGroupViewModel.ResultDefinitionWordGroups.FirstOrDefault(x => x.UUID == PendingCopyModel().ResultDefinitionWordGroupViewModel.UUID);

            if (child != null)
            {
                //deep copy is required for the target parent
                CopySourceItem(targetParent,target);
                return;
            }

            if (targetParent.HasDummyChild)
            {
                SetVMsAndData(targetParent);
                return;
            }

            //Clone to make a shallow copy for new tree item
            var newItem = PendingCopyModel().Clone();
            PasteNewItem(targetParent, target, newItem);
        }

        private void SetVMsAndData(ResultWordGroupTreeViewModel targetParent)
        {
            //simply update the data and the view models and expand the target so that the new children are loaded
            if (targetParent.ResultWordGroupViewModel.ResultDefinitionWordGroups == null)
            {
                //VMs
                targetParent.ResultWordGroupViewModel.ResultDefinitionWordGroups = new List<ResultDefinitionWordGroupViewModel>();
                targetParent.ResultWordGroupViewModel.ResultDefinitionWordGroups.Add(PendingCopyModel().ResultDefinitionWordGroupViewModel);
            }

            //data
            if (targetParent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups == null)
            {
                targetParent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups = new List<ResultDefinitionWordGroup>();
            }
            targetParent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups.Add(PendingCopyModel().ResultDefinitionWordGroupViewModel.ResultDefinitionWordGroup);
            if (targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM != null) 
            { 
                targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.LastModifiedDate = DateTime.Now;

                //add the new child data to the result definition
                if (targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.WordGroups == null) { targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.WordGroups = new List<ResultWordGroupViewModel>(); }
                targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.WordGroups.Add(targetParent.ResultWordGroupViewModel);
                if (targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.ResultDefinition.WordGroups == null) { targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.ResultDefinition.WordGroups = new List<ResultWordGroup>(); }
                targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.ResultDefinition.WordGroups.Add(targetParent.ResultWordGroupViewModel.ResultWordGroup);
            }

            targetParent.IsExpanded = true;

            //find the child that matches the source item
            var wordGroupChildren = targetParent.Children.Where(x => x.GetType() == typeof(ResultDefinitionWordGroupTreeViewModel)).Select(x => x as ResultDefinitionWordGroupTreeViewModel).ToList();
            if (wordGroupChildren.Count > 0)
            {
                var matchingChild = wordGroupChildren.FirstOrDefault(x => x.ResultDefinitionWordGroupViewModel.UUID == PendingCopyModel().ResultDefinitionWordGroupViewModel.UUID);
                if (matchingChild != null)
                {
                    matchingChild.IsSelected = true;
                    return;
                }
            }
            targetParent.Children[0].IsSelected = true;
        }

        private void CopySourceItem(ResultWordGroupTreeViewModel targetParent, ResultDefinitionWordGroupTreeViewModel target =null)
        {
            //Copy and make a deep copy for a new item 
            var newItem = PendingCopyModel().Copy();

            //ensure that label is unique
            var duplicateLabel = false;
            int i = 2;
            {
                duplicateLabel = treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.FirstOrDefault(x => x.ResultDefinitionWord.ToLowerInvariant() == newItem.ResultDefinitionWordGroupViewModel.ResultDefinitionWord.ToLowerInvariant()) != null;
                if (duplicateLabel)
                {
                    i++;
                    newItem.ResultDefinitionWordGroupViewModel.ResultDefinitionWord = string.Format("{0} ({1})", newItem.ResultDefinitionWordGroupViewModel.ResultDefinitionWord, i);
                }

            } while (duplicateLabel) ;

            PasteNewItem(targetParent, target, newItem);

            //store the new data objects that have been created as part of the copy
            treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.Add(newItem.ResultDefinitionWordGroupViewModel);
            treeModel.AllData.ResultDefinitionWordGroups.Add(newItem.ResultDefinitionWordGroupViewModel.ResultDefinitionWordGroup);
        }

        private void PasteNewItem(ResultWordGroupTreeViewModel targetParent, ResultDefinitionWordGroupTreeViewModel target, ResultDefinitionWordGroupTreeViewModel toPaste)
        {
            //ensure correct parentage for the new item being pasted
            toPaste.ResetParentage(targetParent);

            //find the index of the selected node to determine where the new item should be inserted
            var index = target != null ? targetParent.Children.IndexOf(target) : -1;

            //add the word group and move it to the selected index
            if (targetParent.ResultWordGroupViewModel.ResultDefinitionWordGroups == null) { targetParent.ResultWordGroupViewModel.ResultDefinitionWordGroups = new List<ResultDefinitionWordGroupViewModel>(); }
            targetParent.ResultWordGroupViewModel.ResultDefinitionWordGroups.Add(toPaste.ResultDefinitionWordGroupViewModel);
            toPaste.ResultDefinitionWordGroupViewModel.ParentWordGroups.Add(targetParent.ResultWordGroupViewModel);
            if (targetParent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups == null) { targetParent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups = new List<ResultDefinitionWordGroup>(); }
            targetParent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups.Add(toPaste.ResultDefinitionWordGroupViewModel.ResultDefinitionWordGroup);
            if (targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM != null) { targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.LastModifiedDate = DateTime.Now; }

            targetParent.Children.Add(toPaste);
            if (index >= 0)
            {
                targetParent.Children.Move(targetParent.Children.Count - 1, index);
            }
            else
            {
                targetParent.Children.OrderBy(x => x.GetType() == typeof(ResultPromptTreeViewModel)).ThenBy(x => x.GetType() == typeof(ResultDefinitionWordGroupTreeViewModel));
            }

            //ensure that the pasted item is selected and the parent is expanded
            targetParent.IsExpanded = true;
            toPaste.IsSelected = true;

            //inform the result definition that there is a new word group so that it may determine if the short code needs to be edittable
            targetParent.ResultWordGroupViewModel.ParentResultDefinitionVM.SetWordGroupsChanged();
        }
    }
}
