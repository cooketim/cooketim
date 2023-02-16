using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace ExportExcel
{
    public class DataPatchResultDefinitionBuilder : SheetBuilderBase
    {
        ChangeReportData patchData;
        public DataPatchResultDefinitionBuilder(WorkbookPart workbookPart, AllData allData, ChangeReportData patchData) : 
            base("Patched Result Definitions", workbookPart, allData) 
        {
            this.patchData = patchData;
        }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Short code", CellValues.String),
                ConstructCell("Result Label", CellValues.String),
                ConstructCell("Result Prompt Label", CellValues.String),
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
            foreach (var item in patchData.DeletedResults.OrderBy(x => x.Data.Label))
            {
                AppendResultItem(item, "Deleted");
            }
            foreach (var item in patchData.NewResults.OrderBy(x => x.Data.Label))
            {
                AppendResultItem(item, "Added");
            }
            foreach (var item in patchData.ModifiedResults.OrderBy(x => x.Data.Label))
            {
                AppendResultItem(item, "Modified");
            }
            foreach (var item in patchData.ToggledResults.OrderBy(x => x.Data.Label))
            {
                AppendResultItem(item, "Toggled");
            }
        }

        private void AppendResultItem(ChangeReportResultDataItem item, string action)
        {
            var row = AppendResultDefinition(item, action);
            if (item.Comments.Any())
            {
                row.Append(
                    ConstructCell(item.Comments[0].Note, CellValues.String));
                SheetData.AppendChild(row);
                
                for (var i = 1; i < item.Comments.Count; i++)
                {
                    row = AppendResultDefinition(item, action);
                    row.Append(ConstructCell(item.Comments[i].Note, CellValues.String));
                    SheetData.AppendChild(row);
                }
                AppendPrompts(item);
            }
            else
            {
                SheetData.AppendChild(row);
                AppendPrompts(item);
            }
        }

        private void AppendPrompts(ChangeReportResultDataItem item)
        {
            foreach (var rpItem in item.DeletedPrompts.OrderBy(x => x.Data.ResultPrompt.Label))
            {
                AppendResultPromptItem(rpItem, "Deleted");
            }
            foreach (var rpItem in item.RemovedPrompts.OrderBy(x => x.Data.ResultPrompt.Label))
            {
                AppendResultPromptItem(rpItem, "Removed");
            }
            foreach (var rpItem in item.NewPrompts.OrderBy(x => x.Data.ResultPrompt.Label))
            {
                AppendResultPromptItem(rpItem, "Added");
            }
            foreach (var rpItem in item.ModifiedPrompts.OrderBy(x => x.Data.ResultPrompt.Label))
            {
                AppendResultPromptItem(rpItem, "Modified");
            }
        }

        private void AppendResultPromptItem(ChangeReportResultPromptDataItem rpItem, string action)
        {
            var row = AppendResultPrompt(rpItem, action);
            if (rpItem.Comments.Any())
            {
                row.Append(
                    ConstructCell(rpItem.Comments[0].Note, CellValues.String));
                SheetData.AppendChild(row);

                for (var i = 1; i < rpItem.Comments.Count; i++)
                {
                    row = AppendResultPrompt(rpItem, action);
                    row.Append(ConstructCell(rpItem.Comments[i].Note, CellValues.String));
                    SheetData.AppendChild(row);
                }
            }
            else
            {
                SheetData.AppendChild(row);
            }
        }

        private Row AppendResultPrompt(ChangeReportResultPromptDataItem rpItem, string action)
        {
            var row = new Row();
            row.Append(
                ConstructCell(AsString(rpItem.Data.ResultPromptUUID), CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(rpItem.Data.ResultPrompt.Label, CellValues.String),
                ConstructCell(action, CellValues.String),
                ConstructDateCell(rpItem.Data.ResultPrompt.LastModifiedDate),
                ConstructCell(rpItem.Data.ResultPrompt.LastModifiedUser, CellValues.String)
                );

            return row;
        }

        private Row AppendResultDefinition(ChangeReportResultDataItem item, string action)
        {
            var row = new Row();
            row.Append(
                ConstructCell(AsString(item.Data.UUID), CellValues.String),
                ConstructCell(item.Data.ShortCode, CellValues.String),
                ConstructCell(item.Data.Label, CellValues.String),
                ConstructCell(string.Empty, CellValues.String),
                ConstructCell(action, CellValues.String),
                ConstructDateCell(item.Data.LastModifiedDate),
                ConstructCell(item.Data.LastModifiedUser, CellValues.String)
                );

            return row;
        }
    }
}
