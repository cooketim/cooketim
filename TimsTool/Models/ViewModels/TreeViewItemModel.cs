using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Models.ViewModels
{
    public enum SearchRank
    {
        GuidMatch = 0,
        ChildGuidMatch = 1,
        ExactCodeMatch = 2,
        ExactWordGroupMatch = 3,
        ExactLabelMatch = 4,
        ExactChildLabelMatch = 5,
        PartialLabelMatch = 6,
        PartialChildLabelMatch = 7,

        NoMatch = 9
    }
    public class RankedTreeViewItemViewModel
    {
        public TreeViewItemViewModel TreeViewItemViewModel { get; }
        public SearchRank SearchRank { get; }

        public RankedTreeViewItemViewModel(TreeViewItemViewModel treeViewItemViewModel, SearchRank searchRank)
        {
            TreeViewItemViewModel = treeViewItemViewModel;
            SearchRank = searchRank;
        }
    }
    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : ViewModelBase, IComparable
    {
        #region Data

        static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

        readonly SilentObservableCollection<TreeViewItemViewModel> children;

        bool isExpanded,isSelected, isInEditMode;

        #endregion // Data

        #region Constructors

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren, string label, string id)
        {
            Parent = parent;
            Label = label;
            Id = id;

            children = new SilentObservableCollection<TreeViewItemViewModel>();

            if (lazyLoadChildren)
                children.Add(DummyChild);
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        #endregion // Constructors

        #region Presentation Members

        #region Children

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public SilentObservableCollection<TreeViewItemViewModel> Children
        {
            get { return children; }
        }

        #endregion // Children

        #region HasLoadedChildren

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == DummyChild; }
        }

        #endregion // HasLoadedChildren

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (isExpanded && Parent != null)
                    Parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                PrepareChildren();
            }
        }

        public void PrepareChildren()
        {
            // Lazy load the child items, if necessary.
            if (HasDummyChild)
            {
                Children.Remove(DummyChild);
                LoadChildren();
            }
        }

        #endregion IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected

        #region IsInEditMode

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsInEditMode
        {
            get { return isInEditMode; }
            set
            {
                if (value != isInEditMode)
                {
                    isInEditMode = value;
                    this.OnPropertyChanged("IsInEditMode");
                }
            }
        }

        #endregion // IsInEditMode

        #region LoadChildren

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        #endregion // LoadChildren

        #region SequenceChildren

        /// <summary>
        /// Invoked when the child items need to be reordered
        /// </summary>
        public virtual void SequenceChildren()
        {
        }

        #endregion // SequenceChildren

        #region Parent

        public TreeViewItemViewModel Parent
        {
            get;
            private set;
        }

        public void ResetParentage(TreeViewItemViewModel newParent)
        {
            Parent = newParent;
        }

        #endregion // Parent

        #region Properties For Search

        public string Id 
        { 
            get;
            protected set;
        }
        public string ParentId { get => Parent == null ? null : Parent.Id; }
        public string RootId { get => Parent == null ? Id : Parent.RootId; }
        public string Label { get; }
        public string ParentLabel { get => Parent == null ? null :Parent.Label; }

        #endregion

        public virtual Tuple<bool,SearchRank> NameContainsText(string text)
        {
            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }

        public int CompareTo(object obj)
        {
            return Compare(obj);
        }

        protected virtual int Compare(object obj)
        {
            TreeViewItemViewModel t = (TreeViewItemViewModel)obj;
            return String.Compare(this.Label, t.Label, true, System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion // Presentation Members

        #region Drag Drop Members

        public virtual string DragName
        {
            get { return Label; }
        }
        public virtual bool DragSupported
        {
            get { return false; }
        }

        #endregion Drag Drop Members

    }
}
