using DataLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class AllResultRuleViewModel : ViewModelBase
    {
        public AllResultRuleViewModel()
        {
            Rules = new List<ResultRuleViewModel>();
        }

        public List<ResultRuleViewModel> Rules { get; }
    }
}
