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
    public class NowsBuilder : SheetBuilderBase
    {
        public NowsBuilder(WorkbookPart workbookPart, AllData allData) : base("NOWs", workbookPart, allData) { }

        protected override void BuildContent()
        {
            //Construct Header Row
            Row row = new Row();
            row.Append(
                ConstructCell("NOW/EDT UUID", CellValues.String),
                ConstructCell("NOW/EDT Name", CellValues.String),
                ConstructCell("Welsh NOW/EDT Name", CellValues.String),
                ConstructCell("Template Name", CellValues.String),
                ConstructCell("Sub Template Name", CellValues.String),
                ConstructCell("Bilingual Template Name", CellValues.String),
                ConstructCell("Jurisdiction", CellValues.String),
                ConstructCell("Rank", CellValues.String),
                ConstructCell("Urgent time limit in minutes", CellValues.String),
                ConstructCell("Remote Printing Required", CellValues.String),
                ConstructCell("IsStorageRequired", CellValues.String),
                ConstructCell("IsEDT", CellValues.String),
                ConstructCell("IncludeCaseMarkers", CellValues.String),
                ConstructCell("IncludePNCID", CellValues.String),
                ConstructCell("IncludeConvictionStatus", CellValues.String),
                ConstructCell("IncludeNationality", CellValues.String),
                ConstructCell("IncludeDefendantASN", CellValues.String),
                ConstructCell("IncludeDefendantMobileNumber", CellValues.String),
                ConstructCell("IncludeDefendantLandlineNumber", CellValues.String),
                ConstructCell("IncludeDefendantNINO", CellValues.String),
                ConstructCell("IncludeDefendantEthnicity", CellValues.String),
                ConstructCell("IncludeDefendantGender", CellValues.String),
                ConstructCell("IncludeSolicitorsNameAddress", CellValues.String),
                ConstructCell("IncludeDVLAOffenceCode", CellValues.String),
                ConstructCell("IncludeDriverNumber", CellValues.String),
                ConstructCell("IncludePlea", CellValues.String),
                ConstructCell("IncludeVehicleRegistration", CellValues.String),
                ConstructCell("Start Date", CellValues.String),
                ConstructCell("End Date", CellValues.String),
                ConstructCell("Created Date", CellValues.String),
                ConstructCell("Last Modified Date", CellValues.String),
                ConstructCell("Deleted Date", CellValues.String),
                ConstructCell("Publication Status", CellValues.String)
                );
            SheetData.AppendChild(row);

            //Process the data
            AppendData();
        }

        private void AppendData()
        {
            foreach (var now in AllData.Nows.OrderBy(x=>x.Name))
            {
                var row = new Row();
                row.Append(
                    ConstructCell(AsString(now.UUID), CellValues.String),
                    ConstructCell(now.Name, CellValues.String),
                    ConstructCell(now.WelshNowName, CellValues.String),
                    ConstructCell(now.TemplateName, CellValues.String),
                    ConstructCell(now.SubTemplateName, CellValues.String),
                    ConstructCell(now.BilingualTemplateName, CellValues.String),
                    ConstructCell(now.Jurisdiction, CellValues.String),
                    ConstructIntCell(now.Rank),
                    ConstructIntCell(now.UrgentTimeLimitInMinutes),
                    ConstructCell(AsString(now.RemotePrintingRequired), CellValues.Boolean),
                    ConstructCell(AsString(now.IsStorageRequired), CellValues.Boolean),
                    ConstructCell(AsString(now.IsEDT), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeCaseMarkers), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludePNCID), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeConvictionStatus), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeNationality), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeDefendantASN), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeDefendantMobileNumber), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeDefendantLandlineNumber), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeDefendantNINO), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeDefendantEthnicity), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeDefendantGender), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeSolicitorsNameAddress), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeDVLAOffenceCode), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeDriverNumber), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludePlea), CellValues.Boolean),
                    ConstructCell(AsString(now.IncludeVehicleRegistration), CellValues.Boolean),
                    ConstructDateCell(now.StartDate),
                    ConstructDateCell(now.EndDate),
                    ConstructDateCell(now.CreatedDate),
                    ConstructDateCell(now.LastModifiedDate),
                    ConstructDateCell(now.DeletedDate),
                    ConstructCell(GetPublishedStatus(now).GetDescription(), CellValues.String)
                    );
                SheetData.AppendChild(row);
            }
        }
    }
}
