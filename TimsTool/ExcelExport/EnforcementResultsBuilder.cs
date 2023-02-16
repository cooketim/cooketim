using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;

namespace ExportExcel
{
    public class EnforcementResultsBuilder : SheetBuilderBase
    {
        public EnforcementResultsBuilder(WorkbookPart workbookPart, AllData allData) : base("Enforcement Results", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Code", CellValues.String),
                ConstructCell("Result Label", CellValues.String),
                ConstructCell("Result Definition Group", CellValues.String),
                ConstructCell("Created Date", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String),
                ConstructCell("Deleted Date", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
            );
            SheetData.AppendChild(row);

            //Process the data
            ProcessData();
        }

        protected virtual void ProcessData()
        {
            foreach (var rd in AllData.ResultDefinitions
                            .Where(x => !string.IsNullOrEmpty(x.ResultDefinitionGroup) && x.ResultDefinitionGroup.ToLowerInvariant().Contains("enforcement"))
                            .OrderBy(x => x.Label))
            {
                AppendData(rd);
            }
        }

        protected void AppendData(ResultDefinition resultDefinition)
        {
            var row = new Row();
            row.Append(
                ConstructCell(AsString(resultDefinition.UUID), CellValues.String),
                ConstructCell(resultDefinition.Label, CellValues.String),
                ConstructDateCell(resultDefinition.CreatedDate),
                ConstructDateCell(resultDefinition.LastModifiedDate),
                ConstructDateCell(resultDefinition.DeletedDate),
                ConstructCell(GetPublishedStatus(resultDefinition).GetDescription(), CellValues.String)
            );

            SheetData.AppendChild(row);
        }
    }
}
