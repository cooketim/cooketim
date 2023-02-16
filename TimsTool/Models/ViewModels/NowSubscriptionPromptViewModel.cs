using System;
using DataLib;

namespace Models.ViewModels
{
    public class NowSubscriptionPromptViewModel : ViewModelBase
    {
        ResultPromptRule prompt;

        public NowSubscriptionPromptViewModel(ResultPromptRule prompt, NowSubscriptionViewModel nowSubscription)
        {
            this.prompt = prompt;
            NowSubscription = nowSubscription;
        }

        public NowSubscriptionViewModel NowSubscription { get; private set; }

        public string ResultPromptDisplay
        {
            get => prompt.ResultPrompt.PromptReference;
        }

        public ResultPromptRule ResultPrompt
        {
            get => prompt;
            set
            {
                if (SetProperty(() => prompt.UUID == value.UUID, () => prompt = value))
                {
                    OnPropertyChanged("ResultPromptDisplay");
                    NowSubscription.LastModifiedDate = DateTime.Now;
                }
            }
        }
    }
}