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
    public interface IPasteResultDefinitionChildCommand : ICommand { }

    public class PasteResultDefinitionChildCommand : PasteResultDefinitionCommand, IPasteResultDefinitionChildCommand
    {
        private ITreeModel treeModel;
        public PasteResultDefinitionChildCommand(ITreeModel treeModel) : base(treeModel)
        {
            this.treeModel = treeModel;
        }

        protected override void Execute()
        {
            if (SelectedResultDefinition == null)
            {
                return;
            }

            //result definition selected as target of paste
            PasteTarget(SelectedResultDefinition);

            //reset the copied item
            PendingCopyModel().PendingCopyModel = null;
        }

        private ResultDefinitionTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as ResultDefinitionTreeViewModel;
        }

        private void PasteTarget(ResultDefinitionTreeViewModel targetParent)
        {
            //determine if the target already has this result definition
            ResultDefinitionTreeViewModel newItem = null;
            var existingChild = targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules == null ? null : targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.FirstOrDefault(x => x.ChildResultDefinitionViewModel.MasterUUID == PendingCopyModel().ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
            if (existingChild == null)
            {
                //make a shallow copy for the target parent
                newItem = CloneSourceItem(targetParent);
            }
            else
            {
                //make a deep copy as the item already exists for this parent
                newItem = CopySourceItem(targetParent);
            }

            //deal with any related NOWs
            SynchroniseNows(targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID, newItem.ResultRuleViewModel.ChildResultDefinitionViewModel);
        }

        private void SynchroniseNows(Guid? parentRdId, ResultDefinitionViewModel newChildRd)
        {
            //Find any now Requirements for the parent, only dealing with draft requirements when in draft mode
            List<IGrouping<Guid?,NowRequirementViewModel>> matchedNowRequirementsGroup = null;
            matchedNowRequirementsGroup = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(x => x.PublishedStatus == PublishedStatus.Draft && x.ResultDefinition.UUID == parentRdId).GroupBy(x => x.NOWUUID).ToList();

            foreach (var group in matchedNowRequirementsGroup)
            {
                //get the now or edt tree item
                var matchedNow = MatchNOWEDT(group.First());
                if (matchedNow == null) { continue; }

                matchedNow.NowViewModel.LastModifiedDate = DateTime.Now;

                foreach (var matchedNR in group.ToList())
                {
                    RecursivelyMatchNowRequirementTV(matchedNow.Children, matchedNR, newChildRd);
                }
            }
        }

        private void RecursivelyMatchNowRequirementTV(SilentObservableCollection<TreeViewItemViewModel> children, NowRequirementViewModel nrToMatch, ResultDefinitionViewModel newChildRd)
        {            
            foreach (var child in children)
            {
                if (child is NowRequirementTreeViewModel)
                {
                    var nrItem = child as NowRequirementTreeViewModel;
                    if (nrItem.NowRequirementViewModel.UUID == nrToMatch.UUID)
                    {
                        //make a new child  requirement
                        var newItem = MakeNowRequirementModel(newChildRd, nrItem.NowRequirementViewModel.NowEdtVM, nrItem, nrItem.NowRequirementViewModel);

                        AddNewItemToTree(child, newItem);
                        break;
                    }

                    RecursivelyMatchNowRequirementTV(child.Children, nrToMatch, newChildRd);
                }                
            }
        }

        private NowTreeViewModel MatchNOWEDT(NowRequirementViewModel nr)
        {
            var match = treeModel.Nows.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
            if (match != null) { return match; }
            return treeModel.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
        }

        private ResultDefinitionTreeViewModel CloneSourceItem(ResultDefinitionTreeViewModel targetParent)
        {
            //Clone to make a shallow copy for new tree item            
            var newItem = CloneChild(targetParent, PendingCopyModel());

            //treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Add(newItem);

            return newItem;
        }

        private ResultDefinitionTreeViewModel CloneChild(ResultDefinitionTreeViewModel targetParent, ResultDefinitionTreeViewModel source)
        {
            var newItem = source.Clone();

            //make a child rule
            var newRule = new ResultDefinitionRule
                (
                    targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID,
                    newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition,
                    ResultDefinitionRuleType.atleastOneOf
                );            

            newItem.ResultRuleViewModel = new ResultRuleViewModel(treeModel, newItem.ResultRuleViewModel.ChildResultDefinitionViewModel, targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel, newRule);

            if (targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                newItem.ResultRuleViewModel.PublishedStatus = PublishedStatus.Draft;
            }

            PasteNewItem(targetParent, newItem);

            return newItem;
        }

        private ResultDefinitionTreeViewModel CopySourceItem(ResultDefinitionTreeViewModel targetParent)
        {
            //Copy and make a deep copy for a new item
            ResultDefinitionTreeViewModel newItem = null;
            if (targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                newItem = PendingCopyModel().CopyAsDraft();
            }
            else
            {
                newItem = PendingCopyModel().Copy();
            }

            if (targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                newItem.ResultRuleViewModel.PublishedStatus = PublishedStatus.Draft;
            }

            if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank != null && newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank > 0)
            {
                newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank = newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank - 1;
            }            

            //store the view models that have been created as part of the copy
            SetData(newItem);

            //Now clone the tree item as a child i.e. create a separate instance of the root that has just been created and paste as a child to the target
            var childItem = CloneChild(targetParent, newItem);
            return childItem;
        }

        private void PasteNewItem(ResultDefinitionTreeViewModel targetParent, ResultDefinitionTreeViewModel toPasteResultDefinition)
        {
            //ensure parentage is null
            toPasteResultDefinition.ResetParentage(targetParent);

            //add the new item to the parent and to the all definitions collection
            if (targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules == null) { targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules = new List<ResultRuleViewModel>(); }

            //make a new rule for the child result definition, setting the parentage
            targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Add(toPasteResultDefinition.ResultRuleViewModel);
            if (targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules == null) { targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules = new List<ResultDefinitionRule>(); }
            targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.Add(toPasteResultDefinition.ResultRuleViewModel.ResultDefinitionRule);
            targetParent.ResultRuleViewModel.ChildResultDefinitionViewModel.LastModifiedDate = DateTime.Now;

            //sort out the data store objects
            treeModel.AllResultRuleViewModel.Rules.Add(toPasteResultDefinition.ResultRuleViewModel);
            treeModel.AllData.ResultDefinitionRules.Add(toPasteResultDefinition.ResultRuleViewModel.ResultDefinitionRule);

            //Add to the tree after the last result definition
            AddItemToTreeInSequence(targetParent, toPasteResultDefinition);

            //Set the parent expanded and as a group
            toPasteResultDefinition.IsSelected = true;
            targetParent.IsExpanded = true;

            //trigger a change in parentage on the parent resut definition view model
            targetParent.ResultRuleViewModel.ResetStatus();
        }

        private void AddItemToTreeInSequence(ResultDefinitionTreeViewModel parentItem, ResultDefinitionTreeViewModel newItem)
        {
            var index = 0;
            var lastRdItemIndex = 0;
            ResultDefinitionTreeViewModel lastRdItem = null;
            foreach (var item in parentItem.Children)
            {
                if (item is ResultDefinitionTreeViewModel)
                {
                    lastRdItem = item as ResultDefinitionTreeViewModel;
                    lastRdItemIndex = index;
                }

                index++;
            }
            
            if (lastRdItem != null)
            {
                newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank = lastRdItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank + 100;
                parentItem.Children.Insert(lastRdItemIndex+1, newItem);
                return;
            }

            //Add as last child
            newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank = 100;
            parentItem.Children.Add(newItem);
        }
    }
}
