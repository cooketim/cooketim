using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using NowsRequestDataLib.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExportExcel
{
    public class PrimaryChildResultBuilder : SheetBuilderBase
    {
        private bool isEDT;
        public PrimaryChildResultBuilder(WorkbookPart workbookPart, AllData allData, bool isEDT) : base(isEDT ? "Child Primary EDTs" : "Child Primary NOWs", workbookPart, allData) { this.isEDT = isEDT; }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Definition UUID", CellValues.String),
                ConstructCell("Result Definition", CellValues.String),
                ConstructCell("Result Definition Publication Status", CellValues.String),
                ConstructCell(isEDT ? "Primary EDT" : "Primary NOW", CellValues.String),
                ConstructCell(isEDT ? "Child EDT" : "Child NOW", CellValues.String),
                ConstructCell("Primary On Child?", CellValues.String),
                ConstructCell(isEDT ? "Primary EDT Subscriptions" : "Primary NOW Subscriptions", CellValues.String),
                ConstructCell(isEDT ? "Child EDT Subscriptions" : "Child NOW Subscriptions", CellValues.String)
                );

            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private class ReportRow
        {
            public Now ChildNOW { get; }

            public ResultDefinition ResultDefinition { get; }

            public Now PrimaryNOW { get; }

            public bool PrimaryOnChild { get; }

            public string PrimarySubscribers { get; }

            public string ChildSubscribers { get; }

            public ReportRow(Now childNOW, ResultDefinition resultDefinition, Now primaryNOW, bool primaryOnChild, string primarySubscribers, string childSubscribers)
            {
                ChildNOW = childNOW;
                ResultDefinition = resultDefinition;
                PrimaryNOW = primaryNOW;
                PrimaryOnChild = primaryOnChild;
                PrimarySubscribers = primarySubscribers;
                ChildSubscribers = childSubscribers;
            }
        }

        private void AppendData()
        {
            //find non primary child now requirements that are primary results for other nows
            var primaryResults = AllData.NowRequirements.Where(x => x.PrimaryResult && x.DeletedDate == null);
            var childResultGroups = AllData.NowRequirements.Where(x => x.DeletedDate == null && x.ParentNowRequirement != null).GroupBy(x=>x.NOWUUID);

            //check each child to see if it is primary for another type of NOW
            var reportRows = new List<ReportRow>();
            foreach (var group in childResultGroups)
            {
                var childNOW = AllData.Nows.FirstOrDefault(x => x.UUID == group.Key);
                var childUsages = NowUsages(childNOW);
                if (childNOW.IsEDT == isEDT)
                {
                    foreach (var childNR in group.ToList())
                    {
                        var primaryElsewhere = primaryResults.Where(x => x.ResultDefinition.UUID == childNR.ResultDefinition.UUID && x.NOWUUID != childNR.NOWUUID).ToList();
                        foreach (var primaryNR in primaryElsewhere)
                        {
                            var primaryNOW = AllData.Nows.FirstOrDefault(x => x.UUID == primaryNR.NOWUUID);
                            
                            if (primaryNOW.IsEDT == isEDT)
                            {
                                var primaryUsages = NowUsages(primaryNOW);
                                reportRows.Add(new ReportRow(childNOW, childNR.ResultDefinition, primaryNOW, childNR.PrimaryResult,primaryUsages, childUsages));
                            }
                        }
                    }
                }
            }
            if (reportRows.Count>0)
            {
                foreach(var reportRow in reportRows.OrderBy(x=>x.PrimaryNOW.Name))
                {
                    AppendRow(reportRow);
                }
            }
        }

        private void AppendRow(ReportRow reportRow)
        {
            var row = new Row();
            row.Append(
                ConstructCell(AsString(reportRow.ResultDefinition.UUID), CellValues.String),
                ConstructCell(reportRow.ResultDefinition.Label, CellValues.String),
                ConstructCell(GetPublishedStatus(reportRow.ResultDefinition).GetDescription(), CellValues.String),
                ConstructCell(reportRow.PrimaryNOW.Name, CellValues.String),
                ConstructCell(reportRow.ChildNOW.Name, CellValues.String),
                ConstructCell(AsBoolString(reportRow.PrimaryOnChild), CellValues.String),
                ConstructCell(reportRow.PrimarySubscribers, CellValues.String),
                ConstructCell(reportRow.ChildSubscribers, CellValues.String)
                );
            SheetData.AppendChild(row);
        }

        private string NowUsages(Now now)
        {
            var subscribers = HearingResults.GetSubscribers(now, AllData.NowSubscriptions.Where(x => x.DeletedDate == null).ToList(), null, null).Select(x=>x.Name).ToList();
            return string.Join(", ", subscribers);
        }
    }
}
