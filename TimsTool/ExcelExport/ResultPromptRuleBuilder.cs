using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Linq;

namespace ExportExcel
{
    public class ResultPromptRuleBuilder : SheetBuilderBase
    {
        public ResultPromptRuleBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Prompt Rule", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("Result Definition UUID", CellValues.String),
                    ConstructCell("Result Definition (for information only)", CellValues.String),
                    ConstructCell("Result Definition Publication Status (for information only)", CellValues.String),
                    ConstructCell("Result Prompt UUID", CellValues.String),
                    ConstructCell("Result Prompt (for information only)", CellValues.String),
                    ConstructCell("Result Prompt Publication Status (for information only)", CellValues.String),
                    ConstructCell("Rule Type", CellValues.String),
                    ConstructCell("Prompt Sequence", CellValues.String),
                    ConstructCell("User Groups", CellValues.String),
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
            //order the rules based on the parent result definition and the prompt sequence
            var rules = AllData.ResultPromptRules.Select(x => x).OrderBy(x=>x.ResultDefinitionUUID).ThenBy(x=>x.PromptSequence);

            foreach (var rule in rules)
            {
                var resultDefinition = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == rule.ResultDefinitionUUID);
                var row = new Row();
                row.Append(
                        ConstructCell(AsString(rule.ResultDefinitionUUID), CellValues.String),
                        ConstructCell(resultDefinition.Label, CellValues.String),
                        ConstructCell(GetPublishedStatus(resultDefinition).GetDescription(), CellValues.String),
                        ConstructCell(AsString(rule.ResultPrompt.UUID), CellValues.String),
                        ConstructCell(rule.ResultPrompt.Label, CellValues.String),
                        ConstructCell(GetPublishedStatus(rule.ResultPrompt).GetDescription(), CellValues.String),
                        ConstructCell(Enum.GetName(typeof(ResultPromptRuleType), rule.Rule), CellValues.String),
                        ConstructIntCell(rule.PromptSequence),
                        ConstructCell(AsString(rule.UserGroups), CellValues.String),
                        ConstructDateCell(rule.CreatedDate),
                        ConstructDateCell(rule.LastModifiedDate),
                        ConstructDateCell(rule.DeletedDate)
                        );
                SheetData.AppendChild(row);
            }
        }
    }
}
