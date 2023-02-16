using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using DataLib;

namespace ExportExcel
{
    public class JudicialValidationResultPromptExportBuilder : SheetBuilderBase
    {
        public JudicialValidationResultPromptExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Prompts", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Prompt UUID", CellValues.String),
                ConstructCell("Result Prompt", CellValues.String),
                ConstructCell("Result Prompt Type", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );

            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            foreach (var prompt in AllData.ResultPromptRules.Where(x => x.ResultPrompt.JudicialValidation && x.ResultPrompt.DeletedDate == null).GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt).OrderBy(x => x.Label))
            {
                var row = new Row();
                row.Append(
                        ConstructCell(AsString(prompt.UUID), CellValues.String),
                        ConstructCell(prompt.Label, CellValues.String),
                        ConstructCell(prompt.PromptType, CellValues.String),
                        ConstructCell(GetPublishedStatus(prompt).GetDescription(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
