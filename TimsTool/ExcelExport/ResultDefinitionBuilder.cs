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
    public class ResultDefinitionBuilder : SheetBuilderBase
    {
        public ResultDefinitionBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Definitions", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Short code", CellValues.String),
                ConstructCell("Label", CellValues.String),
                ConstructCell("Welsh Label", CellValues.String),
                ConstructCell("Result Wording", CellValues.String),
                ConstructCell("Welsh Result Wording", CellValues.String),
                ConstructCell("Result Word Group", CellValues.String),
                ConstructCell("Level", CellValues.String),
                ConstructCell("Adjournment", CellValues.String),
                ConstructCell("Category", CellValues.String),
                ConstructCell("Urgent", CellValues.String),
                ConstructCell("Financial", CellValues.String),
                ConstructCell("Convicted", CellValues.String),
                ConstructCell("D20", CellValues.String),
                ConstructCell("Result Definition Group", CellValues.String),
                ConstructCell("DVLA Code", CellValues.String),
                ConstructCell("CJS Code", CellValues.String),
                ConstructCell("Qualifier", CellValues.String),
                ConstructCell("Post Hearing Custody Status", CellValues.String),
                ConstructCell("Jurisdiction (Crown/Mags/Both)", CellValues.String),
                ConstructCell("Rank", CellValues.String),
                ConstructCell("Libra code", CellValues.String),
                ConstructCell("L Code", CellValues.String),
                ConstructCell("Start Date", CellValues.String),
                ConstructCell("End Date", CellValues.String),
                ConstructCell("User Groups", CellValues.String),
                ConstructCell("Is Available For Court Extract", CellValues.String),
                ConstructCell("Is Published As A Prompt", CellValues.String),
                ConstructCell("Is Excluded From Results", CellValues.String),
                ConstructCell("Is Always Published", CellValues.String),
                ConstructCell("Is Life Duration", CellValues.String),
                ConstructCell("Terminates Offence Proceedings", CellValues.String),
                ConstructCell("Created Date", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String),
                ConstructCell("Deleted Date", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            foreach (var resultDefinition in AllData.ResultDefinitions.OrderBy(x => x.Rank))
            {
                AppendResultDefinition(resultDefinition);
            }
        }

        private void AppendResultDefinition(ResultDefinition resultDefinition)
        {
            var row = new Row();
            row.Append(
                ConstructCell(AsString(resultDefinition.UUID), CellValues.String),
                ConstructCell(resultDefinition.ShortCode, CellValues.String),
                ConstructCell(resultDefinition.Label, CellValues.String),
                ConstructCell(resultDefinition.WelshLabel, CellValues.String),
                ConstructCell(resultDefinition.ResultWording, CellValues.String),
                ConstructCell(resultDefinition.WelshResultWording, CellValues.String),
                ConstructCell(AsString(resultDefinition.WordGroups), CellValues.String),
                ConstructCell(resultDefinition.Level, CellValues.String),
                ConstructCell(AsString(resultDefinition.Adjournment), CellValues.Boolean),
                ConstructCell(resultDefinition.Category, CellValues.String),
                ConstructCell(AsString(resultDefinition.Urgent), CellValues.Boolean),
                ConstructCell(AsString(resultDefinition.Financial), CellValues.Boolean),
                ConstructCell(AsString(resultDefinition.Convicted), CellValues.Boolean),
                ConstructCell(AsString(resultDefinition.D20), CellValues.Boolean),
                ConstructCell(resultDefinition.ResultDefinitionGroup, CellValues.String),
                ConstructCell(resultDefinition.DVLACode, CellValues.String),
                ConstructCell(resultDefinition.CJSCode, CellValues.String),
                ConstructCell(resultDefinition.Qualifier, CellValues.String),
                ConstructCell(resultDefinition.PostHearingCustodyStatus, CellValues.String),
                ConstructCell(resultDefinition.Jurisdiction, CellValues.String),
                ConstructIntCell(resultDefinition.Rank),
                ConstructCell(resultDefinition.LibraCode, CellValues.String),
                ConstructCell(resultDefinition.LCode, CellValues.String),
                ConstructDateCell(resultDefinition.StartDate),
                ConstructDateCell(resultDefinition.EndDate),
                ConstructCell(AsString(resultDefinition.UserGroups), CellValues.String),
                ConstructCell(AsString(resultDefinition.IsAvailableForCourtExtract), CellValues.Boolean),
                ConstructCell(AsString(resultDefinition.IsPublishedAsAPrompt), CellValues.Boolean),
                ConstructCell(AsString(resultDefinition.IsExcludedFromResults), CellValues.Boolean),
                ConstructCell(AsString(resultDefinition.IsAlwaysPublished), CellValues.Boolean),
                ConstructCell(AsString(resultDefinition.IsLifeDuration), CellValues.Boolean),
                ConstructCell(AsString(resultDefinition.TerminatesOffenceProceedings), CellValues.Boolean),
                ConstructDateCell(resultDefinition.CreatedDate),
                ConstructDateCell(resultDefinition.LastModifiedDate),
                ConstructDateCell(resultDefinition.DeletedDate),
                ConstructCell(GetPublishedStatus(resultDefinition).GetDescription(), CellValues.String)
                );
            SheetData.AppendChild(row);
        }
    }
}
