using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using NowsRequestDataLib.Domain;
using System;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Linq;

namespace ExportExcel
{
    public class NowSubscriptionReportBuilder : SheetBuilderBase
    {
        private bool isEDT;
        public NowSubscriptionReportBuilder(WorkbookPart workbookPart, AllData allData, bool isEDT) : base(isEDT ? "Matching EDT Subscriptions" : "Matching NOW Subscriptions", workbookPart, allData) { this.isEDT = isEDT; }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell(isEDT ? "EDT Name" : "NOW Name", CellValues.String),
                ConstructCell(isEDT ? "EDT Publication Status" : "NOW Publication Status", CellValues.String),
                ConstructCell("Subscription", CellValues.String),
                ConstructCell("Subscription Publication Status", CellValues.String),
                ConstructCell("Distribution Method", CellValues.String),
                ConstructCell("Rules", CellValues.String),
                ConstructCell("Only When Included Results", CellValues.String),
                ConstructCell("Only When Excluded Results", CellValues.String),
                ConstructCell("Only When Included Prompts", CellValues.String),
                ConstructCell("Only When Excluded Prompts", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            //find the subscriptions for the given now
            var nows = AllData.Nows.Where(x => x.DeletedDate == null && x.IsEDT == isEDT).OrderBy(x => x.Name);

            //check each now to find the subscriptions
            foreach (var now in nows)
            {
                var subscribers = HearingResults.GetSubscribers(now, AllData.NowSubscriptions.Where(x => x.DeletedDate == null).ToList(), null, null).ToList();

                if (subscribers.Count == 0)
                {
                    AppendRow(now, null, null, null, null, null, null, null);
                    continue;
                }

                var firstNowRowAppended = false;
                foreach(var subscriber in subscribers)
                {
                    if (subscriber.ParentNowSubscription != null)
                    {
                        ReportRow(firstNowRowAppended ? null : now, subscriber.ParentNowSubscription);
                        firstNowRowAppended = true;
                    }

                    ReportRow(firstNowRowAppended ? null : now, subscriber);
                    firstNowRowAppended = true;                    
                }
            }
        }

        private void ReportRow(Now now, NowSubscription subscriber)
        {
            var rules = GetRulesReport(subscriber);
            var includedResults = GetIncludedResultsReport(subscriber);
            var excludedResults = GetExcludedResultsReport(subscriber);
            var includedPrompts = GetIncludedResultPromptsReport(subscriber);
            var excludedPrompts = GetExcludedResultPromptsReport(subscriber);
            var distributionMethod = GetDistributionMethod(subscriber);

            AppendRow(now, subscriber, distributionMethod, rules, includedResults, excludedResults, includedPrompts, excludedPrompts);
        }

        private string GetExcludedResultsReport(NowSubscription subscriber)
        {
            if (!subscriber.ApplySubscriptionRules || subscriber.SubscriptionVocabulary == null) { return null; }

            return subscriber.SubscriptionVocabulary.GetExcludedResultsReport();
        }

        private string GetExcludedResultPromptsReport(NowSubscription subscriber)
        {
            if (!subscriber.ApplySubscriptionRules || subscriber.SubscriptionVocabulary == null) { return null; }

            return subscriber.SubscriptionVocabulary.GetExcludedResultPromptsReport();
        }

        private string GetIncludedResultPromptsReport(NowSubscription subscriber)
        {
            if (!subscriber.ApplySubscriptionRules || subscriber.SubscriptionVocabulary == null) { return null; }

            return subscriber.SubscriptionVocabulary.GetIncludedResultPromptsReport();
        }

        private string GetIncludedResultsReport(NowSubscription subscriber)
        {
            if (!subscriber.ApplySubscriptionRules || subscriber.SubscriptionVocabulary == null) { return null; }
            
            return subscriber.SubscriptionVocabulary.GetIncludedResultsReport();
        }

        private string GetRulesReport(NowSubscription subscriber)
        {
            if (!subscriber.ApplySubscriptionRules || subscriber.SubscriptionVocabulary == null) { return null; }

            return subscriber.SubscriptionVocabulary.GetRulesReport();
        }

        private string GetDistributionMethod(NowSubscription subscriber)
        {
            if (subscriber == null) { return "No Subscription"; }

            if (!subscriber.IsForDistribution) { return "No Distribution"; }

            if (subscriber.IsEmail) { return "Email"; }

            if (subscriber.IsFirstClassLetter) { return "First Class Letter"; }

            if (subscriber.IsSecondClassLetter) { return "Second Class Letter"; }

            return "Unknown";
        }

        private void AppendRow(Now now, NowSubscription subscriber, string distributionMethod, string rules, string includedResults, string excludedResults, string includedPrompts, string excludedPrompts)
        {
            var row = new Row();
            row.Append(
                ConstructCell(now == null ? null : now.Name, CellValues.String),
                ConstructCell(GetPublishedStatus(now).GetDescription(), CellValues.String),
                ConstructCell(subscriber == null ? "No Subscription" : subscriber.Name, CellValues.String),
                ConstructCell(subscriber == null ? String.Empty : GetPublishedStatus(subscriber).GetDescription(), CellValues.String),
                ConstructCell(distributionMethod, CellValues.String),
                ConstructCell(rules, CellValues.String),
                ConstructCell(includedResults, CellValues.String),
                ConstructCell(excludedResults, CellValues.String),
                ConstructCell(includedPrompts, CellValues.String),
                ConstructCell(excludedPrompts, CellValues.String)
                );

            SheetData.AppendChild(row);
        }
    }
}
