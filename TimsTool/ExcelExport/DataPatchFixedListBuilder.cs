using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace ExportExcel
{
    public class DataPatchFixedListBuilder : SheetBuilderBase
    {
        ChangeReportData patchData;
        public DataPatchFixedListBuilder(WorkbookPart workbookPart, AllData allData, ChangeReportData patchData) : 
            base("Patched Fixed Lists", workbookPart, allData) 
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
            foreach (var item in patchData.DeletedFixedLists.OrderBy(x => x.Data.Label))
            {
                AppendFixedListItem(item, "Deleted");
            }
            foreach (var item in patchData.NewFixedLists.OrderBy(x => x.Data.Label))
            {
                AppendFixedListItem(item, "Added");
            }
            foreach (var item in patchData.ModifiedFixedLists.OrderBy(x => x.Data.Label))
            {
                AppendFixedListItem(item, "Modified");
            }
        }

        private void AppendFixedListItem(ChangeReportFixedListDataItem item, string action)
        {
            var row = AppendFixedList(item, action);
            if (item.Comments.Any())
            {
                row.Append(
                    ConstructCell(item.Comments[0].Note, CellValues.String));
                SheetData.AppendChild(row);
                
                for (var i = 1; i < item.Comments.Count; i++)
                {
                    row = AppendFixedList(item, action);
                    row.Append(ConstructCell(item.Comments[i].Note, CellValues.String));
                    SheetData.AppendChild(row);
                }
            }
            else
            {
                SheetData.AppendChild(row);
            }
        }

        private Row AppendFixedList(ChangeReportFixedListDataItem item, string action)
        {
            var row = new Row();
            row.Append(
                ConstructCell(AsString(item.Data.UUID), CellValues.String),
                ConstructCell(item.Data.Label, CellValues.String),
                ConstructCell(action, CellValues.String),
                ConstructDateCell(item.Data.LastModifiedDate),
                ConstructCell(item.Data.LastModifiedUser, CellValues.String)
                );

            return row;
        }
    }
}
