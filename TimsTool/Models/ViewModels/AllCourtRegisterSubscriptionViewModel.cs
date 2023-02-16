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
    public class AllCourtRegisterSubscriptionViewModel : ViewModelBase
    {
        List<NowSubscriptionViewModel> courtRegisterSubscriptions;

        public AllCourtRegisterSubscriptionViewModel (ITreeModel treeModel, List<NowSubscription> CourtRegisterSubscriptions)
        {
            this.treeModel = treeModel;
            //CourtRegister subscriptions
            this.courtRegisterSubscriptions = new List<NowSubscriptionViewModel>(
                (from x in CourtRegisterSubscriptions.FindAll(x=>x.IsCourtRegister)
                 select new NowSubscriptionViewModel(treeModel, x))
                .ToList());
        }

        public List<NowSubscriptionViewModel> CourtRegisterSubscriptions
        {
            get { return courtRegisterSubscriptions; }
        }
    }
}
