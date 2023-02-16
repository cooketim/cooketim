using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;

namespace ExportExcel
{
    public class PublishAsPromptResultsBuilder : SheetBuilderBase
    {
        public PublishAsPromptResultsBuilder(WorkbookPart workbookPart, AllData allData) : base("Results Published As A Prompt", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Short code", CellValues.String),
                ConstructCell("Label", CellValues.String),
                ConstructCell("Publication Status", CellValues.String),
                ConstructCell("Parent Results", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            var reportLines = new List<ResultReportLine>();
            foreach (var rd in AllData.ResultDefinitions.Where(x=>x.DeletedDate == null && x.IsPublishedAsAPrompt))
            {
                
                var line = new ResultReportLine() { Result = rd};
                reportLines.Add(line);
                var parentRules = AllData.ResultDefinitionRules.Where(x => x.ChildResultDefinitionUUID == rd.UUID);
                var parentResults = new List<string>();
                foreach(var rdr in parentRules)
                {
                    var parentRd = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == rdr.ParentUUID);
                    parentResults.Add(string.Format("{0} : {1} ({2})", parentRd.ShortCode, parentRd.Label, GetPublishedStatus(parentRd).GetDescription()));
                }
                

                line.ParentResults = parentResults.OrderBy(x => x).ToList();
            }

            foreach (var line in reportLines.OrderBy(x=>x.Result.Label))
            {
                var row = new Row();
                row.Append(
                            ConstructCell(AsString(line.Result.UUID), CellValues.String),
                            ConstructCell(line.Result.ShortCode, CellValues.String),
                            ConstructCell(line.Result.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(line.Result).GetDescription(), CellValues.String),
                            ConstructCell(AsStringLines(line.ParentResults), CellValues.String)
                            );
                SheetData.AppendChild(row);
            }
        }

        private class ResultReportLine
        {
            public ResultDefinition Result { get; set; }
            public List<string> ParentResults { get; set; }
        }
    }
}
