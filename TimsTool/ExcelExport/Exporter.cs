using DataLib;
using DataLib.DataModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.IO;

namespace ExportExcel
{
    public class Exporter
    {
        AllData allData;
        IdMap map;

        public Exporter (AllData allData, IdMap map)
        {
            this.allData = allData;
            this.map = map;
        }

        public void CreateDataPatchChangeExcelExport(Stream stream, ChangeReportData changeReportData)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new DataPatchSelectedOptionBuilder(workbookPart,allData, changeReportData),
                    new DataPatchResultDefinitionBuilder(workbookPart,allData, changeReportData),
                    new DataPatchFixedListBuilder(workbookPart,allData, changeReportData),
                    new DataPatchNowBuilder(workbookPart,allData, changeReportData),
                    new DataPatchNowSubscriptionBuilder(workbookPart,allData, changeReportData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
            
        }
        public void CreateFullExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new ResultDefinitionBuilder(workbookPart,allData)
                    ,new ResultDefinitionWordBuilder(workbookPart,allData)
                    ,new ResultDefinitionRuleBuilder(workbookPart,allData)
                    ,new ResultPromptBuilder(workbookPart,allData)
                    ,new ResultPromptWordBuilder(workbookPart,allData)
                    ,new FixedListBuilder(workbookPart,allData)
                    ,new ResultPromptRuleBuilder(workbookPart,allData)
                    ,new NowsBuilder(workbookPart,allData)
                    ,new NowsTextBuilder(workbookPart,allData)
                    ,new NowRequirementsBuilder(workbookPart,allData)
                    ,new NowRequirementPromptsBuilder(workbookPart,allData)
                    ,new NowRequirementsTextBuilder(workbookPart,allData)
                    ,new NowSubscriptionBuilder(workbookPart,allData)
                    ,new NowSubscriptionIncludedExcludedBuilder(workbookPart,allData,false)
                    ,new NowSubscriptionIncludedExcludedBuilder(workbookPart,allData,true)
                    ,new IdMapBuilder(workbookPart,allData,map)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum+1);
                }

                workbookPart.Workbook.Save();
            }
        }
        public void CreateSynonymExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new ResultDefinitionWordSynonymExportBuilder(workbookPart,allData),
                    new ResultPromptWordSynonymExportBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateJudicialValidationExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new JudicialValidationResultDefinitionExportBuilder(workbookPart,allData),
                    new JudicialValidationResultPromptExportBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateMissingSharingRulesExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new MissingSharingRulesResultDefinitionExportBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }        

        public void CreateCourtExtractExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new CourtExtractExportBuilder(workbookPart,allData),
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateRestrictedResultsExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new RestrictedResultsBuilder(workbookPart,allData),
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateCachedItemsReport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new CachedResultDefinitionExportBuilder(workbookPart,allData),
                    new CachedResultPromptExportBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreatePromptReferenceExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new PromptReferenceExportBuilder(workbookPart,allData),
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateNowPrimaryResultExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new NowPrimaryResultReportBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateAlwaysPublishedResultExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new ResultDefinitionAlwaysPublishedBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateResultTextExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new ResultDefinitionTextBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }        

        public void CreateCustodialResultsExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new ResultsWithPrisonPromptBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreatePublishAsPromptExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new PublishAsPromptResultsBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateEnforcementResultsExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new EnforcementResultsBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }        

        public void CreateNowSubscriptionExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new NowSubscriptionReportBuilder(workbookPart,allData, false)
                    ,new NowSubscriptionReportBuilder(workbookPart,allData, true)
                    ,new NowSubscriptionBuilder(workbookPart,allData)
                    ,new NowSubscriptionIncludedExcludedBuilder(workbookPart,allData,true)
                    ,new NowSubscriptionIncludedExcludedBuilder(workbookPart,allData,false)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateSubscriptionEmailExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new SubscriptionEmailReportBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateFixedListExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new FixedListBuilder(workbookPart,allData)
                    ,new ResultPromptWithFixedListsBuilder(workbookPart,allData)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        public void CreateNowSubscriptionChildResultExcelExport(Stream stream)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                     new PrimaryChildResultBuilder(workbookPart,allData, false)
                    ,new PrimaryChildResultBuilder(workbookPart,allData, true)
                    ,new NowSubscriptionBuilder(workbookPart,allData)
                    ,new NowSubscriptionIncludedExcludedBuilder(workbookPart,allData,true)
                    ,new NowSubscriptionIncludedExcludedBuilder(workbookPart,allData,false)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }

        // Makes a new work book
        public static Workbook MakeWorkbook()
        {
            Workbook workbook = new Workbook() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x15 xr xr6 xr10 xr2" } };
            workbook.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            workbook.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            workbook.AddNamespaceDeclaration("x15", "http://schemas.microsoft.com/office/spreadsheetml/2010/11/main");
            workbook.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
            workbook.AddNamespaceDeclaration("xr6", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision6");
            workbook.AddNamespaceDeclaration("xr10", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision10");
            workbook.AddNamespaceDeclaration("xr2", "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");
            FileVersion fileVersion = new FileVersion() { ApplicationName = "xl", LastEdited = "7", LowestEdited = "7", BuildVersion = "21328" };
            WorkbookProperties workbookProperties = new WorkbookProperties() { DefaultThemeVersion = (UInt32Value)166925U };

           
            OpenXmlUnknownElement openXmlUnknownElement = OpenXmlUnknownElement.CreateOpenXmlUnknownElement("<xr:revisionPtr revIDLastSave=\"0\" documentId=\"8_{295ED6B3-5C0F-4A21-A177-1FC1F6F9EDC0}\" xr6:coauthVersionLast=\"41\" xr6:coauthVersionMax=\"41\" xr10:uidLastSave=\"{00000000-0000-0000-0000-000000000000}\" xmlns:xr10=\"http://schemas.microsoft.com/office/spreadsheetml/2016/revision10\" xmlns:xr6=\"http://schemas.microsoft.com/office/spreadsheetml/2016/revision6\" xmlns:xr=\"http://schemas.microsoft.com/office/spreadsheetml/2014/revision\" />");

            BookViews bookViews = new BookViews();

            WorkbookView workbookView = new WorkbookView() { XWindow = 38400, YWindow = 390, WindowWidth = (UInt32Value)28800U, WindowHeight = (UInt32Value)14955U, ActiveTab = (UInt32Value)1U };
            workbookView.SetAttribute(new OpenXmlAttribute("xr2", "uid", "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2", "{1CD3E025-FFC5-4B8E-BF01-C933FF7C55A4}"));

            bookViews.Append(workbookView);

            Sheets sheets = new Sheets();

            workbook.Append(fileVersion);
            workbook.Append(workbookProperties);
            workbook.Append(bookViews);
            workbook.Append(sheets);
            return workbook;
        }

        public void NowsEDTsImportReport(Stream stream, List<NowImport> processedNows, List<NowImport> processedEDTs)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = MakeWorkbook();

                var builders = new List<SheetBuilderBase>()
                {
                    new NowImportReportBuilder(workbookPart,processedNows,"NOWs SIR Report Full Matches", 1, false)
                    ,new NowImportReportBuilder(workbookPart,processedNows,"NOWs SIR Report Partial Matches", 2, false)
                    ,new NowImportReportBuilder(workbookPart,processedNows,"NOWs SIR Report Full Mismatches", 3, false)
                    ,new NowImportReportBuilder(workbookPart,processedEDTs,"EDTs SIR Report Full Matches", 1, true)
                    ,new NowImportReportBuilder(workbookPart,processedEDTs,"EDTs SIR Report Partial Matches", 2, true)
                    ,new NowImportReportBuilder(workbookPart,processedEDTs,"EDTs SIR Report Full Mismatches", 3, true)
                };

                for (var sheetNum = 0; sheetNum < builders.Count; sheetNum++)
                {
                    builders[sheetNum].Build(sheetNum + 1);
                }

                workbookPart.Workbook.Save();
            }
        }
    }
}
