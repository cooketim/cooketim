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
    public class ResultDefinitionWordSynonymExportBuilder : SheetBuilderBase
    {
        public ResultDefinitionWordSynonymExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Definition Word Synonym", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Definition UUID", CellValues.String),
                ConstructCell("Result Definition", CellValues.String),
                ConstructCell("Result Definition Short Code", CellValues.String),
                ConstructCell("Word Group Name", CellValues.String),
                ConstructCell("Result Definition Word UUID", CellValues.String),
                ConstructCell("Result Definition Word", CellValues.String),
                ConstructCell("Synonyms", CellValues.String)
                );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            foreach (var rd in AllData.ResultDefinitions.Where(x=>x.DeletedDate == null).OrderBy(x=>x.Label))
            {
                var row = new Row();
                AppendResultDefinition(row, rd);
                var toProcess = rd.WordGroups == null ? null : rd.WordGroups.Where(x => !string.IsNullOrEmpty(x.WordGroupName)).ToList();
                if (toProcess != null && toProcess.Count>0)
                {
                    AppendWordGroup(row, toProcess.First(), rd);

                    for (var i=1; i<toProcess.Count; i++)
                    {
                        row = new Row();
                        AppendResultDefinition(row, rd);
                        AppendWordGroup(row, toProcess[i], rd);
                    }
                }
                else
                {
                    //no word groups for the result
                    SheetData.AppendChild(row);
                }
            }
        }

        private void AppendResultDefinition(Row row, ResultDefinition rd)
        {
            row.Append(
                        ConstructCell(AsString(rd.UUID), CellValues.String),
                        ConstructCell(rd.Label, CellValues.String),
                        ConstructCell(rd.ShortCode, CellValues.String)
                    );
        }

        private void AppendWordGroup(Row row, ResultWordGroup wg, ResultDefinition rd)
        {
            row.Append(ConstructCell(wg.WordGroupName, CellValues.String));

            var toProcess = wg.ResultDefinitionWordGroups.Where(x => x.DeletedDate == null).ToList();
            if (toProcess.Count > 0)
            {
                AppendResultDefinitionWord(row, toProcess.First());
                SheetData.AppendChild(row);

                for (var i = 1; i < toProcess.Count; i++)
                {
                    row = new Row();
                    AppendResultDefinition(row, rd);
                    row.Append(ConstructCell(wg.WordGroupName, CellValues.String));
                    AppendResultDefinitionWord(row, toProcess[i]);
                    SheetData.AppendChild(row);
                }
            }
            else
            {
                //no result definition words for the word group
                SheetData.AppendChild(row);
            }

        }

        private void AppendResultDefinitionWord(Row row, DataLib.ResultDefinitionWordGroup resultDefinitionWordGroup)
        {
            var synonyms = string.Join(",", resultDefinitionWordGroup.Synonyms.Where(x=>x.DeletedDate == null).Select(x => x.Synonym));
            row.Append(
                ConstructCell(AsString(resultDefinitionWordGroup.UUID), CellValues.String),
                ConstructCell(resultDefinitionWordGroup.ResultDefinitionWord, CellValues.String),
                ConstructCell(synonyms, CellValues.String)
                );
        }
    }
}
