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
    public class AllNowSubscriptionViewModel : ViewModelBase
    {
        List<NowSubscriptionViewModel> nowSubscriptions;

        public AllNowSubscriptionViewModel (ITreeModel treeModel, List<NowSubscription> nowSubscriptions)
        {
            this.treeModel = treeModel;
            //parent now subscriptions
            this.nowSubscriptions = new List<NowSubscriptionViewModel>(
                (from x in nowSubscriptions.FindAll(x=>x.ParentNowSubscription == null && x.IsNow)
                 select new NowSubscriptionViewModel(treeModel, x))
                .ToList());
        }

        public List<NowSubscriptionViewModel> NowSubscriptions
        {
            get { return nowSubscriptions; }
        }
    }
}
