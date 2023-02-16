using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExportExcel;
using DocumentFormat.OpenXml;
using System.IO;

namespace Models.Commands
{
    public abstract class DataImportProviderBase
    {
        protected string filePath = null;
        protected string outFilePath = null;
        private WorkbookPart outWorkbookPart = null;
        protected int rowIndex = 0, columnIndex = 0;

        protected DataImportProviderBase(string filePath, string outFilePath)
        {
            this.filePath = filePath;
            this.outFilePath = outFilePath;
        }

        /// <summary>
        /// Open the file path received in Excel. Then, open the workbook
        /// within the file. Send the workbook to the next function, the internal scan
        /// function. Will throw an exception if a file cannot be found or opened.
        /// </summary>
        private void ExtractDataFromSpreadsheets()
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                Sheets sheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                var sheetNum = 1;
                foreach (Sheet sheet in sheetcollection)
                {
                    string sheetName = sheet.Name;

                    Worksheet worksheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;

                    string sheetId = sheet.Id;
                    WorksheetPart worksheetPart = workbookPart.GetPartById(sheetId) as WorksheetPart;
                    SheetData sheetData = worksheet.GetFirstChild<SheetData>();
                    var rowIndex = 0;

                    //create the output sheet
                    var outSheetData = AddSheet(sheet.Name, sheetNum);                

                    List<string> fieldNames = null;
                    foreach (Row row in sheetData)
                    {
                        if (rowIndex == 0)
                        {
                            fieldNames = GetFieldNames(workbookPart, row);
                            rowIndex++;

                            //make a corresponding header row for the output
                            var outputHeaderRow = new Row();
                            foreach (var columnName in fieldNames)
                            {
                                outputHeaderRow.Append(
                                    ConstructCell(columnName, CellValues.String)
                                );
                            }
                            outputHeaderRow.Append(
                                    ConstructCell("Import Status", CellValues.String)
                                );
                            outSheetData.AppendChild(outputHeaderRow);
                            continue;
                        }

                        ProcessRow(workbookPart, row, fieldNames, sheetName, outSheetData);
                        rowIndex++;
                    }
                    sheetNum++;
                }
            }

            //now that all data is loaded, perform any post processing
            PostLoad();
        }

        public void Load()
        {
            //Generate the report
            using (FileStream fs = new FileStream(outFilePath, FileMode.Create))
            {
                using (var writer = new StreamWriter(fs))
                {
                    using (SpreadsheetDocument document = SpreadsheetDocument.Create(fs, SpreadsheetDocumentType.Workbook))
                    {
                        outWorkbookPart = document.AddWorkbookPart();
                        outWorkbookPart.Workbook = Exporter.MakeWorkbook();

                        //import the data and write to the output excel
                        ExtractDataFromSpreadsheets();

                        outWorkbookPart.Workbook.Save();
                    }
                }
            }
        }

        protected abstract void PostLoad();

        protected abstract void ProcessRow(WorkbookPart workbookPart, Row row, List<string> fieldNames, string sheetName, SheetData outSheetData);

        private List<string> GetFieldNames(WorkbookPart workbookPart, Row row)
        {
            var res = new List<string>();
            foreach (Cell cell in row)
            {
                var cellVal = GetCellValue(workbookPart, cell);
                res.Add(string.IsNullOrEmpty(cellVal) ? "ignore-null-coll" : cellVal);
            }
            return res;
        }

        private SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, int id)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
        }

        protected string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            string val = cell.InnerText;
            if (cell.DataType != null)
            {
                if (cell.DataType == CellValues.Boolean)
                {
                    return val == "1" ? "TRUE" : "FALSE";
                }

                if (cell.DataType == CellValues.SharedString)
                {
                    int id;
                    if (Int32.TryParse(cell.InnerText, out id))
                    {
                        SharedStringItem item = GetSharedStringItemById(workbookPart, id);
                        if (item.InnerText != null)
                        {
                            val = item.InnerText;
                        }
                        else if (item.InnerXml != null)
                        {
                            val = item.InnerXml;
                        }
                    } //end of if (Int32.TryParse ( thecurrentcell.InnerText, out id )
                } //end of if(thecurrentcell.DataType == CellValues.SharedString)
            } // end of if(thecurrentcell.DataType != null)

            return val;
        }

        protected int GetColumnIndex(string cellRef)
        {
            //CellReference is not guaranteed to be in the XML either. In the case where the CellReference is null the cell is placed in the leftmost available cell and therefore we rely on sequential counting
            if (cellRef == null)
            {
                columnIndex++;
                return columnIndex - 1;
            }

            int index = 0;

            for (var i = 0; i < cellRef.Length; i++)
            {
                var partString = cellRef[i].ToString();
                if (Regex.IsMatch(partString, @"^[a-zA-Z]+$"))
                {
                    //adjust index according to allow fo excel columns AA3, AB3, etc
                    var unitValue = char.ToUpper(cellRef[i]);
                    var unitValueAdjusted = unitValue - 64;
                    index = i * 26 + unitValueAdjusted;
                }
                else
                {
                    //non alpha
                    break;
                }
            }

            //make index zero based
            columnIndex = index - 1;
            return columnIndex;
        }

        protected bool AsBool(string val)
        {
            if (string.IsNullOrEmpty(val)) { return false; };
            bool res = false;
            if (Boolean.TryParse(val.Trim(), out res))
            {
                return res;
            }
            return val.ToUpper().StartsWith("Y");
        }

        protected int? AsNullableInt(string val)
        {
            if (string.IsNullOrEmpty(val)) { return null; };
            int res;

            if (int.TryParse(val.Trim(), out res))
            {
                return res;
            }

            var resDecimal = AsNullableDecimal(val.Trim());
            if (resDecimal == null) { return null; }

            return Decimal.ToInt32(resDecimal.Value);
        }

        protected decimal? AsNullableDecimal(string val)
        {
            if (string.IsNullOrEmpty(val)) { return null; };
            decimal res;
            return decimal.TryParse(val.Trim(), out res) ? (int?)res : null;
        }

        protected DateTime? AsNullableDate(string val)
        {
            if (string.IsNullOrEmpty(val)) { return null; };
            DateTime res;
            return DateTime.TryParse(val.Trim(), out res) ? (DateTime?)res : null;
        }

        protected Guid? AsNullableGuid(string val)
        {
            if (string.IsNullOrEmpty(val)) { return null; }
            return new Guid(val.Trim());
        }

        protected string AsStringOrIntegerAsString(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }

            int? myInt = AsNullableInt(val.Trim());
            if (myInt != null)
            {
                return myInt.Value.ToString();
            }

            return val.Trim();
        }

        protected string AsString(string val)
        {
            return string.IsNullOrEmpty(val) ? null : val.Trim();
        }

        protected List<string> AsStringCollection(string val)
        {
            if (string.IsNullOrEmpty(val)) { return null; };
            return val.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(p => p.Trim()).ToList();
        }

        protected List<List<string>> AsStringCollections(string val)
        {
            if (string.IsNullOrEmpty(val)) { return null; };

            var parts = val.Split('~').Where(x => !string.IsNullOrEmpty(x)).Select(p => p.Trim()).ToList();
            var res = new List<List<string>>();
            foreach (var part in parts)
            {
                res.Add(AsStringCollection(part));
            }
            return res;
        }

        private SheetData AddSheet(string sheetName, int sheetNum)
        {
            var sheetData = new SheetData();
            var worksheet = new Worksheet(sheetData) { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac xr xr2 xr3" } };
            worksheet.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            worksheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            worksheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            worksheet.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
            worksheet.AddNamespaceDeclaration("xr2", "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");
            worksheet.AddNamespaceDeclaration("xr3", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision3");
            worksheet.SetAttribute(new OpenXmlAttribute("xr", "uid", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision", "{6EF8E4D7-851D-420B-B043-C324EE8B0E3F}"));

            // Append the new worksheet and associate it with the workbook.
            WorksheetPart worksheetPart = outWorkbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = worksheet;

            Sheets sheets = outWorkbookPart.Workbook.GetFirstChild<Sheets>();
            string relationshipId = outWorkbookPart.GetIdOfPart(worksheetPart);

            var sheet = new Sheet() { Id = relationshipId, Name = sheetName, SheetId = new UInt32Value((uint)sheetNum) };

            sheets.Append(sheet);
            return sheetData;
        }

        protected Cell ConstructCell(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value == null ? string.Empty : value),
                DataType = new EnumValue<CellValues>(dataType)
            };
        }

        protected Cell ConstructDateCell(DateTime? value)
        {
            if (value == null || !value.HasValue)
            {
                return ConstructCell(string.Empty, CellValues.String);
            }

            return new Cell()
            {
                CellValue = new CellValue(value.Value.ToShortDateString()),
                DataType = new EnumValue<CellValues>(CellValues.String)
            };
        }

        protected Cell ConstructIntCell(int? value)
        {
            if (value == null || !value.HasValue)
            {
                return ConstructCell("", CellValues.String);
            }

            return new Cell()
            {
                CellValue = new CellValue(value.Value.ToString()),
                DataType = new EnumValue<CellValues>(CellValues.Number)
            };
        }
    }
}
