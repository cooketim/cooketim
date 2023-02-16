using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Models.ViewModels
{
    public class AllResultDefinitionsTreeViewModel : SearchableTreeViewModel
    {
        private ResultDefinitionRevisionTreeViewModel pendingTree;
        public AllResultDefinitionsTreeViewModel(ITreeModel treeModel, AllResultRuleViewModel allRules)
        {
            this.treeModel = treeModel;
            var definitions = new List<ResultDefinitionTreeViewModel>();

            //make parent result definitions
            foreach(var parent in allRules.Rules.Where(x => x.ParentUUID == null))
            {
                new ResultDefinitionTreeViewModel(treeModel, parent, null, definitions);
            }

            ResultDefinitions = new SilentObservableCollection<ResultDefinitionTreeViewModel>(definitions.OrderBy(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));

            ResultDefinitionRootsDraft = new SilentObservableCollection<ResultDefinitionTreeViewModel>(
                                        definitions.Where(x => x.ResultRuleViewModel.ParentUUID == null &&
                                                            (x.ResultRuleViewModel.PublishedStatus == PublishedStatus.Draft)
                                        ).OrderBy(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));

            ResultDefinitionRootsPublished = new SilentObservableCollection<ResultDefinitionTreeViewModel>(
                                        definitions.Where(x => x.ResultRuleViewModel.ParentUUID == null &&
                                                            (x.ResultRuleViewModel.PublishedStatus == PublishedStatus.Published || x.ResultRuleViewModel.PublishedStatus == PublishedStatus.PublishedPending)
                                        ).OrderBy(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));

            //Create Pending Revision Tree
            ResultDefinitionRevision = new SilentObservableCollection<ResultDefinitionRevisionTreeViewModel>();
            pendingTree = new ResultDefinitionRevisionTreeViewModel(DateTime.MinValue,
                            definitions.Where(x => x.ResultRuleViewModel.ParentUUID == null && x.ResultRuleViewModel.PublishedStatus == PublishedStatus.RevisionPending));
            ResultDefinitionRevision.AddSorted(pendingTree);

            //Create Revision Trees
            MakeRevisionTrees(definitions.Where(x => x.ResultRuleViewModel.ParentUUID == null && x.ResultRuleViewModel.PublishedStatus == PublishedStatus.Revision));

            staticSearchErrorText = "There was not any matching Result Definitions or Result Prompts for '{0}'.";
        }

        private void MakeRevisionTrees(IEnumerable<ResultDefinitionTreeViewModel> revisions)
        {
            foreach (var revisionGroup in 
                            revisions
                                .GroupBy(x=>x.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatusDate.Date)
                                .OrderBy(x=>x.Key))
            {
                var tree = new ResultDefinitionRevisionTreeViewModel(revisionGroup.Key, revisionGroup);
                ResultDefinitionRevision.AddSorted(tree);
            }
        }

        public SilentObservableCollection<ResultDefinitionTreeViewModel> ResultDefinitions { get; }

        public SilentObservableCollection<ResultDefinitionTreeViewModel> ResultDefinitionRootsDraft { get; }

        public SilentObservableCollection<ResultDefinitionTreeViewModel> ResultDefinitionRootsPublished { get; }

        public SilentObservableCollection<ResultDefinitionRevisionTreeViewModel> ResultDefinitionRevision { get; }

        public void AppendRevision(ResultDefinitionTreeViewModel newItem)
        {
            //deal with pending
            if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.RevisionPending)
            {
                AppendRevisionPending(newItem);
                return;
            }
            //match to an existing root item
            var matchedRoot = ResultDefinitionRevision.ToList().FirstOrDefault(x => x.GroupDate == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatusDate.Date);
            if (matchedRoot == null)
            {
                var tree = new ResultDefinitionRevisionTreeViewModel(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatusDate, new List<ResultDefinitionTreeViewModel>() { newItem });
                ResultDefinitionRevision.AddSorted(tree);
            }
            else
            {
                matchedRoot.Children.AddSorted(newItem);
            }
        }

        public void AppendRevisionPending(ResultDefinitionTreeViewModel newItem)
        {
            //match to an child item under pending tree to ensure that the result appears only once
            var revisionTrees = pendingTree.Children.Where(x => x.GetType() == typeof(ResultDefinitionTreeViewModel)).Select(x => (ResultDefinitionTreeViewModel)x).ToList();
            var matchedTree = revisionTrees.FirstOrDefault(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.MasterUUID);
            if (matchedTree == null)
            {
                pendingTree.Children.AddSorted(newItem);
            }
            else
            {
                //Duplicate revision pending is not allowed. The new revision pending has probably been made from a published pending and is not required and therefore
                //must be purged
                if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID != matchedTree.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                {
                    //purge views
                    treeModel.AllResultRuleViewModel.Rules.RemoveAll(X => X.ChildResultDefinitionViewModel.UUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.UUID);
                    treeModel.AllResultDefinitionsViewModel.Definitions.Remove(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel);
                    treeModel.AllResultPromptViewModel.Prompts.RemoveAll(x=>x.ResultDefinitionViewModel.UUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.UUID);

                    //purge data
                    treeModel.AllData.ResultDefinitionRules.RemoveAll(x => x.ChildResultDefinitionUUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.UUID);
                    treeModel.AllData.ResultDefinitions.Remove(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition);
                    treeModel.AllData.ResultPromptRules.RemoveAll(x => x.ResultDefinitionUUID == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.UUID);
                }
            }
        }

        public void Refresh(AllResultRuleViewModel allRules)
        {
            ResultDefinitions.Reset(
                (from rule in allRules.Rules
                 select new ResultDefinitionTreeViewModel(treeModel, rule))
                .ToList().OrderBy(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.Label));

            ResultDefinitionRootsDraft.Reset(
                 from definition in ResultDefinitions where definition.ResultRuleViewModel.ParentUUID == null select definition);
        }

        internal void ResetRevisionPending()
        {
            //get any revision items from the pending tree
            var revisions = new List<ResultDefinitionTreeViewModel>();
            if (pendingTree.Children != null)
            {
                foreach(var child in pendingTree.Children)
                {
                    var rdChild = child as ResultDefinitionTreeViewModel;
                    if (rdChild.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Revision)
                    {
                        rdChild.ResultRuleViewModel.PublishedStatus = PublishedStatus.Revision;
                        revisions.Add(rdChild);
                    }
                }
            }

            //add the revisions to the revisions tree and remove from pending
            foreach(var rd in revisions)
            {
                AppendRevision(rd);
                pendingTree.Children.Remove(rd);
            }
        }
    }
}
