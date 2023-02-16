using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DataLib;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a NowSubscriptionTreeViewModel object that provides a header for the inclusive and exclusive NOWS/EDTs in the subscription
    /// </summary>
    public class NowSubscriptionNowsTreeViewModel : TreeViewItemViewModel
    {
        NowSubscriptionViewModel nowSubscriptionViewModel;
        bool isIncludedNowsTree;
        AllNowsTreeViewModel allNows;
        AllEDTsTreeViewModel allEDTs;

        public NowSubscriptionNowsTreeViewModel(NowSubscriptionTreeViewModel parent, bool isIncludedNowsTree, AllNowsTreeViewModel allNows) : base(parent, false, null, null)
        {
            //share the same model as the parent
            nowSubscriptionViewModel = parent.NowSubscriptionViewModel;
            this.isIncludedNowsTree = isIncludedNowsTree;
            this.allNows = allNows;
            LoadChildren();
        }

        public NowSubscriptionNowsTreeViewModel(NowSubscriptionTreeViewModel parent, bool isIncludedNowsTree, AllEDTsTreeViewModel allEDTs) : base(parent, false, null, null)
        {
            //share the same model as the parent
            nowSubscriptionViewModel = parent.NowSubscriptionViewModel;
            this.isIncludedNowsTree = isIncludedNowsTree;
            this.allEDTs = allEDTs;
            LoadChildren();
        }

        public bool IsIncludedNowsTree
        {
            get { return isIncludedNowsTree; }
        }

        protected override void LoadChildren()
        {
            if (nowSubscriptionViewModel.IsNow || nowSubscriptionViewModel.IsEDT)
            {
                if (isIncludedNowsTree)
                {
                    foreach (var now in nowSubscriptionViewModel.IncludedNOWS.Where(x => x.DeletedDate == null).OrderBy(x=>x.Name))
                    {
                        //find the now tree
                        var nowTree = nowSubscriptionViewModel.IsNow ? allNows.Nows.FirstOrDefault(x => x.NowViewModel.UUID == now.UUID) : allEDTs.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == now.UUID);

                        if (nowTree != null)
                        {
                            //clone the now tree for the purpose of this subscription i.e. so that it may become its own child entry for this subscription
                            var clone = nowTree.Copy(this);
                            Children.Add(clone);
                        }
                    }
                }
                else
                {
                    foreach (var now in nowSubscriptionViewModel.ExcludedNOWS.Where(x => x.DeletedDate == null).OrderBy(x => x.Name))
                    {
                        //find the now tree
                        var nowTree = nowSubscriptionViewModel.IsNow ? allNows.Nows.FirstOrDefault(x => x.NowViewModel.UUID == now.UUID) : allEDTs.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == now.UUID);
                        if (nowTree != null)
                        {
                            //clone the now tree for the purpose of this subscription i.e. so that it may become its own child entry for this subscription
                            var clone = nowTree.Copy(this);
                            Children.Add(clone);
                        }
                    }
                }
            }
        }

        #region Presentation Members

        public NowSubscriptionViewModel NowSubscriptionViewModel
        {
            get { return nowSubscriptionViewModel; }
        }

        public string Name
        {
            get 
            {
                if (isIncludedNowsTree)
                {
                    return nowSubscriptionViewModel.IsNow ? "Included Nows" : "Included EDTs";
                }

                return nowSubscriptionViewModel.IsNow ? "Excluded Nows" : "Excluded EDTs";
            }
        }

        #endregion Presentation Members

    }
}