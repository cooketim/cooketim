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
    public class ResultPromptBuilder : SheetBuilderBase
    {
        public ResultPromptBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Prompt", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Label", CellValues.String),
                ConstructCell("Welsh Label", CellValues.String),
                ConstructCell("Wording", CellValues.String),
                ConstructCell("Welsh Wording", CellValues.String),
                ConstructCell("Prompt Type", CellValues.String),
                ConstructCell("Duration Element", CellValues.String),
                ConstructCell("Welsh Duration Element", CellValues.String),
                ConstructCell("Duration Sequence", CellValues.String),
                ConstructCell("Fixed List UUID", CellValues.String),
                ConstructCell("Fixed List Label", CellValues.String),
                ConstructCell("Min", CellValues.String),
                ConstructCell("Max", CellValues.String),
                ConstructCell("Qual", CellValues.String),
                ConstructCell("Result Prompt Group", CellValues.String),
                ConstructCell("Result Prompt Word Group", CellValues.String),
                ConstructCell("Jurisdiction", CellValues.String),
                ConstructCell("Prompt Reference", CellValues.String),
                ConstructCell("Welsh Label", CellValues.String),
                ConstructCell("Court Extract Y/N CC", CellValues.String),
                ConstructCell("Financial", CellValues.String),
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
            //get a distinct set of result prompts 
            var prompts = AllData.ResultPromptRules.Select(x => x.ResultPrompt).OrderBy(x=>x.Label).Distinct();

            foreach (var prompt in prompts)
            {
                var row = new Row();
                List<string> durationElements = prompt.DurationElements == null ? null : prompt.DurationElements.Select(x => x.DurationElement).ToList();
                List<string> welshDurationElements = prompt.DurationElements == null ? null : prompt.DurationElements.Select(x => x.WelshDurationElement == null ? " " : x.WelshDurationElement).ToList();
                row.Append(
                    ConstructCell(AsString(prompt.UUID), CellValues.String),
                    ConstructCell(prompt.Label, CellValues.String),
                    ConstructCell(prompt.WelshLabel, CellValues.String),
                    ConstructCell(prompt.Wording, CellValues.String),
                    ConstructCell(prompt.WelshWording, CellValues.String),
                    ConstructCell(prompt.PromptType, CellValues.String),
                    ConstructCell(AsString(durationElements), CellValues.String),
                    ConstructCell(AsString(welshDurationElements), CellValues.String),
                    ConstructIntCell(prompt.DurationSequence),
                    ConstructCell(prompt.FixedList == null ? string.Empty : AsString(prompt.FixedList.UUID), CellValues.String),
                    ConstructCell(prompt.FixedList == null ? string.Empty : prompt.FixedList.Label, CellValues.String),
                    ConstructCell(prompt.Min, CellValues.String),
                    ConstructCell(prompt.Max, CellValues.String),
                    ConstructCell(prompt.Qual, CellValues.String),
                    ConstructCell(prompt.ResultPromptGroup == null ? string.Empty : prompt.ResultPromptGroup, CellValues.String),
                    ConstructCell(AsString(prompt.ResultPromptWordGroups), CellValues.String),
                    ConstructCell(prompt.Jurisdiction, CellValues.String),
                    ConstructCell(prompt.PromptReference, CellValues.String),
                    ConstructCell(prompt.WelshLabel, CellValues.String),
                    ConstructCell(AsString(prompt.IsAvailableForCourtExtract), CellValues.Boolean),
                    ConstructCell(AsString(prompt.Financial), CellValues.Boolean),
                    ConstructDateCell(prompt.CreatedDate),
                    ConstructDateCell(prompt.LastModifiedDate),
                    ConstructDateCell(prompt.DeletedDate),
                    ConstructCell(GetPublishedStatus(prompt).GetDescription(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
