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
    public class ResultPromptWordSynonymExportBuilder : SheetBuilderBase
    {
        public ResultPromptWordSynonymExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Prompt Word Synonym", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Prompt UUID", CellValues.String),
                ConstructCell("Result Prompt", CellValues.String),
                ConstructCell("Result Definitions", CellValues.String),
                ConstructCell("Result Prompt Word UUID", CellValues.String),
                ConstructCell("Result Prompt Word", CellValues.String),
                ConstructCell("Synonyms", CellValues.String)
                );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            //group the result definitions for each prompt
            var reportItems = new List<PromptReportItem>();
            var promptGroups = AllData.ResultPromptRules.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null && x.ResultPromptUUID != null).GroupBy(x => x.ResultPromptUUID).ToList();
            foreach(var grouping in promptGroups)
            {
                var item = new PromptReportItem(grouping, AllData);
                if(item.ResultDefinitions.Count>0)
                {
                    reportItems.Add(item);
                }
            }
            
            foreach (var item in reportItems.OrderBy(x=>x.ResultPrompt.Label))
            {
                var row = new Row();
                AppendResultPrompt(row, item);
                var toProcess = item.ResultPrompt.ResultPromptWordGroups == null ? null : item.ResultPrompt.ResultPromptWordGroups.Where(x => x.DeletedDate == null).ToList();
                if (toProcess != null && toProcess.Count>0)
                {
                    AppendResultPromptWord(row, toProcess.First());
                    SheetData.AppendChild(row);

                    for (var i=1; i<toProcess.Count; i++)
                    {
                        row = new Row();
                        AppendResultPrompt(row, item);
                        AppendResultPromptWord(row, toProcess[i]);
                        SheetData.AppendChild(row);
                    }
                }
                else
                {
                    //no result prompt words for the prompt
                    SheetData.AppendChild(row);
                }
            }
        }

        private void AppendResultPrompt(Row row, PromptReportItem prompt)
        {
            var results = string.Join(Environment.NewLine, prompt.ResultDefinitions.OrderBy(x=>x.Label).Select(x => x.Label));
            row.Append(
                        ConstructCell(AsString(prompt.ResultPrompt.UUID), CellValues.String),
                        ConstructCell(prompt.ResultPrompt.Label, CellValues.String),
                        ConstructCell(results, CellValues.String)
                    );
        }

        private void AppendResultPromptWord(Row row, ResultPromptWordGroup resultPromptWordGroup)
        {
            var synonyms = string.Join(",", resultPromptWordGroup.Synonyms.Where(x=>x.DeletedDate == null).Select(x => x.Synonym));
            row.Append(
                ConstructCell(AsString(resultPromptWordGroup.UUID), CellValues.String),
                ConstructCell(resultPromptWordGroup.ResultPromptWord, CellValues.String),
                ConstructCell(synonyms, CellValues.String)
                );
        }

        private class PromptReportItem
        {
            public ResultPrompt ResultPrompt;
            public List<ResultDefinition> ResultDefinitions;

            public PromptReportItem(IGrouping<Guid?, ResultPromptRule> resultPromptGrouping, AllData allData)
            {
                ResultDefinitions = new List<ResultDefinition>();
                var toProcess = resultPromptGrouping.ToList();
                ResultPrompt = toProcess.First().ResultPrompt;
                foreach(var item in toProcess)
                {
                    var rd = allData.ResultDefinitions.FirstOrDefault(x => x.UUID == item.ResultDefinitionUUID);
                    if(rd!=null && rd.DeletedDate == null)
                    {
                        ResultDefinitions.Add(rd);
                    }
                }
            }
        }
    }
}
