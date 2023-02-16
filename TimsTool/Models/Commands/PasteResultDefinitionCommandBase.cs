using DataLib;
using Models.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public abstract class PasteResultDefinitionCommandBase : ICommand
    {
        private ITreeModel treeModel;
        public PasteResultDefinitionCommandBase(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

        protected ResultDefinitionTreeViewModel SelectedResultDefinition;
        protected NowSubscriptionTreeViewModel SelectedNowSubscription;
        protected bool IsExcludedResults;
        protected bool IsIncludedResults;

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
            Log.Information("Start paste of result definition");
            //pasting to a Now Subscription?
            SelectedNowSubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            if (SelectedNowSubscription != null)
            {
                Log.Information(string.Format("Selected Now Subscription is {0}", SelectedNowSubscription.Label));

                //set included/excluded based on command parameter
                if (parameter == null) { return; }
                var includedExcluded = ((string)parameter).ToLowerInvariant();
                if (includedExcluded == "excluded")
                {
                    Log.Information("Pasting to excluded results");
                    IsExcludedResults = true;
                    IsIncludedResults = false;
                }
                else
                {
                    Log.Information("Pasting to included results");
                    IsIncludedResults = true;
                    IsExcludedResults = false;
                }

                SelectedResultDefinition = null;
            }
            else
            {
                //Determine the selected item
                SelectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

                SelectedNowSubscription = null;
                IsExcludedResults = false;
                IsIncludedResults = false;
            }

            Execute();
        }

        protected abstract void Execute();

        private ResultDefinitionTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as ResultDefinitionTreeViewModel;
        }

        protected void AddNewItemToTree(TreeViewItemViewModel targetParent, NowRequirementTreeViewModel newItem)
        {
            //add the now requirement to the tree
            if (targetParent is NowTreeViewModel)
            {
                var nowTRM = targetParent as NowTreeViewModel;
                if (nowTRM.NowViewModel.NowRequirements == null) { nowTRM.NowViewModel.NowRequirements = new List<NowRequirementViewModel>(); }
                nowTRM.NowViewModel.NowRequirements.Add(newItem.NowRequirementViewModel);
                if (nowTRM.NowViewModel.Now.NowRequirements == null) { nowTRM.NowViewModel.Now.NowRequirements = new List<NowRequirement>(); }
                nowTRM.NowViewModel.Now.NowRequirements.Add(newItem.NowRequirementViewModel.NowRequirement);
                nowTRM.NowViewModel.LastModifiedDate = DateTime.Now;
            }

            if (targetParent is NowRequirementTreeViewModel)
            {
                var nowReqTRM = targetParent as NowRequirementTreeViewModel;
                if (nowReqTRM.NowRequirementViewModel.NowRequirements == null) { nowReqTRM.NowRequirementViewModel.NowRequirements = new List<NowRequirementViewModel>(); }
                nowReqTRM.NowRequirementViewModel.NowRequirements.Add(newItem.NowRequirementViewModel);
                if (nowReqTRM.NowRequirementViewModel.NowRequirement.NowRequirements == null) { nowReqTRM.NowRequirementViewModel.NowRequirement.NowRequirements = new List<NowRequirement>(); }
                nowReqTRM.NowRequirementViewModel.NowRequirement.NowRequirements.Add(newItem.NowRequirementViewModel.NowRequirement);
                nowReqTRM.NowRequirementViewModel.LastModifiedDate = DateTime.Now;
            }

            if (targetParent.HasDummyChild)
            {
                AppendTreeSettingSequence(targetParent, newItem, false);
                
            }
            else
            {
                AppendTreeSettingSequence(targetParent, newItem, true);
            }

            //expand the parent
            targetParent.IsExpanded = true;

            //ensure that the pasted item is selected            
            newItem.IsSelected = true;

        }

        private void AppendTreeSettingSequence(TreeViewItemViewModel parentItem, NowRequirementTreeViewModel newItem, bool withAddChild)
        {
            if (newItem.NowRequirementViewModel.ResultDefinitionSequence == null)
            {
                int? lastSeq = 0;
                foreach (var item in parentItem.Children)
                {
                    if (item is NowRequirementTreeViewModel)
                    {
                        var nrItem = item as NowRequirementTreeViewModel;
                        lastSeq = nrItem.NowRequirementViewModel.ResultDefinitionSequence;
                    }
                }
                newItem.NowRequirementViewModel.ResultDefinitionSequence = lastSeq + 100;
            }
            
            if (withAddChild)
            {
                parentItem.Children.Add(newItem);
            }            
        }        

        private NowRequirementViewModel GetExistingNowRequirement(Guid? rdId)
        {
            //first see if we have a now requirement view model as the parent of the source of the copy
            var parentCopy = PendingCopyModel().Parent != null ? PendingCopyModel().Parent as NowRequirementTreeViewModel : null;
            if (parentCopy != null)
            {
                return parentCopy.NowRequirementViewModel;
            }
            var active = treeModel.AllNowRequirementsViewModel.NowRequirements.FirstOrDefault(x => x.ResultDefinition.UUID == rdId && x.DeletedDate == null);
            if (active != null)
            {
                return active;
            }
            return treeModel.AllNowRequirementsViewModel.NowRequirements.FirstOrDefault(x => x.ResultDefinition.UUID == rdId);
        }

        private int CalculateChildSequence(List<NowRequirementViewModel> parentNrs, ResultDefinitionViewModel rdVM)
        {
            if (parentNrs == null || !parentNrs.Any())
            {
                return 100;
            }
            var beforeItem = parentNrs.OrderBy(x => x.ResultDefinition.Rank).FirstOrDefault(x => x.ResultDefinition.Rank > rdVM.Rank);
            if (beforeItem == null)
            {
                beforeItem = parentNrs.Last();
            }
            return beforeItem.ResultDefinitionSequence.Value - 5;
        }

        protected NowRequirementTreeViewModel MakeNowRequirementModel(ResultDefinitionViewModel rdVM, NowViewModel now, TreeViewItemViewModel targetParent, NowRequirementViewModel targetParentNowRequirementViewModel)
        {
            //determine if there is an existing now requirement that could be cloned
            var existingNR = GetExistingNowRequirement(rdVM.UUID);            

            // Make a new data item
            NowRequirement data = null;
            NowRequirementViewModel nrVM = null;
            if (existingNR == null)
            {
                //make a new data item
                var parentNr = targetParentNowRequirementViewModel == null ? null : targetParentNowRequirementViewModel.NowRequirement;
                var rootNr = targetParentNowRequirementViewModel == null ? null : (targetParentNowRequirementViewModel.RootParentNowRequirement == null ? targetParentNowRequirementViewModel.NowRequirement : targetParentNowRequirementViewModel.RootParentNowRequirement);
                data = new NowRequirement(rdVM.ResultDefinition, now.Now, parentNr, rootNr);
                data.ResultDefinitionSequence = CalculateChildSequence(targetParentNowRequirementViewModel != null ? targetParentNowRequirementViewModel.NowRequirements : now.NowRequirements, rdVM);
                nrVM = new NowRequirementViewModel(treeModel, data, rdVM, now, treeModel.AllResultDefinitionsViewModel);

                //create child requirement view models
                nrVM.SetNowRequirements();
            }
            else
            {
                nrVM = existingNR.Copy(now, targetParentNowRequirementViewModel == null ? null : targetParentNowRequirementViewModel.NowRequirement);
                data = nrVM.NowRequirement;
            }

            //set parentage on the data
            if (data.ParentNowRequirement == null)
            {
                data.RootParentNowRequirement = null;
            }
            else
            {
                data.RootParentNowRequirement = data.ParentNowRequirement.ParentNowRequirement == null ? data.ParentNowRequirement : data.ParentNowRequirement.RootParentNowRequirement;
            }            

            //store the data objects, qwerty - needs all items in the hierachy?
            StoreData(nrVM);

            //make a new tree item for the new now requirement
            var nrtvVM = new NowRequirementTreeViewModel(treeModel, nrVM, targetParent);

            //when necessary copy the prompt rule settings from the existing now requirement
            if (existingNR != null && existingNR.NowRequirementPromptRules != null && nrVM.NowRequirementPromptRules != null)
            {
                foreach (var promptRuleVM in existingNR.NowRequirementPromptRules.Where(x => x.DeletedDate == null && x.ResultPromptRule.DeletedDate == null && x.ResultPromptRule.ResultPromptViewModel.DeletedDate == null))
                {
                    //match to the rule on the view model
                    var matchedRule = nrVM.NowRequirementPromptRules.FirstOrDefault(x => x.ResultPromptRule.ResultPromptViewModel.UUID == promptRuleVM.ResultPromptRule.ResultPromptViewModel.UUID);

                    if (matchedRule == null)
                    {
                        matchedRule.DistinctPromptTypes = promptRuleVM.DistinctPromptTypes;
                        matchedRule.IsVariantData = promptRuleVM.IsVariantData;
                    }
                }
            }

            return nrtvVM;
        }

        private void StoreData(NowRequirementViewModel nrVM)
        {
            treeModel.AllNowRequirementsViewModel.NowRequirements.Add(nrVM);
            treeModel.AllData.NowRequirements.Add(nrVM.NowRequirement);

            //recursively store the child now requirements
            RecursivelyStoreData(nrVM.NowRequirements);
        }

        private void RecursivelyStoreData(List<NowRequirementViewModel> nowRequirements)
        {
            if (nowRequirements != null)
            {
                foreach(var nr in nowRequirements)
                {
                    var matchVM = treeModel.AllNowRequirementsViewModel.NowRequirements.FirstOrDefault(x => x.UUID == nr.UUID);
                    if (matchVM == null)
                    {
                        treeModel.AllNowRequirementsViewModel.NowRequirements.Add(nr);
                    }

                    var matchData = treeModel.AllData.NowRequirements.FirstOrDefault(x => x.UUID == nr.NowRequirement.UUID);
                    if (matchData == null)
                    {
                        treeModel.AllData.NowRequirements.Add(nr.NowRequirement);
                    }

                    RecursivelyStoreData(nr.NowRequirements);
                }
            }
        }
    }
}
