using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DataLib;

namespace Models.ViewModels
{
    /// <summary>
    /// A UI-friendly wrapper around a revision of a NowViewModel object.
    /// </summary>
    public class NowRevisionTreeViewModel : TreeViewItemViewModel
    {
        DateTime groupDate;
        bool isEDT;

        public NowRevisionTreeViewModel(DateTime groupDate, bool isEDT, IEnumerable<NowTreeViewModel> children)
            : base(null, false, groupDate == DateTime.MinValue ? "Pending" : groupDate.Date.ToString("d"), null)
        {
            this.groupDate = groupDate.Date;
            this.isEDT = isEDT;

            foreach (var child in children.OrderBy(x => x.NowViewModel.Name))
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
        public bool IsEDT
        {
            get => isEDT;
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
                    var nowChild = child as NowTreeViewModel;
                    if (nowChild.NowViewModel.PublishedStatus == PublishedStatus.RevisionPending)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected override int Compare(object obj)
        {
            NowRevisionTreeViewModel t = (NowRevisionTreeViewModel)obj;
            return DateTime.Compare(GroupDate, t.GroupDate);
        }

        #endregion Presentation Members
    }
}