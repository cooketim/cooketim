using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ViewModels
{
    public class CascadeNowTextProgress: ViewModelBase
    {
        public CascadeNowTextProgress(string progressText, int percentage)
        {
            ProgressText = progressText;
            Percentage = percentage;
        }

        public string ProgressText { get; }
        public int Percentage { get; }
    }
}
