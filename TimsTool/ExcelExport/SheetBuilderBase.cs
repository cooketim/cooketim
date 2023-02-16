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
    public abstract class SheetBuilderBase
    {
        protected AllData AllData { get; }
        protected SheetData SheetData { get; }
        private Sheet sheet;

        public SheetBuilderBase(string sheetName, WorkbookPart workbookPart, AllData allData)
        {
            AllData = allData;
            SheetData = new SheetData();
            var worksheet = new Worksheet(SheetData) { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac xr xr2 xr3" } };
            worksheet.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            worksheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            worksheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            worksheet.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
            worksheet.AddNamespaceDeclaration("xr2", "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");
            worksheet.AddNamespaceDeclaration("xr3", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision3");
            worksheet.SetAttribute(new OpenXmlAttribute("xr", "uid", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision", "{6EF8E4D7-851D-420B-B043-C324EE8B0E3F}"));

            // Append the new worksheet and associate it with the workbook.
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = worksheet;

            Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
            string relationshipId = workbookPart.GetIdOfPart(worksheetPart);

            sheet = new Sheet() { Id = relationshipId, Name = sheetName };

            sheets.Append(sheet);
        }

        public void Build(int sheetNum)
        {
            sheet.SheetId = new UInt32Value((uint)sheetNum);
            BuildContent();
        }

        abstract protected void BuildContent();

        public virtual PublishedStatus GetPublishedStatus(DataAudit data)
        {
            if (data != null && data.PublishedStatus != null) { return data.PublishedStatus.Value; }

            return PublishedStatus.Published;
        }

        protected string AsString(Guid? val)
        {
            return val == null || !val.HasValue ? string.Empty : val.ToString();
        }

        protected string AsString(int? val)
        {
            return val == null || !val.HasValue ? string.Empty : val.ToString();
        }

        protected string AsString(List<ResultWordGroup> wordGroups)
        {
            if (wordGroups == null || wordGroups.Count == 0) { return string.Empty; }
            var wordGroupStrings = wordGroups.Select(x => x.WordGroupName).ToList();
            return wordGroupStrings == null || wordGroupStrings.Count == 0 ? string.Empty : string.Join("~", wordGroupStrings.OrderBy(x => x));
        }

        protected string AsString(bool val)
        {
            var res = val ? 1 : 0;
            return res.ToString();
        }

        protected string AsBoolString(bool? val)
        {
            return val == null ? "No" : val.Value ? "Yes" : "No";
        }

        protected string AsString(bool? val)
        {
            return val == null ? string.Empty : AsString(val.Value);
        }

        protected string AsString(DateTime? val)
        {
            return val == null || !val.HasValue ? string.Empty : val.Value.ToString(CultureInfo.CurrentCulture);
        }

        protected string AsDateString(DateTime? val)
        {
            return val == null || !val.HasValue ? string.Empty : val.Value.Date.ToString(CultureInfo.CurrentCulture);
        }

        protected string AsString(List<ResultPromptWordGroup> val)
        {
            return val == null || val.Count == 0 ? string.Empty : string.Join(",", val.Select(x=>x.ResultPromptWord).OrderBy(x => x));
        }

        protected string AsString(List<string> val)
        {
            return val == null || val.Count == 0 ? string.Empty : string.Join(",", val.Select(x => x));
        }

        protected string AsStringLines(List<string> val)
        {
            return val == null || val.Count == 0 ? string.Empty : string.Join(Environment.NewLine, val.Select(x => x));
        }

        protected string AsString(List<Guid?> val)
        {
            if (val == null || val.Count == 0) { return string.Empty; }
            var nonNullValues = val.Where(x => x.HasValue).ToList();
            return nonNullValues.Count == 0 ? string.Empty : string.Join(",", val.Select(x => x.ToString()));
        }

        protected List<string> AsStringCollection(string val)
        {
            if (string.IsNullOrEmpty(val)) { return null; };
            return val.Split(',').Select(p => p.Trim()).ToList();
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
