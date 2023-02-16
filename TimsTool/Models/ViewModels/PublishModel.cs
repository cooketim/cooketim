using DataLib;
using Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Models.ViewModels
{
    public enum PublishMode
    {
        NOWs = 0,
        Subscriptions = 1,
        Results = 2,
        All = 3
    }
    public class PublishModel
    {
        private string detailedPublicationReport;
        private string summaryPublicationReport;
        private PublishMode publishMode;
        private ITreeModel treeModel;

        public PublishModel(ITreeModel treeModel, DraftResultModel selectedDraftResult, List<DraftResultModel> draftResults, List<DraftPromptModel> allDraftPrompts, List<DraftNowModel> allDraftNows, List<DraftNowSubscriptionModel> allDraftNowSubscriptions)
        {
            this.treeModel = treeModel;
            SelectedDraftResult = selectedDraftResult;
            ChildDraftResults = new List<DraftResultModel>(draftResults);
            AllDraftNows = allDraftNows;
            AllDraftNowSubscriptions = allDraftNowSubscriptions;
            AllDraftPrompts = allDraftPrompts;
            AdditionalPromptUsages = new List<DraftPromptUsageModel>();
            AdditionalParentResultUsages = new List<DraftParentResultUsageModel>();
            AdditionalResultDefinitionWordGroupUsages = new List<DraftResultDefinitionWordGroupUsageModel>();
            AdditionalResultPromptWordGroupUsages = new List<DraftResultModel>();
            publishMode = PublishMode.Results;
        }

        public PublishModel(ITreeModel treeModel, DraftNowModel draftNow)
        {
            this.treeModel = treeModel;
            AllDraftNows = new List<DraftNowModel>() { draftNow };
            publishMode = PublishMode.NOWs;
        }

        public PublishModel(ITreeModel treeModel, DraftNowSubscriptionModel draftNowSubscription)
        {
            this.treeModel = treeModel;
            AllDraftNowSubscriptions = new List<DraftNowSubscriptionModel>() { draftNowSubscription };
            publishMode = PublishMode.Subscriptions;
        }

        public bool HasErrors 
        {
            get => !string.IsNullOrEmpty(ErrorReport);
        }
        public string ErrorReport { get; set; }
        public DraftResultModel SelectedDraftResult { get; set; }
        public List<DraftResultModel> ChildDraftResults { get; set; }
        public List<DraftPromptModel> AllDraftPrompts { get; set; }
        public List<DraftNowModel> AllDraftNows { get; }
        public List<DraftNowSubscriptionModel> AllDraftNowSubscriptions { get; }
        public List<DraftPromptUsageModel> AdditionalPromptUsages { get; set; }
        public List<DraftParentResultUsageModel> AdditionalParentResultUsages { get; set; }
        public List<DraftResultDefinitionWordGroupUsageModel> AdditionalResultDefinitionWordGroupUsages { get; set; }

        public List<DraftResultModel> AdditionalResultPromptWordGroupUsages { get; set; }

        public List<DraftResultModel> ChangedResults 
        { 
            get
            {
                var draftResults = AllDraftResults.Where(x => x.IsChanged).ToList();

                //additional result usages may not have explicit changes, rather the parent has changed because it is associated to a changed child
                //we include additional usages where there are not explicit changes, where explicit changes have been made these will be collected by 
                //way of changed prompt or selected or changed draft results above
                if (AdditionalParentResultUsages != null)
                {
                    draftResults.AddRange(AdditionalParentResultUsages.Where(x => !x.ParentUsage.IsChanged).Select(x => x.ParentUsage));
                }

                //ensure that the results are distinct
                return draftResults.Distinct(new DraftResultModelComparer()).ToList();                
            }
        }

        public List<DraftResultModel> DraftResultsSavePending
        {
            get
            {
                var draftResults = new List<DraftResultModel>();

                if (AdditionalPromptUsages != null)
                {
                    draftResults.AddRange(AdditionalPromptUsages.Where(x => x.Usage.IsChanged).Select(x => x.Usage));
                }

                if (AdditionalResultDefinitionWordGroupUsages != null)
                {
                    draftResults.AddRange(AdditionalResultDefinitionWordGroupUsages.Select(x => x.Usage));
                }

                //additional result usages may not have explicit changes, rather the parent has changed because it is associated to a changed child
                //we include additional usages where there are not explicit changes, where explicit changes have been made these will be collected by 
                //way of changed prompt or selected or changed draft results above
                if (AdditionalParentResultUsages != null)
                {
                    draftResults.AddRange(AdditionalParentResultUsages.Where(x => !x.ParentUsage.IsChanged).Select(x => x.ParentUsage));
                }

                //ensure that the results are distinct
                return draftResults.Distinct(new DraftResultModelComparer()).ToList();
            }
        }

        public List<DraftResultModel> AllDraftResults
        {
            get
            {
                var draftResults = new List<DraftResultModel>();

                if (SelectedDraftResult != null)
                {
                    draftResults.Add(SelectedDraftResult);
                }

                if (ChildDraftResults != null)
                {
                    draftResults.AddRange(ChildDraftResults);
                }

                if (AdditionalPromptUsages != null)
                {
                    draftResults.AddRange(AdditionalPromptUsages.Select(x => x.Usage));
                }

                if (AdditionalParentResultUsages != null)
                {
                    draftResults.AddRange(AdditionalParentResultUsages.Select(x => x.ParentUsage));
                }

                if (AdditionalResultDefinitionWordGroupUsages != null)
                {
                    draftResults.AddRange(AdditionalResultDefinitionWordGroupUsages.Select(x => x.Usage));
                }

                if (AdditionalResultPromptWordGroupUsages != null)
                {
                    draftResults.AddRange(AdditionalResultPromptWordGroupUsages);
                }

                draftResults.Where(x => x.AdditionalDraftChildren.Any()).ToList().ForEach(x => draftResults.AddRange(x.AdditionalDraftChildren));

                //ensure that the results are distinct
                return draftResults.Distinct(new DraftResultModelComparer()).ToList();
            }
        }
        public List<DraftResultDefinitionWordGroupModel> UpdatedResultDefinitionWordGroups
        {
            get
            {
                var draftRdwgs = new List<DraftResultDefinitionWordGroupModel>();

                if (SelectedDraftResult != null && SelectedDraftResult.IsResultDefinitionWordGroupsChanged)
                {
                    draftRdwgs.AddRange(SelectedDraftResult.ChangedResultDefinitionWordGroups.Where(x=>x.IsChanged && !x.IsChangedWithWordChanges));
                }

                if (ChildDraftResults != null)
                {
                    foreach(var dr in ChildDraftResults.Where(x=>x.IsResultDefinitionWordGroupsChanged))
                    {
                        draftRdwgs.AddRange(dr.ChangedResultDefinitionWordGroups.Where(x => x.IsChanged && !x.IsChangedWithWordChanges));
                    }
                }

                if (AdditionalPromptUsages != null)
                {
                    foreach (var dr in AdditionalPromptUsages.Where(x => x.Usage.IsResultDefinitionWordGroupsChanged))
                    {
                        draftRdwgs.AddRange(dr.Usage.ChangedResultDefinitionWordGroups.Where(x => x.IsChanged && !x.IsChangedWithWordChanges));
                    }
                }

                if (AdditionalResultDefinitionWordGroupUsages != null)
                {
                    foreach (var dr in AdditionalResultDefinitionWordGroupUsages.Where(x => x.Usage.IsResultDefinitionWordGroupsChanged))
                    {
                        draftRdwgs.AddRange(dr.Usage.ChangedResultDefinitionWordGroups.Where(x => x.IsChanged && !x.IsChangedWithWordChanges));
                    }
                }

                if (AdditionalParentResultUsages != null)
                {
                    foreach (var dr in AdditionalParentResultUsages.Where(x => x.ParentUsage.IsResultDefinitionWordGroupsChanged))
                    {
                        draftRdwgs.AddRange(dr.ParentUsage.ChangedResultDefinitionWordGroups.Where(x => x.IsChanged && !x.IsChangedWithWordChanges));
                    }
                }

                return draftRdwgs;
            }
        }

        public List<DraftFixedListModel> UpdatedFixedLists
        {
            get
            {
                if (AllDraftPrompts == null) { return null; }

                return AllDraftPrompts.Where(x => x.IsFixedListChanged).Select(x => x.DraftFixedList).ToList();
            }
        }

        public List<DraftResultPromptWordGroupModel> UpdatedResultPromptWordGroups
        {
            get
            {
                if (AllDraftPrompts == null) { return null; }

                var res = new List<DraftResultPromptWordGroupModel>();

                foreach (var prompt in AllDraftPrompts.Where(x => x.HasResultPromptWordGroupChanges))
                {
                    res.AddRange(prompt.DraftResultPromptWordGroups.Where(x => x.IsChanged && !x.IsChangedWithWordChanges));
                }
                return res;
            }
        }

        public List<DraftResultPromptWordGroupModel> ResultPromptWordGroupsWithWordChanges
        {
            get
            {
                if (AllDraftPrompts == null) { return null; }

                var res = new List<DraftResultPromptWordGroupModel>();

                foreach (var prompt in AllDraftPrompts.Where(x => x.HasResultPromptWordGroupChanges))
                {
                    res.AddRange(prompt.DraftResultPromptWordGroups.Where(x => x.IsChanged && x.IsChangedWithWordChanges));
                }
                return res;
            }
        }

        private class DraftResultModelComparer : IEqualityComparer<DraftResultModel>
        {
            public bool Equals(DraftResultModel x, DraftResultModel y)
            {
                if (x == null && y == null)
                    return true;
                else if (x == null || y == null)
                    return false;
                else if (
                            x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == y.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID 
                         && x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == y.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus
                         )
                    return true;
                else
                    return false;
            }

            public int GetHashCode(DraftResultModel dr)
            {
                return 
                    dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID.GetHashCode() ^
                    dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus.GetHashCode();
            }
        }

        public string DetailedPublicationReport 
        { 
            get
            {
                if (string.IsNullOrEmpty(detailedPublicationReport))
                {
                    SetPublicationReport();
                }
                return detailedPublicationReport;
            }
        }
        public string SummaryPublicationReport
        {
            get
            {
                if (string.IsNullOrEmpty(summaryPublicationReport))
                {
                    SetPublicationReport();
                }
                return summaryPublicationReport;
            }
        }

        private void SetPublicationReport()
        {
            var sbDetailReport = new StringBuilder();
            var sbSummaryReport = new StringBuilder();
            if (SelectedDraftResult != null && SelectedDraftResult.IsChanged)
            {
                SetChangedResultReport(SelectedDraftResult, sbDetailReport, sbSummaryReport, true);
                sbDetailReport.AppendLine();
                sbSummaryReport.AppendLine();
            }

            if (ChildDraftResults != null)
            {
                foreach (var rd in ChildDraftResults.Where(x => x.IsChanged))
                {
                    SetChangedResultReport(rd, sbDetailReport, sbSummaryReport, false);
                    sbDetailReport.AppendLine();
                    sbSummaryReport.AppendLine();
                }
            }

            if (AllDraftPrompts != null && AllDraftPrompts.Where(x => x.IsChanged).Count() > 0)
            {
                sbDetailReport.AppendLine();
                foreach (var dp in AllDraftPrompts.Where(x => x.IsChanged))
                {
                    sbSummaryReport.AppendLine(string.Format("Result Prompt: '{0}'...", dp.Prompt.Label));
                    sbDetailReport.AppendLine(string.Format("Result Prompt: '{0}'...", dp.Prompt.Label));
                    if (!string.IsNullOrEmpty(dp.ChangeReport))
                    {
                        sbDetailReport.AppendLine(dp.ChangeReport);
                    }
                }
            }

            //Report Already In Draft
            if (AllDraftResults != null)
            {
                var unselectedInDraft = AllDraftResults.Where(x => x.IsInDraftAlready).ToList();
                if (SelectedDraftResult != null)
                {
                    unselectedInDraft.RemoveAll(x => x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == 
                                                        SelectedDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
                    if (unselectedInDraft.Any())
                    {
                        var inDraftAlreadyList = new List<string>();
                        unselectedInDraft.OrderBy(x=> x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label).ToList()
                                         .ForEach(x=> inDraftAlreadyList.Add(x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        
                        sbSummaryReport.AppendLine();
                        sbDetailReport.AppendLine();
                        sbSummaryReport.AppendLine("The following 'In Draft' results have been selected to be published because of the usage reasons detailed in this report...");
                        sbDetailReport.AppendLine("The following 'In Draft' results have been selected to be published because of the usage reasons detailed in this report...");

                        sbSummaryReport.AppendLine(String.Join(", ", inDraftAlreadyList));
                        sbDetailReport.AppendLine(String.Join(", ", inDraftAlreadyList));
                    }
                }
            }

            //Report additional results that are deemed changed because of the associated changed prompt usages
            ReportPromptUsages(sbDetailReport, sbSummaryReport);

            //Report additional results deemed changed due to an association to a changed result word group.  
            //If the usage is for a result that is changed then it will have already been reported, 
            //therefore we look for usages where the result is not changed
            ReportWordGroupUsages(sbDetailReport, sbSummaryReport);

            //Report additional parent result changes.  If the usage is for a result that is changed then it will have already been reported, 
            //therefore we look for usages where the result is not changed
            ReportParentResultUsages(sbDetailReport, sbSummaryReport);

            //Report the changed result definition word groups that comprise only synonym changes rather than a word change
            ReportWordGroupChanges(sbDetailReport, sbSummaryReport);

            //Report on any Fixed Lists that have been changed
            ReportFixedListChanges(sbDetailReport, sbSummaryReport);

            //Report the changed result prompt word groups that comprise only synonym changes rather than a word change
            ReportPromptWordGroupChanges(sbDetailReport, sbSummaryReport);

            //Report the changed result prompt word groups that comprise a word change
            ReportPromptWordGroupChangeWithWordChange(sbDetailReport, sbSummaryReport);

            //Report on associated now changes
            ReportAssociatedNowsChanges(sbDetailReport, sbSummaryReport);

            //Report on NOW changes
            ReportNowsChanges(sbDetailReport, sbSummaryReport);

            //Report on associated now subscription changes
            ReportAssociatedNowSubscriptionChanges(sbDetailReport, sbSummaryReport);

            //Report on NOW Subscription changes
            ReportNowSubscriptionChanges(sbDetailReport, sbSummaryReport);

            //remove double spacing
            detailedPublicationReport = Regex.Replace(sbDetailReport.ToString(), @"(\r\n){2,}", "\r\n\r\n");
            summaryPublicationReport = Regex.Replace(sbSummaryReport.ToString(), @"(\r\n){2,}", "\r\n\r\n");
        }

        private void ReportNowSubscriptionChanges(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (AllDraftNowSubscriptions == null || !AllDraftNowSubscriptions.Any() || (publishMode != PublishMode.Subscriptions && publishMode != PublishMode.All)) { return; }

            var associatedNowSubscriptions = AllDraftResults.Where(x => x.IsChanged && x.NowSubscriptionUsages != null && x.NowSubscriptionUsages.Where(y => y.IsChanged).Any()).SelectMany(x => x.NowSubscriptionUsages).GroupBy(x => x.DraftNowSub.NowSubscriptionViewModel.UUID).Select(x => x.First()).ToList();
            var toReportIds = AllDraftNowSubscriptions.Select(x => x.DraftNowSub.NowSubscriptionViewModel.UUID).Except(associatedNowSubscriptions.Select(x => x.DraftNowSub.NowSubscriptionViewModel.UUID));
            if (toReportIds.Any())
            {
                var toReportNowSubscriptions = (from dns in AllDraftNowSubscriptions
                                                join dnsId in toReportIds on dns.DraftNowSub.NowSubscriptionViewModel.UUID equals dnsId                                    
                                                select dns).GroupBy(x=>x.DraftNowSub.NowSubscriptionViewModel.SubscriptionType).ToList();
                foreach(var group in toReportNowSubscriptions)
                {
                    sbSummaryReport.AppendLine(string.Format("The following {0} Subscription changes are published...", group.Key));
                    sbDetailReport.AppendLine(string.Format("The following {0} Subscription changes are published...", group.Key));

                    foreach (var ns in group.ToList().OrderBy(x => x.DraftNowSub.NowSubscriptionViewModel.Name))
                    {
                        sbSummaryReport.AppendLine(string.Format("{0}", ns.DraftNowSub.NowSubscriptionViewModel.Name));
                        sbDetailReport.AppendLine(string.Format("{0}", ns.DraftNowSub.NowSubscriptionViewModel.Name));
                        if (!string.IsNullOrEmpty(ns.ChangeReport))
                        {
                            sbDetailReport.AppendLine(ns.ChangeReport);
                        }
                    }
                }
            }
        }

        private void ReportNowsChanges(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (AllDraftNows == null || !AllDraftNows.Any() || (publishMode != PublishMode.NOWs && publishMode != PublishMode.All)) { return; }

            //exclude any associated nows as these will be reported as part of the result changes
            var associatedNowsEdts = AllDraftResults.Where(x => x.IsChanged && x.NowUsages != null && x.NowUsages.Where(y => y.IsChanged).Any()).SelectMany(x => x.NowUsages).GroupBy(x => x.DraftNow.NowViewModel.UUID).Select(x => x.First()).ToList();
            var toReportIds = AllDraftNows.Select(x => x.DraftNow.NowViewModel.UUID).Except(associatedNowsEdts.Select(x => x.DraftNow.NowViewModel.UUID));

            var toReportNows = (from dn in AllDraftNows
                                join dnId in toReportIds on dn.DraftNow.NowViewModel.UUID equals dnId
                                where !dn.DraftNow.NowViewModel.IsEDT
                                select dn).ToList();

            var toReportEDTs = (from dn in AllDraftNows
                                join dnId in toReportIds on dn.DraftNow.NowViewModel.UUID equals dnId
                                where dn.DraftNow.NowViewModel.IsEDT
                                select dn).ToList();

            if (toReportNows.Any())
            {
                sbSummaryReport.AppendLine("The following Nows changes are published...");
                sbDetailReport.AppendLine("The following Nows changes are published...");

                foreach (var dn in toReportNows)
                {
                    sbSummaryReport.AppendLine(string.Format("{0}", dn.DraftNow.NowViewModel.Name));
                    sbDetailReport.AppendLine(string.Format("{0}", dn.DraftNow.NowViewModel.Name));
                    if (!string.IsNullOrEmpty(dn.ChangeReport))
                    {
                        sbDetailReport.AppendLine(dn.ChangeReport);
                    }
                }
            }

            if (toReportEDTs.Any())
            {
                sbSummaryReport.AppendLine("The following EDT changes are published...");
                sbDetailReport.AppendLine("The following EDT changes are published...");

                foreach (var dn in toReportEDTs)
                {
                    sbSummaryReport.AppendLine(string.Format("{0}", dn.DraftNow.NowViewModel.Name));
                    sbDetailReport.AppendLine(string.Format("{0}", dn.DraftNow.NowViewModel.Name));
                    if (!string.IsNullOrEmpty(dn.ChangeReport))
                    {
                        sbDetailReport.AppendLine(dn.ChangeReport);
                    }
                }
            }
        }

        private void ReportAssociatedNowsChanges(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (publishMode != PublishMode.Results && publishMode != PublishMode.All) { return; }

            var associatedNowsEdts = AllDraftResults.Where(x => x.IsChanged && x.NowUsages != null && x.NowUsages.Where(y => y.IsChanged).Any()).SelectMany(x => x.NowUsages).GroupBy(x => x.DraftNow.NowViewModel.UUID).Select(x => x.First()).ToList();
            if (associatedNowsEdts.Any())
            {
                var changedNows = associatedNowsEdts.Where(x => x.IsChanged && !x.DraftNow.NowViewModel.IsEDT).OrderBy(x => x.DraftNow.NowViewModel.Name).ToList();
                var changedEDTs = associatedNowsEdts.Where(x => x.IsChanged && x.DraftNow.NowViewModel.IsEDT).OrderBy(x => x.DraftNow.NowViewModel.Name).ToList();

                if (changedNows.Any())
                {
                    sbSummaryReport.AppendLine("The following Nows are published because they are impacted by changes to results...");
                    sbDetailReport.AppendLine("The following Nows are published because they are impacted by changes to results...");

                    foreach (var dn in changedNows)
                    {
                        sbSummaryReport.AppendLine(string.Format("{0}", dn.DraftNow.NowViewModel.Name));
                        sbDetailReport.AppendLine(string.Format("{0}", dn.DraftNow.NowViewModel.Name));
                        if (!string.IsNullOrEmpty(dn.ChangeReport))
                        {
                            sbDetailReport.AppendLine(dn.ChangeReport);
                        }
                    }
                }

                if (changedEDTs.Any())
                {
                    sbSummaryReport.AppendLine("The following EDTs are published because they are impacted by changes to results...");
                    sbDetailReport.AppendLine("The following EDTs are published because they are impacted by changes to results...");

                    foreach (var dn in changedEDTs)
                    {
                        sbSummaryReport.AppendLine(string.Format("{0}", dn.DraftNow.NowViewModel.Name));
                        sbDetailReport.AppendLine(string.Format("{0}", dn.DraftNow.NowViewModel.Name));
                        if (!string.IsNullOrEmpty(dn.ChangeReport))
                        {
                            sbDetailReport.AppendLine(dn.ChangeReport);
                        }
                    }
                }
            }
        }

        private void ReportAssociatedNowSubscriptionChanges(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (publishMode != PublishMode.Subscriptions && publishMode != PublishMode.All) { return; }

            var associatedNowSubscriptions = AllDraftResults.Where(x => x.IsChanged && x.NowSubscriptionUsages != null && x.NowSubscriptionUsages.Where(y => y.IsChanged).Any()).SelectMany(x => x.NowSubscriptionUsages).GroupBy(x => x.DraftNowSub.NowSubscriptionViewModel.UUID).Select(x => x.First()).ToList();
            if (associatedNowSubscriptions.Any())
            {
                var associatedNowSubscriptionsByType = associatedNowSubscriptions.GroupBy(x => x.DraftNowSub.NowSubscriptionViewModel.SubscriptionType);
                foreach(var group in associatedNowSubscriptionsByType)
                {
                    sbSummaryReport.AppendLine(string.Format("The following {0} Subscriptions are published because they are impacted by changes to results...", group.Key));
                    sbDetailReport.AppendLine(string.Format("The following {0} Subscriptions are published because they are impacted by changes to results...", group.Key));

                    foreach(var ns in group.ToList().OrderBy(x=>x.DraftNowSub.NowSubscriptionViewModel.Name))
                    {
                        sbSummaryReport.AppendLine(string.Format("{0}", ns.DraftNowSub.NowSubscriptionViewModel.Name));
                        sbDetailReport.AppendLine(string.Format("{0}", ns.DraftNowSub.NowSubscriptionViewModel.Name));
                        if (!string.IsNullOrEmpty(ns.ChangeReport))
                        {
                            sbDetailReport.AppendLine(ns.ChangeReport);
                        }
                    }
                }
            }
        }

        private void ReportPromptWordGroupChangeWithWordChange(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (ResultPromptWordGroupsWithWordChanges != null && ResultPromptWordGroupsWithWordChanges.Any())
            {
                sbSummaryReport.AppendLine("The following result prompt word synonms have been changed with word changes...");
                sbDetailReport.AppendLine("The following result prompt word synonms have been changed with word changes...");

                foreach (var rpwg in ResultPromptWordGroupsWithWordChanges)
                {
                    sbSummaryReport.AppendLine(string.Format("Word: {0}", rpwg.DraftResultPromptWordGroup.ResultPromptWord));
                    sbDetailReport.AppendLine(string.Format("Word: {0}", rpwg.DraftResultPromptWordGroup.ResultPromptWord));

                    dynamic parentUsages = rpwg.ParentResultPrompts;

                    if (parentUsages.Count > 0)
                    {
                        sbDetailReport.AppendLine("The following additional result and prompt combinations share the word group and a new revision will be created...");
                        //new { PublishedResultDefinitionTree = rdPub, PublishedResultPromptViewModel = rpr.ResultPromptViewModel }
                        foreach (dynamic item in parentUsages)
                        {
                            sbDetailReport.AppendLine(string.Format("{0} - {1}", item.PublishedResultDefinitionTree.ResultRuleViewModel.ChildResultDefinitionViewModel.Label,
                                                                                item.PublishedResultPromptViewModel.Label));
                        }
                    }
                }
            }
        }

        private void ReportPromptWordGroupChanges(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (UpdatedResultPromptWordGroups != null && UpdatedResultPromptWordGroups.Any())
            {
                sbSummaryReport.AppendLine("The following result prompt word synonms have been changed...");
                sbDetailReport.AppendLine("The following result prompt word synonms have been changed...");

                foreach (var rpwg in UpdatedResultPromptWordGroups)
                {
                    sbSummaryReport.AppendLine(string.Format("Word: {0}", rpwg.DraftResultPromptWordGroup.ResultPromptWord));
                    sbDetailReport.AppendLine(string.Format("Word: {0}", rpwg.DraftResultPromptWordGroup.ResultPromptWord));

                    sbDetailReport.AppendLine(rpwg.ChangeReport);

                    if (rpwg.ExistingPublishedResultPromptWordGroup != null)
                    {
                        sbDetailReport.AppendLine("The update will be applied to the following prompts...");
                        foreach (var item in rpwg.ExistingPublishedResultPromptWordGroup.ParentPrompts.Where(
                            x => x.PublishedStatus == PublishedStatus.Published ||
                                x.PublishedStatus == PublishedStatus.PublishedPending).OrderBy(x => x.Label))
                        {
                            sbDetailReport.AppendLine(item.Label);
                        }
                    }
                }
            }
        }

        private void ReportFixedListChanges(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (UpdatedFixedLists != null && UpdatedFixedLists.Any())
            {
                sbSummaryReport.AppendLine("The following fixed lists have been changed...");
                sbDetailReport.AppendLine("The following fixed lists have been changed...");

                foreach (var fl in UpdatedFixedLists)
                {
                    sbSummaryReport.AppendLine(string.Format("{0}", fl.DraftFixedList.Label));
                    sbDetailReport.AppendLine(string.Format("{0}", fl.DraftFixedList.Label));

                    sbDetailReport.AppendLine(fl.ChangeReport);

                    sbDetailReport.AppendLine("The update will be applied to the following results and prompts...");

                    foreach (var rule in fl.PublishedParentResultPromptRules.OrderBy(x => x.ResultDefinitionViewModel.Label))
                    {
                        sbDetailReport.AppendLine(string.Format("{0} - {1}", rule.ResultDefinitionViewModel.Label, rule.ResultPromptViewModel.Label));
                    }
                }
            }
        }

        private void ReportWordGroupChanges(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (UpdatedResultDefinitionWordGroups != null && UpdatedResultDefinitionWordGroups.Any())
            {
                sbSummaryReport.AppendLine("The following result word synonms have been changed...");
                sbDetailReport.AppendLine("The following result word synonms have been changed...");

                foreach (var rdwg in UpdatedResultDefinitionWordGroups)
                {
                    sbSummaryReport.AppendLine(string.Format("Word: {0}", rdwg.DraftResultDefinitionWordGroupViewModel.ResultDefinitionWord));
                    sbDetailReport.AppendLine(string.Format("Word: {0}", rdwg.DraftResultDefinitionWordGroupViewModel.ResultDefinitionWord));

                    sbDetailReport.AppendLine(rdwg.ChangeReport);

                    sbDetailReport.AppendLine("The update will be applied to the following results...");
                    foreach (var item in rdwg.ParentResults.Where(
                        x => x.PublishedStatus == PublishedStatus.Published ||
                            x.PublishedStatus == PublishedStatus.PublishedPending).OrderBy(x => x.Label))
                    {
                        sbDetailReport.AppendLine(item.Label);
                    }
                }
            }
        }

        private void ReportParentResultUsages(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (AdditionalParentResultUsages != null && AdditionalParentResultUsages.Where(x => !x.ParentUsage.IsChanged).Count() > 0)
            {
                sbSummaryReport.AppendLine("The following additional result changes are published because they are parents of the changed results...");
                sbDetailReport.AppendLine("The following additional result changes are published because they are parents of the changed results...");
                
                foreach (var usageGroup in AdditionalParentResultUsages.GroupBy(x => x.ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID))
                {
                    var usages = usageGroup.ToList();
                    sbSummaryReport.AppendLine(string.Format("Shared Changed Child Result: '{0}'", usages[0].ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                    sbDetailReport.AppendLine(string.Format("Shared Changed Child Result: '{0}'", usages[0].ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));

                    var parentListNewDrafts = new List<string>();
                    usages.Where(x=>!x.ParentUsage.IsInDraftAlready).OrderBy(x => x.ParentUsage.Result.Label).ToList().ForEach(x => parentListNewDrafts.Add(x.ParentUsage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                    var parentListAlreadyInDraft = new List<string>();
                    usages.Where(x => x.ParentUsage.IsInDraftAlready).OrderBy(x => x.ParentUsage.Result.Label).ToList().ForEach(x => parentListAlreadyInDraft.Add(x.ParentUsage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));

                    if (parentListNewDrafts.Any())
                    {
                        sbSummaryReport.AppendLine(string.Format("Additional Published Parent Results: {0}", String.Join(", ", parentListNewDrafts)));
                        sbDetailReport.AppendLine(string.Format("Additional Published Parent Results: {0}", String.Join(", ", parentListNewDrafts)));
                    }

                    if (parentListAlreadyInDraft.Any())
                    {
                        sbSummaryReport.AppendLine(string.Format("Additional Published Parent Results (In Draft): {0}", String.Join(", ", parentListAlreadyInDraft)));
                        sbDetailReport.AppendLine(string.Format("Additional Published Parent Results (In Draft): {0}", String.Join(", ", parentListAlreadyInDraft)));
                    }

                    sbSummaryReport.AppendLine();
                    sbDetailReport.AppendLine();

                    foreach (var usage in usages.OrderBy(x => x.ParentUsage.Result.Label))
                    {
                        SetChangedResultReport(usage.ParentUsage, sbDetailReport, sbSummaryReport, false);
                    }

                    sbSummaryReport.AppendLine();
                    sbDetailReport.AppendLine();
                }
            }
        }

        private void ReportWordGroupUsages(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (AdditionalResultDefinitionWordGroupUsages != null && AdditionalResultDefinitionWordGroupUsages.Any())
            {
                sbSummaryReport.AppendLine("The following additional result changes are published because they are parents of changed Result Definition Word Synonyms...");
                sbDetailReport.AppendLine("The following additional result changes are published because they are parents of changed Result Definition Word Synonyms...");

                foreach (var usageGroup in AdditionalResultDefinitionWordGroupUsages.GroupBy(x => x.Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID))
                {
                    var usages = usageGroup.ToList();

                    if (usages[0].Usage.IsInDraftAlready)
                    {
                        sbDetailReport.AppendLine(string.Format("Result Definition (In Draft): '{0}'", usages[0].Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        sbSummaryReport.AppendLine(string.Format("Result Definition (In Draft): '{0}'", usages[0].Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                    }
                    else
                    {
                        sbDetailReport.AppendLine(string.Format("Result Definition: '{0}'", usages[0].Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        sbSummaryReport.AppendLine(string.Format("Result Definition: '{0}'", usages[0].Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                    }
                    foreach (var usage in usages.OrderBy(x => x.ChangedResultDefinitionWordGroup.DraftResultDefinitionWordGroupViewModel.ResultDefinitionWord))
                    {
                        sbDetailReport.AppendLine(string.Format("Changed Result Word Synonyms: '{0}'", usage.ChangedResultDefinitionWordGroup.DraftResultDefinitionWordGroupViewModel.ResultDefinitionWord));
                        sbSummaryReport.AppendLine(string.Format("Changed Result Word Synonyms: '{0}'", usage.ChangedResultDefinitionWordGroup.DraftResultDefinitionWordGroupViewModel.ResultDefinitionWord));
                        if (!string.IsNullOrEmpty(usage.ChangedResultDefinitionWordGroup.ChangeReport))
                        {
                            sbDetailReport.AppendLine(usage.ChangedResultDefinitionWordGroup.ChangeReport);
                        }
                    }

                    sbSummaryReport.AppendLine();
                    sbDetailReport.AppendLine();
                }
            }
        }

        private void ReportPromptUsages(StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (AdditionalPromptUsages != null && AdditionalPromptUsages.Where(x => x.Usage.IsChanged).Count() > 0)
            {
                var changedPromptUsages = AdditionalPromptUsages.Where(x => x.Usage.IsChanged).ToList();
                sbSummaryReport.AppendLine();
                sbDetailReport.AppendLine();
                sbSummaryReport.AppendLine("The following additional result changes are published because they have usage of the changed result prompts...");
                sbDetailReport.AppendLine("The following additional result changes are published because they have usage of the changed result prompts...");

                foreach (var usageGroup in changedPromptUsages.GroupBy(x => x.DraftPrompt.Prompt.UUID))
                {
                    var usages = usageGroup.ToList();
                    sbSummaryReport.AppendLine(string.Format("Shared Result Prompt: '{0}'", usages[0].DraftPrompt.Prompt.Label));
                    sbDetailReport.AppendLine(string.Format("Shared Result Prompt: '{0}'", usages[0].DraftPrompt.Prompt.Label));
                    foreach (var usage in usages.OrderBy(x => x.Usage.Result.Label))
                    {
                        SetChangedResultReport(usage.Usage, sbDetailReport, sbSummaryReport, false);
                    }
                    sbSummaryReport.AppendLine();
                }
            }
        }

        internal void PrepareForPublication()
        {
            var rdCmd = treeModel.DraftResultDefinitionCommand as DraftResultDefinitionCommand;
            var rpCmd = treeModel.DraftResultPromptCommand as DraftResultPromptCommand;

            //deal with any draft results that have changes pending save
            if (DraftResultsSavePending != null)
            {
                foreach (var draft in DraftResultsSavePending)
                {
                    rdCmd.SaveDraft(draft.Result, false);
                }
            }

            //apply result definition word group usages to any draft rules
            ApplyUsagesToDraftChildRules();

            //deal with any result prompt word group changes that have word changes by creating additional draft results for each of the parents
            if (ResultPromptWordGroupsWithWordChanges != null)
            {                
                foreach (var rpwg in ResultPromptWordGroupsWithWordChanges)
                {
                    MakeAdditionalResultUsagesForChangedResultPromptWordGroup(rpwg, rdCmd, rpCmd);
                    rpwg.PrepareForPublication();
                }
            }

            //prepare the updated result prompt word groups
            if (UpdatedResultPromptWordGroups != null)
            {
                foreach (var rpwg in UpdatedResultPromptWordGroups)
                {
                    rpwg.PrepareForPublication();
                }
            }

            if (ChangedResults != null)
            {
                foreach (var draft in ChangedResults)
                {
                    //Prepare the draft result for publication
                    draft.PrepareForPublication();
                }
            }

            //prepare the updated result definition word groups
            if (UpdatedResultDefinitionWordGroups != null)
            {
                foreach (var rdwg in UpdatedResultDefinitionWordGroups)
                {
                    rdwg.PrepareForPublication();
                }
            }

            //prepare the updated result prompt fixed lists
            if (UpdatedFixedLists != null)
            {
                foreach (var fl in UpdatedFixedLists)
                {
                    fl.PrepareForPublication();
                }
            }

            if (AllDraftNows != null)
            {
                foreach (var now in AllDraftNows)
                {
                    now.PrepareForPublication();
                }
            }

            if (AllDraftNowSubscriptions != null)
            {
                foreach (var sub in AllDraftNowSubscriptions)
                {
                    sub.PrepareForPublication();
                }
            }
        }

        private void MakeAdditionalResultUsagesForChangedResultPromptWordGroup(DraftResultPromptWordGroupModel rpwg, DraftResultDefinitionCommand rdCmd, DraftResultPromptCommand rpCmd)
        {
            dynamic parentUsages = rpwg.ParentResultPrompts;

            if (parentUsages.Count > 0)
            {
                var allPrompts = AllDraftPrompts.Select(x => x.Prompt).ToList();
                
                foreach (dynamic item in parentUsages)
                {
                    //check if draft Result is already being published
                    var draftResult = ChangedResults.FirstOrDefault(
                        x => x.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID ==
                             item.PublishedResultDefinitionTree.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
                    if (draftResult == null)
                    {
                        //split the draft and save operations for performance reasons
                        var draft = rdCmd.DraftResultDefinitionTreeViewModel(item.PublishedResultDefinitionTree, rpwg.DraftResultPromptWordGroup.PublicationTags, false);
                        rdCmd.SaveDraft(draft, false);
                        draftResult = new DraftResultModel(treeModel, draft, false);

                        //add the new draft to the set of changed results
                        AdditionalResultPromptWordGroupUsages.Add(draftResult);
                    }

                    //check if prompt is already being published
                    var draftPrompt = allPrompts.FirstOrDefault(x => x.MasterUUID == item.PublishedResultPromptViewModel.MasterUUID);
                    if (draftPrompt == null)
                    {
                        //create a new draft prompt
                        var rpr = draftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.First(
                            x => x.ResultPromptViewModel.MasterUUID == item.PublishedResultPromptViewModel.MasterUUID);

                        if (rpr.ResultPromptViewModel.PublishedStatus != PublishedStatus.Draft)
                        {
                            //will set the prompt any of the draft results and will also associate to the draft rpwg
                            rpCmd.DraftResultPromptViewModel(rpr.ResultPromptViewModel);

                            //add to the collection of all prompts
                            allPrompts.Add(rpr.ResultPromptViewModel);

                            //set the parentage on the draft result prompt word group
                            var matched = rpwg.DraftResultPromptWordGroup.ParentPrompts.FirstOrDefault(x => x.UUID == rpr.ResultPromptViewModel.UUID);
                            if (matched == null)
                            {
                                rpwg.DraftResultPromptWordGroup.ParentPrompts.Add(rpr.ResultPromptViewModel);
                            }
                        }
                    }                    
                }

                //set the changes on the new draft results
                AdditionalResultPromptWordGroupUsages.ForEach(x => x.SetChanges(AllDraftPrompts, AllDraftNows, AllDraftNowSubscriptions));
            }
        }
        private void ApplyUsagesToDraftChildRules()
        {
            var draftChildRules = treeModel.AllResultRuleViewModel.Rules.Where(x => x.PublishedStatus == PublishedStatus.Draft);

            if (AdditionalResultDefinitionWordGroupUsages != null)
            {
                foreach (var item in AdditionalResultDefinitionWordGroupUsages)
                {
                    foreach (var draftRule in draftChildRules.Where
                        (
                            x => x.ChildResultDefinitionViewModel.MasterUUID == item.Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID &&
                                x.ChildResultDefinitionViewModel.UUID != item.Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                        ))
                    {
                        draftRule.ChildResultDefinitionViewModel = item.Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel;
                    }
                }
            }

            if (AdditionalPromptUsages != null)
            {
                foreach (var item in AdditionalPromptUsages)
                {
                    foreach (var draftRule in draftChildRules.Where
                        (
                            x => x.ChildResultDefinitionViewModel.MasterUUID == item.Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID &&
                                x.ChildResultDefinitionViewModel.UUID != item.Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                        ))
                    {
                        draftRule.ChildResultDefinitionViewModel = item.Usage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel;
                    }
                }
            }

            if (AdditionalParentResultUsages != null)
            {
                foreach (var item in AdditionalParentResultUsages)
                {
                    foreach (var draftRule in draftChildRules.Where
                        (
                            x => x.ChildResultDefinitionViewModel.MasterUUID == item.ParentUsage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID &&
                                x.ChildResultDefinitionViewModel.UUID != item.ParentUsage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                        ))
                    {
                        draftRule.ChildResultDefinitionViewModel = item.ParentUsage.Result.ResultRuleViewModel.ChildResultDefinitionViewModel;
                    }
                }
            }
        }
        private void SetChangedResultReport(DraftResultModel dr, StringBuilder sbDetailReport, StringBuilder sbSummaryReport, bool isSelectedResult)
        {
            if (isSelectedResult)
            {
                if (dr.IsDeleted)
                {
                    sbDetailReport.AppendLine(string.Format("Deleted Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                    sbSummaryReport.AppendLine(string.Format("Deleted Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                }
                else
                {
                    if (dr.IsResultAdded)
                    {
                        sbDetailReport.AppendLine(string.Format("Selected New Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        sbSummaryReport.AppendLine(string.Format("Selected New Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                    }
                    else
                    {
                        sbDetailReport.AppendLine(string.Format("Selected Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        sbSummaryReport.AppendLine(string.Format("Selected Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                    }                    
                }                
            }
            else
            {
                if (dr.IsChanged)
                {
                    if (dr.IsInDraftAlready)
                    {
                        if (dr.IsResultAdded)
                        {
                            sbDetailReport.AppendLine(string.Format("New Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                            sbSummaryReport.AppendLine(string.Format("New Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        }
                        else
                        {
                            sbDetailReport.AppendLine(string.Format("Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                            sbSummaryReport.AppendLine(string.Format("Result Definition (In Draft): '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        }
                    }
                    else
                    {
                        if (dr.IsResultAdded)
                        {
                            sbDetailReport.AppendLine(string.Format("New Result Definition '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                            sbSummaryReport.AppendLine(string.Format("New Result Definition '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        }
                        else
                        {
                            sbDetailReport.AppendLine(string.Format("Result Definition '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                            sbSummaryReport.AppendLine(string.Format("Result Definition '{0}'...", dr.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));
                        }
                    }
                }                
            }

            if (dr.IsResultChanged && !dr.IsResultAdded && !string.IsNullOrEmpty(dr.ChangeReport))
            {
                sbDetailReport.AppendLine(dr.ChangeReport);
            }

            SetChangedResultChildrenReport(dr, sbDetailReport, sbSummaryReport);
        }
        private void SetChangedResultChildrenReport(DraftResultModel dr, StringBuilder sbDetailReport, StringBuilder sbSummaryReport)
        {
            if (dr.IsPromptRulesChanged && dr.ChangedPromptRules != null)
            {
                foreach (var rpr in dr.ChangedPromptRules.Where(x => x.IsChanged))
                {
                    if (rpr.IsRuleChanged || rpr.IsRuleAdded)
                    {
                        if (rpr.IsRuleChanged)
                        {
                            sbDetailReport.AppendLine(string.Format("Changed Child Result Prompt Rule: '{0}'...", rpr.Rule.ResultPromptViewModel.Label));
                            sbSummaryReport.AppendLine(string.Format("Changed Child Result Prompt Rule: '{0}'", rpr.Rule.ResultPromptViewModel.Label));
                        }
                        if (rpr.IsRuleAdded && !rpr.IsPromptAdded)
                        {
                            sbDetailReport.AppendLine(string.Format("New Child Result Prompt Rule: '{0}'...", rpr.Rule.ResultPromptViewModel.Label));
                            sbSummaryReport.AppendLine(string.Format("New Child Result Prompt Rule: '{0}'", rpr.Rule.ResultPromptViewModel.Label));
                        }
                        if (!string.IsNullOrEmpty(rpr.ChangeReport) && !rpr.IsPromptAdded)
                        {
                            sbDetailReport.AppendLine(rpr.ChangeReport);
                        }
                    }

                    if (rpr.IsPromptAdded)
                    {
                        sbDetailReport.AppendLine(string.Format("New Child Result Prompt: '{0}'", rpr.Rule.ResultPromptViewModel.Label));
                        sbSummaryReport.AppendLine(string.Format("New Child Result Prompt: '{0}'", rpr.Rule.ResultPromptViewModel.Label));
                    }
                    else
                    {
                        if (rpr.IsPromptChanged)
                        {
                            sbDetailReport.AppendLine(string.Format("Child Result Prompt: '{0}'", rpr.Rule.ResultPromptViewModel.Label));
                            sbSummaryReport.AppendLine(string.Format("Child Result Prompt: '{0}'", rpr.Rule.ResultPromptViewModel.Label));
                        }
                    }                    
                }
            }

            if (dr.IsResultRulesChanged && dr.ChangedResultRules != null)
            {
                foreach (var rdr in dr.ChangedResultRules.Where(x => x.IsChanged))
                {
                    if (rdr.IsRuleChanged)
                    {
                        sbDetailReport.AppendLine(string.Format("Changed Child Result Rule: '{0}'", rdr.Rule.ChildResultDefinitionViewModel.Label));
                        sbSummaryReport.AppendLine(string.Format("Changed Child Result Rule: '{0}'", rdr.Rule.ChildResultDefinitionViewModel.Label));
                    }
                    if (rdr.IsRuleAdded)
                    {
                        sbDetailReport.AppendLine(string.Format("New Child Result Rule: '{0}'", rdr.Rule.ChildResultDefinitionViewModel.Label));
                        sbSummaryReport.AppendLine(string.Format("New Child Result Rule: '{0}'", rdr.Rule.ChildResultDefinitionViewModel.Label));
                    }
                    if (!string.IsNullOrEmpty(rdr.ChangeReport))
                    {
                        sbDetailReport.AppendLine(rdr.ChangeReport);
                    }
                }
            }

            if (dr.IsResultDefinitionWordGroupsChanged)
            {
                foreach (var rdwg in dr.ChangedResultDefinitionWordGroups)
                {
                    sbDetailReport.AppendLine(string.Format("Result Definition Word Synonyms: '{0}'", rdwg.DraftResultDefinitionWordGroupViewModel.ResultDefinitionWord));
                    sbSummaryReport.AppendLine(string.Format("Result Definition Word Synonyms: '{0}'", rdwg.DraftResultDefinitionWordGroupViewModel.ResultDefinitionWord));
                    if (!string.IsNullOrEmpty(rdwg.ChangeReport))
                    {
                        sbDetailReport.AppendLine(rdwg.ChangeReport);
                    }
                }
            }

            if (dr.IsPromptRulesDeleted)
            {
                foreach (var rpr in dr.DeletedPromptRules.OrderBy(x=>x.ResultPromptViewModel.Label))
                {
                    sbDetailReport.AppendLine(string.Format("Deleted Child Result Prompt Rule: '{0}'", rpr.ResultPromptViewModel.Label));
                    sbSummaryReport.AppendLine(string.Format("Deleted Child Result Prompt Rule: '{0}'", rpr.ResultPromptViewModel.Label));
                }
            }

            if (dr.IsResultRulesDeleted)
            {
                foreach (var rdr in dr.DeletedResultRules.Where(x=>x.ParentResultDefinitionViewModel == null).OrderBy(x => x.ChildResultDefinitionViewModel.Label))
                {
                    sbDetailReport.AppendLine(string.Format("Deleted Root Result Rule: '{0}'", rdr.ChildResultDefinitionViewModel.Label));
                    sbSummaryReport.AppendLine(string.Format("Deleted Root Result Rule: '{0}'", rdr.ChildResultDefinitionViewModel.Label));
                }

                foreach (var rdr in dr.DeletedResultRules.Where(x => x.ParentResultDefinitionViewModel != null).OrderBy(x=>x.ChildResultDefinitionViewModel.Label))
                {
                    sbDetailReport.AppendLine(string.Format("Deleted Child Result Rule: '{0}'", rdr.ChildResultDefinitionViewModel.Label));
                    sbSummaryReport.AppendLine(string.Format("Deleted Child Result Rule: '{0}'", rdr.ChildResultDefinitionViewModel.Label));
                }
            }
        }

        public void SetChanges()
        {
            if (SelectedDraftResult != null)
            {
                SelectedDraftResult.SetChanges(AllDraftPrompts, AllDraftNows, AllDraftNowSubscriptions);
            }
            //determine if child results have changed
            foreach (var dr in ChildDraftResults)
            {
                dr.SetChanges(AllDraftPrompts, AllDraftNows, AllDraftNowSubscriptions);
            }

            //determine if child results from prompt usages have changed
            foreach (var dr in AdditionalPromptUsages.Select(x => x.Usage))
            {
                dr.SetChanges(AllDraftPrompts, AllDraftNows, AllDraftNowSubscriptions);
            }

            //determine if parent results from result usages have changed
            foreach (var dr in AdditionalParentResultUsages.Select(x => x.ParentUsage))
            {
                dr.SetChanges(AllDraftPrompts, AllDraftNows, AllDraftNowSubscriptions);
            }
        }
    }
}
