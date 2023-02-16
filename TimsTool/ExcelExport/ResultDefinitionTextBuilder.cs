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
    public class ResultDefinitionTextBuilder : SheetBuilderBase
    {
        public ResultDefinitionTextBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Text Templates", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("Result Definition UUID", CellValues.String),
                    ConstructCell("Result Definition", CellValues.String),
                    ConstructCell("Result Definition Publication Status", CellValues.String),
                    ConstructCell("Result Definition Short Code", CellValues.String),
                    ConstructCell("Result Text Template", CellValues.String),
                    ConstructCell("Child Results With Result Text", CellValues.String)
                    );
            SheetData.AppendChild(row);

            AppendData();
        }

        private void AppendData()
        {
            var reportItems = PrepareData();

            foreach (var item in reportItems)
            {
                var row = new Row();
                AppendResultDefinition(row, item);
                SheetData.AppendChild(row);
            }
        }

        private void AppendResultDefinition(Row row, ReportItem item)
        {
            var childResults = item.Rules == null || item.Rules.Count == 0 ? null : string.Join(",", item.Rules.Select(x => x.ResultDefinition.ShortCode));
            row.Append(
                            ConstructCell(AsString(item.ResultDefinition.UUID), CellValues.String),
                            ConstructCell(item.ResultDefinition.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(item.ResultDefinition).GetDescription(), CellValues.String),
                            ConstructCell(item.ResultDefinition.ShortCode, CellValues.String),
                            ConstructCell(item.ResultDefinition.ResultTextTemplate, CellValues.String),
                            ConstructCell(childResults, CellValues.String)
                        );
        }

        private List<ReportItem> PrepareData()
        {
            var res = new List<ReportItem>();

            //report on parent results
            foreach (var rdGroup in AllData.ResultDefinitionRules.Where(x => x.DeletedDate == null && x.ResultDefinition.DeletedDate == null).GroupBy(x => x.ParentUUID))
            {
                //check that the parent is not deleted
                var parent = AllData.ResultDefinitions.FirstOrDefault(x => x.UUID == rdGroup.Key);
                if (parent != null && parent.DeletedDate == null && !string.IsNullOrEmpty(parent.ResultTextTemplate))
                {
                    var data = new ReportItem(parent, rdGroup.Where(x => !string.IsNullOrEmpty(x.ResultDefinition.ResultTextTemplate)).OrderBy(x => x.ResultDefinition.Label).ToList());
                    res.Add(data);
                }                
            }

            //report on child results that have result text templates
            foreach (var rd in AllData.ResultDefinitions.Where(x => x.DeletedDate == null && !string.IsNullOrEmpty(x.ResultTextTemplate)))
            {
                //check that the result is a child i.e. is not already included in the collection of parents
                var exists = res.FirstOrDefault(x => x.ResultDefinition.UUID == rd.UUID);
                if (exists == null)
                {
                    var data = new ReportItem(rd, null);
                    res.Add(data);
                }
            }
            return res.OrderBy(x => x.ResultDefinition.Label).ToList();
        }

        private class ReportItem
        {
            public ReportItem(ResultDefinition resultDefinition, List<ResultDefinitionRule> rules)
            {
                ResultDefinition = resultDefinition;
                Rules = rules;
            }

            public ResultDefinition ResultDefinition { get; set; }

            public List<ResultDefinitionRule> Rules { get; set; }
        }
    }
}
