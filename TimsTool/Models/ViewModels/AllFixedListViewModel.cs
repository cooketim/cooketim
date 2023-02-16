using DataLib;
using System.Collections.Generic;

namespace Models.ViewModels
{
    public class AllFixedListViewModel : ViewModelBase
    {
        readonly List<FixedListViewModel> fixedLists;

        public AllFixedListViewModel(ITreeModel treeModel, List<FixedList> fixedLists)
        {
            this.treeModel = treeModel;
            this.fixedLists = new List<FixedListViewModel>();
            foreach(FixedList list in fixedLists.FindAll(x=>x.DeletedDate == null))
            {
                this.fixedLists.Add(new FixedListViewModel(treeModel, list));
            }
        }

        public List<FixedListViewModel> FixedLists
        {
            get { return fixedLists; }
        }
    }
}
