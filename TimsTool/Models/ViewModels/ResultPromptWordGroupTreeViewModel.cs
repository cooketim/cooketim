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
    public class ResultPromptWordGroupTreeViewModel : TreeViewItemViewModel
    {
        ResultPromptWordGroupViewModel resultPromptGroup;

        public ResultPromptWordGroupTreeViewModel(ITreeModel treeModel, ResultPromptWordGroupViewModel resultPromptGroup, TreeViewItemViewModel parent)
            : base(parent, false, resultPromptGroup.ResultPromptWord, resultPromptGroup.ResultPromptWord)
        {
            this.treeModel = treeModel;
            this.resultPromptGroup = resultPromptGroup;
        }

        public ResultPromptWordGroupViewModel ResultPromptWordGroupViewModel
        {
            get => resultPromptGroup;
            set => resultPromptGroup = value;
        }

        public Guid? ParentUUID
        {
            get => Parent is ResultPromptTreeViewModel ? ((ResultPromptTreeViewModel)Parent).ResultPromptRuleViewModel.ResultPromptViewModel.UUID : null;
        }

        public override Tuple<bool, SearchRank> NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.resultPromptGroup.ResultPromptWord))
                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);

            //test for guid
            Guid guidSearch = Guid.NewGuid();
            var isGuid = Guid.TryParse(text, out guidSearch);
            if (isGuid)
            {
                if (resultPromptGroup.UUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }
                if (resultPromptGroup.MasterUUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }

                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
            }

            //match by label
            var isEqual = String.Equals(resultPromptGroup.ResultPromptWord, text, StringComparison.InvariantCultureIgnoreCase);
            if (isEqual) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactLabelMatch); }
            if (resultPromptGroup.ResultPromptWord.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return new Tuple<bool, SearchRank>(true, SearchRank.PartialLabelMatch);
            }
            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }

        #region Copy / Paste Members

        public ResultPromptWordGroupTreeViewModel Copy()
        {
            ResultPromptWordGroupTreeViewModel res = (ResultPromptWordGroupTreeViewModel)this.MemberwiseClone();

            //make new data view model
            res.ResultPromptWordGroupViewModel = res.ResultPromptWordGroupViewModel.Copy();
            return res;
        }

        public ResultPromptWordGroupTreeViewModel Clone()
        {
            ResultPromptWordGroupTreeViewModel res = (ResultPromptWordGroupTreeViewModel)this.MemberwiseClone();
            return res;
        }

        public ResultPromptWordGroupTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as ResultPromptWordGroupTreeViewModel;
            set
            {
                treeModel.CopiedTreeViewModel = value;
            }
        }
        #endregion Copy / Paste Members

        internal void ResetVM()
        {
            OnPropertyChanged("ResultPromptWordGroupViewModel");
        }        

        //#region Drag Drop Members

        //public override string GetDragName()
        //{
        //    return resultPromptGroup.ResultPromptWord;
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