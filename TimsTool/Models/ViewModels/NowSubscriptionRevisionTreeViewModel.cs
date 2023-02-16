using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Models.ViewModels
{
    public enum NowSubscriptionType
    {
        NOW,
        EDT,
        InformantRegister,
        CourtRegister,
        PrisonCourtRegister
    }
    /// <summary>
    /// A UI-friendly wrapper around a revision of a NowSubscriptionViewModel object.
    /// </summary>
    public class NowSubscriptionRevisionTreeViewModel : TreeViewItemViewModel
    {
        DateTime groupDate;
        NowSubscriptionType subscriptionType;

        public NowSubscriptionRevisionTreeViewModel(DateTime groupDate, NowSubscriptionType subscriptionType, IEnumerable<NowSubscriptionTreeViewModel> children)
            : base(null, false, groupDate == DateTime.MinValue ? "Pending" : groupDate.Date.ToString("d"), null)
        {
            this.groupDate = groupDate.Date;
            this.subscriptionType = subscriptionType;

            foreach (var child in children.OrderBy(x => x.NowSubscriptionViewModel.Name))
            {
                child.ResetParentage(this);
                Children.Add(child);
            };
        }

        internal void ResetChildren()
        {
            Children.Clear();
            LoadChildren();
            IsExpanded = false;
        }

        public NowSubscriptionType SubscriptionType
        {
            get => subscriptionType;
        }


        #region Presentation Members

        public DateTime GroupDate
        {
            get => groupDate;
        }

        public bool ContainsRevisionPending
        {
            get
            {
                if (Children == null || !Children.Any()) { return false; }
                foreach (var child in Children)
                {
                    var nowSubChild = child as NowSubscriptionTreeViewModel;
                    if (nowSubChild.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.RevisionPending)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected override int Compare(object obj)
        {
            NowSubscriptionRevisionTreeViewModel t = (NowSubscriptionRevisionTreeViewModel)obj;
            return DateTime.Compare(GroupDate, t.GroupDate);
        }

        #endregion Presentation Members
    }
}