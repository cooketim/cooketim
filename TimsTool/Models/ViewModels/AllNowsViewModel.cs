using System.Collections.Generic;
using System.Linq;
using DataLib;
using DataLib.DataModel;

namespace Models.ViewModels
{
    public class AllNowsViewModel : ViewModelBase
    {
        public AllNowsViewModel(ITreeModel treeModel, List<Now> nows, AllNowRequirementsViewModel allNowRequirements, AllResultDefinitionsViewModel allResultDefinitionsViewModel)
        {
            this.treeModel = treeModel;
            //Nows
            Nows = new List<NowViewModel>(
                (from x in nows.FindAll(x=> !x.IsEDT)
                 select new NowViewModel(treeModel, x, allNowRequirements, allResultDefinitionsViewModel))
                .ToList());
        }

        public List<NowViewModel> Nows { get; }
    }
}
