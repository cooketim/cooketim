using DataLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class AllResultDefinitionWordGroupViewModel : ViewModelBase
    {
        public AllResultDefinitionWordGroupViewModel(ITreeModel treeModel, List<ResultDefinitionWordGroup> groups)
        {
            this.treeModel = treeModel;
            WordGroups = new List<ResultDefinitionWordGroupViewModel>();
            foreach(ResultDefinitionWordGroup group in groups.FindAll(x=>x.DeletedDate == null))
            {
                WordGroups.Add(new ResultDefinitionWordGroupViewModel(treeModel, group));
            }
        }

        public List<ResultDefinitionWordGroupViewModel> WordGroups { get; }
    }
}
