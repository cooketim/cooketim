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
    /// A UI-friendly wrapper around a resultDefinitionViewModel object.
    /// </summary>
    public class ResultDefinitionTreeViewModel : TreeViewItemViewModel
    {
        List<ResultDefinitionTreeViewModel> definitions;
        public List<ResultDefinitionTreeViewModel> FlattenedChildResults
        {
            get
            {
                var res = new List<ResultDefinitionTreeViewModel>();
                RetrieveChildResults(this, res);
                return res;
            }
        }
        public ResultDefinitionTreeViewModel(ITreeModel treeModel, ResultRuleViewModel resultRuleViewModel, TreeViewItemViewModel parent=null, List<ResultDefinitionTreeViewModel> definitions=null)
            : base(parent, false, resultRuleViewModel.ChildResultDefinitionViewModel.Label, resultRuleViewModel.ChildResultDefinitionViewModel.UUID.ToString())
        {
            this.treeModel = treeModel;
            ResultRuleViewModel = resultRuleViewModel;
            this.definitions = definitions;
            LoadChildren();
            if (definitions != null)
            {
                definitions.Add(this);
            }
        }
        protected override void LoadChildren()
        {
            if (ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
            {
                var prompts = ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.OrderBy(x => x.IsDeleted).ThenBy(x => x.PromptSequence).ThenBy(x => x.ResultPromptViewModel.Label);
                foreach (var prompt in prompts)
                {
                    Children.Add(new ResultPromptTreeViewModel(treeModel, prompt, this));
                }
            }

            if (ResultRuleViewModel.ChildResultDefinitionViewModel.WordGroups != null)
            {
                var wordGroups = ResultRuleViewModel.ChildResultDefinitionViewModel.WordGroups.OrderBy(x => x.WordGroupName);
                foreach (var wordGroup in wordGroups)
                {
                    Children.Add(new ResultWordGroupTreeViewModel(treeModel, wordGroup, this));
                }
            }

            if (ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
            {
                var badRules = ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Where(x => x.ChildResultDefinitionViewModel == null);
                var rules = ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Where(x=> x.ChildResultDefinitionViewModel != null && x.ChildResultDefinitionViewModel.Rank.HasValue).ToList();
                if (ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Where(x => x.ChildResultDefinitionViewModel != null && !x.ChildResultDefinitionViewModel.Rank.HasValue).Count() > 0)
                {
                    rules.AddRange(ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Where(x => !x.ChildResultDefinitionViewModel.Rank.HasValue));
                }
                foreach (var rule in rules)
                {
                    var child = new ResultDefinitionTreeViewModel(treeModel, rule, this, definitions);
                    Children.Add(child);
                }
            }
        }

        internal void ResetChildren()
        {
            Children.Clear();
            LoadChildren();
            IsExpanded = false;
        }

        internal void RefreshChildrenWithNewDraftResultDefinition(ResultRuleViewModel newDraft)
        {
            //see if this tree matches the new result and requires a model refresh
            if(this.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == newDraft.ChildResultDefinitionViewModel.MasterUUID)
            {
                //reset the result definition view model for the given rule
                this.ResultRuleViewModel.SetDraftResult(newDraft.ChildResultDefinitionViewModel);

                //update any child prompt & result rules
                if (!HasDummyChild && Children.Count != 0)
                {
                    RefreshChildRules();
                }
            }

            //Recursive call down the tree
            if (!HasDummyChild && Children.Count != 0)
            {
                var childResultDefinitions = Children.Select(x => x as ResultDefinitionTreeViewModel);
                foreach (var childRD in childResultDefinitions.Where(x => x != null))
                {
                    childRD.RefreshChildrenWithNewDraftResultDefinition(newDraft);
                }
            }
        }

        private void RefreshChildRules()
        {
            //Deal With Child Result Definitions
            var childResultDefinitions = Children.Select(x => x as ResultDefinitionTreeViewModel).Where(x=>x!=null).ToList();
            foreach (var childRD in childResultDefinitions)
            {
                //match the existing child result definition rules to the source child result rule from the viewmodel
                if (ResultRuleViewModel.ChildResultDefinitionViewModel != null && ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
                {
                    var sourceVM = ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.FirstOrDefault(
                                            x => x.MasterUUID == childRD.ResultRuleViewModel.MasterUUID &&
                                                 x.UUID != childRD.ResultRuleViewModel.UUID
                                            );
                    if (sourceVM != null)
                    {
                        //reset the result rule view model
                        childRD.ResultRuleViewModel = sourceVM;
                    }
                }
            }

            //Deal With Child Result Prompts
            var childResultPrompts = Children.Select(x => x as ResultPromptTreeViewModel).Where(x => x != null).ToList();
            foreach (var childRP in childResultPrompts)
            {
                //match the existing child result prompt rules to the source child result prompt rule from the viewmodel
                if (ResultRuleViewModel.ChildResultDefinitionViewModel != null && ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
                {
                    var sourceVM = ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.FirstOrDefault(
                                        x => x.MasterUUID == childRP.ResultPromptRuleViewModel.MasterUUID &&
                                             x.UUID != childRP.ResultPromptRuleViewModel.UUID
                                        );
                    if (sourceVM != null)
                    {
                        //reset the result prompt rule view model
                        childRP.ResultPromptRuleViewModel = sourceVM;
                    }
                }
            }
        }

        internal void RefreshChildRulesForNewDraftResultDefinition()
        {
            //reset all child definition rules to match the view model
            if (Children == null || ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus != PublishedStatus.Draft)
            {
                return;
            }

            //Deal With Child Result Definitions
            var childResultDefinitions = Children.Select(x => x as ResultDefinitionTreeViewModel);
            foreach (var childRD in childResultDefinitions.Where(x => x != null))
            {
                //match the existing child result definition rules to the source child result rule from the viewmodel
                if (ResultRuleViewModel.ChildResultDefinitionViewModel != null && ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
                {
                    var sourceVM = ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.FirstOrDefault(x => x.ChildResultDefinitionViewModel.MasterUUID == childRD.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
                    if (sourceVM != null)
                    {
                        //reset the result rule view model
                        childRD.ResultRuleViewModel = sourceVM;
                    }
                }
            }

            //Deal With Child Result Prompts
            var childResultPrompts = Children.Select(x => x as ResultPromptTreeViewModel);
            foreach (var childRP in childResultPrompts.Where(x => x != null))
            {
                //match the existing child result prompt rules to the source child result prompt rule from the viewmodel
                if (ResultRuleViewModel.ChildResultDefinitionViewModel != null && ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
                {
                    var sourceVM = ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.FirstOrDefault(x => x.ResultPromptViewModel.MasterUUID == childRP.ResultPromptRuleViewModel.ResultPromptViewModel.MasterUUID);
                    if (sourceVM != null)
                    {
                        //reset the result prompt rule view model
                        childRP.ResultPromptRuleViewModel = sourceVM;
                    }
                }
            }
        }

        #region resultRuleViewModel Properties

        public ResultRuleViewModel ResultRuleViewModel { get; set; }

        #endregion // resultRuleViewModel Properties

        #region Presentation Members

        public bool IsParent
        {
            get => Parent == null;
        }

        public override Tuple<bool, SearchRank> NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || 
                            (
                                String.IsNullOrEmpty(ResultRuleViewModel.ChildResultDefinitionViewModel.Label) &&
                                String.IsNullOrEmpty(ResultRuleViewModel.ChildResultDefinitionViewModel.ShortCode)
                            )
               )
               return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);

            //test for guid
            Guid guidSearch = Guid.NewGuid();
            var isGuid = Guid.TryParse(text, out guidSearch);
            if (isGuid)
            {
                if (ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }
                if (ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == guidSearch) { return new Tuple<bool, SearchRank>(true, SearchRank.GuidMatch); }

                return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
            }

            //match by short code
            if (!String.IsNullOrEmpty(ResultRuleViewModel.ChildResultDefinitionViewModel.ShortCode))
            {
                var isEqual = String.Equals(ResultRuleViewModel.ChildResultDefinitionViewModel.ShortCode, text, StringComparison.InvariantCultureIgnoreCase);
                if (isEqual) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactCodeMatch); }
            }

            //match by label
            if (String.Equals(ResultRuleViewModel.ChildResultDefinitionViewModel.Label, text, StringComparison.InvariantCultureIgnoreCase)) { return new Tuple<bool, SearchRank>(true, SearchRank.ExactLabelMatch); }
            if (ResultRuleViewModel.ChildResultDefinitionViewModel.Label.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return new Tuple<bool, SearchRank>(true, SearchRank.PartialLabelMatch);
            }

            return new Tuple<bool, SearchRank>(false, SearchRank.NoMatch);
        }

        #endregion Presentation Members

        #region Copy / Paste Members

        public ResultDefinitionTreeViewModel Copy()
        {
            //make new copied data view model
            var rdVM = ResultRuleViewModel.ChildResultDefinitionViewModel.Copy();
            return BuildTreeView(rdVM);
        }

        public ResultDefinitionTreeViewModel CopyAsDraft()
        {
            //make new copied data view model
            var rdVM = ResultRuleViewModel.ChildResultDefinitionViewModel.CopyAsDraft();
            var res = BuildTreeView(rdVM);

            //set the rule as a draft
            res.ResultRuleViewModel.PublishedStatus = PublishedStatus.Draft;

            return res;
        }

        private ResultDefinitionTreeViewModel BuildTreeView(ResultDefinitionViewModel rdVM)
        {
            var ruleVM = new ResultRuleViewModel(treeModel, rdVM, ResultRuleViewModel.ParentResultDefinitionViewModel);

            var res = new ResultDefinitionTreeViewModel(treeModel, ruleVM);
            return res;
        }

        public ResultDefinitionTreeViewModel Draft(List<string> publicationTags)
        {
            //make new draft data view model
            var rdVM = ResultRuleViewModel.ChildResultDefinitionViewModel.Draft(publicationTags);
            return BuildTreeView(rdVM);
        }

        private void RetrieveChildResults(ResultDefinitionTreeViewModel parent, List<ResultDefinitionTreeViewModel> res)
        {
            parent.PrepareChildren();
            var results = parent.Children.Where(x => x.GetType() == typeof(ResultDefinitionTreeViewModel)).Select(x=>x as ResultDefinitionTreeViewModel).ToList();
            if (results.Count() > 0)
            {
                foreach (var item in results)
                {
                    res.Add(item);
                    RetrieveChildResults(item, res);
                }
            }
            var howMany = res.Count();
        }

        public ResultDefinitionTreeViewModel PendingCopyModel
        {
            get => treeModel.CopiedTreeViewModel as ResultDefinitionTreeViewModel;
            set => treeModel.CopiedTreeViewModel = value;
        }

        public ResultDefinitionTreeViewModel Clone()
        {
            ResultDefinitionTreeViewModel res = (ResultDefinitionTreeViewModel)this.MemberwiseClone();
            return res;
        }

        #endregion Copy / Paste Members

        #region Drag Drop Members

        public override string DragName
        {
            get { return string.Format("Result Definition: '{0}'", Label); }
        }

        public override bool DragSupported
        {
            get { return true; }
        }

        #endregion Drag Drop Members
    }
}