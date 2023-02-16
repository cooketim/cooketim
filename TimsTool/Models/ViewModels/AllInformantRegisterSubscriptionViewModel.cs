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
    public class AllInformantRegisterSubscriptionViewModel : ViewModelBase
    {
        List<NowSubscriptionViewModel> informantRegisterSubscriptions;

        public AllInformantRegisterSubscriptionViewModel (ITreeModel treeModel, List<NowSubscription> informantRegisterSubscriptions)
        {
            this.treeModel = treeModel;
            //informantRegister subscriptions
            this.informantRegisterSubscriptions = new List<NowSubscriptionViewModel>(
                (from x in informantRegisterSubscriptions.FindAll(x=>x.IsInformantRegister)
                 select new NowSubscriptionViewModel(treeModel, x))
                .ToList());
        }

        public List<NowSubscriptionViewModel> InformantRegisterSubscriptions
        {
            get { return informantRegisterSubscriptions; }
        }
    }
}
