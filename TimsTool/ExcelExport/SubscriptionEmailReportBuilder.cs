using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExportExcel
{
    public class SubscriptionEmailReportBuilder : SheetBuilderBase
    {
        public SubscriptionEmailReportBuilder(WorkbookPart workbookPart, AllData allData) : base("Subscriptions", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Name", CellValues.String),
                ConstructCell("Publication Status", CellValues.String),
                ConstructCell("Type", CellValues.String),
                ConstructCell("Email By Results", CellValues.String),
                ConstructCell("Hard Coded Emails", CellValues.String),
                ConstructCell("Created Date", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String));
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            var reportLines = BuildReportLines();
            foreach (var line in reportLines.OrderBy(x=>x.Type))
            {
                var row = new Row();
                row.Append(
                    ConstructCell(AsString(line.Subscription.UUID), CellValues.String),
                    ConstructCell(line.Subscription.Name, CellValues.String),
                    ConstructCell(GetPublishedStatus(line.Subscription).GetDescription(), CellValues.String),
                    ConstructCell(line.Type, CellValues.String),
                    ConstructCell(line.Results, CellValues.String),
                    ConstructCell(line.EmailAddresses, CellValues.String),
                    ConstructDateCell(line.Subscription.CreatedDate),
                    ConstructDateCell(line.Subscription.LastModifiedDate)
                    );
                SheetData.AppendChild(row);
            }
        }

        private List<SubscriptionReportLine> BuildReportLines()
        {
            var res = new List<SubscriptionReportLine>();
            foreach (var sub in AllData.NowSubscriptions.Where(x=>x.DeletedDate == null && x.IsEmail && x.NowRecipient != null))
            {
                string type = "Unknown";
                if (sub.IsCourtRegister) { type = "Court Register"; }
                if (sub.IsPrisonCourtRegister) { type = "Prison Court Register"; }
                if (sub.IsInformantRegister) { type = "Informant Register"; }
                if (sub.IsEDT) { type = "EDT"; }
                if (sub.IsNow) { type = "NOW"; }

                if (!string.IsNullOrEmpty(sub.NowRecipient.EmailAddress1) || !string.IsNullOrEmpty(sub.NowRecipient.EmailAddress2))
                {
                    var addresses = new string[] { sub.NowRecipient.EmailAddress1, sub.NowRecipient.EmailAddress2 };
                    var emailAddresses = String.Join(",", addresses.Where(s => !string.IsNullOrEmpty(s)));
                    res.Add(new SubscriptionReportLine() { Type = type, EmailAddresses = emailAddresses, Subscription = sub, Results = null });
                    continue;
                }

                if (sub.NowRecipient.EmailAddress1ResultPrompt != null || sub.NowRecipient.EmailAddress2ResultPrompt != null)
                {
                    var results = new List<string>();
                    if (sub.NowRecipient.EmailAddress1ResultPrompt != null)
                    {
                        var result = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == sub.NowRecipient.EmailAddress1ResultPrompt.ResultDefinitionUUID);
                        results.Add(string.Format("{0}.{1} - ({2})", result.Label, sub.NowRecipient.EmailAddress1ResultPrompt.ResultPrompt.Label, sub.NowRecipient.EmailAddress1ResultPromptReference));
                    }
                    if (sub.NowRecipient.EmailAddress2ResultPrompt != null)
                    {
                        var result = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == sub.NowRecipient.EmailAddress2ResultPrompt.ResultDefinitionUUID);
                        results.Add(string.Format("{0}.{1} - ({2})", result.Label, sub.NowRecipient.EmailAddress2ResultPrompt.ResultPrompt.Label, sub.NowRecipient.EmailAddress2ResultPromptReference));
                    }
                    var resultsString = String.Join(Environment.NewLine, results.Where(s => !string.IsNullOrEmpty(s)));
                    res.Add(new SubscriptionReportLine() { Type = type, EmailAddresses = null, Subscription = sub, Results = resultsString });
                    continue;
                }
            }

            return res;
        }

        private class SubscriptionReportLine
        {
            public string Type { get; set; }
            public string EmailAddresses { get; set; }
            public string Results { get; set; }
            public NowSubscription Subscription { get; set; }
        }
    }
}
