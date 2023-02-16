using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataLib;

namespace Models.ViewModels
{
    public class ResultPromptTreeViewModel : TreeViewItemViewModel
    {
        public ResultPromptTreeViewModel(ITreeModel treeModel, ResultPromptRuleViewModel resultPromptRuleVM, TreeViewItemViewModel parent)
            : base(parent, true, resultPromptRuleVM.ResultPromptViewModel.Label, resultPromptRuleVM.UUID.ToString())
        {
            this.treeModel = treeModel;
            ResultPromptRuleViewModel = resultPromptRuleVM;
        }

        protected override void LoadChildren()
        {
            if (ResultPromptRuleViewModel.ResultPromptViewModel.FixedList != null)
            {
                base.Children.Add(new FixedListTreeViewModel(treeModel, ResultPromptRuleViewModel.ResultPromptViewModel.FixedList, this));
            }

            if (ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups != null)
            {
                var wordGroups = ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups.OrderBy(x => x.ResultPromptWord);

                foreach (var group in wordGroups)
                {
                    base.Children.Add(new ResultPromptWordGroupTreeViewModel(treeModel, group, this));
                }
            }
        }

        #region Copy / Paste Members

        public ResultPromptTreeViewModel Copy()
        {
            ResultPromptTreeViewModel res = (ResultPromptTreeViewModel)this.MemberwiseClone();

            //make new data view model
            res.ResultPromptRuleViewModel = res.ResultPromptRuleViewModel.Copy();

            return res;
        }

        public ResultPromptTreeViewModel Clone()
        {
            ResultPromptTreeViewModel res = (ResultPromptTreeViewModel)this.MemberwiseClone();

            //make new data view model
            res.ResultPromptRuleViewModel = res.ResultPromptRuleViewModel.Clone();

            return res;
        }

        public ResultPromptTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as ResultPromptTreeViewModel;
            set
            {
                treeModel.CopiedTreeViewModel = value;
            }
        }

        #endregion Copy / Paste Members

        public ResultPromptRuleViewModel ResultPromptRuleViewModel { get; set; }

        public Guid? ParentUUID
        {
            get => Parent is ResultDefinitionTreeViewModel ? ((ResultDefinitionTreeViewModel)Parent).ResultRuleViewModel.ChildResultDefinitionViewModel.UUID : null;
        }

        public override Tuple<bool, SearchRank> NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.ResultPromptRuleViewModel.ResultPromptViewModel.Label))
                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);

            //test for guid
            Guid guidSearch = Guid.NewGuid();
            var isGuid = Guid.TryParse(text, out guidSearch);
            if (isGuid)
            {
                if (ResultPromptRuleViewModel.ResultPromptViewModel.UUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }
                if (ResultPromptRuleViewModel.ResultPromptViewModel.MasterUUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }

                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
            }

            //match by label
            var isEqual = String.Equals(ResultPromptRuleViewModel.ResultPromptViewModel.Label, text, StringComparison.InvariantCultureIgnoreCase);
            if (isEqual) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactLabelMatch); }
            if (ResultPromptRuleViewModel.ResultPromptViewModel.Label.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return new Tuple<bool, SearchRank>(true, SearchRank.PartialLabelMatch);
            }
            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }
    }
}