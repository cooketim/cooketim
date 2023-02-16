using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Models.ViewModels
{
    public class ResultDefinitionWordSynonymViewModel : ViewModelBase
    {
        public ResultDefinitionWordSynonymViewModel(ResultDefinitionWordSynonym synonym, bool isReadOnly)
        {
            ResultDefinitionWordSynonym = synonym;
            IsReadOnly = isReadOnly;
        }

        public ResultDefinitionWordSynonym ResultDefinitionWordSynonym
        {
            get { return data as ResultDefinitionWordSynonym; }
            private set { data = value; }
        }

        public string Synonym
        {
            get => ResultDefinitionWordSynonym.Synonym;
            set {
                LastModifiedDate = DateTime.Now;
                SetProperty(() => ResultDefinitionWordSynonym.Synonym == value, () => ResultDefinitionWordSynonym.Synonym = value); }
        }
    }
}