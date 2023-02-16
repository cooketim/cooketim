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
    public class ResultPromptWordBuilder : SheetBuilderBase
    {
        public ResultPromptWordBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Prompt Word Synonym", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Result Prompt Word", CellValues.String),
                ConstructCell("Start Date", CellValues.String),
                ConstructCell("End Date", CellValues.String),
                ConstructCell("Word Created Date", CellValues.String),
                ConstructCell("Word Last Modified Date", CellValues.String),
                ConstructCell("Word Deleted Date", CellValues.String),
                ConstructCell("Synonym", CellValues.String),
                ConstructCell("Synonym Created Date", CellValues.String),
                ConstructCell("Synonym Last Modified Date", CellValues.String),
                ConstructCell("Synonym Deleted Date", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            foreach (var group in AllData.ResultPromptWordGroups.OrderBy(x => x.ResultPromptWord))
            {
                if (group.Synonyms == null || group.Synonyms.Count == 0)
                {
                    var row = new Row();
                    row.Append(
                        ConstructCell(AsString(group.UUID), CellValues.String),
                        ConstructCell(group.ResultPromptWord, CellValues.String),
                        ConstructDateCell(group.StartDate),
                        ConstructDateCell(group.EndDate),
                        ConstructDateCell(group.CreatedDate),
                        ConstructDateCell(group.LastModifiedDate),
                        ConstructDateCell(group.DeletedDate)
                        );
                    SheetData.AppendChild(row);
                }
                else
                {
                    foreach (var synonym in group.Synonyms)
                    {
                        var row = new Row();
                        row.Append(
                            ConstructCell(AsString(group.UUID), CellValues.String),
                            ConstructCell(group.ResultPromptWord, CellValues.String),
                            ConstructDateCell(group.StartDate),
                            ConstructDateCell(group.EndDate),
                            ConstructDateCell(group.CreatedDate),
                            ConstructDateCell(group.LastModifiedDate),
                            ConstructDateCell(group.DeletedDate),
                            ConstructCell(synonym.Synonym, CellValues.String),
                            ConstructDateCell(synonym.CreatedDate),
                            ConstructDateCell(synonym.LastModifiedDate),
                            ConstructDateCell(synonym.DeletedDate)
                            );
                        SheetData.AppendChild(row);
                    }
                }
            }
        }
    }
}
