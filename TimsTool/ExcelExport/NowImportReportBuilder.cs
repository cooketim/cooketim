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
    public class NowImportReportBuilder : SheetBuilderBase
    {
        private List<NowImport> data;
        private int type;
        private bool isEDT;
        public NowImportReportBuilder(WorkbookPart workbookPart, List<NowImport> data, string title, int type, bool isEDT) 
            : base(title, workbookPart, null) { this.data = data; this.type = type; this.isEDT = isEDT; }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("Source Row Number", CellValues.String),
                ConstructCell(isEDT ? "EDT" : "NOW", CellValues.String),
                ConstructCell(isEDT ? "EDT Title" : "NOW Title", CellValues.String),
                ConstructCell(isEDT ? "Matched Existing EDTs" : "Matched Existing Nows", CellValues.String),
                ConstructCell("Possible Duplicate Entries", CellValues.String),
                ConstructCell("Selected/Forced For Import", CellValues.String)
                );
            if (type == 2 || type == 3) //partial matches & full mismatches
            {
                row.Append(
                    ConstructCell("Unmatched Libra Codes", CellValues.String)
                    );
            }
            if (type == 1 || type == 2) //full & partial matches
            {
                row.Append(
                    ConstructCell("Matched Libra Code", CellValues.String),
                    ConstructCell("Matched CJS Code", CellValues.String),
                    ConstructCell("Matched On", CellValues.String),
                    ConstructCell("Matched Result Definition", CellValues.String),
                    ConstructCell("Matched Result Definition UUID", CellValues.String)
                    );
            }
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            //separate out the type of sheets
            if (type == 1)
            {
                //full match
                AppendFullMatches(data.Where(x => (x.UnmatchedResults == null || x.UnmatchedResults.Count == 0) && (x.MatchedResults != null && x.MatchedResults.Count > 0)));
                return;
            }

            if (type == 2)
            {
                //partial match
                AppendPartialMatches(data.Where(x => (x.UnmatchedResults != null && x.UnmatchedResults.Count > 0) && (x.MatchedResults != null && x.MatchedResults.Count > 0)));
                return;
            }

            //full mismatch
            AppendFullMismatches(data.Where(x => (x.UnmatchedResults != null && x.UnmatchedResults.Count > 0) && (x.MatchedResults == null || x.MatchedResults.Count == 0)));

        }

        private void AppendFullMismatches(IEnumerable<NowImport> reportData)
        {
            foreach (var item in reportData.OrderBy(x=>x.RowIndex))
            {
                var row = new Row();
                row.Append(
                    ConstructCell(AsString(item.RowIndex), CellValues.String),
                    ConstructCell(item.Now, CellValues.String),
                    ConstructCell(item.NowTitle, CellValues.String),
                    ConstructCell(item.NowMatchedTitles, CellValues.String),
                    ConstructCell(item.DuplicatedTitles, CellValues.String),
                    ConstructCell(string.Empty, CellValues.String),
                    ConstructCell(AsString(item.UnmatchedResults), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }

        private void AppendFullMatches(IEnumerable<NowImport> reportData)
        {
            foreach (var item in reportData.OrderBy(x => x.RowIndex))
            {
                var row = new Row();
                row.Append(
                    ConstructCell(AsString(item.RowIndex), CellValues.String),
                    ConstructCell(item.Now, CellValues.String),
                    ConstructCell(item.NowTitle, CellValues.String),
                    ConstructCell(item.NowMatchedTitles, CellValues.String),
                    ConstructCell(item.DuplicatedTitles, CellValues.String),
                    ConstructCell(item.SelectedForImport, CellValues.String)
                    );

                AppendMatchedDetails(item.MatchedResults[0], row);
                SheetData.AppendChild(row);

                if (item.MatchedResults.Count > 1)
                {
                    for (var i = 1; i < item.MatchedResults.Count; i++)
                    {
                        row = new Row();
                        row.Append(
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String)
                            );
                        AppendMatchedDetails(item.MatchedResults[i], row);
                        SheetData.AppendChild(row);
                    }
                }
            }
        }

        private void AppendPartialMatches(IEnumerable<NowImport> reportData)
        {
            var dataToReport = reportData.OrderBy(x => x.RowIndex).ToList();
            foreach (var item in dataToReport)
            {
                var row = new Row();
                row.Append(
                    ConstructCell(AsString(item.RowIndex), CellValues.String),
                    ConstructCell(item.Now, CellValues.String),
                    ConstructCell(item.NowTitle, CellValues.String),
                    ConstructCell(item.NowMatchedTitles, CellValues.String),
                    ConstructCell(item.DuplicatedTitles, CellValues.String),
                    ConstructCell(item.SelectedForImport, CellValues.String),
                    ConstructCell(AsString(item.UnmatchedResults), CellValues.String)
                    );

                AppendMatchedDetails(item.MatchedResults[0], row);
                SheetData.AppendChild(row);

                if (item.MatchedResults.Count > 1)
                {
                    for (var i = 1; i < item.MatchedResults.Count; i++)
                    {
                        row = new Row();
                        row.Append(
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String),
                            ConstructCell(string.Empty, CellValues.String)
                            );
                        AppendMatchedDetails(item.MatchedResults[i], row);
                        SheetData.AppendChild(row);
                    }
                }
            }
        }

        private void AppendMatchedDetails(MatchedResult matchedResult, Row row)
        {
            row.Append(
                ConstructCell(matchedResult.ProvidedCode, CellValues.String),
                ConstructCell(matchedResult.ProvidedCJSCode, CellValues.String),
                ConstructCell(matchedResult.MatchedOn, CellValues.String),
                ConstructCell(matchedResult.ResultDefinition.Label, CellValues.String),
                ConstructCell(AsString(matchedResult.ResultDefinition.UUID), CellValues.String)
                );
        }
    }
}
