using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public class ChangeReportData
    {
        public ChangeReportData(List<string> featureToggles, List<string> publicationTags, DateTime patchDate, bool isTestExport, bool isProdExport, string createdBy)
        {
            CreationDateTime = DateTime.Now;
            FeatureToggles = featureToggles;
            PublicationTags = publicationTags;
            PatchDate = patchDate;
            IsTestExport = isTestExport;
            IsProdExport = isProdExport;
            CreatedBy = createdBy;
            DeletedResults = new List<ChangeReportResultDataItem>();
            ModifiedResults = new List<ChangeReportResultDataItem>();
            ToggledResults = new List<ChangeReportResultDataItem>();
            NewResults = new List<ChangeReportResultDataItem>();
            DeletedNows = new List<ChangeReportNowDataItem>();
            ModifiedNows = new List<ChangeReportNowDataItem>();
            NewNows = new List<ChangeReportNowDataItem>();
            DeletedNowSubscriptions = new List<ChangeReportNowSubscriptionDataItem>();
            ModifiedNowSubscriptions = new List<ChangeReportNowSubscriptionDataItem>();
            NewNowSubscriptions = new List<ChangeReportNowSubscriptionDataItem>();
            DeletedFixedLists = new List<ChangeReportFixedListDataItem>();
            ModifiedFixedLists = new List<ChangeReportFixedListDataItem>();
            NewFixedLists = new List<ChangeReportFixedListDataItem>();
        }

        public List<string> FeatureToggles { get; }

        public List<string> PublicationTags { get; }

        public DateTime CreationDateTime { get; }

        public string CreatedBy { get; }

        public DateTime PatchDate { get; }

        public bool IsTestExport { get; }

        public bool IsProdExport { get; }

        public List<ChangeReportResultDataItem> DeletedResults { get; set; }

        public List<ChangeReportResultDataItem> ModifiedResults { get; set; }

        public List<ChangeReportResultDataItem> ToggledResults { get; set; }

        public List<ChangeReportResultDataItem> NewResults { get; set; }

        public List<ChangeReportNowDataItem> DeletedNows { get; set; }

        public List<ChangeReportNowDataItem> ModifiedNows { get; set; }

        public List<ChangeReportNowDataItem> NewNows { get; set; }

        public List<ChangeReportNowSubscriptionDataItem> DeletedNowSubscriptions { get; set; }

        public List<ChangeReportNowSubscriptionDataItem> ModifiedNowSubscriptions { get; set; }

        public List<ChangeReportNowSubscriptionDataItem> NewNowSubscriptions { get; set; }

        public List<ChangeReportFixedListDataItem> DeletedFixedLists { get; set; }

        public List<ChangeReportFixedListDataItem> ModifiedFixedLists { get; set; }

        public List<ChangeReportFixedListDataItem> NewFixedLists { get; set; }

    }
}
