using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;

namespace Models.ViewModels
{
    public class AllEDTsTreeViewModel : SearchableTreeViewModel
    {
        private NowRevisionTreeViewModel pendingTree;
        public AllEDTsTreeViewModel(ITreeModel treeModel, AllEDTsViewModel allEDTs)
        {
            this.treeModel = treeModel;
            //edts
            EDTs = new SilentObservableCollection<NowTreeViewModel>(
                (from result in allEDTs.EDTs.Where(x => x.IsEDT)
                 select new NowTreeViewModel(treeModel, result))
                .ToList().OrderBy(x => x.NowViewModel.Now.Rank));

            EdtsDraft = new SilentObservableCollection<NowTreeViewModel>(
                                        EDTs.Where(x => x.NowViewModel.PublishedStatus == PublishedStatus.Draft
                                        ).OrderBy(x => x.NowViewModel.Name));

            EdtsPublished = new SilentObservableCollection<NowTreeViewModel>(
                                        EDTs.Where(x => x.NowViewModel.PublishedStatus == PublishedStatus.Published || x.NowViewModel.PublishedStatus == PublishedStatus.PublishedPending
                                        ).OrderBy(x => x.NowViewModel.Name));

            // Create Pending Revision Tree
            EdtsRevision = new SilentObservableCollection<NowRevisionTreeViewModel>();
            pendingTree = new NowRevisionTreeViewModel(DateTime.MinValue, true,
                            EDTs.Where(x => x.NowViewModel.PublishedStatus == PublishedStatus.RevisionPending));
            EdtsRevision.AddSorted(pendingTree);

            //Create Revision Trees
            MakeRevisionTrees(EDTs.Where(x => x.NowViewModel.PublishedStatus == PublishedStatus.Revision));

            staticSearchErrorText = "There was not any matching EDTs for '{0}'.";

        }

        private void MakeRevisionTrees(IEnumerable<NowTreeViewModel> revisions)
        {
            foreach (var revisionGroup in
                            revisions
                                .GroupBy(x => x.NowViewModel.PublishedStatusDate.Date)
                                .OrderBy(x => x.Key))
            {
                var tree = new NowRevisionTreeViewModel(revisionGroup.Key, true,revisionGroup);
                EdtsRevision.AddSorted(tree);
            }
        }

        public SilentObservableCollection<NowTreeViewModel> EDTs { get; }

        public SilentObservableCollection<NowTreeViewModel> EdtsDraft { get; }

        public SilentObservableCollection<NowTreeViewModel> EdtsPublished { get; }

        public SilentObservableCollection<NowRevisionTreeViewModel> EdtsRevision { get; }

        public void AppendRevision(NowTreeViewModel newItem)
        {
            //deal with pending
            if (newItem.NowViewModel.PublishedStatus == PublishedStatus.RevisionPending)
            {
                AppendRevisionPending(newItem);
                return;
            }
            //match to an existing root item
            var matchedRoot = EdtsRevision.ToList().FirstOrDefault(x => x.GroupDate == newItem.NowViewModel.PublishedStatusDate.Date);
            if (matchedRoot == null)
            {
                var tree = new NowRevisionTreeViewModel(newItem.NowViewModel.PublishedStatusDate, true, new List<NowTreeViewModel>() { newItem });
                EdtsRevision.AddSorted(tree);
            }
            else
            {
                matchedRoot.Children.AddSorted(newItem);
            }
        }

        public void AppendRevisionPending(NowTreeViewModel newItem)
        {
            //match to an child item under pending tree to ensure that the result appears only once
            var revisionTrees = pendingTree.Children.Where(x => x.GetType() == typeof(NowTreeViewModel)).Select(x => (NowTreeViewModel)x).ToList();
            var matchedTree = revisionTrees.FirstOrDefault(x => x.NowViewModel.MasterUUID == newItem.NowViewModel.MasterUUID);
            if (matchedTree == null)
            {
                pendingTree.Children.AddSorted(newItem);
            }
            else
            {
                //Duplicate revision pending is not allowed. The new revision pending has probably been made from a published pending and is not required and therefore
                //must be purged
                if (newItem.NowViewModel.UUID != matchedTree.NowViewModel.UUID)
                {
                    //purge views
                    treeModel.AllEDTsViewModel.EDTs.RemoveAll(x => x.Now.UUID == newItem.NowViewModel.UUID);
                    treeModel.AllNowRequirementsViewModel.NowRequirements.RemoveAll(x => x.NOWUUID == newItem.NowViewModel.UUID);

                    //purge data
                    treeModel.AllData.Nows.RemoveAll(x => x.UUID == newItem.NowViewModel.UUID);
                    treeModel.AllData.NowRequirements.RemoveAll(x => x.NOWUUID == newItem.NowViewModel.UUID);
                }
            }
        }

        internal void ResetRevisionPending()
        {
            //get any revision items from the pending tree
            var revisions = new List<NowTreeViewModel>();
            if (pendingTree.Children != null)
            {
                foreach (var child in pendingTree.Children)
                {
                    var nowChild = child as NowTreeViewModel;
                    if (nowChild.NowViewModel.PublishedStatus == PublishedStatus.Revision)
                    {
                        revisions.Add(nowChild);
                    }
                }
            }

            //add the revisions to the revisions tree and remove from pending
            foreach (var edt in revisions)
            {
                AppendRevision(edt);
                pendingTree.Children.Remove(edt);
            }
        }
    }
}
