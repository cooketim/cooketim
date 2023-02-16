using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace ExportExcel
{
    public class NowSubscriptionIncludedExcludedBuilder : SheetBuilderBase
    {
        bool isIncluded = false;
        public NowSubscriptionIncludedExcludedBuilder(WorkbookPart workbookPart, AllData allData, bool isIncluded) : base(string.Format("Subscriptions - {0} Nows",isIncluded ? "Included":"Excluded"), workbookPart, allData) 
        {
            this.isIncluded = isIncluded;
        }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();

            row.Append(
                ConstructCell("Subscription UUID", CellValues.String),
                ConstructCell("Subscription Name", CellValues.String),
                ConstructCell("Subscription Publication Status", CellValues.String),
                ConstructCell("NOW UUID", CellValues.String),
                ConstructCell("NOW Name", CellValues.String),
                ConstructCell("NOW Publication Status", CellValues.String)
                );

            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            if (isIncluded)
            {
                foreach (var nowSubscription in AllData.NowSubscriptions.Where(x => x.IncludedNOWS != null && x.IncludedNOWS.Count > 0).OrderBy(x => x.Name))
                {
                    foreach (var now in nowSubscription.IncludedNOWS)
                    {
                        var row = new Row();
                        row.Append(
                            ConstructCell(AsString(nowSubscription.UUID), CellValues.String),
                            ConstructCell(nowSubscription.Name, CellValues.String),
                            ConstructCell(GetPublishedStatus(nowSubscription).GetDescription(), CellValues.String),
                            ConstructCell(AsString(now.UUID), CellValues.String),
                            ConstructCell(now.Name, CellValues.String),
                            ConstructCell(GetPublishedStatus(now).GetDescription(), CellValues.String)
                            );

                        SheetData.AppendChild(row);
                    }
                }
            }
            else
            {
                foreach (var nowSubscription in AllData.NowSubscriptions.Where(x => x.ExcludedNOWS != null && x.ExcludedNOWS.Count > 0).OrderBy(x => x.Name))
                {
                    foreach (var now in nowSubscription.ExcludedNOWS)
                    {
                        var row = new Row();
                        row.Append(
                            ConstructCell(AsString(nowSubscription.UUID), CellValues.String),
                            ConstructCell(nowSubscription.Name, CellValues.String),
                            ConstructCell(GetPublishedStatus(nowSubscription).GetDescription(), CellValues.String),
                            ConstructCell(AsString(now.UUID), CellValues.String),
                            ConstructCell(now.Name, CellValues.String),
                            ConstructCell(GetPublishedStatus(now).GetDescription(), CellValues.String)
                            );

                        SheetData.AppendChild(row);
                    }
                }
            }
        }
    }
}
