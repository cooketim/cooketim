using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Models.ViewModels
{
    public interface IReportViewModel
    {
        bool BtnOKEnabled { get; }
        SilentObservableCollection<ReportItemViewModel> ReportTypes { get; }
        ReportItemViewModel SelectedReport { get; set; }
    }

    /// <summary>
    /// A UI-friendly model for reporting
    /// </summary>
    public class ReportViewModel : ViewModelBase, IReportViewModel
    {
        SilentObservableCollection<ReportItemViewModel> reports;
        ReportItemViewModel selectedReport = null;

        public ReportViewModel(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
            var listOfReports = new List<ReportItemViewModel>()
            {
                new ReportItemViewModel("Full Data Export","fullextract","ResultRefData_FullExport.xlsx"),
                
                new ReportItemViewModel("Synonym Report","synonymextract","ResultRefData_SynonymExport.xlsx"),
                new ReportItemViewModel("Published Group Result Definitions","groupresultreport","PublishedGroupResultReport.csv"),
                new ReportItemViewModel("Published Name & Address Prompt Report","nameaddressreport","PublishedNameAddressReport.csv"),
                new ReportItemViewModel("Associated Reference Data Prompt Report","referencedatareport","AssociatedReferenceDataPromptReport.csv"),
                new ReportItemViewModel("NOW Result Prompt Rule Exception Report","nowpromptrulereport","NowPromptRuleReport.csv"),
                new ReportItemViewModel("Court Extract Report","courtextractreport","Court_Extract_Report.xlsx"),
                new ReportItemViewModel("NOW EDT Child Result Subscription Report", "nowsubscriptionchildresultreport","NOWEDT_Subscription_ChildResult_Report.xlsx"),
                new ReportItemViewModel("NOW EDT Subscription Report", "nowsubscriptionreport","NOWEDT_Subscription_Report.xlsx"),
                new ReportItemViewModel("Subscription Email Report", "subscriptionemailreport","Subscription_Email_Report.xlsx"),                
                new ReportItemViewModel("NOW Primary Result Report", "nowprimaryresultreport","NOW_PrimaryResult_Report.xlsx"),
                new ReportItemViewModel("Cached Results Report", "cacheditemsreport","CachedResults_Report.xlsx"),
                new ReportItemViewModel("Court Order Results Report","courtorderresultreport","CourtOrderResultReport.csv"),
                new ReportItemViewModel("Primary Results For Defendant and Defendant Case Report", "primaryResultLevelReport", "PrimaryResultLevelReport.csv"),
                new ReportItemViewModel("NOW EDT Now Requirement Text Report", "nowrequirementtextreport", "NowRequirementTextReport.csv"),
                new ReportItemViewModel("Prompt Reference Report", "promptreferencereport", "PromptReferenceReport.xlsx"),
                new ReportItemViewModel("NOW EDT Template Name Report", "templatenamereport", "NowEDTTemplateNameReport.csv"),
                new ReportItemViewModel("Fixed List Report", "fixedlistreport", "FixedListReport.xlsx"),
                new ReportItemViewModel("Parent Child Shared Prompt Report", "childresultduplicatepromptReport", "ParentChildSharedPromptReport.csv"),
                new ReportItemViewModel("Restricted Results Report", "restrictedresultsreport","RestrictedResultsReport.xlsx"),
                new ReportItemViewModel("Bad Subscription Report","badsubscriptionreport","BadSubscriptionReport.csv"),
                new ReportItemViewModel("Result Prompt Duration Word Group Conflict Report","resultpromptdurationwordgroupconflictreport","ResultPromptDurationWordGroupConflictReport.csv"),
                new ReportItemViewModel("Judicial Validation Report","judicialvalidationreport","JudicialValidtionReport.xlsx"),
                new ReportItemViewModel("Missing Result Sharing Rules Report","missingsharingrulesreport","MissingSharingRulesReport.xlsx"),
                new ReportItemViewModel("Results That Are Always Published","alwayspublishedreport","AlwaysPublishedResultsReport.xlsx"),
                new ReportItemViewModel("Custodial Results Report","custodialresultsreport","CustodialResultsReport.xlsx"),
                new ReportItemViewModel("Result Text Report","resulttextreport","ResultTextReport.xlsx"),
                new ReportItemViewModel("Results Published As A Prompt Report","createpublishaspromptreport","ResultsPublishedAsAPromptReport.xlsx"),
                new ReportItemViewModel("Enforcement Results Report","enforcementresultsreport","EnforcementResultsReport.xlsx"),
            };

            reports = new SilentObservableCollection<ReportItemViewModel>(listOfReports.OrderBy(x => x.ReportName));
        }

        #region ReportViewModel Properties

        public SilentObservableCollection<ReportItemViewModel> ReportTypes
        {
            get => reports;
        }

        public ReportItemViewModel SelectedReport
        {
            get => selectedReport;
            set
            {
                selectedReport = value;
                OnPropertyChanged("BtnOKEnabled");
            }
        }

        public bool BtnOKEnabled
        {
            get => selectedReport != null;
        }

        #endregion ReportViewModel Properties

    }

    public class ReportItemViewModel : ViewModelBase
    {
        public string ReportName
        {
            get; 
        }

        public string ReportShortName
        {
            get;
        }

        public string FileName
        {
            get;
        }

        public ReportItemViewModel(string reportName, string reportShortName, string fileName)
        {
            ReportName = reportName;
            ReportShortName = reportShortName;
            FileName = fileName;
        }
    }
}