using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataLib;

namespace Models.ViewModels
{
    public class ResultDefinitionWordGroupTreeViewModel : TreeViewItemViewModel
    {
        public ResultDefinitionWordGroupTreeViewModel(ITreeModel treeModel, ResultDefinitionWordGroupViewModel resultDefinitionWordGroup, TreeViewItemViewModel parent)
            : base(parent, false, resultDefinitionWordGroup.ResultDefinitionWord, resultDefinitionWordGroup.UUID.ToString())
        {
            this.treeModel = treeModel;
            ResultDefinitionWordGroupViewModel = resultDefinitionWordGroup;
        }

        public ResultDefinitionWordGroupViewModel ResultDefinitionWordGroupViewModel { get; set; }

        public Guid? ParentUUID
        {
            get => Parent is ResultDefinitionTreeViewModel ? ((ResultDefinitionTreeViewModel)Parent).ResultRuleViewModel.ChildResultDefinitionViewModel.UUID : null;
        }

        public override Tuple<bool, SearchRank> NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(ResultDefinitionWordGroupViewModel.ResultDefinitionWord))
                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);

            //test for guid
            Guid guidSearch = Guid.NewGuid();
            var isGuid = Guid.TryParse(text, out guidSearch);
            if (isGuid)
            {
                if (ResultDefinitionWordGroupViewModel.UUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }
                if (ResultDefinitionWordGroupViewModel.MasterUUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }

                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
            }

            //match by label
            var isEqual = String.Equals(ResultDefinitionWordGroupViewModel.ResultDefinitionWord, text, StringComparison.InvariantCultureIgnoreCase);
            if (isEqual) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactLabelMatch); }
            if (ResultDefinitionWordGroupViewModel.ResultDefinitionWord.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return new Tuple<bool, SearchRank>(true, SearchRank.PartialLabelMatch);
            }
            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }

        #region Copy / Paste Members

        public ResultDefinitionWordGroupTreeViewModel Copy()
        {
            ResultDefinitionWordGroupTreeViewModel res = (ResultDefinitionWordGroupTreeViewModel)this.MemberwiseClone();

            //make new data view model
            res.ResultDefinitionWordGroupViewModel = res.ResultDefinitionWordGroupViewModel.Copy();
            return res;
        }

        internal void ResetVM()
        {
            OnPropertyChanged("ResultDefinitionWordGroupViewModel");
        }

        public ResultDefinitionWordGroupTreeViewModel Clone()
        {
            ResultDefinitionWordGroupTreeViewModel res = (ResultDefinitionWordGroupTreeViewModel)this.MemberwiseClone();
            return res;
        }

        public ResultDefinitionWordGroupTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as ResultDefinitionWordGroupTreeViewModel;
            set
            {
                treeModel.CopiedTreeViewModel = value;
            }
        }
        #endregion Copy / Paste Members

        //#region Drag Drop Members

        //public override string GetDragName()
        //{
        //    return resultDefinitionWordGroup.ResultDefinitionWord;
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
        //        //target must be a Result Definition Word Group to continue
        //        return null;
        //    }

        //    //Check whether the source item can be pasted to this target item
        //    if (!CheckDropTarget(sourceItem)) { return null; }

        //    //set the pending copy and target models
        //    treeModel.CopiedTreeViewModel = sourceItem;
        //    treeModel.SelectedItem = targetItem;
        //    targetItem.IsSelected = true;

        //    return treeModel.PasteResultDefinitionWordGroupCommand;
        //}

        //#endregion Drag Drop Members
    }
}