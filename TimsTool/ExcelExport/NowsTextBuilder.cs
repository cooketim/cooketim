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
    public class NowsTextBuilder : SheetBuilderBase
    {
        public NowsTextBuilder(WorkbookPart workbookPart, AllData allData) : base("NOWs EDTs Text", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                    ConstructCell("NOW/EDT UUID (for reference only)", CellValues.String),
                    ConstructCell("NOW/EDT Name (for reference only)", CellValues.String),
                    ConstructCell("NOW/EDT Publication Status (for reference only)", CellValues.String),
                    ConstructCell("Text Reference", CellValues.String),
                    ConstructCell("NOW/EDT Text", CellValues.String),
                    ConstructCell("Welsh NOW/EDT Text", CellValues.String),
                    ConstructCell("Is EDT", CellValues.String)
                    );
            SheetData.AppendChild(row);

            //Process the data
            ProcessData();
        }

        protected virtual void ProcessData()
        {
            foreach (var now in AllData.Nows.Where(x=>x.DeletedDate == null).Where(x => x.NowTextList != null && x.NowTextList.Count > 0).OrderBy(x => x.IsEDT).ThenBy(x=>x.Name))
            {
                AppendData(now);
            }
        }

        protected void AppendData(Now now)
        {
            foreach (var nowText in now.NowTextList)
            {
                var row = new Row();
                row.Append(
                        ConstructCell(AsString(now.UUID), CellValues.String),
                        ConstructCell(now.Name, CellValues.String),
                        ConstructCell(GetPublishedStatus(now).GetDescription(), CellValues.String),
                        ConstructCell(nowText.Reference, CellValues.String),
                        ConstructCell(nowText.Text, CellValues.String),
                        ConstructCell(nowText.WelshText, CellValues.String),
                        ConstructCell(AsBoolString(now.IsEDT), CellValues.String)
                        );

                SheetData.AppendChild(row);
            }
        }
    }
}
