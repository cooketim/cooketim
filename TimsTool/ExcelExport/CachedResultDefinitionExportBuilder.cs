using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using System.Linq;

namespace ExportExcel
{
    public class CachedResultDefinitionExportBuilder : SheetBuilderBase
    {
        public CachedResultDefinitionExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Cached Result Definitions", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Definition UUID", CellValues.String),
                ConstructCell("Label", CellValues.String),
                ConstructCell("Short Code", CellValues.String),
                ConstructCell("Cacheable", CellValues.String),
                ConstructCell("Cache Data Path", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            foreach (var rd in AllData.ResultDefinitions.Where(x => x.Cacheable != null && x.Cacheable != CacheableEnum.notCached ).OrderBy(x => x.Label))
            {
                var row = new Row();
                row.Append(
                        ConstructCell(AsString(rd.UUID), CellValues.String),
                        ConstructCell(rd.Label, CellValues.String),
                        ConstructCell(rd.ShortCode, CellValues.String),
                        ConstructCell(rd.Cacheable.GetDescription(), CellValues.String),
                        ConstructCell(rd.CacheDataPath, CellValues.String),
                        ConstructCell(rd.LastModifiedDate.Value.ToString("d MMM yyyy h:mm:ss tt", DateTimeFormatInfo.InvariantInfo), CellValues.String),
                        ConstructCell(GetPublishedStatus(rd).GetDescription(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
