using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml;
using NowsRequestDataLib.Domain;
using System;
using System.Linq;
using System.Text;

namespace ExportExcel
{
    public class ReportHelper
    {
        public AllData AllData { get; }
        public ReportHelper(AllData allData)
        {
            AllData = allData;
        }

        public string PromptUsagesReport(Guid? resultPromptId)
        {
            var res = PromptUsagesWithFlagReport(resultPromptId);
            return res.Item2;
        }

        public Tuple<bool, string> PromptUsagesWithFlagReport(Guid? resultPromptId)
        {
            bool hasUsages = false;
            var parentRDs = AllData.ResultPromptRules.Where(x => x.ResultPromptUUID == resultPromptId && x.DeletedDate == null).Select(x => x.ResultDefinitionUUID).Distinct().ToList();
            var sb = new StringBuilder();

            var usages = from rd in AllData.ResultDefinitions
                         where (GetPublishedStatus(rd) == PublishedStatus.Draft || GetPublishedStatus(rd) == PublishedStatus.Published || GetPublishedStatus(rd) == PublishedStatus.PublishedPending)
                         join id in parentRDs on rd.UUID equals id
                         orderby rd.Label ascending
                         select new { rd = rd };

            sb.AppendLine("Parent Result Definitions ...");

            if (usages.Count() > 1) { hasUsages = true; }

            foreach (var item in usages)
            {
                sb.AppendLine(string.Format("{0} ({1})", item.rd.Label, GetPublishedStatus(item.rd).GetDescription()));
            }

            //nows
            var nowEDTUsages = from nr in AllData.NowRequirements
                            join parentRD in parentRDs on nr.ResultDefinition.UUID equals parentRD
                            join now in AllData.Nows on nr.NOWUUID equals now.UUID
                            where nr.NowRequirementPromptRules != null 
                                  && nr.NowRequirementPromptRules.Where(x => x.IsVariantData || x.DistinctPromptTypes).Select(x => x.ResultPromptRule.ResultPrompt.UUID).ToList().Contains(resultPromptId)
                                  && (GetPublishedStatus(now) == PublishedStatus.Draft || GetPublishedStatus(now) == PublishedStatus.Published || GetPublishedStatus(now) == PublishedStatus.PublishedPending)
                               orderby now.Name ascending
                            select new { now, nr.ResultDefinition.Label, promptRule = nr.NowRequirementPromptRules.First(x => x.ResultPromptRule.ResultPrompt.UUID == resultPromptId) };

            if (nowEDTUsages.Count() > 0) { hasUsages = true; }

            var edtUsages = nowEDTUsages.Where(x => x.now.IsEDT).ToList();
            var nowUsages = nowEDTUsages.Where(x => !x.now.IsEDT).ToList();

            if (nowUsages.Count() > 0)
            {
                sb.AppendLine();
                sb.AppendLine("NOWs ...");
                sb.AppendLine("Usage - MDE");
                foreach (var grp in nowUsages.Where(x => x.promptRule.IsVariantData).GroupBy(x => new { x.now.Name, x.Label, x.promptRule.IsVariantData, x.promptRule.DistinctPromptTypes }))
                {
                    var item = grp.First();
                    sb.AppendLine(string.Format("{0} ({1}) - {2}", item.now.Name, GetPublishedStatus(item.now).GetDescription(), item.Label));
                }
                sb.AppendLine();
                sb.AppendLine("Usage - Distinct Prompt Type");
                foreach (var grp in nowUsages.Where(x => x.promptRule.DistinctPromptTypes).GroupBy(x => new { x.now.Name, x.Label, x.promptRule.IsVariantData, x.promptRule.DistinctPromptTypes }))
                {
                    var item = grp.First();
                    sb.AppendLine(string.Format("{0} ({1}) - {2}", item.now.Name, GetPublishedStatus(item.now).GetDescription(), item.Label));
                }
            }

            if (edtUsages.Count() > 0)
            {
                sb.AppendLine();
                sb.AppendLine("EDTs ...");
                sb.AppendLine("Usage - MDE");
                foreach (var grp in edtUsages.Where(x => x.promptRule.IsVariantData).GroupBy(x => new { x.now.Name, x.Label, x.promptRule.IsVariantData, x.promptRule.DistinctPromptTypes }))
                {
                    var item = grp.First();
                    sb.AppendLine(string.Format("{0} ({1}) - {2}", item.now.Name, GetPublishedStatus(item.now).GetDescription(), item.Label));
                }
                sb.AppendLine();
                sb.AppendLine("Usage - Distinct Prompt Type");
                foreach (var grp in edtUsages.Where(x => x.promptRule.DistinctPromptTypes).GroupBy(x => new { x.now.Name, x.Label, x.promptRule.IsVariantData, x.promptRule.DistinctPromptTypes }))
                {
                    var item = grp.First();
                    sb.AppendLine(string.Format("{0} ({1}) - {2}", item.now.Name, GetPublishedStatus(item.now).GetDescription(), item.Label));
                }
            }

            //Subscriptions
            var subsIncludedResults = AllData.NowSubscriptions.Where(x => x.DeletedDate == null
                                                                        && x.SubscriptionVocabulary != null
                                                                        && (GetPublishedStatus(x) == PublishedStatus.Draft || GetPublishedStatus(x) == PublishedStatus.Published || GetPublishedStatus(x) == PublishedStatus.PublishedPending)
                                                                        &&
                                                                            (
                                                                                x.SubscriptionVocabulary.IncludedPromptRules != null
                                                                                && x.SubscriptionVocabulary.IncludedPromptRules.Where(y => y.DeletedDate == null).Any()
                                                                                && x.SubscriptionVocabulary.IncludedPromptRules.Where(y => y.DeletedDate == null)
                                                                                        .Select(z=> z.ResultPrompt.UUID).Contains(resultPromptId)
                                                                            )
                                                                        );

            if (subsIncludedResults.Count() > 0) { hasUsages = true; }

            var subsExcludedResults = AllData.NowSubscriptions.Where(x => x.DeletedDate == null
                                                                        && x.SubscriptionVocabulary != null
                                                                        && (GetPublishedStatus(x) == PublishedStatus.Draft || GetPublishedStatus(x) == PublishedStatus.Published || GetPublishedStatus(x) == PublishedStatus.PublishedPending)
                                                                        &&
                                                                            (
                                                                                x.SubscriptionVocabulary.ExcludedPromptRules != null
                                                                                && x.SubscriptionVocabulary.ExcludedPromptRules.Where(y => y.DeletedDate == null).Any()
                                                                                && x.SubscriptionVocabulary.ExcludedPromptRules.Where(y => y.DeletedDate == null)
                                                                                        .Select(z => z.ResultPrompt.UUID).Contains(resultPromptId)
                                                                            )
                                                                        );
            if (subsExcludedResults.Count() > 0) { hasUsages = true; }

            sb.AppendLine();
            sb.AppendLine("Subscription Rules ...");
            sb.AppendLine("Included Prompts ...");
            foreach (var item in subsIncludedResults)
            {
                sb.AppendLine(string.Format("{0} ({1})", item.Name, GetPublishedStatus(item).GetDescription()));
            }

            sb.AppendLine();
            sb.AppendLine("Excluded Prompts ...");
            foreach (var item in subsExcludedResults)
            {
                sb.AppendLine(item.Name);
            }

            var subsRecipient = AllData.NowSubscriptions.Where(x => x.DeletedDate == null
                                                                        && (GetPublishedStatus(x) == PublishedStatus.Draft || GetPublishedStatus(x) == PublishedStatus.Published || GetPublishedStatus(x) == PublishedStatus.PublishedPending)
                                                                        && x.NowRecipient != null && x.NowRecipient.RecipientFromResults
                                                                        && x.NowRecipient.IsPromptSetForNameAddress(resultPromptId)
                                                                        );

            if (subsRecipient.Count() > 0) { hasUsages = true; }
            sb.AppendLine();
            sb.AppendLine("Recipient Addressee Details...");
            foreach (var item in subsRecipient)
            {
                sb.AppendLine(string.Format("{0} ({1})", item.Name, GetPublishedStatus(item).GetDescription()));
            }

            return new Tuple<bool, string>(hasUsages, sb.ToString());
        }
        public string ResultUsagesReport(Guid? resultDefinitionId)
        {
            var parentRDs = AllData.ResultDefinitionRules.Where(x => x.ChildResultDefinitionUUID == resultDefinitionId 
            && x.DeletedDate == null 
            && x.ResultDefinition.DeletedDate == null            
            ).Select(x => x.ParentUUID).Distinct().ToList();
            var sb = new StringBuilder();

            var parents = from rd in AllData.ResultDefinitions
                          where (GetPublishedStatus(rd) == PublishedStatus.Draft || GetPublishedStatus(rd) == PublishedStatus.Published || GetPublishedStatus(rd) == PublishedStatus.PublishedPending)
                          join id in parentRDs on rd.UUID equals id
                          orderby rd.Label ascending
                          select new { rd = rd };

            sb.AppendLine("Parent Result Definitions ...");
            foreach (var item in parents)
            {
                sb.AppendLine(string.Format("{0} ({1})", item.rd.Label, GetPublishedStatus(item.rd).GetDescription()));
            }
            sb.AppendLine();
            var nonParentageReport = ResultUsagesWithoutParentageReport(resultDefinitionId);
            sb.AppendLine(nonParentageReport.Item2);

            if (parents.Count() > 0 || nonParentageReport.Item1)
            {
                return sb.ToString();
            }
            return string.Empty;
        }
        public Tuple<bool, string> ResultUsagesWithoutParentageReport(Guid? resultDefinitionId)
        {
            var hasUsages = false;
            var sb = new StringBuilder();
            var nrs = AllData.NowRequirements
                            .Where(x => x.ResultDefinition.UUID == resultDefinitionId && x.DeletedDate == null)
                            .Select(usage => new { nowid = usage.NOWUUID, isPrimary = usage.PrimaryResult }).Distinct().ToList();

            var nows = AllData.Nows.Where(x => x.DeletedDate == null && !x.IsEDT);

            var nowusages = from now in nows
                            where (GetPublishedStatus(now) == PublishedStatus.Draft || GetPublishedStatus(now) == PublishedStatus.Published || GetPublishedStatus(now) == PublishedStatus.PublishedPending)
                            join nr in nrs on now.UUID equals nr.nowid
                            orderby now.Name ascending
                            select new { now = now, nr.isPrimary };

            if (nowusages.Count() > 0) { hasUsages = true; }

            sb.AppendLine("Notice Orders & Warrants ...");

            foreach (var item in nowusages)
            {
                if (item.isPrimary)
                {
                    sb.AppendLine(string.Format("{0} (primary) - {1}", item.now.Name, GetPublishedStatus(item.now).GetDescription()));
                }
                else
                {
                    sb.AppendLine(string.Format("{0} ({1})", item.now.Name, GetPublishedStatus(item.now).GetDescription()));
                }
            }

            var edts = AllData.Nows.Where(x => x.DeletedDate == null && x.IsEDT);
            var edtusages = from now in edts
                            where (GetPublishedStatus(now) == PublishedStatus.Draft || GetPublishedStatus(now) == PublishedStatus.Published || GetPublishedStatus(now) == PublishedStatus.PublishedPending)
                            join nr in nrs on now.UUID equals nr.nowid
                            orderby now.Name ascending
                            select new { now = now, nr.isPrimary };

            if (edtusages.Count() > 0) { hasUsages = true; }

            sb.AppendLine();
            sb.AppendLine("EDTs ...");

            foreach (var item in edtusages)
            {
                if (item.isPrimary)
                {
                    sb.AppendLine(string.Format("{0} (primary) - {1}", item.now.Name, GetPublishedStatus(item.now).GetDescription()));
                }
                else
                {
                    sb.AppendLine(string.Format("{0} ({1})", item.now.Name, GetPublishedStatus(item.now).GetDescription()));
                }
            }

            var subsIncludedResults = AllData.NowSubscriptions.Where(x => x.DeletedDate == null
                                                                        && x.SubscriptionVocabulary != null
                                                                        && (GetPublishedStatus(x) == PublishedStatus.Draft || GetPublishedStatus(x) == PublishedStatus.Published || GetPublishedStatus(x) == PublishedStatus.PublishedPending)
                                                                        &&
                                                                            (
                                                                                x.SubscriptionVocabulary.IncludedResults != null && x.SubscriptionVocabulary.IncludedResults.Select(y => y.UUID).Contains(resultDefinitionId)
                                                                            )
                                                                        );

            if (subsIncludedResults.Count() > 0) { hasUsages = true; }

            var subsExcludedResults = AllData.NowSubscriptions.Where(x => x.DeletedDate == null
                                                                        && x.SubscriptionVocabulary != null
                                                                        && (GetPublishedStatus(x) == PublishedStatus.Draft || GetPublishedStatus(x) == PublishedStatus.Published || GetPublishedStatus(x) == PublishedStatus.PublishedPending)
                                                                        &&
                                                                            (
                                                                                x.SubscriptionVocabulary.ExcludedResults != null && x.SubscriptionVocabulary.ExcludedResults.Select(y => y.UUID).Contains(resultDefinitionId)
                                                                            )
                                                                        );
            if (subsExcludedResults.Count() > 0) { hasUsages = true; }


            sb.AppendLine();
            sb.AppendLine("Subscription Rules ...");
            sb.AppendLine("Included Results ...");
            foreach (var item in subsIncludedResults)
            {
                sb.AppendLine(string.Format("{0} ({1})", item.Name, GetPublishedStatus(item).GetDescription()));
            }

            sb.AppendLine();
            sb.AppendLine("Excluded Results ...");
            foreach (var item in subsExcludedResults)
            {
                sb.AppendLine(string.Format("{0} ({1})", item.Name, GetPublishedStatus(item).GetDescription()));
            }

            return new Tuple<bool, string>(hasUsages, sb.ToString());
        }
        public string NowUsagesReport(Guid? nowId)
        {
            var now = AllData.Nows.FirstOrDefault(x => x.UUID == nowId);
            var subscribers = HearingResults.GetSubscribers(now, AllData.NowSubscriptions.Where(
                x => x.DeletedDate == null
                && (GetPublishedStatus(x) == PublishedStatus.Draft || GetPublishedStatus(x) == PublishedStatus.Published || GetPublishedStatus(x) == PublishedStatus.PublishedPending)
                ).ToList(), null, null);
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("{0} Subscribers ...", now.IsEDT ? "EDT" : "NOW"));

            foreach (var item in subscribers)
            {
                sb.AppendLine(string.Format("{0} ({1})", item.Name, GetPublishedStatus(item).GetDescription()));
            }
            return sb.ToString();
        }

        public PublishedStatus GetPublishedStatus(DataAudit data)
        {
            if (data != null && data.PublishedStatus != null) { return data.PublishedStatus.Value; }

            return PublishedStatus.Published;
        }
    }
}
