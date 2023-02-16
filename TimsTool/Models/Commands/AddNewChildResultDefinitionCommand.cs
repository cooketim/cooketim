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
    public interface IAddNewChildResultDefinitionCommand : ICommand { }
    public class AddNewChildResultDefinitionCommand : AddNewResultDefinitionCommand, IAddNewChildResultDefinitionCommand
    {
        private ITreeModel treeModel;
        public AddNewChildResultDefinitionCommand(ITreeModel treeModel) : base(treeModel)
        {
            this.treeModel = treeModel;
        }
        
        protected override void Execute(PublicationTagsModel ptm)
        { 
            //make a new child item
            var newChildItem = MakeAndStoreChildResultDefinitionTreeViewModel();

            //reset any copied item
            newChildItem.PendingCopyModel = null;

            //select the new child
            newChildItem.IsSelected = true;

            //expand the parent
            var parentResultDefinition = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.FirstOrDefault(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == newChildItem.ResultRuleViewModel.ParentUUID);
            parentResultDefinition.IsExpanded = true;

            //auto add now requirement to any now that has the parent as a now requirement
            MakeNowRequirements(parentResultDefinition, newChildItem);
        }

        private void MakeNowRequirements(ResultDefinitionTreeViewModel parentResultDefinition, ResultDefinitionTreeViewModel newChildItem)
        {
            foreach(var now in treeModel.AllNowsTreeViewModel.Nows)
            {
                var matchedParentNowRequirement = now.NowViewModel.NowRequirements.FirstOrDefault(x => x.NowRequirement.ResultDefinition.UUID == parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);

                if (matchedParentNowRequirement == null) { continue; }

                //determine if the child now requirement already exists
                if (matchedParentNowRequirement.NowRequirements != null && matchedParentNowRequirement.NowRequirements.Count > 0)
                {
                    var matchedChildNowRequirement = matchedParentNowRequirement.NowRequirements.FirstOrDefault(x => x.NowRequirement.ResultDefinition.UUID == newChildItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);

                    if (matchedChildNowRequirement != null) { continue; }
                }
                else
                {
                    matchedParentNowRequirement.NowRequirements = new List<NowRequirementViewModel>();
                }

                //add a new child now requirement
                var parentNr = matchedParentNowRequirement.NowRequirement;
                var rootNr = matchedParentNowRequirement.ParentNowRequirement == null ? matchedParentNowRequirement.NowRequirement : matchedParentNowRequirement.RootParentNowRequirement;
                var data = new NowRequirement(newChildItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition, now.NowViewModel.Now, parentNr, rootNr);
                var nrVM = new NowRequirementViewModel(treeModel, data, newChildItem.ResultRuleViewModel.ChildResultDefinitionViewModel, now.NowViewModel, treeModel.AllResultDefinitionsViewModel);

                //set parentage on the data
                matchedParentNowRequirement.NowRequirements.Add(nrVM);
                if (matchedParentNowRequirement.NowRequirement.NowRequirements == null) { matchedParentNowRequirement.NowRequirement.NowRequirements = new List<NowRequirement>(); }
                matchedParentNowRequirement.NowRequirement.NowRequirements.Add(data);

                //get the parent tree view item
                foreach (var item in now.Children)
                {
                    var parentNowRequirementTreeViewItem = item as NowRequirementTreeViewModel;
                    if (parentNowRequirementTreeViewItem != null && parentNowRequirementTreeViewItem.NowRequirementViewModel.ResultDefinition.UUID == parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                    {
                        //make a new tree item for the new now requirement
                        var newItem = new NowRequirementTreeViewModel(treeModel, nrVM, parentNowRequirementTreeViewItem);

                        //add the new now requirement to the parent now requirement at the appropriate position based on rank
                        int index = 0;
                        var sequence = parentNowRequirementTreeViewItem.NowRequirementViewModel.ResultDefinitionSequence;
                        var childAdded = false;
                        foreach (var childTreeViewItem in parentNowRequirementTreeViewItem.Children)
                        {
                            index = index + 1;
                            var childNowRequirementTreeViewItem = childTreeViewItem as NowRequirementTreeViewModel;
                            if (childNowRequirementTreeViewItem == null) { continue; }

                            if (childNowRequirementTreeViewItem.NowRequirementViewModel.ResultDefinition.Rank.HasValue && 
                                childNowRequirementTreeViewItem.NowRequirementViewModel.ResultDefinition.Rank > newItem.NowRequirementViewModel.ResultDefinition.Rank)
                            {
                                parentNowRequirementTreeViewItem.Children.Insert(index-1, newItem);
                                SetSequence(childNowRequirementTreeViewItem.NowRequirementViewModel.ResultDefinitionSequence, sequence, data);
                                childAdded = true;
                                break;
                            }

                            sequence = childNowRequirementTreeViewItem.NowRequirementViewModel.ResultDefinitionSequence;
                        }

                        if (!childAdded)
                        {
                            data.ResultDefinitionSequence = parentNowRequirementTreeViewItem.NowRequirementViewModel.ResultDefinitionSequence + 100;
                            parentNowRequirementTreeViewItem.Children.Add(newItem);
                        }

                        //sort out the other data store objects
                        treeModel.AllNowRequirementsViewModel.NowRequirements.Add(nrVM);
                        treeModel.AllData.NowRequirements.Add(data);
                        break;
                    }
                }                
            }
        }

        private void SetSequence(int? nextItem, int? selectedInsertPoint, NowRequirement data)
        {
            if (selectedInsertPoint + 20 < nextItem)
            {
                data.ResultDefinitionSequence = selectedInsertPoint + 20;
                return;
            }

            if (selectedInsertPoint + 10 < nextItem)
            {
                data.ResultDefinitionSequence = selectedInsertPoint + 10;
                return;
            }
            if (selectedInsertPoint + 5 < nextItem)
            {
                data.ResultDefinitionSequence = selectedInsertPoint + 5;
                return;
            }

            int delta = nextItem.Value - selectedInsertPoint.Value;
            decimal average = delta > 0 ? delta / 2 : 0;
            data.ResultDefinitionSequence = average == 0 ? null : (int?)(Math.Round(average) + selectedInsertPoint);
        }
        
        protected ResultDefinitionTreeViewModel MakeAndStoreChildResultDefinitionTreeViewModel()
        {
            //find the parent and make a child item
            var parentResultDefinition = FindParentResultDefinition();

            //make a new root item
            var newItem = MakeAndStoreResultDefinitionTreeViewModel(parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTagsModel);           

            //make a new child item, setting parentage
            var data = new ResultDefinitionRule(parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID, newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition, ResultDefinitionRuleType.atleastOneOf);
            data.PublishedStatus = newItem.ResultRuleViewModel.PublishedStatus;
            data.PublishedStatusDate = newItem.ResultRuleViewModel.PublishedStatusDate;
            var ruleVM = new ResultRuleViewModel(treeModel, newItem.ResultRuleViewModel.ChildResultDefinitionViewModel, parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel, data);
            if (parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules == null) { parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules = new List<ResultDefinitionRule>(); }
            parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.Add(data);
            parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.LastModifiedDate = DateTime.Now;

            //roll any publication tags from the parent to the child
            if (parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags != null)
            {
                foreach(var tag in parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags)
                {
                    newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTagsModel.SelectedAvailableTag = tag;
                    newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTagsModel.AddTagCommand.Execute(tag);
                }
            }

            //Set the result definition group based on the nearest sibling
            //SetDefinitionGroupBasedOnSibling(parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID, newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition);

            //sort out the data store objects
            treeModel.AllResultRuleViewModel.Rules.Add(ruleVM);
            treeModel.AllData.ResultDefinitionRules.Add(data);

            //make a new tree view, add it to the parent and the collection of all definitions and select it
            var childItem = new ResultDefinitionTreeViewModel(treeModel, ruleVM, parentResultDefinition);
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Add(childItem);
            if (parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules == null) { parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules = new List<ResultRuleViewModel>(); }
            parentResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Add(childItem.ResultRuleViewModel);
            SetPosition(parentResultDefinition, childItem);

            //trigger a change in parentage on the parent result definition view model
            parentResultDefinition.ResultRuleViewModel.ResetStatus();


            return childItem;
        }

        /// <summary>
        /// Ensures that each sibling has the same result definition group
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="data"></param>
        private void SetDefinitionGroupBasedOnSibling(Guid? parentId, ResultDefinition data)
        {
            var sibling = treeModel.AllResultRuleViewModel.Rules.Where(x=>x.ResultDefinitionRule.DeletedDate == null).FirstOrDefault(x => x.ParentUUID == parentId);
            if (sibling != null)
            {
                data.ResultDefinitionGroup = sibling.ChildResultDefinitionViewModel.ResultDefinitionGroup;
            }
        }

        private ResultDefinitionTreeViewModel FindParentResultDefinition()
        {
            var selectedParentResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
            if (selectedParentResultDefinition != null) { return selectedParentResultDefinition; }

            var selectedNowRequirement = treeModel.SelectedItem as NowRequirementTreeViewModel;
            var parentNowRequirement = selectedNowRequirement.Parent as NowRequirementTreeViewModel;

            //find the root result definition
            var parentRD = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.FirstOrDefault(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == parentNowRequirement.NowRequirementViewModel.ResultDefinition.UUID);
            return parentRD;
        }

        private void SetPosition(ResultDefinitionTreeViewModel parent, ResultDefinitionTreeViewModel childItem)
        {
            if (parent.Children.Count == 0)
            {
                parent.Children.Add(childItem);
                return;
            }

            var index = 0;
            for (index = parent.Children.Count() -1; index>=0; index--)
            {
                var child = parent.Children[index] as ResultDefinitionTreeViewModel;
                if (child == null)
                {
                    parent.Children.Insert(index+1, childItem);
                    return;
                }
                else
                {
                    if (child.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank.HasValue && childItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank.HasValue &&
                        child.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank < childItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank)
                    {
                        parent.Children.Insert(index+1, childItem);
                        return;
                    }
                }
            }

            parent.Children.Add(childItem);
        }
    }
}
