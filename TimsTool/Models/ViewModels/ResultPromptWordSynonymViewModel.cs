using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Models.ViewModels
{
    public class ResultPromptWordSynonymViewModel : ViewModelBase
    {
        public ResultPromptWordSynonymViewModel(ResultPromptWordSynonym synonym, bool isReadonly)
        {
            ResultPromptWordSynonym = synonym;
            IsReadOnly = isReadonly;
        }

        public ResultPromptWordSynonym ResultPromptWordSynonym
        {
            get { return data as ResultPromptWordSynonym; }
            private set { data = value; }
        }

        public string Synonym
        {
            get => ResultPromptWordSynonym.Synonym;
            set
            {
                if (SetProperty(() => ResultPromptWordSynonym.Synonym == value, () => ResultPromptWordSynonym.Synonym = value))
                {
                    LastModifiedDate = DateTime.Now;
                }
            }
        }
    }
}