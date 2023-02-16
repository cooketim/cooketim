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
    public interface IDeleteResultDefinitionCommand : ICommand { }

    public class DeleteResultDefinitionCommand : IDeleteResultDefinitionCommand
    {
        protected ITreeModel treeModel;
        public DeleteResultDefinitionCommand(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

        ResultDefinitionTreeViewModel selectedResultDefinition;

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
            ExecuteCommand(parameter);
        }

        protected virtual void ExecuteCommand(object parameter)
        {
            var selectedNowSubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            bool isExcludedResults;
            bool isIncludedResults;
            if (selectedNowSubscription != null)
            {
                //set included/excluded based on command parameter
                if (parameter == null) { return; }
                var includedExcluded = ((string)parameter).ToLowerInvariant();
                if (includedExcluded == "excluded")
                {
                    isExcludedResults = true;
                    isIncludedResults = false;
                }
                else
                {
                    isIncludedResults = true;
                    isExcludedResults = false;
                }

                DeleteNowSubscriptionResult(isExcludedResults, isIncludedResults, selectedNowSubscription);
            }
            else
            {
                DeleteResultDefinitionTreeItem();
            }
        }

        private void DeleteNowSubscriptionResult(bool isExcludedResults, bool isIncludedResults, NowSubscriptionTreeViewModel selectedNowSubscription)
        {
            if (isExcludedResults)
            {
                if (selectedNowSubscription.NowSubscriptionViewModel.SelectedNowExcludedResultValue == null) { return; }
                selectedNowSubscription.NowSubscriptionViewModel.ExcludedResults.Remove(selectedNowSubscription.NowSubscriptionViewModel.SelectedNowExcludedResultValue);
                selectedNowSubscription.NowSubscriptionViewModel.SelectedNowExcludedResultValue = null;
            }

            if (isIncludedResults)
            {
                if (selectedNowSubscription.NowSubscriptionViewModel.SelectedNowIncludedResultValue == null) { return; }
                selectedNowSubscription.NowSubscriptionViewModel.IncludedResults.Remove(selectedNowSubscription.NowSubscriptionViewModel.SelectedNowIncludedResultValue);
                selectedNowSubscription.NowSubscriptionViewModel.SelectedNowIncludedResultValue = null;
            }
        }

        private void DeleteResultDefinitionTreeItem()
        {
            //Determine the selected item
            selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

            //get the parent
            var parent = selectedResultDefinition.Parent as ResultDefinitionTreeViewModel;
            if (parent == null) 
            {
                RemoveFromRoot(selectedResultDefinition); 
            }
            else
            {
                RemoveFromParent(parent, selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel);
            }
        }

        /// <summary>
        /// remove a child definition from its parent definition
        /// </summary>
        /// <param name="resultDefinitionTreeView"></param>
        private void RemoveFromParent(ResultDefinitionTreeViewModel parent, ResultDefinitionViewModel child)
        {
            //when required physically remove a draft child item
            if (child.PublishedStatus == PublishedStatus.Draft)
            {
                RemoveDraftFromParent(parent, child);
                return;
            }

            //find the parent rule
            var rule = parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.First(x => x.ChildResultDefinitionViewModel.UUID == child.UUID);

            //determine if we are removing a draft rule and if not determine if we are deleting an already deleted item
            var isPurge = false;
            if (rule.PublishedStatus == PublishedStatus.Draft)
            {
                //always purge drafts
                isPurge = true;
            }
            else if (rule.IsDeleted)
            {
                isPurge = true;
            }
            else
            {
                //set the rule as logically deleted
                rule.DeletedDate = DateTime.Now;
            }            

            //Set the parent as modified
            parent.ResultRuleViewModel.ChildResultDefinitionViewModel.LastModifiedDate = DateTime.Now;

            //remove the result rule view model and the data
            if (isPurge)
            {
                treeModel.AllResultRuleViewModel.Rules.RemoveAll(x => x.ResultDefinitionRule.ParentUUID == parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID && x.ChildResultDefinitionViewModel.UUID == child.UUID);

                //remove data from parent
                parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.RemoveAll(x => x.ChildResultDefinitionViewModel.UUID == child.UUID);
                parent.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.RemoveAll(x => x.ResultDefinition.UUID == child.UUID);
            }

            //tidy up the tree model for each instance of a treeview model for the parent item
            if (isPurge)
            {
                //remove tree view model from the parent
                if (parent.IsExpanded)
                {
                    parent.Children.RemoveAll(
                        x => x.GetType() == typeof(ResultDefinitionTreeViewModel)
                          && ((ResultDefinitionTreeViewModel)x).ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                          == child.UUID);
                }

                //remove the tree view from all tree view model
                var matchedItems = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Where(x => x.Parent != null && x.ResultRuleViewModel.ParentUUID == parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID && x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == child.UUID).ToList();
                foreach (var item in matchedItems)
                {
                    treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Remove(item);
                }
            }

            //trigger a change in parentage on the parent rule view model
            parent.ResultRuleViewModel.ResetStatus();

            //delete any associated now requirements
            DeleteNowRequirements(parent, child, isPurge);

            //synchronise now subscriptions
            //SyncroniseSubscriptions();
        }

        private NowTreeViewModel MatchNOWEDT(NowRequirementViewModel nr)
        {
            var match = treeModel.Nows.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
            if (match != null) { return match; }
            return treeModel.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
        }

        private void PurgeChildNowRequirementTreeViews(SilentObservableCollection<TreeViewItemViewModel> childTreeItems, NowRequirementViewModel child, bool withRecursion, bool withPurgeData)
        {
            foreach (var childTV in childTreeItems.ToList())
            {
                var nrTV = childTV as NowRequirementTreeViewModel;
                if (nrTV == null)
                {
                    continue;
                }
                if (nrTV.NowRequirementViewModel.UUID == child.UUID)
                {
                    //add to the toDelete collection
                    //purge the child from its parent
                    nrTV.Parent.Children.Remove(nrTV);

                    //set the parent view model as updated
                    var parentNRVM = nrTV.Parent as NowRequirementTreeViewModel;
                    if (parentNRVM == null)
                    {
                        //see if the parent is a now
                        var parentNowVM = nrTV.Parent as NowTreeViewModel;

                        if (parentNowVM == null)
                        {
                            continue;
                        }
                        else
                        {
                            parentNowVM.NowViewModel.LastModifiedDate = DateTime.Now;

                            //remove child VM from parent VM
                            parentNowVM.NowViewModel.NowRequirements.Remove(nrTV.NowRequirementViewModel);

                            if (withPurgeData)
                            {
                                //remove the child from the global collection
                                treeModel.AllNowRequirementsViewModel.NowRequirements.Remove(nrTV.NowRequirementViewModel);

                                //remove the data
                                parentNowVM.NowViewModel.Now.NowRequirements.Remove(nrTV.NowRequirementViewModel.NowRequirement);
                            }
                            else
                            {
                                //set the child now requirement as logically deleted
                                nrTV.NowRequirementViewModel.DeletedDate = DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        parentNRVM.NowRequirementViewModel.LastModifiedDate = DateTime.Now;

                        //remove child VM from parent VM
                        parentNRVM.NowRequirementViewModel.NowRequirements.Remove(nrTV.NowRequirementViewModel);

                        if (withPurgeData)
                        {
                            //remove the child from the global collection
                            treeModel.AllNowRequirementsViewModel.NowRequirements.Remove(nrTV.NowRequirementViewModel);

                            //remove the data
                            parentNRVM.NowRequirementViewModel.NowRequirement.NowRequirements.Remove(nrTV.NowRequirementViewModel.NowRequirement);
                        }
                        else
                        {
                            //set the child now requirement as logically deleted
                            nrTV.NowRequirementViewModel.DeletedDate = DateTime.Now;
                        }
                    }
                }
                if (withRecursion)
                {
                    if (!nrTV.IsExpanded)
                    {
                        nrTV.IsExpanded = true;
                    }
                    PurgeChildNowRequirementTreeViews(nrTV.Children, child, true, withPurgeData);
                }
            }
        }

        private void DeleteRootNowRequirements(ResultDefinitionTreeViewModel rdTV, bool withPurgeData)
        {
            //Find any now root Requirements for the result, only dealing with draft requirements when in draft mode
            List<IGrouping<Guid?, NowRequirementViewModel>> nowRequirementsToRemoveGroup = null;
            nowRequirementsToRemoveGroup = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(
                                                        x => x.PublishedStatus == PublishedStatus.Draft &&
                                                             x.ResultDefinition.UUID == rdTV.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID &&
                                                             x.ParentNowRequirement == null).GroupBy(x => x.NOWUUID).ToList();

            foreach (var group in nowRequirementsToRemoveGroup)
            {
                //set the parent now as modified
                var matchedNow = MatchNOWEDT(group.First());
                if (matchedNow != null)
                {
                    foreach(var rootNR in group.ToList())
                    {
                        //recursively purge child tree view items for the given now
                        PurgeChildNowRequirementTreeViews(matchedNow.Children, rootNR, false, withPurgeData);
                    }                    
                }
            }
        }

        private void DeleteNowRequirements(ResultDefinitionTreeViewModel parent, ResultDefinitionViewModel child, bool withPurgeData)
        {
            List<IGrouping<Guid?, NowRequirementViewModel>> nowRequirementsToRemoveGroup = null;
            nowRequirementsToRemoveGroup = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(
                        x => x.PublishedStatus == PublishedStatus.Draft &&
                             x.ParentNowRequirement != null &&
                             x.ParentNowRequirement.ResultDefinition.UUID == parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID &&
                             x.ResultDefinition.UUID == child.UUID).GroupBy(x => x.NOWUUID).ToList();

            foreach (var group in nowRequirementsToRemoveGroup)
            {
                //set the parent now as modified
                var matchedNow = MatchNOWEDT(group.First());
                if (matchedNow != null)
                {
                    matchedNow.NowViewModel.LastModifiedDate = DateTime.Now;

                    foreach(var nr in group.ToList())
                    {
                        //recursively purge child tree view items for the given now
                        if (!matchedNow.IsExpanded)
                        {
                            matchedNow.IsExpanded = true;
                        }
                        PurgeChildNowRequirementTreeViews(matchedNow.Children, nr, true, withPurgeData);
                    }                    
                }
            }
        }

        private void RemoveFromRoot(ResultDefinitionTreeViewModel rootItem)
        {
            //when required physically remove a draft item
            if (rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                RemoveDraftFromRoot(rootItem);
                return;
            }

            //determine if we are deleting an already deleted item
            var isPurge = false;
            if (rootItem.ResultRuleViewModel.IsDeleted || rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted)
            {
                isPurge = true;
            }

            //remember the current selected index
            var index = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.IndexOf(rootItem);

            //deal with any parent child relationships where the root item is the child of another result
            var parentsOfRootItem = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Where(
                                                x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null &&
                                                     x.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules
                                                     .Where(y => !y.IsDeleted)
                                                     .Select(z => z.ChildResultDefinitionViewModel.UUID).Contains(rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                                                   ).ToList();

            foreach (var parent in parentsOfRootItem)
            {
                RemoveFromParent(parent, rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel);
            }           

            //set the data as logically deleted or remove it
            if (isPurge)
            {
                PurgeResult(rootItem);
            }
            else
            {
                //logically delete the root
                rootItem.ResultRuleViewModel.DeletedDate = DateTime.Now;
                rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.DeletedDate = DateTime.Now;
                rootItem.ResultRuleViewModel.ResetStatus();

                //logically delete the prompt rules for this deleted result definition
                foreach (var item in treeModel.AllResultPromptViewModel.Prompts.Where(x => x.ResultPromptRule.ResultDefinitionUUID == rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID))
                {
                    item.DeletedDate = DateTime.Now;

                    //determine if the prompt is used elsewhere on other results
                    var otherResults = treeModel.AllResultPromptViewModel.Prompts.Where(x => x.ResultPromptRule.ResultDefinitionUUID != rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                                                                    && x.ResultPromptRule.ResultPromptUUID == item.ResultPromptRule.ResultPromptUUID).ToList();
                    if (otherResults.Count == 0)
                    {
                        //logically delete the prompt as well
                        item.ResultPromptViewModel.DeletedDate = DateTime.Now;
                    }
                }

                treeModel.SetRootResultDeleted();
            }

            //deal with the associated NOWs where the result is a root level now requirement
            DeleteRootNowRequirements(rootItem, isPurge);

            //deal with any now subscriptions that have associations to result prompts that have now been deleted
            //SyncroniseSubscriptions();

            //select the next node
            if (index > 0 && treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.Count - 1 <= index)
            {
                index = index - 1;
            }
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft[index].IsSelected = true;

            //reset the copied item
            selectedResultDefinition.PendingCopyModel = null;
        }

        private void ResyncChildNowRequirements(NowRequirementViewModel parent, NowViewModel nowVM)
        {
            //resync the child requirements of this child to the child results of the published result
            if (parent.ResultDefinition.Rules == null)
            {
                if (parent.NowRequirements != null)
                {
                    parent.NowRequirements.Clear();
                    parent.NowRequirement.NowRequirements.Clear();
                    parent.LastModifiedDate = DateTime.Now;
                }
            }
            else
            {
                if (parent.NowRequirements == null)
                {
                    parent.NowRequirements = new List<NowRequirementViewModel>();
                    parent.NowRequirement.NowRequirements = new List<NowRequirement>();
                }

                //remove any current child now requirements that are no longer required
                var currentRds = parent.NowRequirements.Select(x => x.ResultDefinition.MasterUUID).ToList();
                var toBeIds = parent.ResultDefinition.Rules.Select(x => x.ChildResultDefinitionViewModel.MasterUUID).ToList();
                var currentNotToBe = currentRds.Except(toBeIds).ToList();
                foreach(var item in currentNotToBe)
                {
                    parent.NowRequirements.RemoveAll(x => x.ResultDefinition.MasterUUID == item);
                }

                //ensure that we have a child now requirement for each rule
                foreach (var rdr in parent.ResultDefinition.Rules)
                {
                    var matchedNr = parent.NowRequirements.FirstOrDefault(x => (x.ResultDefinition.MasterUUID ?? x.ResultDefinition.UUID) == rdr.ChildResultDefinitionViewModel.MasterUUID);
                    if (matchedNr == null)
                    {
                        //attempt to draft the existing published
                        NowRequirementViewModel newNrVM = DraftExistingPublished(parent, rdr, nowVM);

                        if (newNrVM == null)
                        {
                            //create new now requirement
                            var newNr = new NowRequirement(rdr.ChildResultDefinitionViewModel.ResultDefinition, nowVM.Now, parent.NowRequirement, parent.RootParentNowRequirement ?? parent.NowRequirement);
                            newNrVM = new NowRequirementViewModel(treeModel, newNr, rdr.ChildResultDefinitionViewModel, nowVM, treeModel.AllResultDefinitionsViewModel);

                            //set the now requirement hierachy
                            newNrVM.SetNowRequirements();
                        }

                        //add new now requirement to the parent
                        parent.NowRequirements.Add(newNrVM);
                        parent.NowRequirement.NowRequirements.Add(newNrVM.NowRequirement);
                    }
                }

                //clean up the global store
                var currentVMs = treeModel.AllNowRequirementsViewModel.NowRequirements.Where(x => x.NOWUUID == nowVM.UUID).ToList();
                var toBeVMs = nowVM.AllNowRequirements;
                var currentVMNotToBeVMIds = currentVMs.Select(x=>x.UUID).Except(toBeVMs.Select(x=>x.UUID)).ToList();
                var toBeVMNotCurrentIds = toBeVMs.Select(x => x.UUID).Except(currentVMs.Select(x => x.UUID)).ToList();
                foreach(var item in currentVMNotToBeVMIds)
                {
                    //purge from the global store
                    treeModel.AllNowRequirementsViewModel.NowRequirements.RemoveAll(x => x.UUID == item);
                    treeModel.AllData.NowRequirements.RemoveAll(x => x.UUID == item);
                }
                foreach (var item in toBeVMNotCurrentIds)
                {
                    //add to the global store
                    var vm = toBeVMs.First(x => x.UUID == item);
                    treeModel.AllNowRequirementsViewModel.NowRequirements.Add(vm);
                    treeModel.AllData.NowRequirements.Add(vm.NowRequirement);
                }
            }
        }

        private NowRequirementViewModel DraftExistingPublished(NowRequirementViewModel parent, ResultRuleViewModel rdr, NowViewModel nowVM)
        {
            var publishedNow = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.MasterUUID == nowVM.MasterUUID && (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending));
            if (publishedNow == null)
            {
                return null;
            }
            var publishedNr = treeModel.AllNowRequirementsViewModel.NowRequirements.FirstOrDefault(
                                x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) &&
                                      x.NOWUUID == publishedNow.UUID &&
                                      x.NowRequirement.ResultDefinition.UUID == rdr.ChildResultDefinitionViewModel.UUID &&
                                      x.ParentNowRequirement != null && (x.ParentNowRequirement.MasterUUID ?? x.ParentNowRequirement.UUID) == parent.MasterUUID &&
                                      (x.RootParentNowRequirement.MasterUUID ?? x.RootParentNowRequirement.UUID) == (parent.RootParentNowRequirement.MasterUUID ?? parent.RootParentNowRequirement.UUID)
                                      );
            if (publishedNr == null)
            {
                return null;
            }

            return publishedNr.Draft(nowVM, parent.ParentNowRequirement);
        }

        private void RemoveDraftFromParent(ResultDefinitionTreeViewModel parent, ResultDefinitionViewModel child)
        {
            //reset the draft child back to published
            ResetDraftChildAsPublished(parent, child);

            //reset child for any associated nows and now subscriptions
            ResetNowsAndSubscriptionsChildItem(parent,child);

            //Set the parent as modified
            parent.ResultRuleViewModel.ChildResultDefinitionViewModel.LastModifiedDate = DateTime.Now;

            //remove the result rule view model and the data
            treeModel.AllResultRuleViewModel.Rules.RemoveAll(x => x.ResultDefinitionRule.ParentUUID == parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID && x.ChildResultDefinitionViewModel.UUID == child.UUID);
            treeModel.AllData.ResultDefinitionRules.RemoveAll(x => x.ParentUUID == parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID && x.ChildResultDefinitionUUID == child.UUID);

            //remove data from parent
            parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.RemoveAll(x => x.ChildResultDefinitionViewModel.UUID == child.UUID);
            parent.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.RemoveAll(x => x.ResultDefinition.UUID == child.UUID);

            //trigger a change in parentage on the parent rule view model
            parent.ResultRuleViewModel.ResetStatus();

            //reset the parent tree view to force the child tree items to be refreshed
            parent.ResetChildren();
        }

        private void RemoveDraftFromRoot(ResultDefinitionTreeViewModel rootItem)
        {
            //remember the current selected index
            var index = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.IndexOf(rootItem);

            //deal with any draft prompts
            if (rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
            {
                RemovePromptsFromDraft(rootItem);
            }

            //deal with draft child result rules
            if (rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
            {
                RemoveChildResultRulesFromDraft(rootItem);
            }

            //deal with draft word groups
            if (rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.WordGroups != null)
            {
                RemoveDraftWordGroups(rootItem);
            }

            var parentsOfRootItem = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Where(
                                                x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null &&
                                                     x.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Where(y=>!y.IsDeleted).Select(z => z.ChildResultDefinitionViewModel.UUID).Contains(rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                                                   ).ToList();

            foreach (var parent in parentsOfRootItem)
            {
                ResetDraftChildAsPublished(parent, rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel);
            }

            //reset rootitem for any associated nows and now subscriptions
            ResetNowsAndSubscriptionsRootItem(rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel);

            PurgeResult(rootItem);

            //select the next node
            if (treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.Count > 0 )
            {
                if (index > 0 && treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.Count - 1 <= index)
                {
                    index = index - 1;
                }                
                treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft[index].IsSelected = true;
            }            

            //reset the copied item
            selectedResultDefinition.PendingCopyModel = null;
        }

        private void ResetDraftChildAsPublished(ResultDefinitionTreeViewModel parent, ResultDefinitionViewModel draftChild)
        {
            //find the related published result model
            var publishedChild = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(
                                x => x.MasterUUID == draftChild.MasterUUID
                                  && x.PublishedStatus == PublishedStatus.Published);

            if (publishedChild == null)
            {
                //remove the result rule from the parent, view models, data and child tree item
                parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.RemoveAll(
                                x => x.ChildResultDefinitionViewModel.UUID == draftChild.UUID);
                parent.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.RemoveAll(
                                x => x.ChildResultDefinitionUUID == draftChild.UUID);
                if (parent.IsExpanded)
                {
                    parent.Children.RemoveAll(
                        x => x.GetType() == typeof(ResultDefinitionTreeViewModel)
                          && ((ResultDefinitionTreeViewModel)x).ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                          == draftChild.UUID);
                }
            }
            else
            {
                //update the rule to set the child result as the published version                
                var rule = parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.First(
                    x => x.ChildResultDefinitionViewModel.UUID == draftChild.UUID);
                rule.ChildResultDefinitionViewModel = publishedChild;
            }

            //reset the parent of the root item
            parent.ResetChildren();

            //reset any associated nows
            ResetNows(parent, draftChild, publishedChild);
        }
        protected void ResetNowsAndSubscriptionsRootItem(ResultDefinitionViewModel draftRoot)
        {
            var publishedRoot = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(
                                x => x.MasterUUID == draftRoot.MasterUUID
                                  && x.PublishedStatus == PublishedStatus.Published);

            ResetNowsDraftItem(draftRoot, publishedRoot, null);

            ResetNowSubscriptionsRootItem(draftRoot, publishedRoot);
        }


        private void ResetNowsAndSubscriptionsChildItem(ResultDefinitionTreeViewModel parent, ResultDefinitionViewModel draftChild)
        {
            var publishedChild = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(
                                x => x.MasterUUID == draftChild.MasterUUID
                                  && x.PublishedStatus == PublishedStatus.Published);

            ResetNowsDraftItem(draftChild, publishedChild, parent);

            ResetNowSubscriptionsRootItem(draftChild, publishedChild);
        }

        private void ResetNowSubscriptionsRootItem(ResultDefinitionViewModel draft, ResultDefinitionViewModel published)
        {
            var ns = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                x => x.PublishedStatus == PublishedStatus.Draft &&
                                     x.IncludedResults != null &&
                                     (
                                        x.IncludedResults.Select(y => y.ResultDefinition.UUID).Contains(draft.UUID)
                                        ||
                                        x.ExcludedResults.Select(y => y.ResultDefinition.UUID).Contains(draft.UUID)
                                    )
                                ).ToList();

            foreach (var sub in ns)
            {
                //check included results
                ResetNowSubscriptionResultsAndPrompts(sub, sub.IncludedResults, published, sub.IncludedPrompts, draft);

                //check excluded results
                ResetNowSubscriptionResultsAndPrompts(sub, sub.ExcludedResults, published, sub.ExcludedPrompts, draft);
            }
        }

        private void ResetNowSubscriptionResultsAndPrompts(NowSubscriptionViewModel sub, SilentObservableCollection<NowSubscriptionResultViewModel> results, ResultDefinitionViewModel published, SilentObservableCollection<NowSubscriptionPromptViewModel> prompts, ResultDefinitionViewModel draft)
        {
            if (published == null)
            {
                int rdRemoved = 0, rpRemoved = 0;
                //remove any included results
                if (results != null)
                {
                    rdRemoved = results.RemoveAll(x => x.ResultDefinition.UUID == draft.UUID);
                }

                //remove any associated prompts
                if (prompts != null)
                {
                    rpRemoved = prompts.RemoveAll(x => x.ResultPrompt.ResultDefinitionUUID == draft.UUID);
                }

                //set the now subscription as modified
                if (rdRemoved > 0 || rpRemoved > 0)
                {
                    sub.LastModifiedDate = DateTime.Now;
                }
            }
            else
            {
                //reset result back to the published result
                List<NowSubscriptionResultViewModel> matchedResults = null;
                if (results != null)
                {
                    matchedResults = results.Where(x => x.ResultDefinition.UUID == draft.UUID).ToList();
                    foreach (var item in matchedResults)
                    {
                        //reset draft to published
                        item.ResultDefinition = published.ResultDefinition;
                    }
                }

                //reset result prompts back to the published result prompt if there is one
                List<NowSubscriptionPromptViewModel> matchedPrompts = null;
                if (prompts != null)
                {
                    matchedPrompts = prompts.Where(x => x.ResultPrompt.ResultDefinitionUUID == draft.UUID).ToList();
                    foreach (var item in matchedPrompts)
                    {
                        //find the equivalent published prompt
                        ResultPromptRule publishedRule = null;
                        if (published.ResultDefinition.ResultPrompts != null)
                        {
                            publishedRule = published.ResultDefinition.ResultPrompts.FirstOrDefault(x => (x.ResultPrompt.MasterUUID ?? x.ResultPrompt.UUID) == (item.ResultPrompt.MasterUUID ?? item.ResultPrompt.UUID));
                        }

                        //reset draft to published, may be setting a null value which will be cleared out later
                        item.ResultPrompt = publishedRule;
                    }

                    //remove any prompts that have a null rule
                    prompts.RemoveAll(x => x.ResultPrompt == null);
                }

                //set the now subscription as modified
                if (matchedResults != null && matchedResults.Any() || matchedPrompts != null && matchedPrompts.Any())
                {
                    sub.LastModifiedDate = DateTime.Now;
                }
            }
        }

        private void ResetNowsDraftItem(ResultDefinitionViewModel draft, ResultDefinitionViewModel published, ResultDefinitionTreeViewModel parent)
        {
            //deal with any nows that are consuming the draft root result by resetting the now requirements back to the published result
            List<IGrouping<Guid?, NowRequirementViewModel>> nowRequirementsToResetGroup = null;
            if (parent == null)
            {
                nowRequirementsToResetGroup = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(
                        x => x.PublishedStatus == PublishedStatus.Draft &&
                                x.ParentNowRequirement == null &&
                                x.ResultDefinition.UUID == draft.UUID).GroupBy(x => x.NOWUUID).ToList();
            }
            else
            {
                nowRequirementsToResetGroup = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(
                        x => x.PublishedStatus == PublishedStatus.Draft &&
                                x.ParentNowRequirement != null &&
                                x.ParentNowRequirement.ResultDefinition.UUID == parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID &&
                                x.ResultDefinition.UUID == draft.UUID).GroupBy(x => x.NOWUUID).ToList();
            }

            //Set the changes to the now requirements
            foreach (var group in nowRequirementsToResetGroup)
            {
                //reset the draft now requirements to the published result or delete it from the now or delete it from the parent
                foreach (var nr in group)
                {
                    if (published != null)
                    {
                        //reset to the published result
                        nr.ResultDefinition = published;
                    }
                    else
                    {
                        if (nr.ParentNowRequirement == null)
                        {
                            //remove the now requirement from the parent now
                            nr.NowEdtVM.NowRequirements.RemoveAll(x => x.UUID == nr.UUID);
                            nr.NowEdtVM.LastModifiedDate = DateTime.Now;
                        }
                        else
                        {
                            //remove the now requirement from the parent now requirement
                            nr.ParentNowRequirement.NowRequirements.RemoveAll(x => x.UUID == nr.UUID);
                            nr.ParentNowRequirement.LastModifiedDate = DateTime.Now;
                            nr.NowEdtVM.LastModifiedDate = DateTime.Now;
                        }

                        //remove from the global collection
                        treeModel.AllNowRequirementsViewModel.NowRequirements.RemoveAll(x => x.UUID == nr.UUID);
                        treeModel.AllData.NowRequirements.RemoveAll(x => x.UUID == nr.UUID);
                    }
                }

                //Set the now as modified and reset any treeviews
                var draftTreeView = MatchNOWEDT(group.First());
                if (draftTreeView != null)
                {
                    draftTreeView.ResetChildren();
                }
            }
        }

        private void ResetNows(ResultDefinitionTreeViewModel parent, ResultDefinitionViewModel draftChild, ResultDefinitionViewModel publishedChild)
        {
            //deal with any nows that are consuming the draft child result by resetting the now requirements back to the published result
            var nowRequirementsToResetGroup = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(
                        x => x.PublishedStatus == PublishedStatus.Draft &&
                             x.ParentNowRequirement != null &&
                             x.ParentNowRequirement.ResultDefinition.UUID == parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID &&
                             x.ResultDefinition.UUID == draftChild.UUID).GroupBy(x => x.NOWUUID).ToList();

            //Set the changes to the now requirements
            foreach (var group in nowRequirementsToResetGroup)
            {
                //reset the draft now requirements to the published result or delete it
                foreach (var child in group)
                {
                    if (publishedChild != null)
                    {
                        //reset to the published result
                        child.ResultDefinition = publishedChild;

                        //Resync the child now requirements of this child based on the new result
                        ResyncChildNowRequirements(child, child.NowEdtVM);
                    }
                    else
                    {
                        //remove the now requirement from the parent
                        child.ParentNowRequirement.NowRequirements.RemoveAll(x => x.UUID == child.UUID);
                        child.ParentNowRequirement.LastModifiedDate = DateTime.Now;
                    }
                }

                //Set the now as modified and reset any treeviews
                var draftTreeView = MatchNOWEDT(group.First());
                if (draftTreeView != null)
                {
                    draftTreeView.ResetChildren();
                }
            }
        }

        private void RemovePromptsFromDraft(ResultDefinitionTreeViewModel rootItem)
        {
            //remove all the prompt rules for the draft result from the collection of all prompts for both view models and data
            treeModel.AllResultPromptViewModel.Prompts.RemoveAll(
                        x => x.ResultDefinitionViewModel.UUID == rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            treeModel.AllData.ResultPromptRules.RemoveAll(x => x.ResultDefinitionUUID == rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);

            //now deal with those rules that have a draft prompt
            foreach (var dp in rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.Where(x => x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft))
            {
                //find other usages of the draft prompt
                var otherDraftUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                        x => x.ResultPromptViewModel.UUID == dp.ResultPromptViewModel.UUID);

                if (!otherDraftUsages.Any()) 
                {
                    PurgeDraftPromptChildren(dp);
                }
            }
        }

        private void PurgeDraftPromptChildren(ResultPromptRuleViewModel dp)
        {
            //determine if the draft prompt has a draft fixed list
            if (dp.ResultPromptViewModel.FixedList != null && dp.ResultPromptViewModel.FixedList.PublishedStatus == PublishedStatus.Draft)
            {
                //remove from this prompt                
                PurgeDraftFixedList(dp);                                
            }

            //determine if the draft prompt has any draft word groups
            if (dp.ResultPromptViewModel.WordGroups != null && dp.ResultPromptViewModel.WordGroups.Where(x=>x.PublishedStatus == PublishedStatus.Draft).Any())
            {
                foreach (var rpwg in dp.ResultPromptViewModel.WordGroups.Where(x => x.PublishedStatus == PublishedStatus.Draft))
                {
                    PurgeDraftWordGroup(rpwg, dp);
                }                
            }
        }

        private void PurgeDraftWordGroup(ResultPromptWordGroupViewModel rpwg, ResultPromptRuleViewModel dp)
        {
            //first determine if there are other usages
            var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                        x => x.ResultPromptViewModel.WordGroups != null 
                          && x.UUID != dp.UUID 
                          && x.ResultPromptViewModel.WordGroups.Select(y=>y.UUID).Contains(rpwg.UUID));

            if (otherUsages.Any()) { return; }

            treeModel.AllResultPromptWordGroupViewModel.WordGroups.RemoveAll(x => x.UUID == rpwg.UUID);
            treeModel.AllData.ResultPromptWordGroups.RemoveAll(x => x.UUID == rpwg.UUID);

            //remove usage from this result prompt
            dp.ResultPromptViewModel.WordGroups.RemoveAll(x => x.UUID == rpwg.UUID);
        }

        private void PurgeDraftFixedList(ResultPromptRuleViewModel prompt)
        {
            //first determine if there are other usages
            var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                        x => x.UUID != prompt.UUID && x.ResultPromptViewModel.FixedList != null && x.ResultPromptViewModel.FixedList.UUID == prompt.ResultPromptViewModel.FixedList.UUID);

            if (otherUsages.Any()) { return; }

            treeModel.AllFixedListViewModel.FixedLists.RemoveAll(x => x.UUID == prompt.ResultPromptViewModel.FixedList.UUID);
            treeModel.AllData.FixedLists.RemoveAll(x => x.UUID == prompt.ResultPromptViewModel.FixedList.UUID);

            //remove usage from this result prompt
            prompt.ResultPromptViewModel.FixedList = null;
        }

        private void RemoveChildResultRulesFromDraft(ResultDefinitionTreeViewModel rootItem)
        {
            //remove all the results rules for the draft result from the collect of all prompts for both view models and data
            treeModel.AllResultRuleViewModel.Rules.RemoveAll(
                        x => x.ParentResultDefinitionViewModel != null && x.ParentResultDefinitionViewModel.UUID == rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            treeModel.AllData.ResultDefinitionRules.RemoveAll(x => x.ParentUUID != null && x.ParentUUID == rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
        }

        private void RemoveDraftWordGroups(ResultDefinitionTreeViewModel rootItem)
        {
            if (rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.WordGroups != null)
            {
                foreach(var wg in rootItem.ResultRuleViewModel.ChildResultDefinitionViewModel.WordGroups)
                {
                    //deal with any draft result definition word groups
                    foreach(var rdwg in wg.ResultDefinitionWordGroups.Where(x=>x.PublishedStatus == PublishedStatus.Draft))
                    {
                        //remove the parent word group and see if any other usages remain
                        rdwg.ParentWordGroups.RemoveAll(x => x.WordGroupName == wg.WordGroupName && x.ParentResultDefinitionVM.PublishedStatus == PublishedStatus.Draft);
                        if (!rdwg.ParentWordGroups.Where(x=>x.ParentResultDefinitionVM.PublishedStatus == PublishedStatus.Draft).Any())
                        {
                            //no other usages of the draft rdwg and therefore we can remove it
                            treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.RemoveAll(x => x.UUID == rdwg.UUID);
                            treeModel.AllData.ResultDefinitionWordGroups.RemoveAll(x => x.UUID == rdwg.UUID);
                        }
                    }
                }
            }
        }

        private void PurgeResult(ResultDefinitionTreeViewModel item)
        {
            //remove tree view items
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.RemoveAll(x=> x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == item.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.ToList().RemoveAll(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == item.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);

            //remove view models
            treeModel.AllResultDefinitionsViewModel.Definitions.RemoveAll(x => x.UUID == item.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            treeModel.AllResultRuleViewModel.Rules.RemoveAll(x => x.ChildResultDefinitionViewModel.UUID == item.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            treeModel.AllResultPromptViewModel.Prompts.RemoveAll(x => x.ResultPromptRule.ResultDefinitionUUID == item.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);

            //remove data
            treeModel.AllData.ResultDefinitions.RemoveAll(x => x.UUID == item.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            treeModel.AllData.ResultDefinitionRules.RemoveAll(x => x.ResultDefinition.UUID == item.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            treeModel.AllData.ResultPromptRules.RemoveAll(x => x.ResultDefinitionUUID == item.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
        }

        private void SyncroniseSubscriptions()
        {
            //find the subscriptions that have a recipient based on results
            var subscriptionRecipients = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FindAll(x => x.ResultsRecipient);

            foreach(var item in subscriptionRecipients)
            {
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.TitleResultPrompt), () => item.TitleResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.FirstNameResultPrompt), () => item.FirstNameResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.MiddleNameResultPrompt), () => item.MiddleNameResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.LastNameResultPrompt), () => item.LastNameResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.OrganisationNameResultPrompt), () => item.OrganisationNameResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.Address1ResultPrompt), () => item.Address1ResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.Address2ResultPrompt), () => item.Address2ResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.Address3ResultPrompt), () => item.Address3ResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.Address4ResultPrompt), () => item.Address4ResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.Address5ResultPrompt), () => item.Address5ResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.PostCodeResultPrompt), () => item.PostCodeResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.EmailAddress1ResultPrompt), () => item.EmailAddress1ResultPrompt = null);
                SynchroniseResultPromptRule(() => DeletedPromptRule(item.EmailAddress2ResultPrompt), () => item.EmailAddress2ResultPrompt = null);
            }

            //find any subscriptions with dependant vocabulary
            var subscriptionExcludedVocabItems =
                treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FindAll(x => x.NowSubscription.SubscriptionVocabulary != null
                        && x.NowSubscription.SubscriptionVocabulary.ExcludedPromptRules != null
                        && x.NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.Count > 0
                        );

            var subscriptionIncludedVocabItems =
                treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FindAll(x => x.NowSubscription.SubscriptionVocabulary != null
                        && x.NowSubscription.SubscriptionVocabulary.IncludedPromptRules != null
                        && x.NowSubscription.SubscriptionVocabulary.IncludedPromptRules.Count > 0
                        );

            //check each subscription to remove the result prompt dependency from the subscription vocabulary
            foreach (var item in subscriptionExcludedVocabItems)
            {
                item.NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.RemoveAll(x => DeletedPromptRule(x));
            }

            foreach (var item in subscriptionIncludedVocabItems)
            {
                item.NowSubscription.SubscriptionVocabulary.IncludedPromptRules.RemoveAll(x => DeletedPromptRule(x));
            }
        }

        private bool DeletedPromptRule(ResultPromptRule resultPrompt)
        {
            if (resultPrompt != null && resultPrompt.DeletedDate != null) { return true; }
            return false;
        }

        private void SynchroniseResultPromptRule(Func<bool> equal, Action action)
        {
            if (equal())
            {
                return;
            }

            action();
        }
    }
}
