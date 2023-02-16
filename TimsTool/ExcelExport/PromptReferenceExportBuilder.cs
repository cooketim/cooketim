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
using System.Text;

namespace ExportExcel
{
    public class PromptReferenceExportBuilder : SheetBuilderBase
    {
        public PromptReferenceExportBuilder(WorkbookPart workbookPart, AllData allData) : base("Prompt References", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("Result Prompt UUID", CellValues.String),
                    ConstructCell("Result Prompt", CellValues.String),
                    ConstructCell("Result Prompt Publication Status", CellValues.String),
                    ConstructCell("Prompt Reference", CellValues.String),
                    ConstructCell("Prompt Type", CellValues.String),
                    ConstructCell("Is Hidden", CellValues.String),
                    ConstructCell("Result Definitions", CellValues.String)
                    );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            var distinctPromptGroups = AllData.ResultPromptRules.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null)
                                                .GroupBy(x => x.ResultPromptUUID).ToList();

            var reportLines = new List<PromptReferenceReportLine>();

            foreach (var group in distinctPromptGroups)
            {
                var rd = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == group.First().ResultDefinitionUUID);
                var line = new PromptReferenceReportLine(group.First().ResultPrompt, rd, this);
                reportLines.Add(line);
                if (group.Count() > 1)
                {
                    foreach (var rpr in group.Skip(1))
                    {
                        rd = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == rpr.ResultDefinitionUUID);
                        line.ResultDefinitions.Add(rd);
                    }
                }
            }

            foreach (var line in reportLines.OrderBy(x => x.ResultPrompt.Label))
            {
                if (line.ResultPromptParts == null)
                {
                    var row = new Row();
                    AppendResultPrompt(line, row);
                    SheetData.AppendChild(row);
                }
                else
                {
                    foreach (var part in line.ResultPromptParts)
                    {
                        var row = new Row();
                        AppendResultPromptPart(line, part, row);
                        SheetData.AppendChild(row);
                    }
                }
            }
        }

        private void AppendResultPromptPart(PromptReferenceReportLine line, ResultPromptPart part, Row row)
        {
            row.Append(
                            ConstructCell(AsString(line.ResultPrompt.UUID), CellValues.String),
                            ConstructCell(part.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(line.ResultPrompt).GetDescription(), CellValues.String),
                            ConstructCell(part.Reference, CellValues.String),
                            ConstructCell(line.ResultPrompt.PromptType, CellValues.String),
                            ConstructCell(AsBoolString(part.Hidden), CellValues.String),
                            ConstructCell(line.ResultDefinitionLabels, CellValues.String)
                        );
        }

        private void AppendResultPrompt(PromptReferenceReportLine line, Row row)
        {
            row.Append(
                        ConstructCell(AsString(line.ResultPrompt.UUID), CellValues.String),
                        ConstructCell(line.ResultPrompt.Label, CellValues.String),
                        ConstructCell(GetPublishedStatus(line.ResultPrompt).GetDescription(), CellValues.String),
                        ConstructCell(line.ResultPrompt.PromptReference, CellValues.String),
                        ConstructCell(line.ResultPrompt.PromptType, CellValues.String),
                        ConstructCell(AsBoolString(line.ResultPrompt.Hidden), CellValues.String),
                        ConstructCell(line.ResultDefinitionLabels, CellValues.String)
                    );
        }

        private class PromptReferenceReportLine
        {
            private SheetBuilderBase builder;
            public ResultPrompt ResultPrompt { get; }

            public List<ResultDefinition> ResultDefinitions { get; }

            public List<ResultPromptPart> ResultPromptParts { get; }

            public string ResultDefinitionLabels
            {
                get
                {
                    return string.Join(Environment.NewLine, ResultDefinitions.OrderBy(x => x.Label).Select(x => string.Format("{0} ({1})", x.Label, builder.GetPublishedStatus(x).GetDescription())));
                }
            }

            public PromptReferenceReportLine(ResultPrompt resultPrompt, ResultDefinition rd, SheetBuilderBase builder)
            {
                this.builder = builder;
                ResultPrompt = resultPrompt;
                ResultDefinitions = new List<ResultDefinition>() { rd };

                if (resultPrompt.PromptType == "NAMEADDRESS")
                {
                    ResultPromptParts = ResultDefinition.MakeNameParts(resultPrompt);
                    ResultPromptParts.AddRange(ResultDefinition.MakeAddressParts(resultPrompt));
                }

                if (resultPrompt.PromptType == "ADDRESS")
                {
                    ResultPromptParts = ResultDefinition.MakeAddressParts(resultPrompt);
                }
            }
        }
    }
}
