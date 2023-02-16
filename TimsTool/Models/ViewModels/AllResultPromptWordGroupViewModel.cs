using DataLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class AllResultPromptWordGroupViewModel : ViewModelBase
    {
        public AllResultPromptWordGroupViewModel(ITreeModel treeModel, List<ResultPromptWordGroup> groups)
        {
            this.treeModel = treeModel;
            WordGroups = new List<ResultPromptWordGroupViewModel>();
            foreach(ResultPromptWordGroup group in groups.FindAll(x=>x.DeletedDate == null))
            {
                WordGroups.Add(new ResultPromptWordGroupViewModel(treeModel, group));
            }
        }

        public List<ResultPromptWordGroupViewModel> WordGroups
        {
            get;
        }
    }
}
