using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using System.Text;
using DataLib;

namespace ExportExcel
{
    public class MissingSharingRulesResultDefinitionExportBuilder : SheetBuilderBase
    {
        public MissingSharingRulesResultDefinitionExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Definitions", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Definition UUID", CellValues.String),
                ConstructCell("Result Definition", CellValues.String),
                ConstructCell("Result Definition Short Code", CellValues.String),
                ConstructCell("Parent Results", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            foreach (var rd in AllData.ResultDefinitions.Where(x => x.DeletedDate == null && !x.IsRollupPrompts && !x.IsPublishedAsAPrompt && !x.IsAlwaysPublished && !x.IsExcludedFromResults).OrderBy(x => x.Label))
            {
                //see if this result is a child of any parent
                var sb = new StringBuilder();
                foreach (var parent in AllData.ResultDefinitions.Where(x=> x.DeletedDate == null && x.ResultDefinitionRules != null && x.ResultDefinitionRules.Any() && x.ResultDefinitionRules.Where(y=>y.DeletedDate == null).Select(y=>y.ChildResultDefinitionUUID).Contains(rd.UUID)).OrderBy(x=>x.Label))
                {
                    sb.AppendLine(string.Format("{0} ({1})", parent.Label, GetPublishedStatus(rd).GetDescription()));
                }
                if (sb.Length > 0)
                {
                    var row = new Row();
                    row.Append(
                            ConstructCell(AsString(rd.UUID), CellValues.String),
                            ConstructCell(rd.Label, CellValues.String),
                            ConstructCell(rd.ShortCode, CellValues.String),
                            ConstructCell(sb.ToString(), CellValues.String),
                            ConstructCell(GetPublishedStatus(rd).GetDescription(), CellValues.String)
                        );
                    SheetData.AppendChild(row);
                }
            }
        }
    }
}
