using DataLib;
using System.Collections.Generic;

namespace ExportExcel
{
    public class ChangeReportResultDataItem
    {
        public ChangeReportResultDataItem(ResultDefinition data, List<Comment> comments)
        {
            Data = data;
            Comments = comments;
            NewPrompts = new List<ChangeReportResultPromptDataItem>();
            ModifiedPrompts = new List<ChangeReportResultPromptDataItem>();
            DeletedPrompts = new List<ChangeReportResultPromptDataItem>();
            RemovedPrompts = new List<ChangeReportResultPromptDataItem>();
        }

        public ResultDefinition Data { get; set; }

        public List<DataLib.Comment> Comments { get; set; }

        public List<ChangeReportResultPromptDataItem> DeletedPrompts { get; }
        public List<ChangeReportResultPromptDataItem> RemovedPrompts { get; }
        public List<ChangeReportResultPromptDataItem> ModifiedPrompts { get; }
        public List<ChangeReportResultPromptDataItem> NewPrompts { get; }
    }

    public class ChangeReportResultPromptDataItem
    {
        public ChangeReportResultPromptDataItem(ResultPromptRule data, List<Comment> comments)
        {
            Data = data;
            Comments = comments;
        }

        public ResultPromptRule Data { get; set; }

        public List<DataLib.Comment> Comments { get; set; }
    }

    public class ChangeReportNowDataItem
    {
        public ChangeReportNowDataItem(Now data, List<Comment> comments)
        {
            Data = data;
            Comments = comments;
        }

        public Now Data { get; set; }

        public List<DataLib.Comment> Comments { get; set; }
    }
    public class ChangeReportNowSubscriptionDataItem
    {
        public ChangeReportNowSubscriptionDataItem(NowSubscription data, List<Comment> comments)
        {
            Data = data;
            Comments = comments;
        }

        public NowSubscription Data { get; set; }

        public List<DataLib.Comment> Comments { get; set; }
    }
    public class ChangeReportFixedListDataItem
    {
        public ChangeReportFixedListDataItem(FixedList data, List<Comment> comments)
        {
            Data = data;
            Comments = comments;
        }

        public FixedList Data { get; set; }

        public List<DataLib.Comment> Comments { get; set; }
    }
}
