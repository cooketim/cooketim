using System;
using System.Linq;

namespace Models.ViewModels
{
    public class ResultWordGroupTreeViewModel : TreeViewItemViewModel
    {
        public ResultWordGroupTreeViewModel(ITreeModel treeModel, ResultWordGroupViewModel resultWordGroup, TreeViewItemViewModel parent)
            : base(parent, true, resultWordGroup.WordGroupName, resultWordGroup.WordGroupName)
        {
            this.treeModel = treeModel;
            ResultWordGroupViewModel = resultWordGroup;
        }

        protected override void LoadChildren()
        {
            if (ResultWordGroupViewModel.ResultDefinitionWordGroups != null && ResultWordGroupViewModel.ResultDefinitionWordGroups.Count>0)
            {
                var definitionWordGroups = ResultWordGroupViewModel.ResultDefinitionWordGroups.OrderBy(x => x.ResultDefinitionWord);
                foreach (var definitionWord in definitionWordGroups)
                {
                    base.Children.Add(new ResultDefinitionWordGroupTreeViewModel(treeModel, definitionWord, this));
                }
            }
        }

        public ResultWordGroupViewModel ResultWordGroupViewModel { get; set; }

        public Guid? ParentUUID
        {
            get => Parent is ResultDefinitionTreeViewModel ? ((ResultDefinitionTreeViewModel)Parent).ResultRuleViewModel.ChildResultDefinitionViewModel.UUID : null;
        }

        public override Tuple<bool, SearchRank> NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.ResultWordGroupViewModel.WordGroupName))
                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);

            //match by label
            var isEqual = String.Equals(ResultWordGroupViewModel.WordGroupName, text, StringComparison.InvariantCultureIgnoreCase);
            if (isEqual) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactLabelMatch); }
            if (ResultWordGroupViewModel.WordGroupName.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return new Tuple<bool, SearchRank>(true, SearchRank.PartialLabelMatch);
            }
            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }

        #region Copy / Paste Members

        public ResultWordGroupTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as ResultWordGroupTreeViewModel;
            set
            {
                treeModel.CopiedTreeViewModel = value;
            }
        }
        #endregion Copy / Paste Members

        #region Drag Drop Members

        //public override string GetDragName()
        //{
        //    return resultWordGroup.WordGroupName;
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

        #endregion Drag Drop Members
    }
}