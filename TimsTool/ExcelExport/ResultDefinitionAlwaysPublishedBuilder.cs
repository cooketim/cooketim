using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;

namespace ExportExcel
{
    public class ResultDefinitionAlwaysPublishedBuilder : SheetBuilderBase
    {
        public ResultDefinitionAlwaysPublishedBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Definitions", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Short code", CellValues.String),
                ConstructCell("Label", CellValues.String),
                ConstructCell("Publication Status", CellValues.String),
                ConstructCell("Parent Results", CellValues.String),
                ConstructCell("Has Children?", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            var reportLines = new List<ResultReportLine>();

            foreach (var rd in AllData.ResultDefinitions.Where(x=>x.DeletedDate == null && x.IsAlwaysPublished).OrderBy(x => x.Label))
            {
                var line = new ResultReportLine() { Result = rd};
                var parentResults = new List<string>();
                reportLines.Add(line);

                var childRules = AllData.ResultDefinitionRules.Where(x => x.DeletedDate == null && x.ChildResultDefinitionUUID == rd.UUID);

                foreach(var childRule in childRules)
                {
                    var matchedParent = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == childRule.ParentUUID && x.DeletedDate == null);
                    if(matchedParent != null)
                    {
                        parentResults.Add(string.Format("{0} ({1})", matchedParent.Label, GetPublishedStatus(matchedParent).GetDescription()));
                    }                    
                }

                if (parentResults.Count()>0)
                {
                    var orderedParentResults = parentResults.OrderBy(x => x).ToList();
                    line.ParentResults = AsStringLines(orderedParentResults);
                }

                if (rd.ResultDefinitionRules != null && rd.ResultDefinitionRules.Where(x=>x.DeletedDate == null).Count()>0)
                {
                    line.HasChildren = true;
                }
            }

            foreach(var line in reportLines)
            {
                var row = new Row();
                row.Append(
                            ConstructCell(AsString(line.Result.UUID), CellValues.String),
                            ConstructCell(line.Result.ShortCode, CellValues.String),
                            ConstructCell(line.Result.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(line.Result).GetDescription(), CellValues.String),
                            ConstructCell(line.ParentResults, CellValues.String),
                            ConstructCell(AsBoolString(line.HasChildren), CellValues.String)
                            );
                SheetData.AppendChild(row);
            }
        }

        private class ResultReportLine
        {
            public ResultDefinition Result { get; set; }
            public string ParentResults { get; set; }
            public bool HasChildren { get; set; }
        }
    }
}
