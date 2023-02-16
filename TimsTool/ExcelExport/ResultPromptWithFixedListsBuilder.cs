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
    public class ResultPromptWithFixedListsBuilder : SheetBuilderBase
    {
        public ResultPromptWithFixedListsBuilder(WorkbookPart workbookPart, AllData allData) : base("FIXL Result Prompt", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("UUID", CellValues.String),
                    ConstructCell("Label", CellValues.String),
                    ConstructCell("Publication Status", CellValues.String),
                    ConstructCell("Usages", CellValues.String),
                    ConstructCell("FIXL UUID", CellValues.String),
                    ConstructCell("FIXL Label", CellValues.String)
                    );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            //get a distinct set of fixL result prompts 
            var prompts = AllData.ResultPromptRules.Where(x=> x.DeletedDate == null && x.ResultPrompt.DeletedDate == null && x.ResultPrompt.PromptType.StartsWith("FIXL")).Select(x => x.ResultPrompt).OrderBy(x=>x.Label).Distinct();

            foreach (var prompt in prompts)
            {
                var row = new Row();
                List<string> durationElements = prompt.DurationElements == null ? null : prompt.DurationElements.Select(x => x.DurationElement).ToList();
                List<string> welshDurationElements = prompt.DurationElements == null ? null : prompt.DurationElements.Select(x => x.WelshDurationElement == null ? " " : x.WelshDurationElement).ToList();

                row.Append(
                       ConstructCell(AsString(prompt.UUID), CellValues.String),
                       ConstructCell(prompt.Label, CellValues.String),
                       ConstructCell(GetPublishedStatus(prompt).GetDescription(), CellValues.String),
                       ConstructCell(PromptUsagesReport(prompt.UUID), CellValues.String),
                       ConstructCell(AsString(prompt.FixedListUUID), CellValues.String),
                       ConstructCell(prompt.FixedListLabel, CellValues.String)
                       );
                SheetData.AppendChild(row);
            }
        }

        private string PromptUsagesReport(Guid? resultPromptId)
        {
            var parentRDs = AllData.ResultPromptRules.Where(x => x.ResultPromptUUID == resultPromptId && x.DeletedDate == null).Select(x => x.ResultDefinitionUUID).Distinct().ToList();
            var sb = new System.Text.StringBuilder();

            var usages = from rd in AllData.ResultDefinitions
                         join id in parentRDs on rd.UUID equals id
                         orderby rd.Label ascending
                         select new { rd = rd };

            foreach (var item in usages)
            {
                sb.AppendLine(string.Format("{0} ({1})", item.rd.Label, GetPublishedStatus(item.rd).GetDescription()));
            }
            return sb.ToString();
        }
    }
}
