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
    public class IdMapBuilder : SheetBuilderBase
    {
        private IdMap map;
        public IdMapBuilder(WorkbookPart workbookPart, AllData allData, IdMap map) : base("Id Map", workbookPart, allData) 
        {
            this.map = map;
        }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Primary Id", CellValues.String),
                ConstructCell("Id Type", CellValues.String),
                ConstructCell("File Num", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            foreach (var item in map.Map.OrderBy(x=>x.IdType).ThenBy(x=>x.FileNum))
            {
                var row = new Row();
                row.Append(
                    ConstructCell(AsString(item.PrimaryId), CellValues.String),
                    ConstructCell(Enum.GetName(typeof(IdType), item.IdType), CellValues.String),
                    ConstructCell(item.FileNum.ToString(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
