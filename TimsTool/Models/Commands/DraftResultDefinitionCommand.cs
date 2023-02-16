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
    public interface IDraftResultDefinitionCommand : ICommand { }

    public class DraftResultDefinitionCommand : IDraftResultDefinitionCommand
    {
        private ITreeModel treeModel;
        public DraftResultDefinitionCommand(ITreeModel treeModel)
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
            var ptm = parameter == null ? null : parameter as PublicationTagsModel;

            if (ptm == null)
            {
                Log.Error("Publication Tags Model not sent for drafting");
                return;
            }
            
            //Determine the source
            var source = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

            if (source == null)
            {
                Log.Error("Result Definition not sent for drafting");
                return;
            }

            DraftResultDefinitionTreeViewModel(source, ptm.SelectedPublicationTags.ToList(), true);
        }
        
        public ResultDefinitionTreeViewModel DraftResultDefinitionTreeViewModel(ResultDefinitionTreeViewModel source, List<string> tags, bool withStorage)
        {
            //choose a published root item as the source for drafting
            var rootSource = source.IsParent ? source : treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished.FirstOrDefault(
                x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == source.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                && x.ResultRuleViewModel.ParentResultDefinitionViewModel == null);

            //Draft a new deep copy of the ResultDefinitionTreeViewModel 
            var newItem = rootSource.Draft(tags);

            if (withStorage)
            {
                SaveDraft(newItem, true);
            }

            return newItem;
        }

        public void SaveDraft(ResultDefinitionTreeViewModel newItem, bool withSelection)
        {
            //store the data
            StoreDataAndVMs(newItem, withSelection);

            //Set any existing draft child result rules with the new draft result definition
            UpdateOtherDraftResultRules(newItem);

            //Set any now requirements for draft Nows with the new draft result
            UpdateDraftNows(newItem);

            //Set any draft subscriptions with vocabulary that has result definitions with the draft version of the definition
            UpdateDraftNowSubscriptions(newItem);
        }

        private void UpdateDraftNowSubscriptions(ResultDefinitionTreeViewModel newItem)
        {
            UpdateSubscriptionResults(newItem);

            UpdateSubscriptionResultPrompts(newItem);
        }
        private void UpdateSubscriptionResultPrompts(ResultDefinitionTreeViewModel newItem)
        {
            var included = (  from sub in treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                            x => x.PublishedStatus == PublishedStatus.Draft &&
                                                 x.IncludedResults != null).Select(x => x.IncludedPrompts).SelectMany(x => x)
                              join rpr in newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts
                                on sub.ResultPrompt.MasterUUID ?? sub.ResultPrompt.UUID equals rpr.MasterUUID ?? rpr.UUID
                            select new { sub, rpr }).ToList();

            foreach (var item in included)
            {
                //reset the subscription prompt rule to the new draft version
                item.sub.ResultPrompt = item.rpr.ResultPromptRule;
            }

            var excluded = (from sub in treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                          x => x.PublishedStatus == PublishedStatus.Draft &&
                                               x.ExcludedResults != null).Select(x => x.ExcludedPrompts).SelectMany(x => x)
                            join rpr in newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts
                              on sub.ResultPrompt.MasterUUID ?? sub.ResultPrompt.UUID equals rpr.MasterUUID ?? rpr.UUID
                            select new { sub, rpr }).ToList();

            foreach (var item in excluded)
            {
                //reset the subscription prompt rule to the new draft version
                item.sub.ResultPrompt = item.rpr.ResultPromptRule;
            }
        }

        private void UpdateSubscriptionResults(ResultDefinitionTreeViewModel newItem)
        {
            var ns = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                x => x.PublishedStatus == PublishedStatus.Draft &&
                                     x.IncludedResults != null &&
                                     (
                                        x.IncludedResults.Select(y => y.ResultDefinition.MasterUUID ?? y.ResultDefinition.UUID)
                                                         .Contains(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ??
                                                                newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                                        ||
                                        x.ExcludedResults.Select(y => y.ResultDefinition.MasterUUID ?? y.ResultDefinition.UUID)
                                                         .Contains(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ??
                                                                newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                                    )
                                ).ToList();

            foreach (var sub in ns)
            {
                //check included results
                if (sub.IncludedResults != null)
                {
                    var included = sub.IncludedResults.ToList().Where(
                                            x => (x.ResultDefinition.MasterUUID ?? x.ResultDefinition.UUID) ==
                                                 (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ??
                                                    newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                                                  )
                                           );
                    foreach (var item in included)
                    {
                        item.ResultDefinition = newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition;
                    }
                }

                //check excluded results
                if (sub.ExcludedResults != null)
                {
                    var excluded = sub.ExcludedResults.ToList().Where(
                                        x => (x.ResultDefinition.MasterUUID ?? x.ResultDefinition.UUID) ==
                                             (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ??
                                                newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                                              )
                                       );
                    foreach (var item in excluded)
                    {
                        item.ResultDefinition = newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition;
                    }
                }
            }
        }

        private void UpdateDraftNows(ResultDefinitionTreeViewModel newItem)
        {
            var nrsGroup = treeModel.AllNowRequirementsViewModel.NowRequirements.Where(
                    x => x.PublishedStatus == PublishedStatus.Draft &&
                         x.ResultDefinition.MasterUUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID).GroupBy(x=>x.NOWUUID).ToList();

            //deal with any associated Now requirements
            foreach (var nrGroup in nrsGroup)
            {
                foreach (var nr in nrGroup)
                {
                    nr.ResultDefinition = newItem.ResultRuleViewModel.ChildResultDefinitionViewModel;
                }

                //force a reset of the now tree view
                var tree = MatchNOWEDT(nrGroup.First());
                tree.ResetChildren();
            }
        }

        private NowTreeViewModel MatchNOWEDT(NowRequirementViewModel nr)
        {
            var match = treeModel.Nows.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
            if (match != null) { return match; }
            return treeModel.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
        }

        private void UpdateOtherDraftResultRules(ResultDefinitionTreeViewModel source)
        {
            //inform each draft root result definition tree view of the new details
            foreach(var draftRoot in treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft)
            {
                //only when the draft result is not the new result
                if (draftRoot.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID != source.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                {
                    draftRoot.RefreshChildrenWithNewDraftResultDefinition(source.ResultRuleViewModel);
                }
            }
        }

        private void StoreDataAndVMs(ResultDefinitionTreeViewModel newItem, bool withSelection)
        {
            //Store the tree view model
            var matchedTree = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.FirstOrDefault(
                x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            if (matchedTree == null)
            {
                treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.Add(newItem);

                //add to the drafts collection when selection is required
                if (withSelection)
                {
                    treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.AddSorted(newItem);
                }
            }

            //Store the root result rule vm
            var matchedRootRule = treeModel.AllResultRuleViewModel.Rules.FirstOrDefault(x => x.ResultDefinitionRule.UUID == newItem.ResultRuleViewModel.UUID);
            if (matchedRootRule == null)
            {
                treeModel.AllResultRuleViewModel.Rules.Add(newItem.ResultRuleViewModel);
            }

            //store result rule data
            if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
            {
                foreach (var rule in newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules)
                {
                    //add data when necessary, rule VMs will have been stored as part of the draft methods
                    var matchData = treeModel.AllData.ResultDefinitionRules.FirstOrDefault(x => x.UUID == rule.UUID);
                    if (matchData == null)
                    {
                        treeModel.AllData.ResultDefinitionRules.Add(rule.ResultDefinitionRule);
                    }
                }
            }
            //store result prompt rule data, rule VMs will have been stored as part of the draft methods
            if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
            {
                foreach (var rule in newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts)
                {
                    //add data when necessary
                    var matchData = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => x.UUID == rule.UUID);
                    if (matchData == null)
                    {
                        treeModel.AllData.ResultPromptRules.Add(rule.ResultPromptRule);
                    }
                }
            }

            //Store the new draft result definition VM
            var matchedRDVM = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            if (matchedRDVM == null)
            {
                treeModel.AllResultDefinitionsViewModel.Definitions.Add(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel);                
            }

            //Store the new draft result definition Data
            var matchedRDData = treeModel.AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            if (matchedRDData == null)
            {
                treeModel.AllData.ResultDefinitions.Add(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition);
            }

            //Select the new item
            if (withSelection)
            {
                newItem.IsSelected = true;
            }
        }
    }
}
