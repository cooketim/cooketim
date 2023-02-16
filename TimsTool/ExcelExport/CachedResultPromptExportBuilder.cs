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
    public class CachedResultPromptExportBuilder : SheetBuilderBase
    {
        public CachedResultPromptExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Cached Result Prompts", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Result Prompt UUID", CellValues.String),
                ConstructCell("Label", CellValues.String),
                ConstructCell("Prompt Type", CellValues.String),
                ConstructCell("Prompt Reference", CellValues.String),
                ConstructCell("Parent Results", CellValues.String),
                ConstructCell("Cacheable", CellValues.String),
                ConstructCell("Cache Data Path", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            var reportLines = new List<CachedResultPromptReportLine>();
            foreach (var rprGroup in AllData.ResultPromptRules.Where(x => x.DeletedDate == null 
                                                                    && x.ResultPrompt.DeletedDate == null 
                                                                    && x.ResultPrompt.Cacheable != null 
                                                                    && x.ResultPrompt.Cacheable != CacheableEnum.notCached ).GroupBy(x => x.ResultPromptUUID))
            {
                var parentResults = new List<string>();
                foreach(var rpr in rprGroup)
                {
                    var rd = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == rpr.ResultDefinitionUUID && x.DeletedDate == null);
                    if (rd != null) 
                    {
                        parentResults.Add(string.Format("{0} ({1})", rd.Label, GetPublishedStatus(rd).GetDescription()));
                    }
                }
                reportLines.Add(new CachedResultPromptReportLine()
                {
                    ResultPrompt = rprGroup.First().ResultPrompt,
                    ParentResults = parentResults.OrderBy(x => x).ToList()
                }); ;                
            }
            foreach(var line in reportLines.OrderBy(x=>x.ResultPrompt.Label))
            {
                var row = new Row();
                row.Append(
                        ConstructCell(AsString(line.ResultPrompt.UUID), CellValues.String),
                        ConstructCell(line.ResultPrompt.Label, CellValues.String),
                        ConstructCell(line.ResultPrompt.PromptType, CellValues.String),
                        ConstructCell(line.ResultPrompt.PromptReference, CellValues.String),
                        ConstructCell(AsStringLines(line.ParentResults), CellValues.String),
                        ConstructCell(line.ResultPrompt.Cacheable.GetDescription(), CellValues.String),
                        ConstructCell(line.ResultPrompt.CacheDataPath, CellValues.String),
                        ConstructCell(line.ResultPrompt.LastModifiedDate.Value.ToString("d MMM yyyy h:mm:ss tt", DateTimeFormatInfo.InvariantInfo), CellValues.String),
                        ConstructCell(GetPublishedStatus(line.ResultPrompt).GetDescription(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }            
        }

        private class CachedResultPromptReportLine
        {
            public ResultPrompt ResultPrompt { get; set; }
            public List<string> ParentResults { get; set; }
        }
    }
}
