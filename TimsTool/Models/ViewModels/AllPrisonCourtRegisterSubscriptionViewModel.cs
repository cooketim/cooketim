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
    public class AllPrisonCourtRegisterSubscriptionViewModel : ViewModelBase
    {
        List<NowSubscriptionViewModel> prisonCourtRegisterSubscriptions;

        public AllPrisonCourtRegisterSubscriptionViewModel(ITreeModel treeModel, List<NowSubscription> nowSubscriptions)
        {
            this.treeModel = treeModel;
            var subsData = nowSubscriptions.FindAll(x => x.IsPrisonCourtRegister).ToList();
            //prison court register subscriptions
            this.prisonCourtRegisterSubscriptions = new List<NowSubscriptionViewModel>(
                (from x in subsData
                 select new NowSubscriptionViewModel(treeModel, x))
                .ToList());
        }

        public List<NowSubscriptionViewModel> PrisonCourtRegisterSubscriptions
        {
            get { return prisonCourtRegisterSubscriptions; }
        }
    }
}
