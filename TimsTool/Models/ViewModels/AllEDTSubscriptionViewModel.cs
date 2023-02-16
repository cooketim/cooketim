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
    public class AllEDTSubscriptionViewModel : ViewModelBase
    {
        List<NowSubscriptionViewModel> edtSubscriptions;

        public AllEDTSubscriptionViewModel (ITreeModel treeModel, List<NowSubscription> edtSubscriptions)
        {
            this.treeModel = treeModel; 
            //edt subscriptions
            this.edtSubscriptions = new List<NowSubscriptionViewModel>(
                (from x in edtSubscriptions.FindAll(x=>x.IsEDT)
                 select new NowSubscriptionViewModel(treeModel, x))
                .ToList());
        }

        public List<NowSubscriptionViewModel> EDTSubscriptions
        {
            get { return edtSubscriptions; }
        }
    }
}
