using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using DataLib;

namespace Models.ViewModels
{
    public class AllEDTsViewModel : ViewModelBase
    {
        public AllEDTsViewModel(ITreeModel treeModel, List<Now> nows, AllNowRequirementsViewModel allNowRequirements, AllResultDefinitionsViewModel allResultDefinitionsViewModel)
        {
            this.treeModel = treeModel;
            //EDTs
            EDTs = new List<NowViewModel>(
                (from x in nows.FindAll(x=>x.IsEDT)
                 select new NowViewModel(treeModel, x, allNowRequirements, allResultDefinitionsViewModel))
                .ToList());
        }

        public List<NowViewModel> EDTs { get; }

    }
}
