using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace ExportExcel
{
    public class DataPatchNowBuilder : SheetBuilderBase
    {
        ChangeReportData patchData;
        public DataPatchNowBuilder(WorkbookPart workbookPart, AllData allData, ChangeReportData patchData) : 
            base("Patched NOWs & EDTs", workbookPart, allData) 
        {
            this.patchData = patchData;
        }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Name", CellValues.String),
                ConstructCell("NOW/EDT", CellValues.String),
                ConstructCell("Action", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String),
                ConstructCell("Last Modified By", CellValues.String),
                ConstructCell("Change Comments", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            foreach (var item in patchData.DeletedNows.OrderBy(x => x.Data.Name))
            {
                AppendNowItem(item, "Deleted");
            }
            foreach (var item in patchData.NewNows.OrderBy(x => x.Data.Name))
            {
                AppendNowItem(item, "Added");
            }
            foreach (var item in patchData.ModifiedNows.OrderBy(x => x.Data.Name))
            {
                AppendNowItem(item, "Modified");
            }
        }

        private void AppendNowItem(ChangeReportNowDataItem item, string action)
        {
            var row = AppendNow(item, action);
            if (item.Comments.Any())
            {
                row.Append(
                    ConstructCell(item.Comments[0].Note, CellValues.String));
                SheetData.AppendChild(row);
                
                for (var i = 1; i < item.Comments.Count; i++)
                {
                    row = AppendNow(item, action);
                    row.Append(ConstructCell(item.Comments[i].Note, CellValues.String));
                    SheetData.AppendChild(row);
                }
            }
            else
            {
                SheetData.AppendChild(row);
            }
        }

        private Row AppendNow(ChangeReportNowDataItem item, string action)
        {
            var row = new Row();
            row.Append(
                ConstructCell(AsString(item.Data.UUID), CellValues.String),
                ConstructCell(item.Data.Name, CellValues.String),
                ConstructCell(item.Data.IsEDT ? "EDT":"NOW", CellValues.String),
                ConstructCell(action, CellValues.String),
                ConstructDateCell(item.Data.LastModifiedDate),
                ConstructCell(item.Data.LastModifiedUser, CellValues.String)
                );

            return row;
        }
    }
}
