using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Drawing.Charts;
using Models.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IPublishDraftsCommand : ICommand { }

    public class PublishDraftsCommand : IPublishDraftsCommand
    {
        private ITreeModel treeModel;
        public PublishDraftsCommand(ITreeModel treeModel)
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
            //see if we have been sent the publish model.
            var publishModel = parameter as PublishModel;

            if (publishModel == null)
            {
                Log.Error("No details sent for publishing");
                return;
            }

            //publish draft results
            Publish(publishModel);
        }

        public PublishModel CollectDrafts(NowTreeViewModel source, bool withValidation)
        {
            if (source == null)
            {
                return null;
            }

            var exitingPublished = MatchPublishedNOWEDT(source);
            var draftModel = new DraftNowModel(treeModel, source, exitingPublished, false);
            var res = new PublishModel(treeModel,draftModel);

            //determine if the now can be published
            if (withValidation)
            {
                ValidateNowsForPublishing(res);
            }

            return res;
        }

        public PublishModel CollectDrafts(NowSubscriptionTreeViewModel source, bool withValidation)
        {
            if (source == null)
            {
                return null;
            }

            var exitingPublished = MatchPublishedSubscription(source);
            var draftModel = new DraftNowSubscriptionModel(treeModel, source, exitingPublished, false);
            var res = new PublishModel(treeModel,draftModel);

            //determine if the now can be published
            if (withValidation)
            {
                ValidateNowSubscriptionsForPublishing(res);
            }

            return res;
        }

        public PublishModel CollectDrafts(ResultDefinitionTreeViewModel source)
        {
            if (source == null)
            {
                return null;
            }

            var selectedDraftResult = new DraftResultModel(treeModel, source, true);

            //find draft versions of all child results
            var draftResults = FindChildDraftResults(source);

            //find draft prompt rules of all draft results
            var allDraftResults = new List<DraftResultModel>() { selectedDraftResult };
            if (draftResults != null)
            {
                allDraftResults.AddRange(draftResults);
            }
            var draftPromptRules = FindDraftPromptRules(allDraftResults);

            //determine if any draft prompt rules have draft prompts as well and build the collection of all draft prompts
            var allDraftPrompts = new List<DraftPromptModel>();
            var draftPrompts = draftPromptRules.Where(
                x => x.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
                .Select(x => x.ResultPromptRuleViewModel.ResultPromptViewModel).ToList();
            foreach (var dp in draftPrompts)
            {
                var match = allDraftPrompts.FirstOrDefault(x => x.Prompt.UUID == dp.UUID);
                if (match == null)
                {
                    allDraftPrompts.Add(new DraftPromptModel(treeModel, dp, allDraftPrompts));
                }
            }

            //get the changed prompts
            var changedPrompts = FilterDraftPromptsRemoveUnchangedUnwantedItems(allDraftPrompts);

            //create a publish model
            var res = new PublishModel(treeModel, selectedDraftResult, draftResults, allDraftPrompts, GetAllDraftNowsEDTs(), GetAllDraftNowSubscriptions());

            //Collect more draft results based on draft prompt usages
            CollectAllUsagesForDraftPrompts(res.AllDraftResults, draftPromptRules, changedPrompts, res.AdditionalPromptUsages, res.AllDraftPrompts, 
                                            source.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags);
            
            //Collect more drafts based on draft child result definition word group usages
            CollectAllUsagesForDraftResultDefinitionWordGroup(res.AllDraftResults, res.AllDraftResults, res.AllDraftNows, res.AllDraftNowSubscriptions,
                                                                res.AdditionalResultDefinitionWordGroupUsages, 
                                                                source.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, res.AllDraftPrompts);

            //Collect more drafts based on parent result usages
            CollectAllParentResultUsages(res.AllDraftResults, res.AllDraftResults, res.AdditionalParentResultUsages, res.AllDraftPrompts, res.AllDraftNows, res.AllDraftNowSubscriptions);

            //Identify draft items that have not changed and elaborate any changed items
            res.SetChanges();

            return res;
        }

        private void ValidateNowSubscriptionsForPublishing(PublishModel res)
        {
            var isErrors = false;
            var sbErrorReport = new StringBuilder();

            foreach (var subGroup in res.AllDraftNowSubscriptions.GroupBy(x=>x.DraftNowSub.NowSubscriptionViewModel.SubscriptionType))
            {
                var sbSubErrors = new StringBuilder();
                var isSubErrors = false;
                sbSubErrors.AppendLine(string.Format("The following {0} Subscription errors are preventing publication of drafts ...", subGroup.Key));

                foreach (var sub in subGroup.OrderBy(x => x.DraftNowSub.NowSubscriptionViewModel.Name))
                {
                    if (ValidateSubscription(sbSubErrors, sub, subGroup.Key))
                    {
                        isSubErrors = true;
                    }
                }

                if (isSubErrors)
                {
                    sbErrorReport.AppendLine(sbSubErrors.ToString());
                    isErrors = true;
                }
            }

            if (isErrors)
            {
                res.ErrorReport = Regex.Replace(sbErrorReport.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            }
        }

        private bool ValidateSubscription(StringBuilder sbErrors, DraftNowSubscriptionModel sub, string subType)
        {
            bool isError = false;
            var sb = new StringBuilder();
            if (sub.DraftNowSub.NowSubscriptionViewModel.PublishedStatus != PublishedStatus.Draft)
            {
                sb.AppendLine("Draft version not provided");
                sb.AppendLine();
                isError = true;
            }

            var allNows = sub.DraftNowSub.NowSubscriptionViewModel.IncludedNOWS == null ? null :
                            (from incSub in sub.DraftNowSub.NowSubscriptionViewModel.IncludedNOWS
                             join nowVM in treeModel.AllNowsViewModel.Nows on incSub.UUID equals nowVM.UUID
                             where sub.DraftNowSub.NowSubscriptionViewModel.IncludedNOWS != null &&
                                   nowVM.PublishedStatus == PublishedStatus.Draft
                             select nowVM).ToList();
            if (sub.DraftNowSub.NowSubscriptionViewModel.ExcludedNOWS != null)
            {
                if (allNows == null) { allNows = new List<NowViewModel>(); }
                allNows.AddRange(from excSub in sub.DraftNowSub.NowSubscriptionViewModel.ExcludedNOWS
                                 join nowVM in treeModel.AllNowsViewModel.Nows on excSub.UUID equals nowVM.UUID
                                 where sub.DraftNowSub.NowSubscriptionViewModel.ExcludedNOWS != null &&
                                       nowVM.PublishedStatus == PublishedStatus.Draft &&
                                       !(
                                           from incl in allNows
                                           select incl.UUID
                                        ).Contains(nowVM.UUID)
                                 select nowVM);
            }

            if (allNows != null && allNows.Any())
            {
                sb.AppendLine(string.Format("The subscription is associated to the following draft {0}.  Please publish the {0} before publishing the subsciption.", subType));
                sb.AppendLine();
                isError = true;
                foreach (var now in allNows.OrderBy(x=>x.Name))
                {
                    sb.AppendLine(now.Name);
                }
            }

            var allResults = sub.DraftNowSub.NowSubscriptionViewModel.IncludedResults == null ? null :
                          (from incSub in sub.DraftNowSub.NowSubscriptionViewModel.IncludedResults
                           join rdVM in treeModel.AllResultDefinitionsViewModel.Definitions on incSub.ResultDefinition.UUID equals rdVM.UUID
                           where sub.DraftNowSub.NowSubscriptionViewModel.IncludedResults != null &&
                                 rdVM.PublishedStatus == PublishedStatus.Draft
                           select rdVM).ToList();

            if (sub.DraftNowSub.NowSubscriptionViewModel.ExcludedResults != null)
            {
                if (allResults == null) { allResults = new List<ResultDefinitionViewModel>(); }
                allResults.AddRange(from excSub in sub.DraftNowSub.NowSubscriptionViewModel.ExcludedResults
                                    join rdVM in treeModel.AllResultDefinitionsViewModel.Definitions on excSub.ResultDefinition.UUID equals rdVM.UUID
                                    where sub.DraftNowSub.NowSubscriptionViewModel.ExcludedResults != null &&
                                          rdVM.PublishedStatus == PublishedStatus.Draft &&
                                          !(
                                              from incl in allResults
                                              select incl.UUID
                                           ).Contains(rdVM.UUID)
                                    select rdVM);
            }

            if (allResults!= null && allResults.Any())
            {
                sb.AppendLine("The subscription is associated to the following draft result definitions.  Please publish the result definitions before publishing the subsciption.");
                sb.AppendLine();
                isError = true;
                foreach (var rd in allResults.OrderBy(x=>x.Label))
                {
                    sb.AppendLine(rd.Label);
                }
            }

            var allPrompts = sub.DraftNowSub.NowSubscriptionViewModel.IncludedPrompts == null ? null :
                             (from incSub in sub.DraftNowSub.NowSubscriptionViewModel.IncludedPrompts
                              join rpVM in treeModel.AllResultPromptViewModel.Prompts on incSub.ResultPrompt.UUID equals rpVM.ResultPromptRule.UUID
                              where sub.DraftNowSub.NowSubscriptionViewModel.IncludedPrompts != null &&
                                    rpVM.PublishedStatus == PublishedStatus.Draft
                              select rpVM).ToList();

            if (sub.DraftNowSub.NowSubscriptionViewModel.ExcludedPrompts != null)
            {
                if (allPrompts == null) { allPrompts = new List<ResultPromptRuleViewModel>(); }
                allPrompts.AddRange(from excSub in sub.DraftNowSub.NowSubscriptionViewModel.ExcludedPrompts
                                    join rpVM in treeModel.AllResultPromptViewModel.Prompts on excSub.ResultPrompt.UUID equals rpVM.ResultPromptRule.UUID
                                    where sub.DraftNowSub.NowSubscriptionViewModel.ExcludedPrompts != null &&
                                          rpVM.PublishedStatus == PublishedStatus.Draft &&
                                          !(
                                              from incl in allPrompts
                                              select incl.UUID
                                           ).Contains(rpVM.UUID)
                                    select rpVM);
            }

            if (allPrompts != null && allPrompts.Any())
            {
                sb.AppendLine("The subscription is associated to the following draft result prompts.  Please publish the result prompts before publishing the subsciption.");
                sb.AppendLine();
                isError = true;
                foreach (var rpr in allPrompts.OrderBy(x=>x.ResultDefinitionViewModel.Label).ThenBy(x=>x.ResultPromptViewModel.Label))
                {
                    sb.AppendLine(string.Format("{0} - {1}", rpr.ResultDefinitionViewModel.Label, rpr.ResultPromptViewModel.Label));
                }
            }

            if (isError)
            {
                sbErrors.AppendLine(sb.ToString());
            }

            return isError;
        }

        private void ValidateNowsForPublishing(PublishModel res)
        {
            var isNowError = false;
            var isEDTError = false;
            var sbNowErrors = new StringBuilder();
            var sbEDTErrors = new StringBuilder();
            var sbErrorReport = new StringBuilder();

            sbNowErrors.AppendLine("The following NOW errors are preventing publication of drafts ...");
            foreach (var now in res.AllDraftNows.Where(x=>!x.DraftNow.NowViewModel.IsEDT).OrderBy(x=>x.DraftNow.NowViewModel.Name))
            {
                if (ValidateNOWEDT(sbNowErrors, now))
                {
                    isNowError = true;
                }
            }

            sbEDTErrors.AppendLine("The following EDT errors are preventing publication of drafts ...");
            foreach (var edt in res.AllDraftNows.Where(x => x.DraftNow.NowViewModel.IsEDT).OrderBy(x => x.DraftNow.NowViewModel.Name))
            {
                if (ValidateNOWEDT(sbEDTErrors, edt))
                {
                    isEDTError = true;
                }
            }

            if (isNowError)
            {
                sbErrorReport.AppendLine(sbNowErrors.ToString());
            }
            if (isEDTError)
            {
                sbErrorReport.AppendLine(sbEDTErrors.ToString());
            }

            if (isNowError || isEDTError)
            {
                res.ErrorReport = Regex.Replace(sbErrorReport.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            }
        }

        private bool ValidateNOWEDT(StringBuilder sbErrors, DraftNowModel nowEDT)
        {
            bool isError = false;
            var sb = new StringBuilder();
            if (nowEDT.DraftNow.NowViewModel.PublishedStatus != PublishedStatus.Draft)
            {
                sb.AppendLine("Draft version not provided");
                sb.AppendLine();
                isError = true;
            }

            var allNowRequirements = treeModel.AllNowRequirementsViewModel.NowRequirements.Where(x => x.NOWUUID == nowEDT.DraftNow.NowViewModel.UUID);
            var requirementsWithDraftResult = allNowRequirements.Where(x => x.ResultDefinition.PublishedStatus == PublishedStatus.Draft).ToList();
            if (requirementsWithDraftResult.Any())
            {
                sb.AppendLine("The following requirements are associated to a draft result definition.  Please publish the Result Definition before publishing the nows.");
                sb.AppendLine();
                isError = true;
                foreach (var nr in requirementsWithDraftResult)
                {
                    if (nr.ParentNowRequirement == null)
                    {
                        sb.AppendLine(string.Format("Root item '{0}'", nr.ResultDefinition.Label));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("'{0}' for parent '{1}'", nr.ResultDefinition.Label, nr.ParentNowRequirement.ResultDefinition.Label));
                    }
                }
            }

            if (isError)
            {
                sbErrors.AppendLine(string.Format("NOW: {0}", nowEDT.DraftNow.NowViewModel.Name));
                sbErrors.AppendLine(sb.ToString());
            }

            return isError;
        }

        private NowTreeViewModel MatchPublishedNOWEDT(NowTreeViewModel draft)
        {
            var match = treeModel.Nows.FirstOrDefault(x => 
                                                                (x.NowViewModel.MasterUUID ?? x.NowViewModel.UUID) == draft.NowViewModel.MasterUUID &&
                                                                (x.NowViewModel.PublishedStatus == PublishedStatus.Published || x.NowViewModel.PublishedStatus == PublishedStatus.PublishedPending));
            if (match != null) { return match; }
            return treeModel.EDTs.FirstOrDefault(x =>
                                                                (x.NowViewModel.MasterUUID ?? x.NowViewModel.UUID) == draft.NowViewModel.MasterUUID &&
                                                                (x.NowViewModel.PublishedStatus == PublishedStatus.Published || x.NowViewModel.PublishedStatus == PublishedStatus.PublishedPending));
        }

        private NowSubscriptionTreeViewModel MatchPublishedSubscription(NowSubscriptionTreeViewModel draft)
        {
            if (draft.NowSubscriptionViewModel.IsNow)
            {
                return treeModel.NowSubscriptions.FirstOrDefault(x =>
                                                                    (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == draft.NowSubscriptionViewModel.MasterUUID &&
                                                                    (x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Published || x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.PublishedPending));
            }

            if (draft.NowSubscriptionViewModel.IsEDT)
            {
                return treeModel.EDTSubscriptions.FirstOrDefault(x =>
                                                                    (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == draft.NowSubscriptionViewModel.MasterUUID &&
                                                                    (x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Published || x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.PublishedPending));
            }

            if (draft.NowSubscriptionViewModel.IsCourtRegister)
            {
                return treeModel.CourtRegisterSubscriptions.FirstOrDefault(x =>
                                                                    (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == draft.NowSubscriptionViewModel.MasterUUID &&
                                                                    (x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Published || x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.PublishedPending));
            }

            if (draft.NowSubscriptionViewModel.IsPrisonCourtRegister)
            {
                return treeModel.PrisonCourtRegisterSubscriptions.FirstOrDefault(x =>
                                                                    (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == draft.NowSubscriptionViewModel.MasterUUID &&
                                                                    (x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Published || x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.PublishedPending));
            }

            if (draft.NowSubscriptionViewModel.IsInformantRegister)
            {
                return treeModel.InformantRegisterSubscriptions.FirstOrDefault(x =>
                                                                    (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == draft.NowSubscriptionViewModel.MasterUUID &&
                                                                    (x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Published || x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.PublishedPending));
            }

            return null;
        }

        private class ResultsWithPromptsWithsChangedWordGroups
        {
            public ResultDefinitionViewModel ResultDefinitionVM { get;}

            public List<ResultPromptViewModel> PromptVMs { get; }

            public ResultsWithPromptsWithsChangedWordGroups(ResultDefinitionViewModel resultDefinitionVM, ResultPromptViewModel promptVM)
            {
                ResultDefinitionVM = resultDefinitionVM;
                PromptVMs = new List<ResultPromptViewModel>() { promptVM };
            }
        }

        private void CollectAllParentResultUsages(
            List<DraftResultModel> draftResults,
            List<DraftResultModel> allDraftResults,
            List<DraftParentResultUsageModel> additionalParentResultUsages,
            List<DraftPromptModel> allDraftPrompts,
            List<DraftNowModel> allDraftNows,
            List<DraftNowSubscriptionModel> allDraftNowSubscriptions
            )
        {
            if (draftResults == null || draftResults.Count == 0) { return; }

            //prepare draft result by setting their changes
            draftResults.ForEach(x => x.SetChanges(allDraftPrompts, allDraftNows, allDraftNowSubscriptions));

            var otherResults = FindAdditionalParentResultUsages(draftResults, allDraftResults, additionalParentResultUsages);

            if (otherResults.Count == 0)
            {
                return;
            }

            //collect a draft version of the other results
            var newDraftResults = new List<DraftResultModel>();
            var cmd = treeModel.DraftResultDefinitionCommand as DraftResultDefinitionCommand;
            foreach (var rd in otherResults)
            {
                //ensure that we dont record duplicate additional usages
                var matchedAdditionalUsage = additionalParentResultUsages.FirstOrDefault(
                    x => x.ExistingParentPublishedResult.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == 
                                rd.ExistingParentPublishedResult.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
                if (matchedAdditionalUsage != null)
                {
                    continue;
                }

                additionalParentResultUsages.Add(rd);

                DraftResultModel draftResult = null;

                //see if there is already a draft version
                var draft = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.FirstOrDefault
                                            (
                                                x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ==
                                                                    rd.ExistingParentPublishedResult.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                                            );
                if (draft == null)
                {
                    //make a new draft just for the purposes of publishing
                    draft = cmd.DraftResultDefinitionTreeViewModel(rd.ExistingParentPublishedResult, rd.ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTags, false);
                    draftResult = new DraftResultModel(treeModel, draft,false);
                }
                else
                {
                    draftResult = new DraftResultModel(treeModel, draft, true);
                    //find any draft children of for the existing draft result and add to the collection of alldraftresults and newdraftresults
                    var draftChildren = FindChildDraftResults(draft);
                    foreach(var child in draftChildren)
                    {
                        var match = allDraftResults.FirstOrDefault(x => x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == child.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
                        if (match == null)
                        {
                            allDraftResults.Add(child);
                            newDraftResults.Add(child);
                            draftResult.AdditionalDraftChildren.Add(child);
                        }
                    }
                }

                //ensure that the child result rule on the draft parent is set to the changed child result
                var matchedRule = draft.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.FirstOrDefault(
                    x => x.ChildResultDefinitionViewModel.MasterUUID ==
                        rd.ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
                if(matchedRule != null)
                {
                    if (matchedRule.ChildResultDefinitionViewModel.UUID != rd.ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                    {
                        matchedRule.ChildResultDefinitionViewModel = rd.ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel;
                    }
                }

                //add the draft result to the collection of results to be published
                allDraftResults.Add(draftResult);

                //add the draft result to the collection of draft results for recursion purposes
                newDraftResults.Add(draftResult);

                //set the usage to the draft result definition
                rd.ParentUsage = draftResult;
            }

            CollectAllParentResultUsages(newDraftResults, allDraftResults, additionalParentResultUsages, allDraftPrompts, allDraftNows, allDraftNowSubscriptions);
        }

        private List<DraftPromptModel> FilterDraftPromptsRemoveUnchangedUnwantedItems(List<DraftPromptModel> drafts)
        {
            var res = new List<DraftPromptModel>();
            //determine if draft prompts have changed
            foreach (var dp in drafts)
            {
                var published = treeModel.AllResultPromptViewModel.Prompts.FirstOrDefault(
                        x => x.ResultPromptViewModel.MasterUUID == dp.Prompt.MasterUUID &&
                        (x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Published || x.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending));

                dp.SetPromptChanges(published);

                if (dp.IsChanged || dp.IsFixedListChanged || dp.HasResultPromptWordGroupChanges)
                {
                    res.Add(dp);
                }
            }

            return res;
        }
        
        private void Publish(PublishModel model)
        {
            //prepare for publication
            model.PrepareForPublication();

            //publish changed results
            if (model.ChangedResults != null)
            {
                foreach (var draft in model.ChangedResults)
                {
                    //replace the corresponding root published tree item with the draft item
                    if (draft.ExistingPublishedResult == null)
                    {
                        treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished.AddSorted(draft.Result);
                    }
                    else
                    {
                        var index = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished.IndexOf(draft.ExistingPublishedResult);
                        treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished.RemoveAt(index);
                        treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished.Insert(index, draft.Result);
                        treeModel.AllResultDefinitionsTreeViewModel.AppendRevision(draft.ExistingPublishedResult);
                    }
                }

                //clear out published results from the draft results tree
                treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.RemoveAll(
                    x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus != PublishedStatus.Draft);
            }

            //publish updated result definition word groups
            if (model.UpdatedResultDefinitionWordGroups != null)
            {
                foreach (var rdwg in model.UpdatedResultDefinitionWordGroups)
                {
                    foreach (var rwg in rdwg.ParentWordGroups.Where(
                        x => x.ParentResultDefinitionVM.PublishedStatus == PublishedStatus.Published ||
                        x.ParentResultDefinitionVM.PublishedStatus == PublishedStatus.PublishedPending))
                    {
                        //Update the RDWG VM
                        var matched = rwg.ResultDefinitionWordGroups.First(x => x.MasterUUID == rdwg.DraftResultDefinitionWordGroupViewModel.MasterUUID);
                        if (matched.UUID != rdwg.DraftResultDefinitionWordGroupViewModel.UUID)
                        {
                            var index = rwg.ResultDefinitionWordGroups.IndexOf(matched);
                            rwg.ResultDefinitionWordGroups.RemoveAt(index);
                            rwg.ResultDefinitionWordGroups.Insert(index, rdwg.DraftResultDefinitionWordGroupViewModel);
                        }
                    }
                }
            }

            //publish updated fixedlists
            if (model.UpdatedFixedLists != null)
            {
                foreach (var fl in model.UpdatedFixedLists)
                {
                    foreach (var rpr in fl.PublishedParentResultPromptRules)
                    {
                        //Update the fixed list vm
                        rpr.ResultPromptViewModel.FixedList = fl.DraftFixedList;
                    }
                }
            }

            //publish updated result prompt word groups
            if (model.UpdatedResultPromptWordGroups != null)
            {
                foreach (var rpwg in model.UpdatedResultPromptWordGroups)
                {
                    if (rpwg.ExistingPublishedResultPromptWordGroup != null)
                    {
                        foreach (var prompt in rpwg.ExistingPublishedResultPromptWordGroup.ParentPrompts)
                        {
                            //Update the RPWG VM & Data for the published prompt
                            var matchedVM = prompt.WordGroups.First(x => x.MasterUUID == rpwg.DraftResultPromptWordGroup.MasterUUID);

                            //VM
                            var index = prompt.WordGroups.IndexOf(matchedVM);
                            prompt.WordGroups.RemoveAt(index);
                            prompt.WordGroups.Insert(index, rpwg.DraftResultPromptWordGroup);

                            //Data
                            index = prompt.ResultPrompt.ResultPromptWordGroups.IndexOf(matchedVM.ResultPromptWordGroup);
                            prompt.ResultPrompt.ResultPromptWordGroups.RemoveAt(index);
                            prompt.ResultPrompt.ResultPromptWordGroups.Insert(index, rpwg.DraftResultPromptWordGroup.ResultPromptWordGroup);

                            //Update the parents collection for the draft rpwg
                            var matched = rpwg.DraftResultPromptWordGroup.ParentPrompts.FirstOrDefault(x => x.MasterUUID == prompt.MasterUUID);
                            if (matched == null)
                            {
                                rpwg.DraftResultPromptWordGroup.ParentPrompts.Add(prompt);
                            }
                        }
                    }
                }
            }

            //publish changed nows
            if (model.AllDraftNows != null && model.AllDraftNows.Any())
            {
                foreach (var now in model.AllDraftNows)
                {
                    //replace the corresponding root published tree item with the draft item
                    if (now.ExistingPublishedNow == null)
                    {
                        if (now.DraftNow.NowViewModel.IsEDT)
                        {
                            treeModel.AllEDTsTreeViewModel.EdtsPublished.AddSorted(now.DraftNow);
                        }
                        else
                        {
                            treeModel.AllNowsTreeViewModel.NowsPublished.AddSorted(now.DraftNow);
                        }
                    }
                    else
                    {
                        if (now.ExistingPublishedNow.NowViewModel.IsEDT)
                        {
                            var index = treeModel.AllEDTsTreeViewModel.EdtsPublished.IndexOf(now.ExistingPublishedNow);
                            treeModel.AllEDTsTreeViewModel.EdtsPublished.RemoveAt(index);
                            treeModel.AllEDTsTreeViewModel.EdtsPublished.Insert(index, now.DraftNow);
                            treeModel.AllEDTsTreeViewModel.AppendRevision(now.ExistingPublishedNow);
                        }
                        else
                        {
                            var index = treeModel.AllNowsTreeViewModel.NowsPublished.IndexOf(now.ExistingPublishedNow);
                            treeModel.AllNowsTreeViewModel.NowsPublished.RemoveAt(index);
                            treeModel.AllNowsTreeViewModel.NowsPublished.Insert(index, now.DraftNow);
                            treeModel.AllNowsTreeViewModel.AppendRevision(now.ExistingPublishedNow);
                        }
                    }
                }

                //clear out published nows & edts from the draft trees
                treeModel.AllNowsTreeViewModel.NowsDraft.RemoveAll(x => x.NowViewModel.PublishedStatus != PublishedStatus.Draft);
                treeModel.AllEDTsTreeViewModel.EdtsDraft.RemoveAll(x => x.NowViewModel.PublishedStatus != PublishedStatus.Draft);
            }

            //publish changed now subscriptions
            if (model.AllDraftNowSubscriptions != null && model.AllDraftNowSubscriptions.Any())
            {
                foreach (var sub in model.AllDraftNowSubscriptions)
                {
                    //replace the corresponding root published tree item with the draft item
                    if (sub.ExistingPublishedNowSub == null)
                    {
                        PublishNewNowSubscription(sub);                        
                    }
                    else
                    {
                        PublishChangedNowSubscription(sub);                        
                    }
                }
                //clear out published nows & edts from the draft trees
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsDraft.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus != PublishedStatus.Draft);
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsDraft.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus != PublishedStatus.Draft);
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsDraft.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus != PublishedStatus.Draft);
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsDraft.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus != PublishedStatus.Draft);
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsDraft.RemoveAll(x => x.NowSubscriptionViewModel.PublishedStatus != PublishedStatus.Draft);
            }

            //the following should not be required but is implemented due to data inconsistencies being introduced in the publish process for unknown reasons
            //DraftResultModel.PrepareForPublication() should apply the relevant updates that prevent the need for this logic but extensive testing has not 
            //reproduced the production issue that causes inconsistencies
            EnsureNowRequirementConsistency();
            EnsureNowSubscriptionConsistency();
        }

        private void EnsureNowSubscriptionConsistency()
        {
            EnsureNowSubscriptionPromptRuleConsistency();
            EnsureNowSubscriptionResultConsistency();
            EnsureNowSubscriptionNowConsistency();
        }

        private void EnsureNowSubscriptionNowConsistency()
        {
            //ensure that Published/PublishPending Now Subscriptions do not have references to revision pending NOWs
            foreach (var item in treeModel.AllData.NowSubscriptions.Where(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending ||
                                                                                  x.CalculatedPublishedStatus == PublishedStatus.Published) &&                                                                                  
                                                                                ((x.IncludedNOWS != null && x.IncludedNOWS.Any()) ||
                                                                                 (x.ExcludedNOWS != null && x.ExcludedNOWS.Any())
                                                                                )
                                                                               )
                                                                          )
            {
                if (item.IncludedNOWS != null && item.IncludedNOWS.Any())
                {
                    var newIncludedNows = new List<Now>();
                    foreach (var now in item.IncludedNOWS)
                    {
                        if (now.CalculatedPublishedStatus == PublishedStatus.PublishedPending || now.CalculatedPublishedStatus == PublishedStatus.Published)
                        {
                            newIncludedNows.Add(now);
                        }
                        else
                        {
                            //remove the now from the view model
                            var vmToUpdate = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.First(x => x.UUID == item.UUID);
                            vmToUpdate.IncludedNOWS.RemoveAll(x => x.UUID == now.UUID);

                            var replacementNow = treeModel.AllData.Nows.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
                                                                         (x.MasterUUID ?? x.UUID) == (now.MasterUUID ?? now.UUID));
                            if (replacementNow != null)
                            {
                                newIncludedNows.Add(replacementNow);

                                //add the new now to the view model
                                vmToUpdate.IncludedNOWS.Add(replacementNow);
                            }
                        }
                    }
                    item.IncludedNOWS = newIncludedNows;
                }

                if (item.ExcludedNOWS != null && item.ExcludedNOWS.Any())
                {
                    var newExcludedNows = new List<Now>();
                    foreach (var now in item.ExcludedNOWS)
                    {
                        if (now.CalculatedPublishedStatus == PublishedStatus.PublishedPending || now.CalculatedPublishedStatus == PublishedStatus.Published)
                        {
                            newExcludedNows.Add(now);
                        }
                        else
                        {
                            //remove the now from the view model
                            var vmToUpdate = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.First(x => x.UUID == item.UUID);
                            vmToUpdate.ExcludedNOWS.RemoveAll(x => x.UUID == now.UUID);

                            var replacementNow = treeModel.AllData.Nows.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
                                                                         (x.MasterUUID ?? x.UUID) == (now.MasterUUID ?? now.UUID));
                            if (replacementNow != null)
                            {
                                newExcludedNows.Add(replacementNow);

                                //add the now to the view model
                                vmToUpdate.ExcludedNOWS.Add(replacementNow);
                            }
                        }
                    }
                    item.ExcludedNOWS = newExcludedNows;
                }
            }
        }

        private void EnsureNowSubscriptionResultConsistency()
        {
            //ensure that Published/PublishPending Now Subscriptions do not have references to revision pending results
            foreach (var item in treeModel.AllData.NowSubscriptions.Where(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending ||
                                                                                  x.CalculatedPublishedStatus == PublishedStatus.Published) &&
                                                                                  x.SubscriptionVocabulary != null &&
                                                                                ((x.SubscriptionVocabulary.IncludedResults != null && x.SubscriptionVocabulary.IncludedResults.Any()) ||
                                                                                 (x.SubscriptionVocabulary.ExcludedResults != null && x.SubscriptionVocabulary.ExcludedResults.Any())
                                                                                )
                                                                               )
                                                                          )
            {
                if (item.SubscriptionVocabulary.IncludedResults != null && item.SubscriptionVocabulary.IncludedResults.Any())
                {
                    var newIncludedResults = new List<ResultDefinition>();
                    foreach (var rd in item.SubscriptionVocabulary.IncludedResults)
                    {
                        if (rd.CalculatedPublishedStatus == PublishedStatus.PublishedPending || rd.CalculatedPublishedStatus == PublishedStatus.Published)
                        {
                            newIncludedResults.Add(rd);
                        }
                        else
                        {
                            //remove the result from the view model
                            var vmToUpdate = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.First(x => (x.MasterUUID ?? x.UUID) == (item.MasterUUID ?? item.UUID));
                            vmToUpdate.IncludedResults.RemoveAll(x => x.ResultDefinition.UUID == rd.UUID);

                            var replacementRd = treeModel.AllData.ResultDefinitions.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
                                                                         (x.MasterUUID ?? x.UUID) == (rd.MasterUUID ?? rd.UUID));
                            if (replacementRd != null)
                            {
                                newIncludedResults.Add(replacementRd);

                                //add the new result to the view model
                                vmToUpdate.IncludedResults.Add(new NowSubscriptionResultViewModel(replacementRd, vmToUpdate));
                            }
                        }
                    }
                    item.SubscriptionVocabulary.IncludedResults = newIncludedResults;
                }

                if (item.SubscriptionVocabulary.ExcludedResults != null && item.SubscriptionVocabulary.ExcludedResults.Any())
                {
                    var newExcludedResults = new List<ResultDefinition>();
                    foreach (var rd in item.SubscriptionVocabulary.ExcludedResults)
                    {
                        if (rd.CalculatedPublishedStatus == PublishedStatus.PublishedPending || rd.CalculatedPublishedStatus == PublishedStatus.Published)
                        {
                            newExcludedResults.Add(rd);
                        }
                        else
                        {
                            //remove the result from the view model
                            var vmToUpdate = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.First(x => (x.MasterUUID ?? x.UUID) == (item.MasterUUID ?? item.UUID));
                            vmToUpdate.ExcludedResults.RemoveAll(x => x.ResultDefinition.UUID == rd.UUID);

                            var replacementRd = treeModel.AllData.ResultDefinitions.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
                                                                         (x.MasterUUID ?? x.UUID) == (rd.MasterUUID ?? rd.UUID));
                            if (replacementRd != null)
                            {
                                newExcludedResults.Add(replacementRd);

                                //add the new result to the view model
                                vmToUpdate.ExcludedResults.Add(new NowSubscriptionResultViewModel(replacementRd, vmToUpdate));
                            }
                        }
                    }
                    item.SubscriptionVocabulary.ExcludedResults = newExcludedResults;
                }
            }
        }

        private void EnsureNowSubscriptionPromptRuleConsistency()
        {
            //ensure that Published/PublishPending Now Subscriptions do not have references to revision pending prompt rules
            foreach (var item in treeModel.AllData.NowSubscriptions.Where(x => (  x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || 
                                                                                  x.CalculatedPublishedStatus == PublishedStatus.Published) &&
                                                                                  x.SubscriptionVocabulary != null &&
                                                                                ((x.SubscriptionVocabulary.IncludedPromptRules != null && x.SubscriptionVocabulary.IncludedPromptRules.Any())||
                                                                                 (x.SubscriptionVocabulary.ExcludedPromptRules != null && x.SubscriptionVocabulary.ExcludedPromptRules.Any())
                                                                                )
                                                                               )
                                                                          )
            {
                if (item.SubscriptionVocabulary.IncludedPromptRules != null && item.SubscriptionVocabulary.IncludedPromptRules.Any())
                {
                    if (item.SubscriptionVocabulary.IncludedPromptRules != null && item.SubscriptionVocabulary.IncludedPromptRules.Any())
                    {
                        var newIncludedPromptRules = new List<ResultPromptRule>();
                        var includedPromptRuleViewReplacements = new List<Tuple<Guid, NowSubscriptionPromptViewModel>>();
                            
                            //new List<NowSubscriptionPromptViewModel>();
                        foreach (var rule in item.SubscriptionVocabulary.IncludedPromptRules)
                        {
                            if (rule.CalculatedPublishedStatus == PublishedStatus.PublishedPending || rule.CalculatedPublishedStatus == PublishedStatus.Published)
                            {
                                newIncludedPromptRules.Add(rule);
                            }
                            else
                            {
                                //get the subscription prompt rule view model that is required to be reset
                                var vmToUpdate = GetNowSubscriptionViewModel(item);

                                var replacementRule = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
                                                                             (x.MasterUUID ?? x.UUID) == (rule.MasterUUID ?? rule.UUID));
                                if (replacementRule != null)
                                {
                                    newIncludedPromptRules.Add(replacementRule);

                                    //add the new rule to the view model and save the view model to process later outside of this loop
                                    includedPromptRuleViewReplacements.Add(new Tuple<Guid, NowSubscriptionPromptViewModel>(rule.UUID.Value, new NowSubscriptionPromptViewModel(replacementRule, vmToUpdate)));
                                }
                            }
                        }
                        //set the data
                        item.SubscriptionVocabulary.IncludedPromptRules = newIncludedPromptRules;

                        //set the view model
                        foreach (var replacement in includedPromptRuleViewReplacements)
                        {
                            //remove the rule that is required to be replaced
                            replacement.Item2.NowSubscription.IncludedPrompts.RemoveAll(x => x.ResultPrompt.UUID == replacement.Item1);

                            //added here outside of the loop
                            replacement.Item2.NowSubscription.IncludedPrompts.Add(replacement.Item2);
                        }
                    }
                }

                if (item.SubscriptionVocabulary.ExcludedPromptRules != null && item.SubscriptionVocabulary.ExcludedPromptRules.Any())
                {
                    var newExcludedPromptRules = new List<ResultPromptRule>();
                    var excludedPromptRuleViewReplacements = new List<Tuple<Guid, NowSubscriptionPromptViewModel>>();
                    foreach (var rule in item.SubscriptionVocabulary.ExcludedPromptRules)
                    {
                        if (rule.CalculatedPublishedStatus == PublishedStatus.PublishedPending || rule.CalculatedPublishedStatus == PublishedStatus.Published)
                        {
                            newExcludedPromptRules.Add(rule);
                        }
                        else
                        {
                            //get the subscription prompt rule view model that is required to be reset
                            var vmToUpdate = GetNowSubscriptionViewModel(item);

                            var replacementRule = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => (x.CalculatedPublishedStatus == PublishedStatus.PublishedPending || x.CalculatedPublishedStatus == PublishedStatus.Published) &&
                                                                         (x.MasterUUID ?? x.UUID) == (rule.MasterUUID ?? rule.UUID));
                            if (replacementRule != null)
                            {
                                newExcludedPromptRules.Add(replacementRule);

                                //add the new rule to the view model and save the view model to process later outside of this loop
                                excludedPromptRuleViewReplacements.Add(new Tuple<Guid, NowSubscriptionPromptViewModel>(rule.UUID.Value, new NowSubscriptionPromptViewModel(replacementRule, vmToUpdate)));
                            }

                        }
                    }
                    //set the data
                    item.SubscriptionVocabulary.IncludedPromptRules = newExcludedPromptRules;

                    //set the view model
                    foreach (var replacement in excludedPromptRuleViewReplacements)
                    {
                        //remove the rule that is required to be replaced
                        replacement.Item2.NowSubscription.ExcludedPrompts.RemoveAll(x => x.ResultPrompt.UUID == replacement.Item1);

                        //added here outside of the loop
                        replacement.Item2.NowSubscription.ExcludedPrompts.Add(replacement.Item2);
                    }
                    item.SubscriptionVocabulary.ExcludedPromptRules = newExcludedPromptRules;
                }
            }
        }

        private NowSubscriptionViewModel GetNowSubscriptionViewModel(NowSubscription item)
        {
            if (item.IsNow)
            {
                return treeModel.AllNowSubscriptionViewModel.NowSubscriptions.First(x => (x.MasterUUID ?? x.UUID) == (item.MasterUUID ?? item.UUID));
            }

            if (item.IsEDT)
            {
                return treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.First(x => (x.MasterUUID ?? x.UUID) == (item.MasterUUID ?? item.UUID));
            }

            if (item.IsCourtRegister)
            {
                return treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.First(x => (x.MasterUUID ?? x.UUID) == (item.MasterUUID ?? item.UUID));
            }

            if (item.IsPrisonCourtRegister)
            {
                return treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.First(x => (x.MasterUUID ?? x.UUID) == (item.MasterUUID ?? item.UUID));
            }

            if (item.IsInformantRegister)
            {
                return treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.First(x => (x.MasterUUID ?? x.UUID) == (item.MasterUUID ?? item.UUID));
            }

            throw new Exception("Unknown subscription type");
        }

        private void EnsureNowRequirementConsistency()
        {
            //ensure that every PublishPending Now Requirement that has a reference to a PublishPending or a Publish version of a result definition
            foreach (var item in treeModel.AllData.NowRequirements.Where(x => x.CalculatedPublishedStatus == PublishedStatus.PublishedPending))
            {
                if (item.ResultDefinition.CalculatedPublishedStatus == PublishedStatus.RevisionPending)
                {
                    var replacementRd = treeModel.AllData.ResultDefinitions.FirstOrDefault(x => x.CalculatedPublishedStatus == PublishedStatus.PublishedPending &&
                                                                         (x.MasterUUID ?? x.UUID) == (item.ResultDefinition.MasterUUID ?? item.ResultDefinition.UUID));
                    if (replacementRd == null)
                    {
                        replacementRd = treeModel.AllData.ResultDefinitions.First(x => x.CalculatedPublishedStatus == PublishedStatus.Published &&
                                                                         (x.MasterUUID ?? x.UUID) == (item.ResultDefinition.MasterUUID ?? item.ResultDefinition.UUID));
                    }
                    //update the view models which will encapsulate the update of the data
                    var vmToUpdate = treeModel.AllNowRequirementsViewModel.NowRequirements.First(x => x.UUID == item.UUID);
                    var replacementVM = treeModel.AllResultDefinitionsViewModel.Definitions.First(x => x.UUID == replacementRd.UUID);
                    vmToUpdate.ResultDefinition = replacementVM;
                }
            }

            //ensure that every RevisionPending Now Requirement has a reference to a RevisionPending or a Publish version of a result definition
            foreach (var item in treeModel.AllData.NowRequirements.Where(x => x.CalculatedPublishedStatus == PublishedStatus.RevisionPending))
            {
                if (item.ResultDefinition.CalculatedPublishedStatus != PublishedStatus.RevisionPending)
                {
                    var replacementRd = treeModel.AllData.ResultDefinitions.FirstOrDefault(x => x.CalculatedPublishedStatus == PublishedStatus.RevisionPending &&
                                                                         (x.MasterUUID ?? x.UUID) == (item.ResultDefinition.MasterUUID ?? item.ResultDefinition.UUID));
                    if (replacementRd == null)
                    {
                        replacementRd = treeModel.AllData.ResultDefinitions.First(x => x.CalculatedPublishedStatus == PublishedStatus.Published &&
                                                                         (x.MasterUUID ?? x.UUID) == (item.ResultDefinition.MasterUUID ?? item.ResultDefinition.UUID));
                    }
                    if (item.ResultDefinition.UUID != replacementRd.UUID)
                    {
                        //update the view models which will encapsulate the update of the data
                        var vmToUpdate = treeModel.AllNowRequirementsViewModel.NowRequirements.First(x => x.UUID == item.UUID);
                        var replacementVM = treeModel.AllResultDefinitionsViewModel.Definitions.First(x => x.UUID == replacementRd.UUID);
                        vmToUpdate.ResultDefinition = replacementVM;
                    }
                }
            }
        }

        private void PublishChangedNowSubscription(DraftNowSubscriptionModel sub)
        {
            if (sub.ExistingPublishedNowSub.NowSubscriptionViewModel.IsNow)
            {
                var index = treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsPublished.IndexOf(sub.ExistingPublishedNowSub);
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsPublished.RemoveAt(index);
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsPublished.Insert(index, sub.DraftNowSub);
                treeModel.AllNowSubscriptionsTreeViewModel.AppendRevision(sub.ExistingPublishedNowSub);
            }
            if (sub.ExistingPublishedNowSub.NowSubscriptionViewModel.IsEDT)
            {
                var index = treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsPublished.IndexOf(sub.ExistingPublishedNowSub);
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsPublished.RemoveAt(index);
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsPublished.Insert(index, sub.DraftNowSub);
                treeModel.AllEDTSubscriptionsTreeViewModel.AppendRevision(sub.ExistingPublishedNowSub);
            }
            if (sub.ExistingPublishedNowSub.NowSubscriptionViewModel.IsCourtRegister)
            {
                var index = treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsPublished.IndexOf(sub.ExistingPublishedNowSub);
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsPublished.RemoveAt(index);
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsPublished.Insert(index, sub.DraftNowSub);
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.AppendRevision(sub.ExistingPublishedNowSub);
            }
            if (sub.ExistingPublishedNowSub.NowSubscriptionViewModel.IsPrisonCourtRegister)
            {
                var index = treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsPublished.IndexOf(sub.ExistingPublishedNowSub);
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsPublished.RemoveAt(index);
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsPublished.Insert(index, sub.DraftNowSub);
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.AppendRevision(sub.ExistingPublishedNowSub);
            }
            if (sub.ExistingPublishedNowSub.NowSubscriptionViewModel.IsInformantRegister)
            {
                var index = treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsPublished.IndexOf(sub.ExistingPublishedNowSub);
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsPublished.RemoveAt(index);
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsPublished.Insert(index, sub.DraftNowSub);
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.AppendRevision(sub.ExistingPublishedNowSub);
            }
        }

        private void PublishNewNowSubscription(DraftNowSubscriptionModel sub)
        {
            if (sub.DraftNowSub.NowSubscriptionViewModel.IsNow)
            {
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsPublished.AddSorted(sub.DraftNowSub);
                return;
            }
            if (sub.DraftNowSub.NowSubscriptionViewModel.IsEDT)
            {
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsPublished.AddSorted(sub.DraftNowSub);
                return;
            }
            if (sub.DraftNowSub.NowSubscriptionViewModel.IsCourtRegister)
            {
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsPublished.AddSorted(sub.DraftNowSub);
                return;
            }
            if (sub.DraftNowSub.NowSubscriptionViewModel.IsPrisonCourtRegister)
            {
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsPublished.AddSorted(sub.DraftNowSub);
                return;
            }
            if (sub.DraftNowSub.NowSubscriptionViewModel.IsInformantRegister)
            {
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsPublished.AddSorted(sub.DraftNowSub);
                return;
            }
        }

        private void CollectAllUsagesForDraftPrompts(
            List<DraftResultModel> draftResults, 
            List<ResultPromptTreeViewModel> draftPromptRules, 
            List<DraftPromptModel> draftPrompts, 
            List<DraftPromptUsageModel> additionalUsages,
            List<DraftPromptModel> allDraftPrompts,
            List<string> publicationTags)
        {
            if (draftPrompts == null || draftPrompts.Count==0) { return; }

            //Get additional Result Usages based on the draft prompts
            var otherResults = FindAdditionalPromptUsages(draftPrompts, draftResults);

            if (otherResults.Count == 0)
            {
                return;
            }

            var newDrafts = new List<DraftResultModel>();

            //collect a draft version of the other results
            var cmd = treeModel.DraftResultDefinitionCommand as DraftResultDefinitionCommand;
            foreach (var rd in otherResults)
            {
                //ensure that we dont record duplicate additional usages
                var matchedAdditionalUsage = additionalUsages.FirstOrDefault(x => x.ExistingPublishedResult.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == 
                                                                rd.ExistingPublishedResult.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
                if (matchedAdditionalUsage != null)
                {
                    continue;
                }

                additionalUsages.Add(rd);

                DraftResultModel draftResult = null;
                //see if there is already a draft version
                var draft = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.FirstOrDefault
                                            (
                                                x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == 
                                                                    rd.ExistingPublishedResult.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                                            );
                if (draft == null)
                {
                    //make a new draft just for the purposes of publishing
                    draft = cmd.DraftResultDefinitionTreeViewModel(rd.ExistingPublishedResult, publicationTags, false);
                    draftResult = new DraftResultModel(treeModel, draft, false);
                }
                else
                {
                    draftResult = new DraftResultModel(treeModel, draft, true);
                    //find any draft children of for the existing draft result and add to the collection of alldraftresults and newdraftresults
                    var draftChildren = FindChildDraftResults(draft);
                    foreach (var child in draftChildren)
                    {
                        var match = draftResults.FirstOrDefault(x => x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == child.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
                        if (match == null)
                        {
                            draftResult.AdditionalDraftChildren.Add(child);
                            draftResults.Add(child);
                            newDrafts.Add(child);
                        }
                    }
                }

                //add the draft result to the collection of results to be published
                draftResults.Add(draftResult);

                //add to the collection of new drafts for recursion purposes
                newDrafts.Add(draftResult);

                //set the usage to the draft result definition
                rd.Usage = draftResult;
            }

            //look for additional draftPrompts from the new draft results
            var otherDraftPromptRules = FindDraftPromptRules(newDrafts);

            //filter the other prompt rules to ensure that we do not repeat the prompt rules that we have already collected
            var otherDraftPromptRulesFiltered = otherDraftPromptRules.Where(
                x => !draftPromptRules.Any(
                                    y => y.ResultPromptRuleViewModel.ResultPromptViewModel.MasterUUID == x.ResultPromptRuleViewModel.ResultPromptViewModel.MasterUUID)).ToList();

            //collect the additional filtered prompt rules
            draftPromptRules.AddRange(otherDraftPromptRulesFiltered);

            //determine if any other filtered Draft prompt rules have draft prompts as well
            var otherDraftPrompts = otherDraftPromptRulesFiltered.Where(x => x.ResultPromptRuleViewModel.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
                                        .Select(x => x.ResultPromptRuleViewModel.ResultPromptViewModel).ToList();

            //append new items to the collection of allprompts
            var newDraftPrompts = new List<DraftPromptModel>();
            foreach (var dp in otherDraftPrompts)
            {
                var match = allDraftPrompts.FirstOrDefault(x => x.Prompt.UUID == dp.UUID);
                if (match == null)
                {
                    var newDp = new DraftPromptModel(treeModel, dp, allDraftPrompts);
                    newDraftPrompts.Add(newDp);
                    allDraftPrompts.Add(newDp);
                }
            }

            //remove any unchanged items
            newDraftPrompts = FilterDraftPromptsRemoveUnchangedUnwantedItems(newDraftPrompts);

            //recursive call to obtain more drafts
            CollectAllUsagesForDraftPrompts(draftResults, draftPromptRules, newDraftPrompts, additionalUsages, allDraftPrompts, publicationTags);
        }

        private void CollectAllUsagesForDraftResultDefinitionWordGroup(List<DraftResultModel> allDraftResults, List<DraftResultModel> draftResults, 
                                                                        List<DraftNowModel> allDraftNows, List<DraftNowSubscriptionModel> allDraftNowSubscriptions,
                                                                        List<DraftResultDefinitionWordGroupUsageModel> additionalUsages, 
                                                                        List<string> publicationTags, List<DraftPromptModel> allDraftPrompts)
        {
            //prepare draft result by setting their changes
            draftResults.ForEach(x => x.SetChanges(allDraftPrompts, allDraftNows, allDraftNowSubscriptions));

            //process draft results with result definition word group changes to collect additional usages of those changes
            var cmd = treeModel.DraftResultDefinitionCommand as DraftResultDefinitionCommand;
            var newDrafts = new List<DraftResultModel>();
            foreach (var dr in draftResults.Where(x=>x.IsResultDefinitionWordGroupsChangedWithWordChanges))
            {
                foreach(var draftRdwg in dr.ChangedResultDefinitionWordGroups.Where(x=>x.IsChangedWithWordChanges))
                {
                    var parentGroups = new List<ResultWordGroupViewModel>(draftRdwg.DraftResultDefinitionWordGroupViewModel.ParentWordGroups);
                    foreach (var rwg in parentGroups)
                    {
                        //determine if the word group already has a draft result entry
                        var matchedDraft = allDraftResults.FirstOrDefault(
                            x => x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == rwg.ParentResultDefinitionVM.MasterUUID);

                        if (matchedDraft != null) { continue; }

                        var existingPublishedResult = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished.First(
                            x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == rwg.ParentResultDefinitionVM.MasterUUID);

                        DraftResultModel draftResult = null;
                        //see if there is already a draft version
                        var draft = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.FirstOrDefault
                                                    (
                                                        x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ==
                                                                            rwg.ParentResultDefinitionVM.MasterUUID
                                                    );
                        if (draft == null)
                        {
                            //make a new draft just for the purposes of publishing
                            draft = cmd.DraftResultDefinitionTreeViewModel(existingPublishedResult, publicationTags, false);

                            //make a new draft for the usage
                            draftResult = new DraftResultModel(treeModel, draft, false);
                        }
                        else
                        {
                            //make a new draft for the usage
                            draftResult = new DraftResultModel(treeModel, draft, true);

                            //find any draft children of for the existing draft result and add to the collection of alldraftresults and newdraftresults
                            var draftChildren = FindChildDraftResults(draft);
                            foreach (var child in draftChildren)
                            {
                                var match = draftResults.FirstOrDefault(x => x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == child.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
                                if (match == null)
                                {
                                    draftResult.AdditionalDraftChildren.Add(child);
                                    draftResults.Add(child);
                                    newDrafts.Add(child);
                                }
                            }
                        }

                        //add the draft to the collection of drafts
                        newDrafts.Add(draftResult);
                        allDraftResults.Add(draftResult);

                        //Set the usage
                        var usage = new DraftResultDefinitionWordGroupUsageModel(draftResult, draftRdwg, rwg);
                        additionalUsages.Add(usage);
                    }
                }
            }

            //recursively process the new drafts
            if (draftResults.Count>0)
            {
                CollectAllUsagesForDraftResultDefinitionWordGroup(allDraftResults, newDrafts, allDraftNows, allDraftNowSubscriptions, additionalUsages, publicationTags, allDraftPrompts);
            }
        }

        private List<DraftPromptUsageModel> FindAdditionalPromptUsages(List<DraftPromptModel> draftPrompts, List<DraftResultModel> draftResults)
        {
            return (from dp in draftPrompts
                            join rpr in treeModel.AllResultPromptViewModel.Prompts
                                on dp.Prompt.MasterUUID equals rpr.ResultPromptViewModel.MasterUUID
                            join rdPub in treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished
                               on rpr.ResultDefinitionViewModel.MasterUUID equals rdPub.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                            where rpr.ResultPromptViewModel.IsDeleted == false
                               && rpr.IsDeleted == false
                               && rpr.ResultDefinitionViewModel.IsDeleted == false
                               && !(
                                       from rdDraft in draftResults 
                                       select rdDraft.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                                    ).Contains(rpr.ResultDefinitionViewModel.MasterUUID)
                            select new DraftPromptUsageModel(dp, rdPub)).ToList();
        }

        private List<DraftParentResultUsageModel> FindAdditionalParentResultUsages(
            List<DraftResultModel> draftResults, 
            List<DraftResultModel> allDraftResults,
            List<DraftParentResultUsageModel> additionalParentResultUsages)
        {
            var usages = (from pr in treeModel.AllResultRuleViewModel.Rules.Where(x=>x.ParentResultDefinitionViewModel != null 
                                                                                          && !x.IsDeleted
                                                                                          //&& x.ChildResultDefinitionViewModel != null
                                                                                          && !x.ChildResultDefinitionViewModel.IsDeleted)
                    join dr in draftResults
                        on pr.ChildResultDefinitionViewModel.MasterUUID equals dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                    join rdPub in treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsPublished
                       on pr.ParentResultDefinitionViewModel.MasterUUID equals rdPub.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                    where !(
                               from rdDraft in allDraftResults
                               select rdDraft.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                            ).Contains(rdPub.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID)
                    select new DraftParentResultUsageModel(dr, rdPub)).ToList();

            //harvest any parent child usage where the child has been changed
            var res = usages.Where(x => x.IsChildResultChanged).ToList();

            //Include any additional items where the child is not changed but has already been included as a parent in the collection of additional parent result changes.  
            //This supports walking up the tree for a changed child
            foreach(var item in usages.Where(x=>!x.IsChildResultChanged))
            {
                var match = additionalParentResultUsages.FirstOrDefault(
                    x => x.ExistingParentPublishedResult.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ==
                        item.ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
                if (match != null)
                {
                    res.Add(item);
                }
            }

            return res;
        }

        private List<ResultPromptTreeViewModel> FindDraftPromptRules(List<DraftResultModel> draftResults)
        {
            var allPrompts = new List<ResultPromptTreeViewModel>();
            foreach(var item in draftResults.Select(x=> x.Result))
            {
                var prompts = item.Children.Where(x => x.GetType() == typeof(ResultPromptTreeViewModel)).Select(x => x as ResultPromptTreeViewModel);
                allPrompts.AddRange(prompts);
            }
            return allPrompts.Where(x => x.ResultPromptRuleViewModel.PublishedStatus == PublishedStatus.Draft && !x.ResultPromptRuleViewModel.IsDeleted).ToList();
        }

        private List<DraftResultModel> FindChildDraftResults(ResultDefinitionTreeViewModel source)
        {
            var children = source.FlattenedChildResults;
            var res = new List<DraftResultModel>();
            foreach (var child in children.Where(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft && !x.ResultRuleViewModel.IsDeleted))
            {
                //get the corresponding draft root item
                var root = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.First(
                x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == child.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID
                && x.ResultRuleViewModel.ParentResultDefinitionViewModel == null);

                res.Add(new DraftResultModel(treeModel, root, true));
            }
            return res;
        }

        private List<DraftNowModel> GetAllDraftNowsEDTs()
        {
            //left join the draft nows and edts to their published equivalents
            var nows = (from dn in treeModel.AllNowsTreeViewModel.NowsDraft
                          join pn in treeModel.AllNowsTreeViewModel.NowsPublished
                            on dn.NowViewModel.MasterUUID equals pn.NowViewModel.MasterUUID ?? pn.NowViewModel.UUID
                          into draftWithPublished
                          from subDraftWithPublished in draftWithPublished.DefaultIfEmpty()
                          select new DraftNowModel(treeModel, dn, subDraftWithPublished, false)).ToList();

            var edts = (from de in treeModel.AllEDTsTreeViewModel.EdtsDraft
                        join pe in treeModel.AllEDTsTreeViewModel.EdtsPublished
                          on de.NowViewModel.MasterUUID equals pe.NowViewModel.MasterUUID ?? pe.NowViewModel.UUID
                        into draftWithPublished
                        from subDraftWithPublished in draftWithPublished.DefaultIfEmpty()
                        select new DraftNowModel(treeModel, de, subDraftWithPublished, false)).ToList();
            nows.AddRange(edts);
            return nows;
        }

        private List<DraftNowSubscriptionModel> GetAllDraftNowSubscriptions()
        {
            //left join the draft now subscriptions to their published equivalents
            var nows = (from ds in treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsDraft
                        join ps in treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsPublished
                          on ds.NowSubscriptionViewModel.MasterUUID equals ps.NowSubscriptionViewModel.MasterUUID ?? ps.NowSubscriptionViewModel.UUID
                        into draftWithPublished
                        from subDraftWithPublished in draftWithPublished.DefaultIfEmpty()
                        select new DraftNowSubscriptionModel(treeModel, ds, subDraftWithPublished, false)).ToList();

            var edts = (from ds in treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsDraft
                        join ps in treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsPublished
                          on ds.NowSubscriptionViewModel.MasterUUID equals ps.NowSubscriptionViewModel.MasterUUID ?? ps.NowSubscriptionViewModel.UUID
                        into draftWithPublished
                        from subDraftWithPublished in draftWithPublished.DefaultIfEmpty()
                        select new DraftNowSubscriptionModel(treeModel, ds, subDraftWithPublished, false)).ToList();

            nows.AddRange(edts);

            var crtRegs = (from ds in treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsDraft
                        join ps in treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsPublished
                          on ds.NowSubscriptionViewModel.MasterUUID equals ps.NowSubscriptionViewModel.MasterUUID ?? ps.NowSubscriptionViewModel.UUID
                        into draftWithPublished
                        from subDraftWithPublished in draftWithPublished.DefaultIfEmpty()
                        select new DraftNowSubscriptionModel(treeModel, ds, subDraftWithPublished, false)).ToList();

            nows.AddRange(crtRegs);

            var pCrtRegs = (from ds in treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsDraft
                           join ps in treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsPublished
                             on ds.NowSubscriptionViewModel.MasterUUID equals ps.NowSubscriptionViewModel.MasterUUID ?? ps.NowSubscriptionViewModel.UUID
                           into draftWithPublished
                           from subDraftWithPublished in draftWithPublished.DefaultIfEmpty()
                           select new DraftNowSubscriptionModel(treeModel, ds, subDraftWithPublished, false)).ToList();

            nows.AddRange(pCrtRegs);

            var infRegs = (from ds in treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsDraft
                           join ps in treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsPublished
                              on ds.NowSubscriptionViewModel.MasterUUID equals ps.NowSubscriptionViewModel.MasterUUID ?? ps.NowSubscriptionViewModel.UUID
                            into draftWithPublished
                            from subDraftWithPublished in draftWithPublished.DefaultIfEmpty()
                            select new DraftNowSubscriptionModel(treeModel, ds, subDraftWithPublished, false)).ToList();

            nows.AddRange(infRegs);

            return nows;
        }

    }
}
