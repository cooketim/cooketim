using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Models.ViewModels
{
    public class DurationElementViewModel : ViewModelBase
    {
        readonly ResultPromptDuration duration;
        readonly ResultPromptViewModel prompt;

        public DurationElementViewModel(ResultPromptDuration duration, ResultPromptViewModel prompt)
        {
            this.duration = duration;
            this.prompt = prompt;
        }

        public ResultPromptDuration Duration
        {
            get => duration;
        }

        public string DurationElement
        {
            get => duration.DurationElement;
            set
                {
                    if (SetProperty(() => duration.DurationElement == value, () => duration.DurationElement = value))
                    {
                        prompt.LastModifiedDate = DateTime.Now;

                        //null out the welsh translation since this item is now required to be retranslated
                        WelshDurationElement = null;
                    }
                }
        }

        public string WelshDurationElement
        {
            get => duration.WelshDurationElement;
            set
            {
                if (SetProperty(() => duration.WelshDurationElement == value, () => duration.WelshDurationElement = value))
                {
                    prompt.LastModifiedDate = DateTime.Now;
                }
            }
        }

        public override bool IsReadOnly 
        { 
            get => prompt.IsReadOnly;  
        }
    }
}