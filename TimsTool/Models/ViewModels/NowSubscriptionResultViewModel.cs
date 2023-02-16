using System;
using DataLib;

namespace Models.ViewModels
{
    public class NowSubscriptionResultViewModel : ViewModelBase
    {
        ResultDefinition result;

        public NowSubscriptionResultViewModel(ResultDefinition result, NowSubscriptionViewModel nowSubscription)
        {
            this.result = result;
            NowSubscription = nowSubscription;
        }

        public NowSubscriptionViewModel NowSubscription { get; private set; }

        public string ResultDisplay
        {
            get => result.Label;
        }

        public ResultDefinition ResultDefinition
        {
            get => result;
            set
            {
                if (SetProperty(() => result.UUID == value.UUID, () => result = value))
                {
                    OnPropertyChanged("ResultDisplay");
                    NowSubscription.LastModifiedDate = DateTime.Now;
                }
            }
        }
    }
}