using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExportExcel
{
    public class NowRequirementsBuilder : SheetBuilderBase
    {
        public NowRequirementsBuilder(WorkbookPart workbookPart, AllData allData) : base("NOW Requirements", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("NOW UUID (for reference only)", CellValues.String),
                    ConstructCell("NOW Name (for reference only)", CellValues.String),
                    ConstructCell("NOW Publication Status (for reference only)", CellValues.String),
                    ConstructCell("Result Definition UUID (for reference only)", CellValues.String),
                    ConstructCell("Result Definition (for reference only)", CellValues.String),
                    ConstructCell("Result Definition Publication Status (for reference only)", CellValues.String),
                    ConstructCell("Now Requirement UUID", CellValues.String),
                    ConstructCell("Parent Now Requirement UUID", CellValues.String),
                    ConstructCell("Mandatory", CellValues.String),
                    ConstructCell("Primary Result", CellValues.String),
                    ConstructCell("Result Definition Sequence", CellValues.String),
                    ConstructCell("Created Date", CellValues.String),
                    ConstructCell("Last Modified Date", CellValues.String),
                    ConstructCell("Deleted Date", CellValues.String)
                    );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            var data = from req in AllData.NowRequirements select new { NowRequirement = req, Now = AllData.Nows.Find(x => x.UUID == req.NOWUUID) };
            foreach (var x in data.OrderBy(x=>x.Now.Name).ThenBy(x => x.NowRequirement.ParentNowRequirement != null).ThenBy(x=>x.NowRequirement.ResultDefinition.Label).ThenBy(x=>x.NowRequirement.ResultDefinitionSequence))
            {
                var row = new Row();
                row.Append(
                        ConstructCell(AsString(x.NowRequirement.NOWUUID), CellValues.String),
                        ConstructCell(x.Now.Name, CellValues.String),
                        ConstructCell(GetPublishedStatus(x.Now).GetDescription(), CellValues.String),
                        ConstructCell(AsString(x.NowRequirement.ResultDefinition.UUID), CellValues.String),
                        ConstructCell(x.NowRequirement.ResultDefinition.Label, CellValues.String),
                        ConstructCell(GetPublishedStatus(x.NowRequirement.ResultDefinition).GetDescription(), CellValues.String),
                        ConstructCell(AsString(x.NowRequirement.UUID), CellValues.String),
                        ConstructCell(AsString(x.NowRequirement.ParentNowRequirementUUID), CellValues.String),
                        ConstructCell(AsString(x.NowRequirement.Mandatory), CellValues.Boolean),
                        ConstructCell(AsString(x.NowRequirement.PrimaryResult), CellValues.Boolean),
                        ConstructIntCell(x.NowRequirement.ResultDefinitionSequence),
                        ConstructDateCell(x.NowRequirement.CreatedDate),
                        ConstructDateCell(x.NowRequirement.LastModifiedDate),
                        ConstructDateCell(x.NowRequirement.DeletedDate)
                        );
                SheetData.AppendChild(row);
            }
        }
    }
}
