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
    public class CourtExtractExportBuilder : SheetBuilderBase
    {
        public CourtExtractExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Court Extract Availability", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Definition UUID", CellValues.String),
                ConstructCell("Result Definition", CellValues.String),
                ConstructCell("Result Definition Publication Status", CellValues.String),
                ConstructCell("Is Available For Court Extract", CellValues.String),
                ConstructCell("Result Prompt UUID", CellValues.String),
                ConstructCell("Result Prompt", CellValues.String),
                ConstructCell("Is Available For Court Extract", CellValues.String),
                ConstructCell("Other Prompt Usages", CellValues.String),
                ConstructCell("Prompt Publication Status", CellValues.String));
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            foreach (var rd in AllData.ResultDefinitions.Where(x=>x.DeletedDate == null).OrderBy(x=>x.Label))
            {
                if (rd.ResultPrompts == null || rd.ResultPrompts.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null).Count() == 0)
                {
                    var row = new Row();
                    AppendResultDefinition(row, rd);
                    SheetData.AppendChild(row);
                    continue;
                }

                foreach (var prompt in rd.ResultPrompts.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null).OrderBy(x => x.ResultPrompt.Label))
                {
                    var row = new Row();
                    AppendResultPrompt(row, prompt.ResultPrompt, rd);
                    SheetData.AppendChild(row);
                }
            }
        }

        private void AppendResultPrompt(Row row, ResultPrompt rp, ResultDefinition rd)
        {
            var usagesReport = PromptUsagesReport(rp.UUID, rd.UUID);

            row.Append(
                ConstructCell(AsString(rd.UUID), CellValues.String),
                ConstructCell(rd.Label, CellValues.String),
                ConstructCell(GetPublishedStatus(rd).GetDescription(), CellValues.String),
                ConstructCell(AsBoolString(rd.IsAvailableForCourtExtract), CellValues.String),
                ConstructCell(AsString(rp.UUID), CellValues.String),
                ConstructCell(rp.Label, CellValues.String),
                ConstructCell(AsBoolString(rp.IsAvailableForCourtExtract), CellValues.String),
                ConstructCell(usagesReport, CellValues.String),
                ConstructCell(GetPublishedStatus(rp).GetDescription(), CellValues.String));
        }

        private void AppendResultDefinition(Row row, ResultDefinition rd)
        {
            row.Append(
                        ConstructCell(AsString(rd.UUID), CellValues.String),
                        ConstructCell(GetPublishedStatus(rd).GetDescription(), CellValues.String),
                        ConstructCell(rd.Label, CellValues.String),
                        ConstructCell(AsBoolString(rd.IsAvailableForCourtExtract), CellValues.String)
                        );
        }

        private string PromptUsagesReport(Guid? resultPromptId, Guid? ResultDefinitionId)
        {
            var parentRDs = AllData.ResultPromptRules.Where(x => x.ResultPromptUUID == resultPromptId && x.DeletedDate == null && x.ResultDefinitionUUID != ResultDefinitionId).Select(x => x.ResultDefinitionUUID).Distinct();
            var sb = new StringBuilder();
            foreach (var id in parentRDs)
            {
                var rd = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == id);
                if (rd != null)
                {
                    sb.AppendLine(String.Format("{0} ({1})", rd.Label, GetPublishedStatus(rd).GetDescription()));
                }
            }
            return sb.ToString();
        }

    }
}
