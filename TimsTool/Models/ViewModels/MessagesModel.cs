using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ViewModels
{
    public class MessagesModel : ViewModelBase
    {
        public MessagesModel()
        {
            WindowTitle = "Data Validation";
        }
        private bool detailVisible = false;
        private bool showSummary = true;
        public bool ShowSummary 
        { 
            get
            {
                if (string.IsNullOrEmpty(DetailedMessages))
                {
                    return false;
                }
                return showSummary;
            }
        }
        public bool ShowDetail
        {
            get => !string.IsNullOrEmpty(DetailedMessages) && !ShowSummary;
        }

        public string WindowTitle { get; set; }
        public string Title { get; set; }
        public string Messages { get; set; }
        public string DetailedMessages { get; set; }
        public bool CancelVisible { get; set; }

        public bool SummaryVisible 
        {
            get => !DetailVisible;
        }

        public bool DetailVisible 
        {
            get => detailVisible;
            set
            {
                if (!string.IsNullOrEmpty(DetailedMessages))
                {
                    if (SetProperty(() => detailVisible == value, () => detailVisible = value))
                    {
                        detailVisible = value;
                        showSummary = !value;

                        OnPropertyChanged("SummaryVisible");
                        OnPropertyChanged("ShowSummary");
                        OnPropertyChanged("ShowDetail");
                    }
                }
            }
        }

        public bool DetailAvailable 
        { 
            get => !string.IsNullOrEmpty(DetailedMessages);
        }
    }
}
