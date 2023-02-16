using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace ExportExcel
{
    public class DataPatchSelectedOptionBuilder : SheetBuilderBase
    {
        ChangeReportData patchData;
        public DataPatchSelectedOptionBuilder(WorkbookPart workbookPart, AllData allData, ChangeReportData patchData) : 
            base("Audit", workbookPart, allData) 
        {
            this.patchData = patchData;
        }

        protected override void BuildContent()
        {
            Row row = null;
            //Construct 3 empty rows
            for (var i=0; i<3; i++)
            {
                row = new Row();
                SheetData.AppendChild(row);
            }

            //Construct row 1
            row = new Row();
            row.Append(
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell("Test Patch", CellValues.String),
                ConstructCell(AsBoolString(patchData.IsTestExport), CellValues.String)
                );
            SheetData.AppendChild(row);

            //Construct row 3
            row = new Row();
            row.Append(
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell("Prod Patch", CellValues.String),
                ConstructCell(AsBoolString(patchData.IsProdExport), CellValues.String)
                );
            SheetData.AppendChild(row);

            //Construct row 4
            row = new Row();
            row.Append(
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell("Patch Date", CellValues.String),
                ConstructCell(AsDateString(patchData.PatchDate), CellValues.String)
                );
            SheetData.AppendChild(row);

            //Construct row 5
            row = new Row();
            row.Append(
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell("Toggled Features", CellValues.String),
                ConstructCell(AsString(patchData.FeatureToggles), CellValues.String)
                );
            SheetData.AppendChild(row);

            //Construct row 6
            row = new Row();
            row.Append(
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell("Selected Tags", CellValues.String),
                ConstructCell(AsString(patchData.PublicationTags), CellValues.String)
                );
            SheetData.AppendChild(row);

            //Construct row 7
            row = new Row();
            row.Append(
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell("Created Date Time", CellValues.String),
                ConstructCell(AsString(patchData.CreationDateTime), CellValues.String)
                );
            SheetData.AppendChild(row);

            //Construct row 8
            row = new Row();
            row.Append(
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell("Created By", CellValues.String),
                ConstructCell(patchData.CreatedBy, CellValues.String)
                );
            SheetData.AppendChild(row);
        }
    }
}
