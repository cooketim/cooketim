using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace ExportExcel
{
    public class JudicialValidationResultDefinitionExportBuilder : SheetBuilderBase
    {
        public JudicialValidationResultDefinitionExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Definitions", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Definition UUID", CellValues.String),
                ConstructCell("Result Definition", CellValues.String),
                ConstructCell("Result Definition Short Code", CellValues.String),
                ConstructCell("Result Definition Level", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            foreach (var rd in AllData.ResultDefinitions.Where(x => x.JudicialValidation && x.DeletedDate == null).OrderBy(x => x.Label))
            {
                var row = new Row();
                row.Append(
                        ConstructCell(AsString(rd.UUID), CellValues.String),
                        ConstructCell(rd.Label, CellValues.String),
                        ConstructCell(rd.ShortCode, CellValues.String),
                        ConstructCell(rd.Level, CellValues.String),
                        ConstructCell(GetPublishedStatus(rd).GetDescription(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
