using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportExcel
{
    public class NowPrimaryResultReportBuilder : SheetBuilderBase
    {
        public NowPrimaryResultReportBuilder(WorkbookPart workbookPart, AllData allData) : base("NOW Subscriptions", workbookPart, allData) {}

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("NOW Name", CellValues.String),
                ConstructCell("Triggering Results", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            //find the subscriptions for the given now
            var nows = AllData.Nows.Where(x => x.DeletedDate == null && !x.IsEDT).OrderBy(x => x.Name);

            //check each now to find the primary results
            foreach (var now in nows)
            {
                //get the triggering results
                var primaryNowRequirements = AllData.NowRequirements.Where(x => x.DeletedDate == null && x.PrimaryResult && x.NOWUUID == now.UUID && x.ResultDefinition != null).ToList();

                var orderedPrimaryRequirements = primaryNowRequirements.OrderBy(x=>x.ResultDefinition.Label).GroupBy(x => x.ResultDefinitionUUID).Select(x => x.First());

                var sb = new StringBuilder();
                foreach(var nr in orderedPrimaryRequirements)
                {
                    sb.AppendLine(string.Format("{0} ({1}) - {2}", nr.ResultDefinition.Label, nr.ResultDefinition.ShortCode, GetPublishedStatus(nr.ResultDefinition).GetDescription()));
                }

                //append a new row
                var row = new Row();
                row.Append(
                    ConstructCell(now.Name, CellValues.String),
                    ConstructCell(sb.ToString(), CellValues.String),
                    ConstructCell(GetPublishedStatus(now).GetDescription(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
