using Models.ViewModels;
using System;
using System.Windows.Input;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using DataLib;
using System.Collections.Generic;

namespace Models.Commands
{
    public interface IDeleteResultPromptCommand : ICommand { }

    public class DeleteResultPromptCommand : IDeleteResultPromptCommand
    {
        private ITreeModel treeModel;
        public DeleteResultPromptCommand(ITreeModel treeModel)
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
            var selectedNowSubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            bool isExcludedPrompts;
            bool isIncludedPrompts;
            if (selectedNowSubscription != null)
            {
                //set included/excluded based on command parameter
                if (parameter == null) { return; }
                var includedExcluded = ((string)parameter).ToLowerInvariant();
                if (includedExcluded == "excluded")
                {
                    isExcludedPrompts = true;
                    isIncludedPrompts = false;
                }
                else
                {
                    isIncludedPrompts = true;
                    isExcludedPrompts = false;
                }

                DeleteNowSubscriptionPromptRule(isExcludedPrompts, isIncludedPrompts, selectedNowSubscription);
            }
            else
            {
                DeleteResultPromptTreeItem();
            }
        }

        private void DeleteNowSubscriptionPromptRule(bool isExcludedPrompts, bool isIncludedPrompts, NowSubscriptionTreeViewModel selectedNowSubscription)
        {
            if (isExcludedPrompts)
            {
                if (selectedNowSubscription.NowSubscriptionViewModel.SelectedNowExcludedPromptValue == null) { return; }
                selectedNowSubscription.NowSubscriptionViewModel.ExcludedPrompts.Remove(selectedNowSubscription.NowSubscriptionViewModel.SelectedNowExcludedPromptValue);
                selectedNowSubscription.NowSubscriptionViewModel.SelectedNowExcludedPromptValue = null;
            }

            if (isIncludedPrompts)
            {
                if (selectedNowSubscription.NowSubscriptionViewModel.SelectedNowIncludedPromptValue == null) { return; }
                selectedNowSubscription.NowSubscriptionViewModel.IncludedPrompts.Remove(selectedNowSubscription.NowSubscriptionViewModel.SelectedNowIncludedPromptValue);
                selectedNowSubscription.NowSubscriptionViewModel.SelectedNowIncludedPromptValue = null;
            }
        }

        private void DeleteResultPromptTreeItem()
        {
            //Determine the selected item
            var selectedResultPrompt = treeModel.SelectedItem as ResultPromptTreeViewModel;

            //get the parent
            var parent = selectedResultPrompt.Parent as ResultDefinitionTreeViewModel;
            if (parent == null) { return; }

            //determine if we are deleting an already deleted item
            if (selectedResultPrompt.ResultPromptRuleViewModel.IsDeleted)
            {
                PurgeResultPrompt(selectedResultPrompt, parent);
            }
            else
            {
                //determine if we are dealing with a newly created draft prompt
                DeleteResultPrompt(selectedResultPrompt, parent);
            }            

            //Synchronise the Nows
            SynchroniseNows(selectedResultPrompt);

            //Deal with any subscriptions removing dependencies on the deleted result prompt rule
            //SynchroniseSubscriptions(rpr);

            //select the parent
            parent.IsSelected = true;
        }

        private void DeleteResultPrompt(ResultPromptTreeViewModel selectedResultPrompt, ResultDefinitionTreeViewModel parent)
        {
            //set the prompt rule as logically deleted
            selectedResultPrompt.ResultPromptRuleViewModel.DeletedDate = DateTime.Now;

            //Set the parent as modified
            parent.ResultRuleViewModel.ChildResultDefinitionViewModel.LastModifiedDate = DateTime.Now;

            //determine if the prompt is now an orphan by looking for other parents, when an orphan tidy up its children
            if (treeModel.AllResultPromptViewModel.GetOtherParents(selectedResultPrompt).Count == 0)
            {
                //No other parents, set the prompt as logically deleted
                selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.DeletedDate = DateTime.Now;

                //check to see if the child fixed list is now also orphaned
                CheckFixedList(selectedResultPrompt);

                //see if the child word groups are now orphaned
                CheckWordGroups(selectedResultPrompt);
            }
        }

        private void CheckWordGroups(ResultPromptTreeViewModel selectedResultPrompt)
        {
            if (selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups != null)
            {
                foreach (var rpwg in selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups)
                {
                    var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                                x => x.ResultPromptViewModel.UUID != selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.UUID &&
                                x.ResultPromptViewModel.WordGroups != null &&
                                x.ResultPromptViewModel.WordGroups.Select(y => y.UUID).Contains(rpwg.UUID));
                    if (!otherUsages.Any())
                    {
                        //set the word group view models as logically deleted
                        var wgs = treeModel.AllResultPromptWordGroupViewModel.WordGroups.FindAll(
                                x => x.UUID == rpwg.UUID);
                        SetWordGroupDeleted(wgs);
                    }
                }
            }
        }

        private void CheckFixedList(ResultPromptTreeViewModel selectedResultPrompt)
        {
            if (selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList != null)
            {
                var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                                x => x.ResultPromptViewModel.UUID != selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.UUID &&
                                x.ResultPromptViewModel.FixedList != null &&
                                x.ResultPromptViewModel.FixedList.UUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList.UUID);
                if (!otherUsages.Any())
                {
                    //set the fixed list view model as logically deleted
                    var fls = treeModel.AllFixedListViewModel.FixedLists.FindAll(
                            x => x.UUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList.UUID);
                    SetFixedListDeleted(fls);
                }
            }
        }

        private void PurgeResultPrompt(ResultPromptTreeViewModel selectedResultPrompt, ResultDefinitionTreeViewModel parent)
        {
            //remove the prompt view model and the data
            treeModel.AllResultPromptViewModel.Prompts.RemoveAll(x => x.UUID == selectedResultPrompt.ResultPromptRuleViewModel.UUID);
            parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.RemoveAll(x => x.UUID == selectedResultPrompt.ResultPromptRuleViewModel.UUID);

            //tidy up the tree model for each instance of a treeview model for the parent item
            RemoveFromTreeViewModel(parent, selectedResultPrompt);

            //determine if the prompt is now an orphan by looking for other parents, when an orphan tidy up its children
            if (treeModel.AllResultPromptViewModel.GetOtherParents(selectedResultPrompt).Count == 0)
            {
                //No other parents, check to see if the child fixed list is now also orphaned

                //check to see if the child fixed list is now also orphaned
                CheckFixedList(selectedResultPrompt);

                //see if the child word groups are now orphaned
                CheckWordGroups(selectedResultPrompt);
            }
        }

        private static void SetWordGroupDeleted(List<ResultPromptWordGroupViewModel> wgs)
        {
            //update the data
            foreach (var wg in wgs)
            {
                wg.DeletedDate = DateTime.Now;

                if (wg.Synonyms != null)
                {
                    foreach (var val in wg.Synonyms)
                    {
                        val.DeletedDate = DateTime.Now;
                        val.LastModifiedDate = DateTime.Now;
                    }
                }
            }
        }

        private static void SetFixedListDeleted(List<FixedListViewModel> fls)
        {
            //update the data
            foreach (var fl in fls)
            {
                fl.DeletedDate = DateTime.Now;

                if (fl.FixedListValues != null)
                {
                    foreach (var val in fl.FixedListValues)
                    {
                        val.DeletedDate = DateTime.Now;
                        val.LastModifiedDate = DateTime.Now;
                    }
                }
            }
        }

        private void SynchroniseSubscriptions(ResultPromptRule rpr)
        {
            //find any subscriptions with dependant recipients
            var subscriptionRecipients = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FindAll(x => x.ResultsRecipient);

            //check each subscription to remove the result prompt dependency from the subscription recipient
            foreach (var item in subscriptionRecipients)
            {
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.TitleResultPrompt, rpr), () => item.TitleResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.FirstNameResultPrompt, rpr), () => item.FirstNameResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.MiddleNameResultPrompt, rpr), () => item.MiddleNameResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.LastNameResultPrompt, rpr), () => item.LastNameResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.OrganisationNameResultPrompt, rpr), () => item.OrganisationNameResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.Address1ResultPrompt, rpr), () => item.Address1ResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.Address2ResultPrompt, rpr), () => item.Address2ResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.Address3ResultPrompt, rpr), () => item.Address3ResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.Address4ResultPrompt, rpr), () => item.Address4ResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.Address5ResultPrompt, rpr), () => item.Address5ResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.PostCodeResultPrompt, rpr), () => item.PostCodeResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.EmailAddress1ResultPrompt, rpr), () => item.EmailAddress1ResultPrompt = null);
                SynchroniseResultPromptRule(() => MatchedPromptRule(item.EmailAddress2ResultPrompt, rpr), () => item.EmailAddress2ResultPrompt = null);
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
                item.NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.RemoveAll(x => MatchedPromptRule(x, rpr));               
            }

            foreach (var item in subscriptionIncludedVocabItems)
            {
                item.NowSubscription.SubscriptionVocabulary.IncludedPromptRules.RemoveAll(x => MatchedPromptRule(x, rpr));
            }
        }

        private bool MatchedPromptRule(ResultPromptRule rpr, ResultPromptRule deletedPrompt)
        {
            return rpr != null && rpr.ResultDefinitionUUID == deletedPrompt.ResultDefinitionUUID && rpr.ResultPromptUUID == deletedPrompt.ResultPromptUUID;
        }

        private void SynchroniseResultPromptRule(Func<bool> equal, Action action)
        {
            if (equal())
            {
                return;
            }

            action();
        }

        private void SynchroniseNows(ResultPromptTreeViewModel selectedResultPrompt)
        {
            //Find any now Requirements for the parent result definition
            var matchedNowRequirements = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(x => x.ResultDefinition.UUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptRule.ResultDefinitionUUID);

            //When drafting mode then only deal with those requirements that are status draft
            matchedNowRequirements.RemoveAll(x => x.PublishedStatus != PublishedStatus.Draft);

            foreach (var nr in matchedNowRequirements)
            {
                //get the now or edt tree item
                var matchedNow = MatchNOWEDT(nr);
                if (matchedNow != null)
                {
                    if (!matchedNow.IsExpanded)
                    {
                        matchedNow.IsExpanded = true;
                    }
                    RecursivelyRemoveFromNowRequirement(matchedNow.Children, selectedResultPrompt);
                }
            }
        }
        
        private NowTreeViewModel MatchNOWEDT(NowRequirementViewModel nr)
        {
            var match = treeModel.Nows.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
            if (match != null) { return match; }
            return treeModel.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
        }

        private void RecursivelyRemoveFromNowRequirement(SilentObservableCollection<TreeViewItemViewModel> children, ResultPromptTreeViewModel selectedResultPrompt)
        {
            foreach (var child in children)
            {
                if (child is NowRequirementTreeViewModel)
                {
                    var nrItem = child as NowRequirementTreeViewModel;
                    if (!nrItem.IsExpanded)
                    {
                        nrItem.IsExpanded = true;
                    }
                    if (nrItem.NowRequirementViewModel.ResultDefinition.UUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptRule.ResultDefinitionUUID)
                    {
                        RemoveFromTreeViewModel(nrItem, selectedResultPrompt);
                    }
                    RecursivelyRemoveFromNowRequirement(nrItem.Children, selectedResultPrompt);
                }
            }
        }

        private void RemoveFromTreeViewModel(NowRequirementTreeViewModel parent, ResultPromptTreeViewModel selectedResultPrompt)
        {
            int index;
            //find the selected prompt in the tree of children
            for (index = 0; index < parent.Children.Count; index++)
            {
                var promptChild = parent.Children[index] as NowRequirementPromptRuleTreeViewModel;
                if (promptChild != null && promptChild.NowRequirementPromptRuleViewModel.NowRequirementPromptRule.ResultPromptRule.ResultPromptUUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.UUID)
                {
                    //index is the location of the prompt for removal
                    parent.Children.RemoveAt(index);

                    //logically delete from parent view model
                    parent.NowRequirementViewModel.NowRequirementPromptRules.Where(x => x.NowRequirementPromptRule.ResultPromptRule.ResultPromptUUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.UUID).ToList().ForEach(x => x.DeletedDate = DateTime.Now);

                    break;
                }
            }
        }

        private void RemoveFromTreeViewModel(ResultDefinitionTreeViewModel parent, ResultPromptTreeViewModel selectedResultPrompt)
        {
            int index;
            //find the selected prompt in the tree of children
            for (index = 0; index < parent.Children.Count; index++)
            {
                var promptChild = parent.Children[index] as ResultPromptTreeViewModel;
                if (promptChild != null && promptChild.ResultPromptRuleViewModel.ResultPromptViewModel.UUID == selectedResultPrompt.ResultPromptRuleViewModel.ResultPromptViewModel.UUID)
                {
                    //index is the location of the prompt for removal
                    parent.Children.RemoveAt(index);
                    break;
                }
            }

            //select the next node
            if (parent.Children.Count - 1 <= index && index > 0)
            {
                index = index - 1;
            }
            parent.Children[index].IsSelected = true;
        }
    }
}
