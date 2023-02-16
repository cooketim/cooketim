using Models.ViewModels;
using System;
using System.Windows.Input;
using System.Linq;
using DataLib;
using System.Collections.Generic;

namespace Models.Commands
{
    public interface IUndeleteResultPromptCommand : ICommand { }
    public class UndeleteResultPromptCommand : IUndeleteResultPromptCommand
    {
        private ITreeModel treeModel;
        public UndeleteResultPromptCommand(ITreeModel treeModel)
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
            var selectedResultPrompt = treeModel.SelectedItem as ResultPromptTreeViewModel;
            if (selectedResultPrompt == null) { return; }

            //get the parent
            var parent = selectedResultPrompt.Parent as ResultDefinitionTreeViewModel;
            if (parent == null) { return; }

            //reinstate the data
            selectedResultPrompt.ResultPromptRuleViewModel.DeletedDate = null;
            selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.DeletedDate = null;

            //Set the parent as modified
            parent.ResultRuleViewModel.ChildResultDefinitionViewModel.LastModifiedDate = DateTime.Now;

            //Synchronise the Nows
            SynchroniseNows(selectedResultPrompt);

            //select the parent
            parent.IsSelected = true;
        }

        private void SynchroniseNows(ResultPromptTreeViewModel selectedResultPrompt)
        {
            //Find any now Requirements for the parent result definition
            var matchedNowRequirements = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(x => x.ResultDefinition.UUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptRule.ResultDefinitionUUID);

            //When drafting mode then only deal with those requirements that are status draft
            matchedNowRequirements.RemoveAll(x => x.PublishedStatus != PublishedStatus.Draft);

            foreach (var nr in matchedNowRequirements)
            {
                //undelete the data
                nr.DeletedDate = null;

                //get the now or edt tree item
                var matchedNow = MatchNOWEDT(nr);
                if (matchedNow != null)
                {
                    //recursively match the now requirement and reset the prompt
                    RecursivelyMatchNowRequirement(matchedNow.Children, selectedResultPrompt);
                    matchedNow.NowViewModel.LastModifiedDate = DateTime.Now;
                }
            }
        }

        private NowTreeViewModel MatchNOWEDT(NowRequirementViewModel nr)
        {
            var match = treeModel.Nows.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
            if (match != null) { return match; }
            return treeModel.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
        }

        private void RecursivelyMatchNowRequirement(SilentObservableCollection<TreeViewItemViewModel> children, ResultPromptTreeViewModel selectedResultPrompt)
        {
            foreach (var child in children)
            {
                if (child is NowRequirementTreeViewModel)
                {
                    var nrItem = child as NowRequirementTreeViewModel;
                    if (nrItem.NowRequirementViewModel.ResultDefinition.UUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptRule.ResultDefinitionUUID)
                    {
                        //create a new result prompt rule
                        SetNowRequirementPromptRule(nrItem, selectedResultPrompt);
                    }
                    RecursivelyMatchNowRequirement(nrItem.Children, selectedResultPrompt);
                }
            }
        }

        private void SetNowRequirementPromptRule(NowRequirementTreeViewModel nrItem, ResultPromptTreeViewModel selectedResultPrompt)
        {
            var index = 0;
            var lastPromptIndex = 0;
            var lastResultDefinitionIndex = 0;
            foreach(var child in nrItem.Children)
            {
                var promptChild = child as NowRequirementPromptRuleTreeViewModel;
                if (promptChild != null)
                {
                    lastPromptIndex = index;
                    if (promptChild.NowRequirementPromptRuleViewModel.ResultPromptRule.ResultPromptViewModel.UUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptRule.ResultPromptUUID)
                    {
                        //already present
                        promptChild.NowRequirementPromptRuleViewModel.DeletedDate = null;
                        return;
                    }
                }
                if (child is ResultDefinitionTreeViewModel)
                {
                    lastResultDefinitionIndex = index;
                }
                index++;
            }

            //not found add a new treeitem
            var newItem = MakeNowRequirementPromptRuleModel(selectedResultPrompt.ResultPromptRuleViewModel, nrItem);
            if (lastPromptIndex > 0)
            {
                nrItem.Children.Insert(lastPromptIndex+1, newItem);
            }
            else
            {
                nrItem.Children.Insert(lastResultDefinitionIndex+1, newItem);
            }            

            //set the now requirement as modified
            nrItem.NowRequirementViewModel.LastModifiedDate = DateTime.Now;
        }

        private NowRequirementPromptRuleTreeViewModel MakeNowRequirementPromptRuleModel(ResultPromptRuleViewModel promptRuleVM, NowRequirementTreeViewModel targetParent)
        {
            //determine if there is already a now requirement prompt rule vm for the now requirement
            if (targetParent.NowRequirementViewModel.NowRequirementPromptRules != null)
            {
                var matchedNRPRVM = targetParent.NowRequirementViewModel.NowRequirementPromptRules.FirstOrDefault(x => x.ResultPromptRule.UUID == promptRuleVM.UUID);
                if (matchedNRPRVM != null)
                {
                    matchedNRPRVM.DeletedDate = null;
                    return new NowRequirementPromptRuleTreeViewModel(treeModel, matchedNRPRVM, targetParent);
                }
            }

            //determine if there is already a now requirement prompt rule data item for the now requirement
            NowRequirementPromptRuleViewModel dataVM = null;
            if (targetParent.NowRequirementViewModel.NowRequirement.NowRequirementPromptRules != null)
            {
                
                dataVM = targetParent.NowRequirementViewModel.NowRequirementPromptRules.FirstOrDefault(x => x.ResultPromptRule.UUID == promptRuleVM.UUID);
                if (dataVM != null)
                {
                    dataVM.DeletedDate = null;
                }
            }

            //make a new data item when required
            if (dataVM == null)
            {
                var data = new NowRequirementPromptRule(promptRuleVM.ResultPromptRule, targetParent.NowRequirementViewModel.NowRequirement);
                if (targetParent.NowRequirementViewModel.NowRequirement.NowRequirementPromptRules == null)
                {
                    targetParent.NowRequirementViewModel.NowRequirement.NowRequirementPromptRules = new List<NowRequirementPromptRule>();                    
                }
                targetParent.NowRequirementViewModel.NowRequirement.NowRequirementPromptRules.Add(data);

                //make a new VM
                dataVM = new NowRequirementPromptRuleViewModel(data, promptRuleVM, targetParent.NowRequirementViewModel, treeModel);
                if (targetParent.NowRequirementViewModel.NowRequirementPromptRules == null)
                {
                    targetParent.NowRequirementViewModel.NowRequirementPromptRules = new List<NowRequirementPromptRuleViewModel>();
                }
                targetParent.NowRequirementViewModel.NowRequirementPromptRules.Add(dataVM);
            }

            //make a new tree item for the now requirement prompt rule
            return new NowRequirementPromptRuleTreeViewModel(treeModel, dataVM, targetParent);
        }
    }
}
