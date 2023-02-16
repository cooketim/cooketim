using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace ExportExcel
{
    public class DataPatchNowSubscriptionBuilder : SheetBuilderBase
    {
        ChangeReportData patchData;
        public DataPatchNowSubscriptionBuilder(WorkbookPart workbookPart, AllData allData, ChangeReportData patchData) : 
            base("Patched Subscriptions", workbookPart, allData) 
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
                ConstructCell("Type", CellValues.String),
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
            foreach (var item in patchData.DeletedNowSubscriptions.OrderBy(x => x.Data.Name))
            {
                AppendNowSubscriptionItem(item, "Deleted");
            }
            foreach (var item in patchData.NewNowSubscriptions.OrderBy(x => x.Data.Name))
            {
                AppendNowSubscriptionItem(item, "Added");
            }
            foreach (var item in patchData.ModifiedNowSubscriptions.OrderBy(x => x.Data.Name))
            {
                AppendNowSubscriptionItem(item, "Modified");
            }
        }

        private void AppendNowSubscriptionItem(ChangeReportNowSubscriptionDataItem item, string action)
        {
            var row = AppendNowSubscription(item, action);
            if (item.Comments.Any())
            {
                row.Append(
                    ConstructCell(item.Comments[0].Note, CellValues.String));
                SheetData.AppendChild(row);
                
                for (var i = 1; i < item.Comments.Count; i++)
                {
                    row = AppendNowSubscription(item, action);
                    row.Append(ConstructCell(item.Comments[i].Note, CellValues.String));
                    SheetData.AppendChild(row);
                }
            }
            else
            {
                SheetData.AppendChild(row);
            }
        }

        private Row AppendNowSubscription(ChangeReportNowSubscriptionDataItem item, string action)
        {
            var row = new Row();
            row.Append(
                ConstructCell(AsString(item.Data.UUID), CellValues.String),
                ConstructCell(item.Data.Name, CellValues.String),
                ConstructCell(GetTypeOfSubscription(item.Data), CellValues.String),
                ConstructCell(action, CellValues.String),
                ConstructDateCell(item.Data.LastModifiedDate),
                ConstructCell(item.Data.LastModifiedUser, CellValues.String)
                );

            return row;
        }

        private string GetTypeOfSubscription(NowSubscription data)
        {
            if (data.IsNow)
            {
                return "NOW";
            }
            if (data.IsEDT)
            {
                return "EDT";
            }
            if (data.IsCourtRegister)
            {
                return "Court Register";
            }
            if (data.IsPrisonCourtRegister)
            {
                return "Prison Court Register";
            }
            if (data.IsInformantRegister)
            {
                return "Informant Register";
            }
            return "Unknown Type";
        }
    }
}
