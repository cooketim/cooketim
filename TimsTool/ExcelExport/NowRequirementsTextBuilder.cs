using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace ExportExcel
{
    public class NowRequirementsTextBuilder : SheetBuilderBase
    {
        public NowRequirementsTextBuilder(WorkbookPart workbookPart, AllData allData) : base("NOWs EDTs Requirement Text", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            BuildHeader();

            //Process the data
            ProcessData();
        }

        protected void BuildHeader()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("NOW/EDT UUID (for reference only)", CellValues.String),
                    ConstructCell("NOW/EDT Name (for reference only)", CellValues.String),
                    ConstructCell("NOW/EDT Publication Status (for reference only)", CellValues.String),
                    ConstructCell("Result Definition (for reference only)", CellValues.String),
                    ConstructCell("Result Definition Publication Status (for reference only)", CellValues.String),
                    ConstructCell("NOW/EDT Requirement UUID", CellValues.String),
                    ConstructCell("Parent NOW/EDT Requirement UUID", CellValues.String),
                    ConstructCell("Text Reference", CellValues.String),
                    ConstructCell("NOW/EDT Text", CellValues.String),
                    ConstructCell("Welsh NOW/EDT Text", CellValues.String),
                    ConstructCell("Is EDT", CellValues.String)
                    );
            SheetData.AppendChild(row);
        }

        protected virtual void ProcessData()
        {
            var data = from req in AllData.NowRequirements.Where(x => x.NowRequirementTextList != null && x.NowRequirementTextList.Count > 0) select new { NowRequirement = req, Now = AllData.Nows.Find(x => x.UUID == req.NOWUUID) };
            foreach (var x in data.OrderBy(x => x.Now.Name).ThenBy(x => x.NowRequirement.ParentNowRequirement != null).ThenBy(x => x.NowRequirement.ResultDefinition.Label).ThenBy(x => x.NowRequirement.ResultDefinitionSequence))
            {
                AppendData(x.NowRequirement, x.Now);
            }
        }

        protected void AppendData(NowRequirement nowRequirement, Now now)
        {
            foreach (var nowText in nowRequirement.NowRequirementTextList)
            {
                var row = new Row();
                row.Append(
                        ConstructCell(AsString(nowRequirement.NOWUUID), CellValues.String),
                        ConstructCell(now.Name, CellValues.String),
                        ConstructCell(GetPublishedStatus(now).GetDescription(), CellValues.String),
                        ConstructCell(nowRequirement.ResultDefinition.Label, CellValues.String),
                        ConstructCell(GetPublishedStatus(nowRequirement.ResultDefinition).GetDescription(), CellValues.String),
                        ConstructCell(AsString(nowRequirement.UUID), CellValues.String),
                        ConstructCell(AsString(nowRequirement.ParentNowRequirementUUID), CellValues.String),
                        ConstructCell(nowText.NowReference, CellValues.String),
                        ConstructCell(nowText.NowText, CellValues.String),
                        ConstructCell(nowText.NowWelshText, CellValues.String),
                        ConstructCell(AsBoolString(now.IsEDT), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
