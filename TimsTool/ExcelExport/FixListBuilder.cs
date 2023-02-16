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
    public class FixedListBuilder : SheetBuilderBase
    {
        public FixedListBuilder(WorkbookPart workbookPart, AllData allData) : base("Fixed List", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("UUID", CellValues.String),
                ConstructCell("Fixed List Label", CellValues.String),
                ConstructCell("Fixed List Publication Status", CellValues.String),
                ConstructCell("Code", CellValues.String),
                ConstructCell("Value", CellValues.String),
                ConstructCell("Welsh Value", CellValues.String),
                ConstructCell("CJS Qualifier", CellValues.String),
                ConstructCell("Start Date", CellValues.String),
                ConstructCell("End Date", CellValues.String),
                ConstructCell("Created Date", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String),
                ConstructCell("Deleted Date", CellValues.String));
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            foreach (var list in AllData.FixedLists.OrderBy(x=>x.Label))
            {
                foreach (var value in list.Values)
                {
                    var row = new Row();
                    row.Append(
                        ConstructCell(AsString(list.UUID), CellValues.String),
                        ConstructCell(list.Label, CellValues.String),
                        ConstructCell(GetPublishedStatus(list).GetDescription(), CellValues.String),
                        ConstructCell(value.Code, CellValues.String),
                        ConstructCell(value.Value, CellValues.String),
                        ConstructCell(value.WelshValue, CellValues.String),
                        ConstructCell(value.CJSQualifier, CellValues.String),
                        ConstructDateCell(list.StartDate),
                        ConstructDateCell(list.EndDate),
                        ConstructDateCell(list.CreatedDate),
                        ConstructDateCell(list.LastModifiedDate),
                        ConstructDateCell(list.DeletedDate)
                        );
                    SheetData.AppendChild(row);
                }
            }
        }
    }
}
