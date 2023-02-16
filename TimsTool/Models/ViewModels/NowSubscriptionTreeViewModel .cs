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
    /// A UI-friendly wrapper around a NowSubscriptionTreeViewModel object.
    /// </summary>
    public class NowSubscriptionTreeViewModel : TreeViewItemViewModel
    {
        NowSubscriptionViewModel nowSubscriptionViewModel;
        AllNowsTreeViewModel allNows;
        AllEDTsTreeViewModel allEDTs;

        //Nows
        public NowSubscriptionTreeViewModel(ITreeModel treeModel, NowSubscriptionViewModel nowSubscriptionViewModel, AllNowsTreeViewModel allNows)
            : base(null, true, nowSubscriptionViewModel.Name, nowSubscriptionViewModel.UUID.ToString())
        {
            this.treeModel = treeModel;
            this.nowSubscriptionViewModel = nowSubscriptionViewModel;
            this.allNows = allNows;
        }

        //EDTs
        public NowSubscriptionTreeViewModel(ITreeModel treeModel,NowSubscriptionViewModel nowSubscriptionViewModel, AllEDTsTreeViewModel allEDTs)
            : base(null, true, nowSubscriptionViewModel.Name, nowSubscriptionViewModel.UUID.ToString())
        {
            this.treeModel = treeModel;
            this.nowSubscriptionViewModel = nowSubscriptionViewModel;
            this.allEDTs = allEDTs;
        }

        //Registers
        public NowSubscriptionTreeViewModel(ITreeModel treeModel, NowSubscriptionViewModel nowSubscriptionViewModel)
            : base(null, false, nowSubscriptionViewModel.Name, nowSubscriptionViewModel.UUID.ToString())
        {
            this.treeModel = treeModel;
            this.nowSubscriptionViewModel = nowSubscriptionViewModel;
        }

        protected override void LoadChildren()
        {
            if (nowSubscriptionViewModel.NowSubscription.IsNow)
            {
                //Included NOWs
                if (nowSubscriptionViewModel.ParentNowSubscription == null)
                {
                    Children.Add(new NowSubscriptionNowsTreeViewModel(this, true, allNows));
                }

                //Excluded NOWs
                Children.Add(new NowSubscriptionNowsTreeViewModel(this, false, allNows));

                //Child Now Subscriptions
                if (nowSubscriptionViewModel.ChildNowSubscriptions != null)
                {
                    foreach (var childVM in nowSubscriptionViewModel.ChildNowSubscriptions)
                    {
                        //add a child subscription
                        Children.Add(new NowSubscriptionTreeViewModel(treeModel, childVM, allNows));
                    }
                }
            }

            if (nowSubscriptionViewModel.NowSubscription.IsEDT)
            {
                //Included EDTs
                if (nowSubscriptionViewModel.ParentNowSubscription == null)
                {
                    Children.Add(new NowSubscriptionNowsTreeViewModel(this, true, allEDTs));
                }

                //Excluded EDTs
                Children.Add(new NowSubscriptionNowsTreeViewModel(this, false, allEDTs));

                //Child Now Subscriptions
                if (nowSubscriptionViewModel.ChildNowSubscriptions != null)
                {
                    foreach (var childVM in nowSubscriptionViewModel.ChildNowSubscriptions)
                    {
                        //add a child subscription
                        Children.Add(new NowSubscriptionTreeViewModel(treeModel, childVM, allEDTs));
                    }
                }
            }
        }

        public NowSubscriptionTreeViewModel Draft(List<string> publicationTags)
        {
            //make new draft data view model
            var nowSubsVM = NowSubscriptionViewModel.Draft(publicationTags);

            //Make the tree view depending on the type of subscription
            if (nowSubsVM.IsNow || nowSubsVM.IsEDT)
            {
                if (nowSubsVM.IsNow)
                {
                    return new NowSubscriptionTreeViewModel(treeModel,nowSubsVM, allNows);
                }
                else
                {
                    return new NowSubscriptionTreeViewModel(treeModel,nowSubsVM, allEDTs);
                }
            }
            else
            {
                //register
                return new NowSubscriptionTreeViewModel(treeModel,nowSubsVM);
            }
        }

        #region nowSubscriptionViewModel Properties

        public NowSubscriptionViewModel NowSubscriptionViewModel
        {
            get => nowSubscriptionViewModel;
            set => nowSubscriptionViewModel = value;
        }

        internal void ResetChildren()
        {
            Children.Clear();
            LoadChildren();
            IsExpanded = false;
        }

        #endregion // nowSubscriptionViewModel Properties

        #region Presentation Members

        public override Tuple<bool, SearchRank> NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.nowSubscriptionViewModel.Name))
                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);

            //test for guid
            Guid guidSearch = Guid.NewGuid();
            var isGuid = Guid.TryParse(text, out guidSearch);
            if (isGuid)
            {
                if (nowSubscriptionViewModel.UUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }
                if (nowSubscriptionViewModel.MasterUUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }

                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
            }

            //match by label
            var isEqual = String.Equals(nowSubscriptionViewModel.Name, text, StringComparison.InvariantCultureIgnoreCase);
            if (isEqual) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactLabelMatch); }
            if (nowSubscriptionViewModel.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return new Tuple<bool, SearchRank>(true, SearchRank.PartialLabelMatch);
            }

            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }

        #endregion Presentation Members

        #region Copy / Paste Members

        public NowSubscriptionTreeViewModel Copy()
        {
            NowSubscriptionTreeViewModel nowSubscription = (NowSubscriptionTreeViewModel)this.MemberwiseClone();

            //make new data view model
            nowSubscription.NowSubscriptionViewModel = nowSubscription.NowSubscriptionViewModel.Copy();
            return nowSubscription;
        }

        public NowSubscriptionTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as NowSubscriptionTreeViewModel;
            set
            {
                treeModel.CopiedTreeViewModel = value;
            }
        }

        #endregion Copy / Paste Members

    }
}