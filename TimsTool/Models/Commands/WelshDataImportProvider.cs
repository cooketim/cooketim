using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Models.ViewModels;
using DocumentFormat.OpenXml;
using System.IO;

namespace Models.Commands
{
    public interface IWelshDataImportProvider { }
    public class WelshDataImportProvider : DataImportProviderBase, IWelshDataImportProvider
    {
        private ITreeModel treeModel;
        
        public WelshDataImportProvider(ITreeModel treeModel, string filePath, string outFilePath) : base(filePath, outFilePath) 
        {
            this.treeModel = treeModel;
        }

        protected override void PostLoad()
        {
            //nothing to do
        }

        protected override void ProcessRow(WorkbookPart workbookPart, Row row, List<string> fieldNames, string sheetName, SheetData outSheetData)
        {
            switch (sheetName.ToLower())
            {
                case "result definitions":
                    ProcessResultDefinitions(workbookPart, row, fieldNames, outSheetData);
                    break;
                case "result prompt":
                    ProcessResultPrompts(workbookPart, row, fieldNames, outSheetData);
                    break;
                case "fixed list":
                    ProcessFixedLists(workbookPart, row, fieldNames, outSheetData);
                    break;
                case "nows edts":
                    ProcessNowsEdts(workbookPart, row, fieldNames, outSheetData);
                    break;
                case "nows edts text":
                    ProcessNowsEdtText(workbookPart, row, fieldNames, outSheetData);
                    break;
                case "nows edts requirement text":
                    ProcessNowsEdtRequirementText(workbookPart, row, fieldNames, outSheetData);
                    break;
                default:
                    ProcessUnknown(workbookPart, row, outSheetData);
                    break;
            }
        }

        private void ProcessUnknown(WorkbookPart workbookPart, Row row, SheetData outSheetData)
        {
            columnIndex = 0;
            var outputRow = new Row();

            foreach (Cell cell in row)
            {
                var val = GetCellValue(workbookPart, cell);
                //set the output row
                outputRow.Append(ConstructCell(string.IsNullOrEmpty(val) ? string.Empty : val, CellValues.String));
            }

            outputRow.Append(ConstructCell("Unknown Sheet Value", CellValues.String));

            //write the row to the output workbook
            outSheetData.AppendChild(outputRow);
        }

        private void ProcessResultDefinitions(WorkbookPart workbookPart, Row row, List<string> fieldNames, SheetData outSheetData)
        {
            columnIndex = 0;
            var outputRow = new Row();
            ResultDefinitionViewModel match = null;
            string rowStatus = "Nothing Updated";
            foreach (Cell cell in row)
            {
                var index = GetColumnIndex(cell.CellReference);
                if (index == fieldNames.Count) { break; }
                var fieldName = fieldNames[index].ToLowerInvariant();
                var val = GetCellValue(workbookPart, cell);
                var newVal = string.Empty;

                //process the values
                switch (fieldName)
                {
                    case "uuid":
                        var id = AsNullableGuid(val);
                        if (id != null)
                        {
                            match = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.UUID == id);
                        }
                        if (match == null)
                        {
                            rowStatus = "Not Matched";
                        }
                        break;
                    case "welsh label":
                        newVal = AsString(val);
                        if (match != null)
                        {
                            if (!string.IsNullOrEmpty(newVal))
                            {
                                if (string.IsNullOrEmpty(match.WelshLabel) || match.WelshLabel.ToLowerInvariant() != newVal.ToLowerInvariant())
                                {
                                    match.WelshLabel = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(match.WelshLabel))
                                {
                                    match.WelshLabel = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                        }
                        break;
                    case "welsh result wording":
                        newVal = AsString(val);
                        if (match != null)
                        {
                            if (!string.IsNullOrEmpty(newVal))
                            {
                                if (string.IsNullOrEmpty(match.WelshResultWording) || match.WelshResultWording.ToLowerInvariant() != newVal.ToLowerInvariant())
                                {
                                    match.WelshResultWording = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(match.WelshResultWording))
                                {
                                    match.WelshResultWording = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }

                        }
                        break;
                    default:
                        break;
                }

                //set the output row
                outputRow.Append(ConstructCell(string.IsNullOrEmpty(val) ? string.Empty : val, CellValues.String));
            }

            outputRow.Append(ConstructCell(rowStatus, CellValues.String));

            //write the row to the output workbook
            outSheetData.AppendChild(outputRow);
        }

        private void ProcessResultPrompts(WorkbookPart workbookPart, Row row, List<string> fieldNames, SheetData outSheetData)
        {
            columnIndex = 0;
            var outputRow = new Row();
            ResultPromptRuleViewModel match = null;
            string rowStatus = "Nothing Updated";
            foreach (Cell cell in row)
            {
                var index = GetColumnIndex(cell.CellReference);
                if (index == fieldNames.Count) { break; }
                var fieldName = fieldNames[index];
                var val = GetCellValue(workbookPart, cell);
                var newVal = string.Empty;

                //process the values
                switch (fieldName)
                {
                    case "uuid":
                        var id = AsNullableGuid(val);
                        if (id != null)
                        {
                            match = treeModel.AllResultPromptViewModel.Prompts.FirstOrDefault(x => x.ResultPromptViewModel.UUID == id);
                        }
                        if (match == null)
                        {
                            rowStatus = "Not Matched";
                        }
                        break;
                    case "welsh label":
                        newVal = AsString(val);
                        if (match != null)
                        {
                            if (!string.IsNullOrEmpty(newVal))
                            {
                                if (string.IsNullOrEmpty(match.ResultPromptViewModel.WelshLabel) || match.ResultPromptViewModel.WelshLabel.ToLowerInvariant() != newVal.ToLowerInvariant())
                                {
                                    match.ResultPromptViewModel.WelshLabel = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(match.ResultPromptViewModel.WelshLabel))
                                {
                                    match.ResultPromptViewModel.WelshLabel = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                        }
                        break;
                    case "welsh result wording":
                        newVal = AsString(val);
                        if (match != null)
                        {
                            if (!string.IsNullOrEmpty(newVal))
                            {
                                if (string.IsNullOrEmpty(match.ResultPromptViewModel.WelshWording) || match.ResultPromptViewModel.WelshWording.ToLowerInvariant() != newVal.ToLowerInvariant())
                                {
                                    match.ResultPromptViewModel.WelshWording = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(match.ResultPromptViewModel.WelshWording))
                                {
                                    match.ResultPromptViewModel.WelshWording = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                //set the output row
                outputRow.Append(ConstructCell(string.IsNullOrEmpty(val) ? string.Empty : val, CellValues.String));
            }

            outputRow.Append(ConstructCell(rowStatus, CellValues.String));

            //write the row to the output workbook
            outSheetData.AppendChild(outputRow);
        }

        private void ProcessFixedLists(WorkbookPart workbookPart, Row row, List<string> fieldNames, SheetData outSheetData)
        {
            columnIndex = 0;
            var outputRow = new Row();
            FixedListViewModel matchFL = null;
            FixedListValueViewModel matchFLV = null;
            string rowStatus = "Nothing Updated";
            foreach (Cell cell in row)
            {
                var index = GetColumnIndex(cell.CellReference);
                if (index == fieldNames.Count) { break; }
                var fieldName = fieldNames[index].ToLowerInvariant();
                var val = GetCellValue(workbookPart, cell);
                var newVal = string.Empty;

                //process the values
                switch (fieldName)
                {
                    case "uuid":
                        var id = AsNullableGuid(val);
                        if (id != null)
                        {
                            matchFL = treeModel.AllFixedListViewModel.FixedLists.FirstOrDefault(x => x.UUID == id);
                        }
                        break;
                    case "code":
                        if (!string.IsNullOrEmpty(val) && matchFL != null && matchFL.FixedListValues != null)
                        {
                            matchFLV = matchFL.FixedListValues.FirstOrDefault(x=>!string.IsNullOrEmpty(x.Code) && x.Code.ToLowerInvariant() == val.ToLowerInvariant());
                        }
                        if (matchFLV == null)
                        {
                            rowStatus = "Not Matched";
                        }
                        break;
                    case "welsh value":
                        newVal = AsString(val);
                        if (matchFLV != null)
                        {
                            if (!string.IsNullOrEmpty(newVal))
                            {
                                if (string.IsNullOrEmpty(matchFLV.WelshValue) || matchFLV.WelshValue.ToLowerInvariant() != newVal.ToLowerInvariant())
                                {
                                    matchFLV.WelshValue = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(matchFLV.WelshValue))
                                {
                                    matchFLV.WelshValue = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                //set the output row
                outputRow.Append(ConstructCell(string.IsNullOrEmpty(val) ? string.Empty : val, CellValues.String));
            }

            outputRow.Append(ConstructCell(rowStatus, CellValues.String));

            //write the row to the output workbook
            outSheetData.AppendChild(outputRow);
        }

        private void ProcessNowsEdts(WorkbookPart workbookPart, Row row, List<string> fieldNames, SheetData outSheetData)
        {
            columnIndex = 0;
            var outputRow = new Row();
            NowViewModel matchNow = null;
            string rowStatus = "Nothing Updated";
            foreach (Cell cell in row)
            {
                var index = GetColumnIndex(cell.CellReference);
                if (index == fieldNames.Count) { break; }
                var fieldName = fieldNames[index].ToLowerInvariant();
                var val = GetCellValue(workbookPart, cell);
                var newVal = string.Empty;

                //process the values
                switch (fieldName)
                {
                    case "now/edt uuid":
                        var id = AsNullableGuid(val);
                        if (id != null)
                        {
                            matchNow = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.UUID == id);
                        }
                        if (matchNow == null)
                        {
                            matchNow = treeModel.AllEDTsViewModel.EDTs.FirstOrDefault(x => x.UUID == id);
                        }
                        if (matchNow == null)
                        {
                            rowStatus = "Not Matched";
                        }
                        break;
                    case "welsh now/edt name":
                        newVal = AsString(val);
                        if (matchNow != null)
                        {
                            if (!string.IsNullOrEmpty(newVal))
                            {
                                if (string.IsNullOrEmpty(matchNow.WelshNowName) || matchNow.WelshNowName.ToLowerInvariant() != newVal.ToLowerInvariant())
                                {
                                    matchNow.WelshNowName = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(matchNow.WelshNowName))
                                {
                                    matchNow.WelshNowName = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                //set the output row
                outputRow.Append(ConstructCell(string.IsNullOrEmpty(val) ? string.Empty : val, CellValues.String));
            }

            outputRow.Append(ConstructCell(rowStatus, CellValues.String));

            //write the row to the output workbook
            outSheetData.AppendChild(outputRow);
        }

        private void ProcessNowsEdtText(WorkbookPart workbookPart, Row row, List<string> fieldNames, SheetData outSheetData)
        {
            columnIndex = 0;
            var outputRow = new Row();
            NowViewModel matchNow = null;
            NowTextViewModel matchNowText = null;
            string rowStatus = "Nothing Updated";
            foreach (Cell cell in row)
            {
                var index = GetColumnIndex(cell.CellReference);
                if (index == fieldNames.Count) { break; }
                var fieldName = fieldNames[index].ToLowerInvariant();
                var val = GetCellValue(workbookPart, cell);
                var newVal = string.Empty;

                //process the values
                switch (fieldName)
                {
                    case "now/edt uuid":
                        var id = AsNullableGuid(val);
                        if (id != null)
                        {
                            matchNow = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.UUID == id);
                        }
                        if (matchNow == null)
                        {
                            matchNow = treeModel.AllEDTsViewModel.EDTs.FirstOrDefault(x => x.UUID == id);
                        }
                        break;
                    case "text reference":
                        if (!string.IsNullOrEmpty(val) && matchNow != null && matchNow.TextValues != null)
                        {
                            matchNowText = matchNow.TextValues.FirstOrDefault(x => !string.IsNullOrEmpty(x.NowReference) && x.NowReference.ToLowerInvariant() == val.ToLowerInvariant());
                        }
                        if (matchNowText == null)
                        {
                            rowStatus = "Not Matched";
                        }
                        break;
                    case "welsh now/edt text":
                        newVal = AsString(val);
                        if (matchNowText != null)
                        {
                            if (!string.IsNullOrEmpty(newVal))
                            {
                                if (string.IsNullOrEmpty(matchNowText.NowWelshText) || matchNowText.NowWelshText.ToLowerInvariant() != newVal.ToLowerInvariant())
                                {
                                    matchNowText.NowWelshText = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(matchNowText.NowWelshText))
                                {
                                    matchNowText.NowWelshText = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                //set the output row
                outputRow.Append(ConstructCell(string.IsNullOrEmpty(val) ? string.Empty : val, CellValues.String));
            }

            outputRow.Append(ConstructCell(rowStatus, CellValues.String));

            //write the row to the output workbook
            outSheetData.AppendChild(outputRow);
        }

        private void ProcessNowsEdtRequirementText(WorkbookPart workbookPart, Row row, List<string> fieldNames, SheetData outSheetData)
        {
            columnIndex = 0;
            var outputRow = new Row();
            NowRequirementViewModel matchNowReq = null;
            NowRequirementTextViewModel matchNowReqText = null;
            string rowStatus = "Nothing Updated";
            foreach (Cell cell in row)
            {
                var index = GetColumnIndex(cell.CellReference);
                if (index == fieldNames.Count) { break; }
                var fieldName = fieldNames[index].ToLowerInvariant();
                var val = GetCellValue(workbookPart, cell);
                var newVal = string.Empty;

                //process the values
                switch (fieldName)
                {
                    case "now/edt requirement uuid":
                        var id = AsNullableGuid(val);
                        if (id != null)
                        {
                            matchNowReq = treeModel.AllNowRequirementsViewModel.NowRequirements.FirstOrDefault(x => x.UUID == id);
                        }
                        break;
                    case "text reference":
                        if (!string.IsNullOrEmpty(val) && matchNowReq != null && matchNowReq.TextValues != null)
                        {
                            matchNowReqText = matchNowReq.TextValues.FirstOrDefault(x => !string.IsNullOrEmpty(x.NowReference) && x.NowReference.ToLowerInvariant() == val.ToLowerInvariant());
                        }
                        if (matchNowReqText == null)
                        {
                            rowStatus = "Not Matched";
                        }
                        break;
                    case "welsh now/edt text":
                        newVal = AsString(val);
                        if (matchNowReqText != null)
                        {
                            if (!string.IsNullOrEmpty(newVal))
                            {
                                if (string.IsNullOrEmpty(matchNowReqText.NowWelshText) || matchNowReqText.NowWelshText.ToLowerInvariant() != newVal.ToLowerInvariant())
                                {
                                    matchNowReqText.NowWelshText = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(matchNowReqText.NowWelshText))
                                {
                                    matchNowReqText.NowWelshText = newVal;
                                    rowStatus = "Updates Applied";
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                //set the output row
                outputRow.Append(ConstructCell(string.IsNullOrEmpty(val) ? string.Empty : val, CellValues.String));
            }

            outputRow.Append(ConstructCell(rowStatus, CellValues.String));

            //write the row to the output workbook
            outSheetData.AppendChild(outputRow);
        }
    }
}
