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
    public class ResultDefinitionRuleBuilder : SheetBuilderBase
    {
        public ResultDefinitionRuleBuilder(WorkbookPart workbookPart, AllData allData) : base("Result Definition Rules", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                   ConstructCell("UUID", CellValues.String),
                   ConstructCell("Parent Result Definition UUID", CellValues.String),
                   ConstructCell("Parent Label (For Information Only)", CellValues.String),
                   ConstructCell("Parent Publication Status (For Information Only)", CellValues.String),
                   ConstructCell("Child Result Definition UUID", CellValues.String),
                   ConstructCell("Child Label (For Information Only)", CellValues.String),
                   ConstructCell("Child Publication Status (For Information Only)", CellValues.String),
                   ConstructCell("Rule Type", CellValues.String),
                   ConstructCell("Start Date", CellValues.String),
                   ConstructCell("End Date", CellValues.String),
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
            foreach (var resultDefinition in AllData.ResultDefinitions.Where(x => x.ResultDefinitionRules != null && x.ResultDefinitionRules.Count > 0).OrderBy(x=>x.Rank))
            {
                foreach (var rule in resultDefinition.ResultDefinitionRules)
                {
                    var row = new Row();
                    row.Append(
                            ConstructCell(AsString(rule.UUID), CellValues.String),
                            ConstructCell(AsString(resultDefinition.UUID), CellValues.String),
                            ConstructCell(resultDefinition.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(resultDefinition).GetDescription(), CellValues.String),
                            ConstructCell(AsString(rule.ResultDefinition.UUID), CellValues.String),
                            ConstructCell(rule.ResultDefinition.Label, CellValues.String),
                            ConstructCell(GetPublishedStatus(rule.ResultDefinition).GetDescription(), CellValues.String),
                            ConstructCell(Enum.GetName(typeof(ResultDefinitionRuleType), rule.Rule), CellValues.String),
                            ConstructDateCell(rule.StartDate),
                            ConstructDateCell(rule.EndDate),
                            ConstructDateCell(rule.CreatedDate),
                            ConstructDateCell(rule.LastModifiedDate),
                            ConstructDateCell(rule.DeletedDate)
                            );
                    SheetData.AppendChild(row);
                }
            }
        }
    }
}
