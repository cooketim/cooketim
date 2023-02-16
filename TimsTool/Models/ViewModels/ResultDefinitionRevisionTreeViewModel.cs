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
    /// A UI-friendly wrapper around a revision of a resultDefinitionViewModel object.
    /// </summary>
    public class ResultDefinitionRevisionTreeViewModel : TreeViewItemViewModel
    {
        DateTime groupDate;

        public ResultDefinitionRevisionTreeViewModel(DateTime groupDate, IEnumerable<ResultDefinitionTreeViewModel> children)
            : base(null, false, groupDate == DateTime.MinValue ? "Pending" : groupDate.Date.ToString("d"), null)
        {
            this.groupDate = groupDate.Date;

            foreach (var child in children.OrderBy(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.Label))
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


        #region Presentation Members

        public bool IsParent
        {
            get => Parent == null;
        }

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
                    var rdChild = child as ResultDefinitionTreeViewModel;
                    if (rdChild.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.RevisionPending)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected override int Compare(object obj)
        {
            ResultDefinitionRevisionTreeViewModel t = (ResultDefinitionRevisionTreeViewModel)obj;
            return DateTime.Compare(GroupDate, t.GroupDate);
        }

        #endregion Presentation Members
    }
}