using ExportExcel;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using DataLib;
using Configuration;

namespace Models.Commands
{
    public interface IGenerateReportCommand : ICommand { }

    public class GenerateReportCommand : IGenerateReportCommand
    {
        private ITreeModel treeModel;
        private IAppSettings appSettings;
        private ReportHelper reportHelper;        
        public GenerateReportCommand(ITreeModel treeModel, IAppSettings appSettings)
        {
            this.treeModel = treeModel;
            this.appSettings = appSettings;
            reportHelper = new ReportHelper(treeModel.AllData);
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
            var reportVM = parameter as ReportViewModel;
            if (reportVM == null) { return; }

            //determine the target directory
            var directoryPath = Path.Combine(appSettings.DataFileDirectory, "Reports");
            Directory.CreateDirectory(directoryPath);
            var filePath = Path.Combine(directoryPath, reportVM.SelectedReport.FileName);

            //Generate the report
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                using (var writer = new StreamWriter(fs))
                {
                    GenerateReport(reportVM.SelectedReport.ReportShortName, fs, writer);
                }
            }
        }

        private void GenerateReport(string reportShortName, Stream stream, StreamWriter writer)
        {
            switch (reportShortName.ToLowerInvariant())
            {
                case "fullextract":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateFullExcelExport(stream);
                        break;
                    }
                case "synonymextract":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateSynonymExcelExport(stream);
                        break;
                    }
                case "judicialvalidationreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateJudicialValidationExcelExport(stream);
                        break;
                    }
                case "missingsharingrulesreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateMissingSharingRulesExcelExport(stream);
                        break;
                    }
                case "groupresultreport":
                    {
                        MakeGroupResultReport(writer);
                        break;
                    }
                case "nameaddressreport":
                    {
                        MakeNameAddressReport(writer);
                        break;
                    }
                case "referencedatareport":
                    {
                        MakeReferenceDataReport(writer);
                        break;
                    }
                case "nowpromptrulereport":
                    {
                        MakeNowPromptRuleReport(writer);
                        break;
                    }
                case "childresultduplicatepromptreport":
                    {
                        MakeChildResultDuplicatePromptReport(writer);
                        break;
                    }
                case "courtorderresultreport":
                    {
                        MakeCourtOrderResultReport(writer);
                        break;
                    }
                case "primaryresultlevelreport":
                    {
                        MakePrimaryResultLevelReport(writer);
                        break;
                    }
                case "nowrequirementtextreport":
                    {
                        MakeNowRequirementTextReport(writer);
                        break;
                    }                    
                case "badsubscriptionreport":
                    {
                        MakeBadSubscriptionsReport(writer);
                        break;
                    }                    
                case "templatenamereport":
                    {
                        MakeTemplateNameReport(writer);
                        break;
                    }
                case "promptreferencereport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreatePromptReferenceExcelExport(stream);
                        break;
                    }
                case "courtextractreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateCourtExtractExcelExport(stream);
                        break;
                    }
                case "restrictedresultsreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateRestrictedResultsExcelExport(stream);
                        break;
                    }
                case "nowsubscriptionchildresultreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateNowSubscriptionChildResultExcelExport(stream);
                        break;
                    }
                case "nowsubscriptionreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateNowSubscriptionExcelExport(stream);
                        break;
                    }
                case "subscriptionemailreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateSubscriptionEmailExport(stream);
                        break;
                    }
                case "nowprimaryresultreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateNowPrimaryResultExcelExport(stream);
                        break;
                    }
                case "cacheditemsreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateCachedItemsReport(stream);
                        break;
                    }
                case "fixedlistreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateFixedListExcelExport(stream);
                        break;
                    }
                case "resultpromptdurationwordgroupconflictreport":
                    {
                        MakeResultPromptDurationElementConflictReport(writer);
                        break;
                    }
                case "alwayspublishedreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateAlwaysPublishedResultExcelExport(stream);
                        break;
                    }
                case "custodialresultsreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateCustodialResultsExcelExport(stream);
                        break;
                    }
                case "resulttextreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateResultTextExcelExport(stream);
                        break;
                    }
                case "createpublishaspromptreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreatePublishAsPromptExcelExport(stream);
                        break;
                    }
                case "enforcementresultsreport":
                    {
                        var exporter = new Exporter(treeModel.AllData, treeModel.Map);
                        exporter.CreateEnforcementResultsExcelExport(stream);
                        break;
                    }
            }
        }

        public string PromptUsagesReport(Guid? resultPromptId)
        {
            return reportHelper.PromptUsagesReport(resultPromptId);
        }
        public Tuple<bool, string> PromptUsagesWithFlagReport(Guid? resultPromptId)
        {
            return reportHelper.PromptUsagesWithFlagReport(resultPromptId);            
        }

        public string NowUsagesReport(Guid? nowId)
        {
            return reportHelper.NowUsagesReport(nowId);
        }

        public string ResultUsagesReport(Guid? resultDefinitionId)
        {
            return reportHelper.ResultUsagesReport(resultDefinitionId);
        }

        public Tuple<bool,string> ResultUsagesWithoutParentageReport(Guid? resultDefinitionId)
        {
            return reportHelper.ResultUsagesWithoutParentageReport(resultDefinitionId);
        }

        public Tuple<List<NowRequirementViewModel>, string> OtherNowRequirementsReport(NowRequirement nr)
        {
            //get the published and draft now and edt requirement usages for the passed resultDefinitionId and NowRequirement
            var otherNRs = treeModel.AllNowRequirementsViewModel.NowRequirements
                            .Where(x => x.ResultDefinition.UUID == nr.ResultDefinitionUUID
                                        && x.PublishedStatus != PublishedStatus.Revision
                                        && x.PublishedStatus != PublishedStatus.RevisionPending
                                        && x.UUID != nr.UUID
                                        && x.DeletedDate == null).ToList();

            var otherNOWUsages = otherNRs.Where(x => x.NOWUUID != nr.NOWUUID)
                                    .Select(usage => new { nowid = usage.NOWUUID, isPrimary = usage.PrimaryResult }).Distinct().ToList();


            var nows = treeModel.AllNowsViewModel.Nows.Where(x => x.DeletedDate == null && !x.IsEDT);

            var nowusages = from now in nows
                            join otherNow in otherNOWUsages on now.UUID equals otherNow.nowid
                            orderby now.Name ascending
                            select new { name = now.Name, publishedStatus = now.PublishedStatus, otherNow.isPrimary };

            var sb = new StringBuilder();

            sb.AppendLine("Notice Orders & Warrants ...");

            foreach (var item in nowusages)
            {
                if (item.isPrimary)
                {
                    if (item.publishedStatus == PublishedStatus.Draft)
                    {
                        sb.AppendLine(string.Format("{0} (Primary - Draft))", item.name));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{0} (Primary)", item.name));
                    }
                }
                else
                {
                    if (item.publishedStatus == PublishedStatus.Draft)
                    {
                        sb.AppendLine(string.Format("{0} (Draft))", item.name));
                    }
                    else
                    {
                        sb.AppendLine(item.name);
                    }
                }
            }

            var edts = treeModel.AllEDTsViewModel.EDTs.Where(x => x.DeletedDate == null && x.IsEDT);
            var edtusages = from now in edts
                            join otherNow in otherNOWUsages on now.UUID equals otherNow.nowid
                            orderby now.Name ascending
                            select new { name = now.Name, publishedStatus = now.PublishedStatus, otherNow.isPrimary };

            sb.AppendLine();
            sb.AppendLine("EDTs ...");

            foreach (var item in edtusages)
            {
                if (item.isPrimary)
                {
                    if (item.publishedStatus == PublishedStatus.Draft)
                    {
                        sb.AppendLine(string.Format("{0} (Primary - Draft))", item.name));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{0} (Primary)", item.name));
                    }
                }
                else
                {
                    if (item.publishedStatus == PublishedStatus.Draft)
                    {
                        sb.AppendLine(string.Format("{0} (Draft))", item.name));
                    }
                    else
                    {
                        sb.AppendLine(item.name);
                    }
                }
            }

            return new Tuple<List<NowRequirementViewModel>, string>(otherNRs, sb.ToString());

        }

        private void MakeCourtOrderResultReport(StreamWriter writer)
        {
            //report header
            writer.WriteLine("Result Definition Id,Result Definition Label,Result Definition Publication Status,Result Definition Code,Subject Of Breach, Subject Of Variation,Prompt Id,Prompt Reference,Prompt Label,Rule");

            foreach (var rd in treeModel.AllData.ResultDefinitions.Where(x => x.DeletedDate == null && (x.CanBeSubjectOfBreach || x.CanBeSubjectOfVariation)).OrderBy(x=>x.Label))
            {
                if (rd.ResultPrompts != null)
                {
                    foreach (var rule in rd.ResultPrompts.Where(x => x.DeletedDate == null).OrderBy(x => x.ResultPrompt.Label).ThenBy(x=>x.ResultPrompt.PromptReference))
                    {
                        writer.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", rd.UUID, rd.Label, GetPublishedStatus(rd).GetDescription(), rd.ShortCode, rd.CanBeSubjectOfBreach ? "Yes" : "No", rd.CanBeSubjectOfVariation ? "Yes" : "No", rule.ResultPromptUUID, rule.ResultPrompt.PromptReference, rule.ResultPrompt.Label, rule.Rule);
                    }
                }
            }
        }

        private void MakeResultPromptDurationElementConflictReport(StreamWriter writer)
        {
            //report header
            writer.WriteLine("Result Prompt UUID,Result Prompt Label,Words,Durations");
            var distinctPromptsWithDurationsAndWords = treeModel.AllData.ResultPromptRules.Where(x => x.ResultPrompt.DeletedDate == null && x.ResultPrompt.DeletedDate == null 
                                                && x.ResultPrompt.DurationElements != null && x.ResultPrompt.DurationElements.Count>0
                                                && x.ResultPrompt.ResultPromptWordGroups != null && x.ResultPrompt.ResultPromptWordGroups.Where(y=>y.DeletedDate == null).Count() > 0)
                                             .GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt).ToList();
            foreach (var prompt in distinctPromptsWithDurationsAndWords)
            {
                var words = new List<string>();
                var durationsMatched = true;
                foreach(var word in prompt.ResultPromptWordGroups.Where(y => y.DeletedDate == null))
                {
                    words.Add(word.ResultPromptWord);
                    if (durationsMatched)
                    {
                        var matchedDuration = prompt.DurationElements.FirstOrDefault(x => x.DurationElement.ToLowerInvariant() == word.ResultPromptWord.ToLowerInvariant());
                        if(matchedDuration == null)
                        {
                            durationsMatched = false;
                        }
                    }
                }
                if(!durationsMatched)
                {
                    var durations = string.Join(",",prompt.DurationElements.Select(x => x.DurationElement));
                    var wordsNotMatched = string.Join(",", words.Select(x => x));
                    writer.WriteLine("{0},{1},\"{2}\",\"{3}\"", prompt.UUID, prompt.Label, wordsNotMatched, durations);
                }                
            }
        }

        private void MakeTemplateNameReport(StreamWriter writer)
        {
            //report header
            writer.WriteLine("NOW Name,EDT Name,Template Name,Bilingual Template Name");
            var items = treeModel.AllData.Nows.Where(x => x.DeletedDate == null).OrderBy(x => x.IsEDT).ThenBy(x=>x.Name).ToList();
            foreach (var now in items)
            {
                writer.WriteLine("{0},{1},{2},{3}", now.IsEDT ? string.Empty : now.Name, now.IsEDT ? now.Name : string.Empty, now.TemplateName, now.BilingualTemplateName);
            }
        }

        private void MakePrimaryResultLevelReport(StreamWriter writer)
        {
            //report header
            writer.WriteLine("Result Definition Id,Result Definition Label,Level,Is Primary?,NOW,EDT");
            var items = treeModel.AllData.NowRequirements.Where(x => x.DeletedDate == null && x.ResultDefinition.LevelFirstLetter.ToLowerInvariant() != "o").OrderBy(x => x.ResultDefinition.Label).ToList();
            foreach (var nr in items)
            {
                var now = treeModel.AllData.Nows.FirstOrDefault(x => x.UUID == nr.NOWUUID);
                writer.WriteLine("{0},{1},{2},{3},{4},{5}", nr.ResultDefinition.UUID, nr.ResultDefinition.Label, nr.ResultDefinition.Level, nr.PrimaryResult ? "Yes" : "No", now.IsEDT ? null : now.Name, now.IsEDT ? now.Name : null);
            }
        }

        private void MakeNowRequirementTextReport(StreamWriter writer)
        {
            //report header
            writer.WriteLine("Is EDT?,Name,Result,Now Requirement Id,NowText Label,English,Welsh");

            var items = from nr in treeModel.AllData.NowRequirements
                        join nowEdt in treeModel.AllData.Nows on nr.NOWUUID equals nowEdt.UUID
                        where nr.NowRequirementTextList != null && nr.NowRequirementTextList.Count>0
                        orderby nowEdt.IsEDT, nowEdt.Name ascending
                         select new {nr = nr, nowEdt = nowEdt, resultDefinition = nr.ResultDefinition };

            foreach (var item in items)
            {
                foreach (var textItem in item.nr.NowRequirementTextList)
                {
                    writer.WriteLine("{0},\"{1}\",\"{2}\",{3},{4},\"{5}\",\"{6}\"", item.nowEdt.IsEDT ? "Yes" : "No", item.nowEdt.Name, item.resultDefinition.Label, item.nr.UUID, textItem.NowReference, textItem.NowText, textItem.NowWelshText);
                }
            }
        }

        private void MakeBadSubscriptionsReport(StreamWriter writer)
        {
            //report header
            writer.WriteLine("Subscription Id,Subscription Name,Removed Excluded Result Id,Removed Excluded Result,Removed Excluded Prompt Id,Removed Excluded Prompt,Removed Included Result Id,Removed Included Result,Removed Included Prompt Id,Removed Included Prompt");
            var items = treeModel.AllData.NowSubscriptions.Where(x => x.DeletedDate == null && x.SubscriptionVocabulary != null 
                    && (
                            (x.SubscriptionVocabulary.ExcludedResults != null && x.SubscriptionVocabulary.ExcludedResults.Count>0) || 
                            (x.SubscriptionVocabulary.ExcludedPromptRules != null && x.SubscriptionVocabulary.ExcludedPromptRules.Count > 0) ||
                            (x.SubscriptionVocabulary.IncludedResults != null && x.SubscriptionVocabulary.IncludedResults.Count > 0) ||
                            (x.SubscriptionVocabulary.IncludedPromptRules != null && x.SubscriptionVocabulary.IncludedPromptRules.Count > 0)
                        )
                        ).OrderBy(x => x.Name).ToList();
            foreach (var sub in items)
            {
                if (sub.SubscriptionVocabulary.ExcludedResults != null && sub.SubscriptionVocabulary.ExcludedResults.Count>0)
                {
                    foreach(var excludedRD in sub.SubscriptionVocabulary.ExcludedResults)
                    {
                        var match = treeModel.AllData.ResultDefinitions.FirstOrDefault(x=>x.UUID == excludedRD.UUID);
                        if (match == null || match.DeletedDate != null)
                        {
                            writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", sub.UUID, sub.Name, excludedRD.UUID, excludedRD.Label, null, null, null, null, null, null);
                        }
                    }
                }
                if (sub.SubscriptionVocabulary.ExcludedPromptRules != null && sub.SubscriptionVocabulary.ExcludedPromptRules.Count > 0)
                {
                    foreach (var excludedPR in sub.SubscriptionVocabulary.ExcludedPromptRules)
                    {
                        var match = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => x.ResultPromptUUID == excludedPR.ResultPromptUUID && x.ResultPrompt.DeletedDate == null);
                        if (match == null || match.DeletedDate != null)
                        {
                            writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", sub.UUID, sub.Name, null, null, excludedPR.ResultPromptUUID, excludedPR.ResultPrompt.Label, null, null, null, null);
                        }
                    }
                }
                if (sub.SubscriptionVocabulary.IncludedResults != null && sub.SubscriptionVocabulary.IncludedResults.Count > 0)
                {
                    foreach (var includedRD in sub.SubscriptionVocabulary.IncludedResults)
                    {
                        var match = treeModel.AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == includedRD.UUID);
                        if (match == null || match.DeletedDate != null)
                        {
                            writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", sub.UUID, sub.Name, null, null, null, null, includedRD.UUID, includedRD.Label, null, null);
                        }
                    }
                }
                if (sub.SubscriptionVocabulary.IncludedPromptRules != null && sub.SubscriptionVocabulary.IncludedPromptRules.Count > 0)
                {
                    foreach (var includedPR in sub.SubscriptionVocabulary.IncludedPromptRules)
                    {
                        var match = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => x.ResultPromptUUID == includedPR.ResultPromptUUID && x.ResultPrompt.DeletedDate == null);
                        if (match == null || match.DeletedDate != null)
                        {
                            writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", sub.UUID, sub.Name, null, null, null, null, null, null, includedPR.ResultPromptUUID, includedPR.ResultPrompt.Label);
                        }
                    }
                }
            }
        }

        private void MakeNowPromptRuleReport(StreamWriter writer)
        {
            var allRules = new List<NowRequirementPromptRule>();
            foreach (var nr in treeModel.AllData.NowRequirements.Where(x => x.DeletedDate == null && x.NowRequirementPromptRules != null))
            {
                foreach (var rule in nr.NowRequirementPromptRules.Where(x => x.DeletedDate == null))
                {
                    allRules.Add(rule);
                }
            }

            //report header
            writer.WriteLine("Id,NOW Name,EDT Name,Now/EDT Publication Status,Result Definition Id,Result Definition Label,Prompt Id,Prompt Label,Prompt Reference,Is Variant Data?,Distinct Prompts?,Primary Result?");
            var lines = new List<NowPromptRuleReportLine>();

            //group all rules by now
            var nowGroups = allRules.GroupBy(x => x.ParentNowRequirement.NOWUUID);
            foreach (var nowGroup in nowGroups)
            {
                //group by promptId
                var promptGroups = nowGroup.ToList().GroupBy(x => x.ResultPromptRule.ResultPrompt.UUID);

                foreach (var promptGroup in promptGroups)
                {
                    //group by mde and distinctprompts
                    var mdeGroups = promptGroup.ToList().GroupBy(x => new { x.DistinctPromptTypes, x.IsVariantData });
                    var count = mdeGroups.Count();
                    if (mdeGroups.Count() > 1)
                    {
                        var grps = mdeGroups.ToList();
                        //only one group expected, report the prompt group
                        var now = treeModel.AllData.Nows.FirstOrDefault(x => x.UUID == nowGroup.Key);
                        foreach (var mdeGroup in mdeGroups)
                        {
                            foreach (var item in mdeGroup)
                            {
                                //append the report line
                                lines.Add(new NowPromptRuleReportLine(now.UUID,
                                                                        now.Name,
                                                                        GetPublishedStatus(now).GetDescription(),
                                                                        now.IsEDT,
                                                                        item.ParentNowRequirement.ResultDefinition.UUID,
                                                                        item.ParentNowRequirement.ResultDefinition.Label,
                                                                        item.ResultPromptRule.ResultPrompt.UUID,
                                                                        item.ResultPromptRule.ResultPrompt.Label,
                                                                        item.ResultPromptRule.ResultPrompt.PromptReference,
                                                                        item.DistinctPromptTypes,
                                                                        item.IsVariantData,
                                                                        item.ParentNowRequirement.PrimaryResult,
                                                                        mdeGroup.Count()));
                            }
                        }
                    }
                }
            }

            var reportLines = lines.Where(x=>!string.IsNullOrEmpty(x.NowName)).OrderBy(x => x.NowName).ThenBy(x => x.PromptLabel).ThenBy(x => x.IsVariantData).ThenBy(x => x.DistinctPromptTypes).ThenBy(x => x.ResultLabel);
            foreach (var line in reportLines)
            {
                writer.WriteLine(line.ToString());
            }

            reportLines = lines.Where(x => !string.IsNullOrEmpty(x.EDTName)).OrderBy(x => x.EDTName).ThenBy(x => x.PromptLabel).ThenBy(x => x.IsVariantData).ThenBy(x => x.DistinctPromptTypes).ThenBy(x => x.ResultLabel);
            foreach (var line in reportLines)
            {
                writer.WriteLine(line.ToString());
            }

        }
        private class NowPromptRuleReportLine
        {
            public Guid? Id { get;  }
            public string NowName { get;  }
            public string EDTName { get; }
            public string PublicationStatus { get; }
            public Guid? ResultDefinitionUUID { get;  }
            public string ResultLabel { get;  }
            public Guid? ResultPromptUUID { get;  }
            public string PromptLabel { get;  }
            public string PromptReference { get;  }
            public bool DistinctPromptTypes { get;}
            public bool IsVariantData { get; }
            public bool IsPrimary { get; }
            public int NoItems { get; }

            public NowPromptRuleReportLine(Guid? id, string nowName, string publicationStatus, bool isEDT, Guid? resultDefinitionUUID, string resultLabel, 
                Guid? resultPromptUUID, string promptLabel, string promptReference, bool distinctPromptTypes, bool isVariantData, bool isPrimary, int noItems)
            {
                Id = id;
                NowName = !isEDT ? nowName : string.Empty;
                EDTName = isEDT ? nowName : string.Empty;
                PublicationStatus = publicationStatus;
                ResultDefinitionUUID = resultDefinitionUUID;
                ResultLabel = resultLabel;
                ResultPromptUUID = resultPromptUUID;
                PromptLabel = promptLabel;
                PromptReference = promptReference;
                DistinctPromptTypes = distinctPromptTypes;
                IsVariantData = isVariantData;
                NoItems = noItems;
            }

            public override string ToString()
            {
                return string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\"", Id, NowName, EDTName, PublicationStatus, ResultDefinitionUUID, ResultLabel, ResultPromptUUID, PromptLabel, PromptReference, DistinctPromptTypes ? "Y" : "N", IsVariantData ? "Y" : "N", IsPrimary ? "Y" : "N");
            }
        }

        private List<ResultDefinition> GetRoots()
        {
            var res = new List<ResultDefinition>();
            if (treeModel.AllData.ResultDefinitionRules == null || treeModel.AllData.ResultDefinitionRules.Count() == 0)
            {
                return res;
            }
            var distinctChildren = treeModel.AllData.ResultDefinitionRules.FindAll(x => x.DeletedDate == null && 
                            (x.CalculatedPublishedStatus == PublishedStatus.Published || x.CalculatedPublishedStatus == PublishedStatus.PublishedPending))
                    .Select(x => x.ChildResultDefinitionUUID).Distinct().ToList();
            var parents = treeModel.AllData.ResultDefinitionRules.FindAll(x => x.DeletedDate == null &&
                            (x.CalculatedPublishedStatus == PublishedStatus.Published || x.CalculatedPublishedStatus == PublishedStatus.PublishedPending))
                    .GroupBy(x => x.ParentUUID).Select(x => x.Key).ToList();

            var roots = parents.Except(distinctChildren).ToList();

            var activeResults = treeModel.AllData.ResultDefinitions.Where(x => x.DeletedDate == null &&
                            (x.CalculatedPublishedStatus == PublishedStatus.Published || x.CalculatedPublishedStatus == PublishedStatus.PublishedPending))
                   .Select(x => x.UUID).ToList();

            var activeRoots = roots.Intersect(activeResults).ToList();
            foreach(var id in activeRoots)
            {
                res.Add(treeModel.AllData.ResultDefinitions.First(x => x.UUID == id));
            }
            return res;
        }

        private void MakeGroupResultReport(StreamWriter writer)
        {
            var header = string.Empty;

            var lines = new List<string>();
            var maxLevels = 2;
            var roots = GetRoots();
            foreach (var root in roots)
            {
                foreach (var child in root.ResultDefinitionRules.Where(x => x.DeletedDate == null && x.ResultDefinition.DeletedDate == null))
                {
                    var parentChild = string.Format("\"{0}\",\"{1}\"", root.Label, child.ResultDefinition.Label);
                    var levels = 2;

                    if (child.ResultDefinition.ResultDefinitionRules != null && child.ResultDefinition.ResultDefinitionRules.Where(x => x.DeletedDate == null && x.ResultDefinition.DeletedDate == null).Count() > 0)
                    {
                        foreach (var grandChild in child.ResultDefinition.ResultDefinitionRules.Where(x => x.DeletedDate == null && x.ResultDefinition.DeletedDate == null))
                        {
                            levels = IterateParentReport(grandChild, parentChild, lines, levels);
                            if (levels > maxLevels)
                            {
                                maxLevels = levels;
                            }
                        }
                    }
                    else
                    {
                        lines.Add(parentChild);
                    }
                }
            }

            //make a header row
            string headerline = "RootResultDefinition";
            for (var i = 2; i <= 5; i++)
            {
                headerline = string.Format("{0},Level {1} Result Definition", headerline, i);
            }

            var reportLines = lines.OrderBy(x => x).ToList();
            reportLines.Insert(0, headerline);

            foreach (var reportLine in reportLines)
            {
                writer.WriteLine(reportLine);
            }
        }

        private int IterateParentReport(ResultDefinitionRule child, string prefix, List<string> lines, int levels)
        {
            var parentChild = string.Format("{0},\"{1}\"", prefix, child.ResultDefinition.Label);
            levels++;

            if (child.ResultDefinition.ResultDefinitionRules != null && child.ResultDefinition.ResultDefinitionRules.Where(x => x.DeletedDate == null && x.ResultDefinition.DeletedDate == null).Count() > 0)
            {
                foreach (var grandChild in child.ResultDefinition.ResultDefinitionRules.Where(x => x.DeletedDate == null && x.ResultDefinition.DeletedDate == null))
                {
                    var grandChildStatus = GetPublishedStatus(grandChild.ResultDefinition);
                    if (grandChildStatus != PublishedStatus.Published && grandChildStatus != PublishedStatus.PublishedPending)
                    {
                        continue;
                    }
                    IterateParentReport(grandChild, parentChild, lines, levels);
                }
            }
            else
            {
                lines.Add(parentChild);
            }

            return levels;
        }

        private class PromptNowUsage
        {
            public DataLib.ResultPrompt ResultPrompt { get; set; }

            public List<Now> Nows { get; set; }

            public bool IsRecipient { get; set; }

            public bool IsContent { get; set; }

            public PromptNowUsage(DataLib.ResultPrompt prompt, List<Now> nows, bool isRecipient, bool isContent)
            {
                ResultPrompt = prompt;
                Nows = nows;
                IsContent = isContent;
                IsRecipient = isRecipient;
            }
        }

        private void AppendPromptsForNowsUsage(Dictionary<Guid?, PromptNowUsage> promptsForNowSubscriptions, DataLib.ResultPrompt prompt, NowSubscription sub)
        {
            List<Now> nows = null;

            if (promptsForNowSubscriptions.ContainsKey(prompt.UUID))
            {
                var val = promptsForNowSubscriptions[prompt.UUID];
                nows = val.Nows;
            }
            else
            {
                nows = new List<Now>();
                var val = new PromptNowUsage(prompt, nows, true, false);
                promptsForNowSubscriptions.Add(prompt.UUID, val);
            }

            if (sub.IncludedNOWS != null)
            {
                foreach (var now in sub.IncludedNOWS.Where(x => x.DeletedDate == null))
                {
                    var match = nows.FirstOrDefault(x => x.UUID == now.UUID);
                    if (match == null)
                    {
                        var nowStatus = GetPublishedStatus(now);
                        if (nowStatus != PublishedStatus.Published && nowStatus != PublishedStatus.PublishedPending)
                        {
                            continue;
                        }
                        nows.Add(now);
                    }
                }
            }
        }

        private void MakeChildResultDuplicatePromptReport(StreamWriter writer)
        {
            var parentResults = treeModel.AllData.ResultDefinitions.Where(x => x.ResultDefinitionRules != null && x.ResultDefinitionRules.Count() > 0 && x.ResultPrompts != null && x.ResultPrompts.Count>0);

            writer.WriteLine("Parent Result Id,Parent Result,Child Result Id,Child Result,Shared Prompt Id, Shared Prompt");

            foreach (var parent in parentResults.OrderBy(x=>x.Label))
            {
                foreach(var childRule in parent.ResultDefinitionRules.Where(x=>x.DeletedDate == null 
                                                    && (x.ResultDefinition.IsRollupPrompts || x.ResultDefinition.IsPublishedAsAPrompt) 
                                                    && x.ResultDefinition.DeletedDate == null 
                                                    && x.ResultDefinition.ResultPrompts != null 
                                                    && x.ResultDefinition.ResultPrompts.Count>0).OrderBy(x=>x.ResultDefinition.Label))
                {
                    foreach(var childPrompt in childRule.ResultDefinition.ResultPrompts.Where(x=>x.DeletedDate == null).OrderBy(x=>x.ResultPrompt.Label))
                    {
                        var matchedParent = parent.ResultPrompts.FirstOrDefault(x => x.DeletedDate == null && x.ResultPromptUUID == childPrompt.ResultPromptUUID);
                        if(matchedParent != null)
                        {
                            writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", parent.UUID, parent.Label, childRule.ResultDefinition.UUID, childRule.ResultDefinition.Label, matchedParent.ResultPrompt.UUID, matchedParent.ResultPrompt.Label));
                        }
                    }
                }
            }
        }

        private void MakeReferenceDataReport(StreamWriter writer)
        {
            var promptsForNowUsage = new Dictionary<Guid?, PromptNowUsage>();

            //find all of the reference data prompts
            var distinctRefDataPrompts = treeModel.AllData.ResultPromptRules.Where(x => x.ResultPrompt.DeletedDate == null
                                                    && x.ResultPrompt.AssociateToReferenceData)
                                                    .GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt).ToList();

            List<NowRequirementPromptRule> rules = null;
            foreach (var prompt in distinctRefDataPrompts.OrderBy(x=>x.Label))
            {
                //check if this prompt has distinct prompt settings in any NOWs
                if (rules == null) { rules = GetNowRequirementPromptUsage(); }

                var nowIds = rules.Where(x => x.ResultPromptRule.ResultPrompt.UUID == prompt.UUID).GroupBy(y => y.NOWUUID).Select(x => x.First()).Select(x => x.NOWUUID).ToList();
                var nows = new List<Now>();
                foreach (var id in nowIds)
                {
                    var now = treeModel.AllData.Nows.FirstOrDefault(x => x.UUID == id);
                    if (now != null)
                    {
                        nows.Add(now);
                    }
                }

                promptsForNowUsage.Add(prompt.UUID, new PromptNowUsage(prompt, nows, false, true));
            }

            //now make the report
            writer.WriteLine("Result Prompt Id,Prompt Label,Prompt Publication Status,Prompt Reference,Prompt Type,Is Available For Court Extract,Content In NOW");
            foreach (var item in promptsForNowUsage)
            {
                if (item.Value.Nows == null || item.Value.Nows.Count == 0)
                {
                    writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", item.Key, item.Value.ResultPrompt.Label, GetPublishedStatus(item.Value.ResultPrompt).GetDescription(), item.Value.ResultPrompt.PromptReference, item.Value.ResultPrompt.PromptType, item.Value.ResultPrompt.IsAvailableForCourtExtract ? "Y" : "N"));
                    continue;
                }
                foreach (var now in item.Value.Nows.OrderBy(x => x.Name))
                {
                    if (item.Value.IsContent)
                    {
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{8}\"", item.Key, item.Value.ResultPrompt.Label, GetPublishedStatus(item.Value.ResultPrompt).GetDescription(), item.Value.ResultPrompt.PromptReference, item.Value.ResultPrompt.PromptType, item.Value.ResultPrompt.IsAvailableForCourtExtract ? "Y" : "N", now.Name));
                    }
                    else
                    {
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"", item.Key, item.Value.ResultPrompt.Label, GetPublishedStatus(item.Value.ResultPrompt).GetDescription(), item.Value.ResultPrompt.PromptReference, item.Value.ResultPrompt.PromptType, item.Value.ResultPrompt.IsAvailableForCourtExtract ? "Y" : "N", string.Empty, now.Name));
                    }
                }
            }
        }

        private void MakeNameAddressReport(StreamWriter writer)
        {
            var subs = treeModel.AllData.NowSubscriptions.Where(x => x.DeletedDate == null && x.NowRecipient != null && x.ResultRecipient).ToList();
            var promptsForNowUsage = new Dictionary<Guid?, PromptNowUsage>();
            foreach (var sub in subs)
            {
                var subStatus = GetPublishedStatus(sub);
                if (subStatus != PublishedStatus.Published && subStatus != PublishedStatus.PublishedPending)
                {
                    continue;
                }
                //check if the name is a nameaddress prompt
                if (sub.NowRecipient.LastNameResultPromptId != null)
                {
                    var match = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => x.ResultPromptUUID == sub.NowRecipient.LastNameResultPromptId);

                    if (match != null && (match.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" || match.ResultPrompt.PromptType.ToLowerInvariant() == "address"))
                    {
                        AppendPromptsForNowsUsage(promptsForNowUsage, match.ResultPrompt, sub);
                        continue;
                    }
                }

                if (sub.NowRecipient.OrganisationNameResultPromptId != null)
                {
                    var match = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => x.ResultPromptUUID == sub.NowRecipient.OrganisationNameResultPromptId);

                    if (match != null && (match.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" || match.ResultPrompt.PromptType.ToLowerInvariant() == "address"))
                    {
                        AppendPromptsForNowsUsage(promptsForNowUsage, match.ResultPrompt, sub);
                        continue;
                    }
                }

                if (sub.NowRecipient.Address1ResultPromptId != null)
                {
                    var match = treeModel.AllData.ResultPromptRules.FirstOrDefault(x => x.ResultPromptUUID == sub.NowRecipient.Address1ResultPromptId);

                    if (match != null && (match.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" || match.ResultPrompt.PromptType.ToLowerInvariant() == "address"))
                    {
                        AppendPromptsForNowsUsage(promptsForNowUsage, match.ResultPrompt, sub);
                        continue;
                    }
                }
            }

            //now find all of the other name address prompts
            var distinctNameAddressPrompts = treeModel.AllData.ResultPromptRules.Where(x => x.ResultPrompt.DeletedDate == null
                                                    && (!string.IsNullOrEmpty(x.ResultPrompt.PromptType) && (x.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" || x.ResultPrompt.PromptType.ToLowerInvariant() == "address")))
                                                .GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt).ToList();

            List<NowRequirementPromptRule> rules = null;
            foreach (var prompt in distinctNameAddressPrompts.OrderBy(x=>x.Label))
            {
                if (promptsForNowUsage.ContainsKey(prompt.UUID))
                {
                    continue;
                }

                var promptStatus = GetPublishedStatus(prompt);
                if (promptStatus != PublishedStatus.Published && promptStatus != PublishedStatus.PublishedPending)
                {
                    continue;
                }
                //check if this prompt has distinct prompt settings in any NOWs
                if (rules == null) { rules = GetNowRequirementPromptUsage(); }

                var nowIds = rules.Where(x => x.ResultPromptRule.ResultPrompt.UUID == prompt.UUID).GroupBy(y => y.NOWUUID).Select(x => x.First()).Select(x => x.NOWUUID).ToList();
                var nows = new List<Now>();
                foreach (var id in nowIds)
                {
                    var now = treeModel.AllData.Nows.FirstOrDefault(x => x.UUID == id);
                    if (now != null)
                    {
                        nows.Add(now);
                    }
                }

                promptsForNowUsage.Add(prompt.UUID, new PromptNowUsage(prompt, nows, false, true));
            }

            //now make the report
            writer.WriteLine("Result Prompt Id,Prompt Label,Prompt Reference,Prompt Type,Is Available For Court Extract,Associated To Reference Data,Recipient On NOW,Content In NOW");
            foreach (var item in promptsForNowUsage)
            {
                if (item.Value.Nows == null || item.Value.Nows.Count == 0)
                {
                    writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", item.Key, item.Value.ResultPrompt.Label, item.Value.ResultPrompt.PromptReference, item.Value.ResultPrompt.PromptType, item.Value.ResultPrompt.IsAvailableForCourtExtract ? "Y" : "N", item.Value.ResultPrompt.AssociateToReferenceData ? "Y" : "N"));
                    continue;
                }
                foreach (var now in item.Value.Nows.OrderBy(x=>x.Name))
                {
                    if (item.Value.IsRecipient)
                    {
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"", item.Key, item.Value.ResultPrompt.Label, item.Value.ResultPrompt.PromptReference, item.Value.ResultPrompt.PromptType, item.Value.ResultPrompt.IsAvailableForCourtExtract ? "Y" : "N", item.Value.ResultPrompt.AssociateToReferenceData ? "Y" : "N", now.Name));
                    }
                    else
                    {
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"", item.Key, item.Value.ResultPrompt.Label, item.Value.ResultPrompt.PromptReference, item.Value.ResultPrompt.PromptType, item.Value.ResultPrompt.IsAvailableForCourtExtract ? "Y" : "N", item.Value.ResultPrompt.AssociateToReferenceData ? "Y" : "N", string.Empty, now.Name));
                    }
                }
            }
        }

        private List<NowRequirementPromptRule> GetNowRequirementPromptUsage()
        {
            var resp = new List<NowRequirementPromptRule>();
            foreach (var nr in treeModel.AllData.NowRequirements.Where(x => x.DeletedDate == null && x.NowRequirementPromptRules != null))
            {
                foreach (var rule in nr.NowRequirementPromptRules.Where(x => x.DeletedDate == null && x.DistinctPromptTypes))
                {
                    resp.Add(rule);
                }
            }

            return resp;
        }

        private PublishedStatus GetPublishedStatus(DataAudit data)
        {
            if (data != null && data.PublishedStatus != null) { return data.PublishedStatus.Value; }

            return PublishedStatus.Published;
        }
    }
}
