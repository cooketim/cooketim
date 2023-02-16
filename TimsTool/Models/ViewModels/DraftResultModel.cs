using DataLib;
using Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Models.ViewModels
{
    public class DraftResultModel
    {
        private bool changesSet;
        private ITreeModel treeModel;
        public DraftResultModel(ITreeModel treeModel, ResultDefinitionTreeViewModel source, bool isInDraftAlready)
        {
            this.treeModel = treeModel;
            Result = source;
            DeletedResultRules = new List<ResultRuleViewModel>();
            DeletedPromptRules = new List<ResultPromptRuleViewModel>();
            ChangedPromptRules = new List<DraftPromptRuleModel>();
            ChangedResultRules = new List<DraftResultRuleModel>();
            UnchangedPromptRules = new List<DraftPromptRuleModel>();
            UnchangedResultRules = new List<DraftResultRuleModel>();
            ChangedResultDefinitionWordGroups = new List<DraftResultDefinitionWordGroupModel>();
            UnchangedResultDefinitionWordGroups = new List<DraftResultDefinitionWordGroupModel>();
            NowUsages = new List<DraftNowModel>();
            NowSubscriptionUsages = new List<DraftNowSubscriptionModel>();
            IsInDraftAlready = isInDraftAlready;
            AdditionalDraftChildren = new List<DraftResultModel>();
        }

        public ResultDefinitionTreeViewModel Result { get; }
        public List<DraftResultModel> AdditionalDraftChildren { get; private set; }
        public bool IsInDraftAlready { get; }
        public bool IsResultChanged { get; set; }
        public bool IsResultAdded { get; set; }

        public bool IsDeleted
        {
            get
            {
                return Result.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted;
            }
        }

        public bool HasStructuralChanges
        {
            get
            {
                if (IsPromptRulesAdded || IsPromptRulesDeleted || IsResultRulesAdded || IsResultRulesDeleted)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsPromptRulesChanged
        {
            get
            {
                if (ChangedPromptRules.Where(x=>x.IsChanged).Any())
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsPromptRulesAdded
        {
            get
            {
                if (ChangedPromptRules.Where(x => x.IsRuleAdded).Any())
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsPromptRulesDeleted
        {
            get
            {
                if (DeletedPromptRules.Any())
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsResultRulesChanged
        {
            get
            {
                if (ChangedResultRules.Where(x => x.IsChanged).Any())
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsResultRulesAdded
        {
            get
            {
                if (ChangedResultRules.Where(x => x.IsRuleAdded).Any())
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsResultRulesDeleted
        {
            get
            {
                if (DeletedResultRules.Any())
                {
                    return true;
                }
                return false;
            }
        }                

        public bool IsResultDefinitionWordGroupsChangedWithWordChanges
        {
            get
            {
                if (ChangedResultDefinitionWordGroups.Where(x=>x.IsChangedWithWordChanges).Any())
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsResultDefinitionWordGroupsChanged
        {
            get
            {
                if (ChangedResultDefinitionWordGroups.Where(x => x.IsChanged).Any())
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsChanged
        {
            get
            {
                return IsResultChanged || 
                       IsResultRulesChanged || 
                       IsPromptRulesChanged || 
                       IsResultDefinitionWordGroupsChangedWithWordChanges ||
                       IsResultRulesDeleted ||
                       IsPromptRulesDeleted;
            }
        }

        public string ChangeReport { get; set; }

        public List<ResultRuleViewModel> DeletedResultRules { get; }
        public List<ResultPromptRuleViewModel> DeletedPromptRules { get; }
        public List<DraftPromptRuleModel> ChangedPromptRules { get; }
        public List<DraftResultRuleModel> ChangedResultRules { get; }
        public List<DraftPromptRuleModel> UnchangedPromptRules { get; }
        public List<DraftResultRuleModel> UnchangedResultRules { get; }
        public List<DraftResultDefinitionWordGroupModel> ChangedResultDefinitionWordGroups { get; }
        public List<DraftResultDefinitionWordGroupModel> UnchangedResultDefinitionWordGroups { get; }
        public List<NowRequirementViewModel> NowRequirementsUpdatePending { get; private set; }
        public List<DraftNowModel> NowUsages { get; private set; }
        public List<NowSubscriptionViewModel> NowSubscriptionsUpdatePending { get; private set; }
        public List<DraftNowSubscriptionModel> NowSubscriptionUsages { get; private set; }
        public ResultDefinitionTreeViewModel ExistingPublishedResult { get; private set; }   

        internal void PrepareForPublication()
        {
            if (ExistingPublishedResult != null)
            {
                //existing published moves to revision status
                SetResultPublishedStatus(ExistingPublishedResult, PublishedStatus.RevisionPending);
                ExistingPublishedResult.ResetChildren();
            }

            //new draft moves to published pending status
            SetResultPublishedStatus(Result, PublishedStatus.PublishedPending);

            //deal with the changed and unchanged rules
            foreach (var prompt in ChangedPromptRules)
            {
                //prepare the changed prompt rule & prompt for publication
                prompt.PrepareForPublication();
                
                if (prompt.IsPromptChanged)
                {
                    //prepare the prompt for publication
                    prompt.DraftPrompt.PrepareForPublication();
                }
            }

            foreach (var prompt in UnchangedPromptRules)
            {
                //the unchanged prompt rule has a different parent id and therefore still needs to be published as a change
                prompt.PrepareForPublication();
            }

            foreach (var resultRule in ChangedResultRules)
            {
                //prepare the changed result rule for publication
                resultRule.PrepareForPublication();
            }

            foreach (var resultRule in UnchangedResultRules)
            {
                //the unchanged result rule has a different parent id and therefore still needs to be published as a change
                resultRule.PrepareForPublication();
            }

            //deal with the changed and unchanged result definition word groups
            foreach(var rdwg in ChangedResultDefinitionWordGroups)
            {
                rdwg.PrepareForPublication();
            }

            foreach (var rdwg in UnchangedResultDefinitionWordGroups)
            {
                //the unchanged result definition word groups need to be readded to the relevant result word group
                rdwg.ResetPublished();
            }

            //deal with any deleted prompt rules
            if (!Result.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted && DeletedPromptRules.Any())
            {
                ProcessDeletedPromptRules();
            }

            //deal with any deleted result rules
            if (!Result.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted && DeletedResultRules.Any())
            {
                ProcessDeletedResultRules();
            }

            //deal with any pending non structural Now requirement changes
            if (NowRequirementsUpdatePending != null)
            {
                foreach(var nr in NowRequirementsUpdatePending.Where(x=>!x.IsRevision && !x.IsRevisionPending))
                {
                    nr.ResultDefinition = Result.ResultRuleViewModel.ChildResultDefinitionViewModel;
                }
            }

            //deal with any pending non structural Now Subscription changes
            if (NowSubscriptionsUpdatePending != null)
            {
                foreach (var ns in NowSubscriptionsUpdatePending.Where(x => !x.IsRevision && !x.IsRevisionPending))
                {
                    ns.ResetRules(Result.ResultRuleViewModel.ChildResultDefinitionViewModel);                    
                }
            }

            SetPublicationComment();
        }

        private void ProcessDeletedResultRules()
        {
            //update the view models
            Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.RemoveAll(x => x.IsDeleted);

            foreach (var rule in DeletedResultRules)
            {
                //Remove from the collection of all rules
                treeModel.AllResultRuleViewModel.Rules.RemoveAll(x => x.UUID == rule.UUID);

                //Remove the tree item for this rule
                if (!Result.HasDummyChild)
                {
                    Result.Children.RemoveAll(
                                                x => x.GetType() == typeof(ResultDefinitionTreeViewModel) &&
                                                    ((ResultDefinitionTreeViewModel)x).ResultRuleViewModel.UUID == rule.UUID);
                }
            }
        }

        private void ProcessDeletedPromptRules()
        {
            //update the view models
            Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.RemoveAll(x => x.IsDeleted);

            foreach (var rpr in DeletedPromptRules)
            {
                //Remove from the collection of all prompts view models and data
                treeModel.AllResultPromptViewModel.Prompts.RemoveAll(x => x.UUID == rpr.UUID);
                treeModel.AllData.ResultPromptRules.RemoveAll(x => x.UUID == rpr.UUID);

                //Remove the tree item for this prompt
                if (!Result.HasDummyChild)
                {
                    Result.Children.RemoveAll(
                                                x => x.GetType() == typeof(ResultPromptTreeViewModel) &&
                                                    ((ResultPromptTreeViewModel)x).ResultPromptRuleViewModel.UUID == rpr.UUID);
                }

                //if the deleted prompt is a draft deal with any orphans
                if (rpr.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
                {
                    //determine if there is a draft fixed list that is now orphaned
                    if (rpr.ResultPromptViewModel.FixedList != null && rpr.ResultPromptViewModel.FixedList.PublishedStatus == PublishedStatus.Draft)
                    {
                        var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                                    x => x.ResultPromptViewModel.FixedList != null &&
                                         x.ResultPromptViewModel.FixedList.UUID == rpr.ResultPromptViewModel.FixedList.UUID);
                        if (!otherUsages.Any())
                        {
                            //remove orphan view model and data
                            treeModel.AllFixedListViewModel.FixedLists.RemoveAll(x => x.UUID == rpr.ResultPromptViewModel.FixedList.UUID);
                            treeModel.AllData.FixedLists.RemoveAll(x => x.UUID == rpr.ResultPromptViewModel.FixedList.UUID);
                        }
                    }

                    if (rpr.ResultPromptViewModel.WordGroups != null && rpr.ResultPromptViewModel.WordGroups.Where(x=>x.PublishedStatus == PublishedStatus.Draft).Any())
                    {
                        foreach (var rpwg in rpr.ResultPromptViewModel.WordGroups.Where(x => x.PublishedStatus == PublishedStatus.Draft))
                        {
                            var otherUsages = treeModel.AllResultPromptViewModel.Prompts.Where(
                                                    x => x.ResultPromptViewModel.WordGroups != null &&
                                                        x.ResultPromptViewModel.WordGroups.Select(y => y.UUID).Contains(rpwg.UUID));
                            if (!otherUsages.Any())
                            {
                                //remove orphan view model and data
                                treeModel.AllResultPromptWordGroupViewModel.WordGroups.RemoveAll(x => x.UUID == rpwg.UUID);
                                treeModel.AllData.ResultPromptWordGroups.RemoveAll(x => x.UUID == rpwg.UUID);
                            }
                        }
                    }
                }
            }
        }

        private void SetPublicationComment()
        {
            //set the publication comment
            var parent = Result.ResultRuleViewModel.ChildResultDefinitionViewModel;
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Draft revision published pending at {0}.", parent.PublishedStatusDate.ToString("d MMM yyyy h:mm:ss tt")));
            sb.AppendLine(string.Format("New revision identifier: '{0}'.", parent.UUID));
            if (parent.PublicationTags != null && parent.PublicationTags.Count > 0)
            {
                sb.AppendLine("The following publication tags were applied...");
                foreach (var tag in parent.PublicationTags)
                {
                    sb.AppendLine(tag);
                }
            }
            sb.AppendLine();
            sb.AppendLine("The following property changes were applied...");
            sb.AppendLine(ChangeReport);

            //deal with the changed prompt rules
            if (ChangedPromptRules.Count > 0)
            {
                foreach (var prompt in ChangedPromptRules)
                {
                    //append the publication comment with any child rule changes
                    if (prompt.IsRuleChanged)
                    {
                        sb.AppendLine(string.Format("Child prompt '{0}' changes...", prompt.ExistingPublishedResultPromptRule.ResultPromptViewModel.Label));
                        sb.AppendLine(prompt.ChangeReport);
                    }
                    if (prompt.IsRuleAdded)
                    {
                        sb.AppendLine(string.Format("Child prompt '{0}' added...", prompt.Rule.ResultPromptViewModel.Label));
                    }
                    if (prompt.IsPromptChanged)
                    {
                        if (prompt.IsPromptAdded)
                        {
                            sb.AppendLine(string.Format("New prompt '{0}' added, please review prompt comments for details", prompt.Rule.ResultPromptViewModel.Label));
                        }
                        else
                        {
                            sb.AppendLine(string.Format("Prompt '{0}' properties changed, please review prompt comments for details", prompt.Rule.ResultPromptViewModel.Label));
                        }

                        //set the publication comment on the result prompt
                        prompt.DraftPrompt.SetPublicationComment();
                    }
                }
            }

            if (ChangedResultRules.Count > 0)
            {
                sb.AppendLine();
                foreach (var resultRule in ChangedResultRules)
                {
                    //append the publication comment
                    sb.AppendLine(string.Format("Child result rule '{0}' changes...", resultRule.Rule.ChildResultDefinitionViewModel.Label));
                    sb.AppendLine(resultRule.ChangeReport);
                    sb.AppendLine();
                }
            }

            //create a new comment 
            var newComment = new CommentViewModel(parent);

            //remove double spacing and set the comment note and system type
            newComment.Note = Regex.Replace(sb.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            newComment.SystemCommentType = "Publication";

            //add comment to the parents collection of comments
            parent.AssociatedCommentsModel.AddCommentCommand.Execute(newComment);

            //reset the children on the result so that they are rebuilt
            Result.ResetChildren();
        }

        private void SetResultPublishedStatus(ResultDefinitionTreeViewModel result, PublishedStatus status)
        {
            //result definition status
            result.ResultRuleViewModel.PublishedStatus = status;
            result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus = status;

            //reset any flags for rendering purposes
            result.ResultRuleViewModel.ResetStatus();

            //any child result rules status
            if (result.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
            {
                foreach (var rule in result.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules)
                {
                    rule.PublishedStatus = status;
                    rule.ResetStatus();
                }
            }

            //any child prompt rules status
            if (result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
            {
                foreach (var prompt in result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts)
                {
                    prompt.PublishedStatus = status;
                }
            }
        }

        internal void SetChanges(List<DraftPromptModel> allDraftPrompts, List<DraftNowModel> allDraftNows, List<DraftNowSubscriptionModel> allDraftNowSubscriptions)
        {
            if (changesSet) { return; }

            changesSet = true;

            //compare data to existing published result
            var publishedRd = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished.FirstOrDefault(
                x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID &&
                    (x.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Published || x.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.PublishedPending));

            if (publishedRd == null)
            {
                //draft must be a new item
                IsResultChanged = true;
                IsResultAdded = true;
                ChangeReport = "New Result Added";
                return;
            }

            //set the existingPublished on the draft result
            ExistingPublishedResult = publishedRd;

            //when the result is deleted no need to detail any further changes to the result
            if (Result.ResultRuleViewModel.ChildResultDefinitionViewModel.IsDeleted)
            {
                //draft is a deleted item intending to remove the current published item
                IsResultChanged = true;
                ChangeReport = "Result Deleted";

                //implicitly all of the rules are deleted
                DeletedResultRules.Add(Result.ResultRuleViewModel);
                DeletedResultRules.AddRange(Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules);
                DeletedPromptRules.AddRange(Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts);
            }
            else
            {
                //compare the result details
                CompareResultDetails(publishedRd, allDraftPrompts);
            }            

            //Deal with any associated Now Requirements that are already published.  
            //When there are not any structural changes to the result we will simply update the association upon publication.
            //When there are structural changes to the result there will need to be corresponding structural changes made to the NOW
            SetAssociatedNows(allDraftNows);

            //Deal with any associated Now Subscriptions
            //When there are not any structural changes to the result we will simply update the association upon publication.
            //When there are structural changes that are removals of prompts or child results there will need to be corresponding structural changes made to the NOW Subscription
            SetAssociatedNowSubscriptions(allDraftNowSubscriptions);
        }

        private void CompareResultDetails(ResultDefinitionTreeViewModel publishedRd, List<DraftPromptModel> allDraftPrompts)
        {
            var publishedData = publishedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition;
            var draftData = Result.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition;

            if (publishedData != draftData)
            {
                //changes made, make a change report
                IsResultChanged = true;
                ChangeReport = publishedData.GetChangedProperties(draftData);
            }

            //collect changed result rules
            if (Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
            {
                foreach (var rule in Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules)
                {
                    //deal with any deletions
                    if (rule.IsDeleted)
                    {
                        DeletedResultRules.Add(rule);
                        continue;
                    }

                    var draftResultRule = new DraftResultRuleModel(treeModel, rule);
                    if (draftResultRule.IsChanged)
                    {
                        ChangedResultRules.Add(draftResultRule);
                    }
                    else
                    {
                        UnchangedResultRules.Add(draftResultRule);
                    }
                }

                //determine deleted result rules by way of determining published rules that no longer exist in the draft result
                if (publishedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
                {
                    var onlyPublished = publishedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Select(x => x.MasterUUID).ToList()
                                                    .Except(
                                                Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Select(x => x.MasterUUID).ToList()).ToList();

                    foreach (var rdrId in onlyPublished)
                    {
                        var match = DeletedResultRules.FirstOrDefault(x => x.MasterUUID == rdrId);
                        if (match == null)
                        {
                            DeletedResultRules.Add(publishedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.First(x => x.MasterUUID == rdrId));
                        }
                    }
                }
            }       

            if (Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
            {
                foreach (var prompt in Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts)
                {
                    //deal with any deletions
                    if (prompt.IsDeleted)
                    {
                        DeletedPromptRules.Add(prompt);
                        continue;
                    }

                    //set changed and unchanged collections
                    var draftPromptRule = new DraftPromptRuleModel(treeModel, prompt, allDraftPrompts);
                    if (draftPromptRule.IsChanged)
                    {
                        ChangedPromptRules.Add(draftPromptRule);
                    }
                    else
                    {
                        UnchangedPromptRules.Add(draftPromptRule);
                    }
                }

                //determine deleted prompt rules by way of determining published rules that no longer exist in the draft result
                if (publishedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
                {
                    var onlyPublished = publishedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.Select(x => x.MasterUUID).ToList()
                                                    .Except(
                                                Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.Select(x => x.MasterUUID).ToList()).ToList();

                    foreach (var rprId in onlyPublished)
                    {
                        var match = DeletedPromptRules.FirstOrDefault(x => x.MasterUUID == rprId);
                        if (match == null)
                        {
                            DeletedPromptRules.Add(publishedRd.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.First(x => x.MasterUUID == rprId));
                        }
                    }
                }
            }

            //collect changed word groups
            if (Result.ResultRuleViewModel.ChildResultDefinitionViewModel.WordGroups != null)
            {
                foreach (var rwg in Result.ResultRuleViewModel.ChildResultDefinitionViewModel.WordGroups)
                {
                    foreach (var rdwg in rwg.ResultDefinitionWordGroups.Where(x => x.PublishedStatus == PublishedStatus.Draft && !x.IsDeleted))
                    {
                        //determine if the result word group has changed
                        var publishedRdWordGroup = treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.FirstOrDefault(
                            x => x.MasterUUID == rdwg.MasterUUID && x.PublishedStatus == PublishedStatus.Published);

                        if (publishedRdWordGroup == null)
                        {
                            //must be a new result definition word group
                            ChangedResultDefinitionWordGroups.Add(item: new DraftResultDefinitionWordGroupModel(rdwg, null, true, true, rwg, string.Format("New Result Definition Word '{0}' Added", rdwg.ResultDefinitionWord)));
                        }
                        else
                        {
                            //determine if the data for the existing published and this draft are different
                            if (publishedRdWordGroup.ResultDefinitionWordGroup != rdwg.ResultDefinitionWordGroup)
                            {
                                var changes = publishedRdWordGroup.ResultDefinitionWordGroup.GetChangedProperties(rdwg.ResultDefinitionWordGroup);

                                if (publishedRdWordGroup.ResultDefinitionWordGroup.WordChanged(rdwg.ResultDefinitionWordGroup))
                                {
                                    //changes need to be applied when publishing is confirmed
                                    ChangedResultDefinitionWordGroups.Add(new DraftResultDefinitionWordGroupModel(rdwg, publishedRdWordGroup, true, true, rwg, changes));
                                }
                                else
                                {
                                    ChangedResultDefinitionWordGroups.Add(new DraftResultDefinitionWordGroupModel(rdwg, publishedRdWordGroup, true, false, rwg, changes));
                                }
                            }
                            else
                            {
                                //no changes needed, restore the original rdwg when publishing is confirmed
                                UnchangedResultDefinitionWordGroups.Add(new DraftResultDefinitionWordGroupModel(rdwg, publishedRdWordGroup, false, false, rwg, null));
                            }
                        }
                    }
                }
            }
        }

        private void SetAssociatedNowSubscriptions(List<DraftNowSubscriptionModel> allDraftNowSubscriptions)
        {
            if (IsPromptRulesDeleted)
            {
                //deal with any deleted result prompt rules
                SetDeletedChangesOnAssociatedNowSubscriptions(allDraftNowSubscriptions);
            }

            //deal with any associated now subscriptions that are not drafted but contain references to the new set of draft rules for this result
            //these rules will need to be updated when the result is published as they are simple association updates rather than anything structural
            SetNowSubscriptionsUpdatePending(allDraftNowSubscriptions);
        }

        private void SetNowSubscriptionsUpdatePending(List<DraftNowSubscriptionModel> allDraftNowSubscriptions)
        {
            NowSubscriptionsUpdatePending = new List<NowSubscriptionViewModel>();

            //deal with matches by prompt rule
            var allMatches = GetPublishedNowSubscriptionsByPromptRules(Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.ToList());

            //exclude any draft now subscriptions because we are only interested in identifying subscriptions that will have updates pending that are non structural i.e. just 
            //updating an association to the latest result prompt rule
            var nonDraftedIds = allMatches.Select(x => x.MasterUUID ?? x.UUID).Except(allDraftNowSubscriptions.Select(x => x.DraftNowSub.NowSubscriptionViewModel.MasterUUID ?? x.DraftNowSub.NowSubscriptionViewModel.UUID)).ToList();
            foreach(var nsId in nonDraftedIds)
            {
                NowSubscriptionsUpdatePending.Add(allMatches.First(x => (x.MasterUUID ?? x.UUID) == nsId));
            }

            //deal with any additional matches by result
            NowSubscriptionsUpdatePending.AddRange(treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                    x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) &&
                                        x.NowSubscription.SubscriptionVocabulary != null &&
                                        (
                                            (
                                                x.NowSubscription.SubscriptionVocabulary.IncludedResults != null &&
                                                x.NowSubscription.SubscriptionVocabulary.IncludedResults.Select(y => y.MasterUUID ?? y.UUID).Contains(
                                                        Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ?? Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                                            )
                                            ||
                                            (
                                                x.NowSubscription.SubscriptionVocabulary.ExcludedResults != null &&
                                                x.NowSubscription.SubscriptionVocabulary.ExcludedResults.Select(y => y.MasterUUID ?? y.UUID).Contains(
                                                        Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ?? Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                                            )
                                        ) &&
                                        !allDraftNowSubscriptions.Select(y=> y.DraftNowSub.NowSubscriptionViewModel.MasterUUID).Contains(x.MasterUUID ?? x.UUID) &&
                                        !NowSubscriptionsUpdatePending.Select(y=> y.MasterUUID ?? y.UUID).Contains(x.MasterUUID ?? x.UUID)
                                        ));
        }

        private List<NowSubscriptionViewModel> GetPublishedNowSubscriptionsByPromptRules(List<ResultPromptRuleViewModel> rules)
        {
            var allMatches = new List<NowSubscriptionViewModel>();
            foreach (var rpr in rules)
            {
                //look for now recipient usage
                allMatches.AddRange(treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                    x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) &&
                                        !allMatches.Select(y => y.UUID).Contains(x.UUID) &&
                                        x.NowSubscription.NowRecipient != null &&
                                        x.NowSubscription.NowRecipient.IsPromptRuleSetForNameAddress(rpr.ResultPromptRule)));

                //look for now subscription vocabulary usage
                allMatches.AddRange(treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Where(
                                    x => (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending) &&
                                        !allMatches.Select(y => y.UUID).Contains(x.UUID) &&
                                        x.NowSubscription.SubscriptionVocabulary != null &&
                                        (
                                            (
                                                x.NowSubscription.SubscriptionVocabulary.IncludedPromptRules != null &&
                                                x.NowSubscription.SubscriptionVocabulary.IncludedPromptRules.Select(y => y.MasterUUID ?? y.UUID).Contains(rpr.ResultPromptRule.MasterUUID ?? rpr.ResultPromptRule.UUID)
                                            )
                                            ||
                                            (
                                                x.NowSubscription.SubscriptionVocabulary.ExcludedPromptRules != null &&
                                                x.NowSubscription.SubscriptionVocabulary.ExcludedPromptRules.Select(y => y.MasterUUID ?? y.UUID).Contains(rpr.ResultPromptRule.MasterUUID ?? rpr.ResultPromptRule.UUID)
                                            )
                                        )));
            }
            return allMatches;
        }

        private void SetDeletedChangesOnAssociatedNowSubscriptions(List<DraftNowSubscriptionModel> allDraftNowSubscriptions)
        {
            var allMatches = GetPublishedNowSubscriptionsByPromptRules(DeletedPromptRules);

            //create or associate draft now subscriptions for each of the impacted published now subscriptions
            var cmd = treeModel.DraftNowSubscriptionCommand as DraftNowSubscriptionCommand;
            foreach (var published in allMatches)
            {
                var draft = allDraftNowSubscriptions.FirstOrDefault(x => x.DraftNowSub.NowSubscriptionViewModel.MasterUUID == (published.MasterUUID ?? published.UUID));
                if (draft != null)
                {
                    NowSubscriptionUsages.Add(draft);
                    continue;
                }

                if (published.IsNow)
                {
                    var publishedTree = treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsPublished.First(x => x.NowSubscriptionViewModel.UUID == published.UUID);
                    var draftTree = cmd.DraftNowSubscriptionTreeViewModel(publishedTree, Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, false);
                    draft = new DraftNowSubscriptionModel(treeModel, draftTree, publishedTree, true);
                }

                if (published.IsEDT)
                {
                    var publishedTree = treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsPublished.First(x => x.NowSubscriptionViewModel.UUID == published.UUID);
                    var draftTree = cmd.DraftNowSubscriptionTreeViewModel(publishedTree, Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, false);
                    draft = new DraftNowSubscriptionModel(treeModel, draftTree, publishedTree, true);
                }

                if (published.IsCourtRegister)
                {
                    var publishedTree = treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsPublished.First(x => x.NowSubscriptionViewModel.UUID == published.UUID);
                    var draftTree = cmd.DraftNowSubscriptionTreeViewModel(publishedTree, Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, false);
                    draft = new DraftNowSubscriptionModel(treeModel, draftTree, publishedTree, true);
                }

                if (published.IsPrisonCourtRegister)
                {
                    var publishedTree = treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsPublished.First(x => x.NowSubscriptionViewModel.UUID == published.UUID);
                    var draftTree = cmd.DraftNowSubscriptionTreeViewModel(publishedTree, Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, false);
                    draft = new DraftNowSubscriptionModel(treeModel, draftTree, publishedTree, true);
                }

                if (published.IsInformantRegister)
                {
                    var publishedTree = treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsPublished.First(x => x.NowSubscriptionViewModel.UUID == published.UUID);
                    var draftTree = cmd.DraftNowSubscriptionTreeViewModel(publishedTree, Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, false);
                    draft = new DraftNowSubscriptionModel(treeModel, draftTree, publishedTree, true);
                }

                //on the new draft remove the prompt rule

                NowSubscriptionUsages.Add(draft);
                allDraftNowSubscriptions.Add(draft);
            }
        }

        private void SetAssociatedNows(List<DraftNowModel> allDraftNows)
        {
            if (HasStructuralChanges || IsDeleted)
            {
                var cmd = treeModel.DraftNowCommand as DraftNowCommand;
                //create or associate draft nows for each of the impacted published nows
                var publishedNowGroups =
                    treeModel.AllNowRequirementsViewModel.NowRequirements.Where(
                    x => (x.ResultDefinition.MasterUUID ?? x.ResultDefinition.UUID) == Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                      && (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending))
                            .Select(x => x.NowEdtVM).GroupBy(x => x.Now.UUID);
                foreach (var nowGroup in publishedNowGroups)
                {
                    var nowEDT = nowGroup.First();
                    //determine if there is a draft tree already
                    var draft = allDraftNows.FirstOrDefault(x => x.DraftNow.NowViewModel.MasterUUID == (nowEDT.MasterUUID ?? nowEDT.UUID));
                    if (draft != null)
                    {
                        NowUsages.Add(draft);
                        continue;
                    }

                    if (nowEDT.IsEDT)
                    {
                        var publishedEDT = treeModel.AllEDTsTreeViewModel.EdtsPublished.First(x => x.NowViewModel.UUID == nowEDT.UUID);
                        var draftTree = cmd.DraftNowTreeViewModel(publishedEDT, Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, false);
                        draft = new DraftNowModel(treeModel, draftTree, publishedEDT, true);
                    }
                    else
                    {
                        var publishedNow = treeModel.AllNowsTreeViewModel.NowsPublished.First(x => x.NowViewModel.UUID == nowEDT.UUID);
                        var draftTree = cmd.DraftNowTreeViewModel(publishedNow, Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, false);
                        draft = new DraftNowModel(treeModel, draftTree, publishedNow, true);
                    }
                    NowUsages.Add(draft);
                    allDraftNows.Add(draft);
                }
            }
            else
            {
                NowRequirementsUpdatePending = treeModel.AllNowRequirementsViewModel.NowRequirements.Where(
                    x => x.ResultDefinition.MasterUUID == Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID).ToList();

                //Check the draft Nows that may not yet been saved to see if any further now requirements are required to be updated upon publish
                foreach(var now in allDraftNows)
                {
                    foreach(var nr in now.DraftNow.NowViewModel.AllNowRequirements)
                    {
                        if(nr.ResultDefinition.MasterUUID == Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID)
                        {
                            if (NowRequirementsUpdatePending == null)
                            {
                                NowRequirementsUpdatePending = new List<NowRequirementViewModel>() { nr };
                            }
                            else
                            {
                                var matched = NowRequirementsUpdatePending.FirstOrDefault(x=>x.UUID == nr.UUID);
                                if (matched == null)
                                {
                                    NowRequirementsUpdatePending.Add(nr);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
