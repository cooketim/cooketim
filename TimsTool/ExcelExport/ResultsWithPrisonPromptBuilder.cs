using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;

namespace ExportExcel
{
    public class ResultsWithPrisonPromptBuilder : SheetBuilderBase
    {
        public ResultsWithPrisonPromptBuilder(WorkbookPart workbookPart, AllData allData) : base("Custodial Results", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("UUID", CellValues.String),
                    ConstructCell("Short code", CellValues.String),
                    ConstructCell("Label", CellValues.String),
                    ConstructCell("Publication Status", CellValues.String),
                    ConstructCell("Other Prompts", CellValues.String)
                    );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            var reportLines = new List<ResultReportLine>();
            foreach (var rpr in AllData.ResultPromptRules.Where(x=>x.DeletedDate == null && x.ResultPrompt.Label == "Prison"))
            {
                var rd = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == rpr.ResultDefinitionUUID);
                if (rd.DeletedDate != null) { continue; }

                var line = new ResultReportLine() { Result = rd };
                reportLines.Add(line);
                line.OtherPrompts = AsStringLines(new List<string>(AllData.ResultPromptRules.Where(x => x.DeletedDate == null
                                                                    && x.ResultPromptUUID != rpr.ResultPromptUUID
                                                                    && x.ResultDefinitionUUID == rd.UUID)
                                                                    .OrderBy(x => x.ResultPrompt.Label)
                                                                    .Select(x => string.Format("{0} ({1})", x.ResultPrompt.Label, GetPublishedStatus(x.ResultPrompt).GetDescription()))));

            }

            foreach (var line in reportLines.OrderBy(x=>x.Result.Label))
            {
                var row = new Row();
                row.Append(
                            ConstructCell(AsString(line.Result.UUID), CellValues.String),
                            ConstructCell(line.Result.ShortCode, CellValues.String),
                            ConstructCell(line.Result.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(line.Result).GetDescription(), CellValues.String),
                            ConstructCell(line.OtherPrompts, CellValues.String)
                            );
                SheetData.AppendChild(row);
            }
        }

        private class ResultReportLine
        {
            public ResultDefinition Result { get; set; }
            public string OtherPrompts { get; set; }
        }
    }
}
