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
using System.Text;

namespace ExportExcel
{
    public class RestrictedResultsBuilder : SheetBuilderBase
    {
        private ReportHelper reportHelper;
        public RestrictedResultsBuilder(WorkbookPart workbookPart, AllData allData) : base("Restricted Results", workbookPart, allData) 
        {
            reportHelper = new ReportHelper(allData);
        }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("Result Definition UUID", CellValues.String),
                    ConstructCell("Result Definition", CellValues.String),
                    ConstructCell("Result Definition Publication Status", CellValues.String),
                    ConstructCell("Restricted User Groups For Result", CellValues.String),
                    ConstructCell("Result Usages", CellValues.String),
                    ConstructCell("Result Prompt UUID", CellValues.String),
                    ConstructCell("Result Prompt", CellValues.String),
                    ConstructCell("Result Prompt Publication Status", CellValues.String),
                    ConstructCell("Restricted User Groups For Prompt", CellValues.String),
                    ConstructCell("Result Prompt Usages", CellValues.String)
                    );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            foreach (var rd in AllData.ResultDefinitions.Where(x => x.DeletedDate == null).OrderBy(x => x.Label))
            {
                if (rd.ResultPrompts == null || rd.ResultPrompts.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null).Count() == 0)
                {
                    if (rd.UserGroups != null && rd.UserGroups.Count > 0)
                    {
                        var row = new Row();
                        AppendResultDefinition(row, rd);
                        SheetData.AppendChild(row);
                    }
                    continue;
                }

                var restrictedResult = rd.UserGroups != null && rd.UserGroups.Count > 0;

                var anyRestrictedPrompts = rd.ResultPrompts.FirstOrDefault(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null && x.UserGroups != null && x.UserGroups.Count > 0) != null;

                if (restrictedResult || anyRestrictedPrompts)
                {
                    bool withResultUsages = true;
                    foreach (var prompt in rd.ResultPrompts.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null).OrderBy(x => x.ResultPrompt.Label))
                    {
                        var row = new Row();
                        AppendResultPrompt(row, prompt, rd, withResultUsages);
                        SheetData.AppendChild(row);
                        withResultUsages = false;
                    }
                }
            }
        }

        private void AppendResultPrompt(Row row, ResultPromptRule rpr, ResultDefinition rd, bool withResultUsages)
        {
            var promptUsagesReport = string.Empty;
            if (rpr.UserGroups != null && rpr.UserGroups.Count>0)
            {
                var report = reportHelper.PromptUsagesWithFlagReport(rpr.ResultPromptUUID);
                promptUsagesReport = report.Item1 ? report.Item2 : string.Empty;
            }
            var resultUsagesReport = withResultUsages ? reportHelper.ResultUsagesReport(rd.UUID) : string.Empty;

            row.Append(
                            ConstructCell(AsString(rd.UUID), CellValues.String),
                            ConstructCell(rd.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(rd).GetDescription(), CellValues.String),
                            ConstructCell(AsString(rd.UserGroups), CellValues.String),
                            ConstructCell(resultUsagesReport, CellValues.String),
                            ConstructCell(AsString(rpr.ResultPromptUUID), CellValues.String),
                            ConstructCell(rpr.ResultPrompt.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(rpr.ResultPrompt).GetDescription(), CellValues.String),
                            ConstructCell(AsString(rpr.UserGroups), CellValues.String),
                            ConstructCell(promptUsagesReport, CellValues.String)
                        );
        }

        private void AppendResultDefinition(Row row, ResultDefinition rd)
        {
            var resultUsagesReport = string.Empty;
            if (rd.UserGroups != null && rd.UserGroups.Count > 0)
            {
                resultUsagesReport = reportHelper.ResultUsagesReport(rd.UUID);
            }

            row.Append(
                        ConstructCell(AsString(rd.UUID), CellValues.String),
                        ConstructCell(rd.Label, CellValues.String),
                        ConstructCell(GetPublishedStatus(rd).GetDescription(), CellValues.String),
                        ConstructCell(AsString(rd.UserGroups), CellValues.String),
                        ConstructCell(resultUsagesReport, CellValues.String)
                    );
        }
    }
}
