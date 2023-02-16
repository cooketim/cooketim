using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataLib;
using Models.Commands;

namespace Models.ViewModels
{
    public class FixedListTreeViewModel : TreeViewItemViewModel
    {
        public FixedListTreeViewModel(ITreeModel treeModel, FixedListViewModel fixedList, TreeViewItemViewModel parent)
            : base(parent, true, fixedList.Label, fixedList.UUID.ToString())
        {
            this.treeModel = treeModel;
            FixedListViewModel = fixedList;
        }

        public FixedListViewModel FixedListViewModel { get; set; }

        public Guid? ParentUUID
        {
            get => Parent is ResultPromptTreeViewModel ? ((ResultPromptTreeViewModel)Parent).ResultPromptRuleViewModel.ResultPromptViewModel.UUID : null;
        }

        public override Tuple<bool, SearchRank> NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.FixedListViewModel.Label))
                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);

            //test for guid
            Guid guidSearch;
            var isGuid = Guid.TryParse(text, out guidSearch);
            if (isGuid)
            {
                if (FixedListViewModel.UUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }
                if (FixedListViewModel.MasterUUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }

                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
            }

            //match by label
            var isEqual = String.Equals(FixedListViewModel.Label, text, StringComparison.InvariantCultureIgnoreCase);
            if (isEqual) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactLabelMatch); }
            if (FixedListViewModel.Label.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return new Tuple<bool, SearchRank>(true, SearchRank.PartialLabelMatch);
            }
            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }

        #region Copy / Paste Members

        public FixedListTreeViewModel Copy()
        {
            FixedListTreeViewModel res = (FixedListTreeViewModel)this.MemberwiseClone();

            //make new data view model
            res.FixedListViewModel = res.FixedListViewModel.Copy();
            return res;
        }

        internal void ResetVM()
        {
            OnPropertyChanged("FixedListViewModel");
        }

        public FixedListTreeViewModel Clone()
        {
            FixedListTreeViewModel res = (FixedListTreeViewModel)this.MemberwiseClone();
            return res;
        }

        public FixedListTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as FixedListTreeViewModel;
            set
            {
                treeModel.CopiedTreeViewModel = value;
            }
        }

        #endregion Copy / Paste Members

        #region Drag Drop Members

        //public override string GetDragName()
        //{
        //    return fixedList.Label;
        //}

        //public override bool CheckDropTarget(TreeViewItemViewModel sourceItem)
        //{
        //    //Check whether the source item can be pasted to this target item
        //    var sourceType = sourceItem.GetType();
        //    if (sourceType == GetType()) { return true; }
        //    return false;
        //}

        //public override ICommand DoDrop(TreeViewItemViewModel sourceItem, TreeViewItemViewModel targetItem)
        //{
        //    if (targetItem.GetType() != GetType())
        //    {
        //        //target must be a Fixed List to continue
        //        return null;
        //    }

        //    //Check whether the source item can be pasted to this target item
        //    if (!CheckDropTarget(sourceItem)) { return null; }

        //    //set the pending copy and target models
        //    treeModel.CopiedTreeViewModel = sourceItem;
        //    treeModel.SelectedItem = targetItem;
        //    targetItem.IsSelected = true;

        //    return treeModel.PasteFixedListCommand;
        //}

        #endregion Drag Drop Members
    }
}