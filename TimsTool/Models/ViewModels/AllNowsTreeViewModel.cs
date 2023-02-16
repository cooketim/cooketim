using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;

namespace Models.ViewModels
{
    public class AllNowsTreeViewModel : SearchableTreeViewModel
    {
        private NowRevisionTreeViewModel pendingTree;
        public AllNowsTreeViewModel(ITreeModel treeModel, AllNowsViewModel allNows)
        {
            this.treeModel = treeModel;
            //nows
            Nows = new SilentObservableCollection<NowTreeViewModel>(
                (from now in allNows.Nows.Where(x=>!x.IsEDT)
                 select new NowTreeViewModel(treeModel, now))
                .ToList().OrderBy(x=>x.NowViewModel.Rank));

            NowsDraft = new SilentObservableCollection<NowTreeViewModel>(
                                        Nows.Where(x => x.NowViewModel.PublishedStatus == PublishedStatus.Draft
                                        ).OrderBy(x => x.NowViewModel.Name));

            NowsPublished = new SilentObservableCollection<NowTreeViewModel>(
                                        Nows.Where(x => x.NowViewModel.PublishedStatus == PublishedStatus.Published || x.NowViewModel.PublishedStatus == PublishedStatus.PublishedPending
                                        ).OrderBy(x => x.NowViewModel.Name));

            //Create Pending Revision Tree
            NowsRevision = new SilentObservableCollection<NowRevisionTreeViewModel>();
            pendingTree = new NowRevisionTreeViewModel(DateTime.MinValue, false,
                            Nows.Where(x => x.NowViewModel.PublishedStatus == PublishedStatus.RevisionPending));
            NowsRevision.AddSorted(pendingTree);

            //Create Revision Trees
            MakeRevisionTrees(Nows.Where(x => x.NowViewModel.PublishedStatus == PublishedStatus.Revision));

            staticSearchErrorText = "There was not any matching NOWs for '{0}'.";
        }

        private void MakeRevisionTrees(IEnumerable<NowTreeViewModel> revisions)
        {
            foreach (var revisionGroup in
                            revisions
                                .GroupBy(x => x.NowViewModel.PublishedStatusDate.Date)
                                .OrderBy(x => x.Key))
            {
                var tree = new NowRevisionTreeViewModel(revisionGroup.Key, false, revisionGroup);
                NowsRevision.AddSorted(tree);
            }
        }

        public SilentObservableCollection<NowTreeViewModel> Nows { get; }

        public SilentObservableCollection<NowTreeViewModel> NowsDraft { get; }

        public SilentObservableCollection<NowTreeViewModel> NowsPublished { get; }

        public SilentObservableCollection<NowRevisionTreeViewModel> NowsRevision { get; }

        public void AppendRevision(NowTreeViewModel newItem)
        {
            //deal with pending
            if (newItem.NowViewModel.PublishedStatus == PublishedStatus.RevisionPending)
            {
                AppendRevisionPending(newItem);
                return;
            }
            //match to an existing root item
            var matchedRoot = NowsRevision.ToList().FirstOrDefault(x => x.GroupDate == newItem.NowViewModel.PublishedStatusDate.Date);
            if (matchedRoot == null)
            {
                var tree = new NowRevisionTreeViewModel(newItem.NowViewModel.PublishedStatusDate, false, new List<NowTreeViewModel>() { newItem });
                NowsRevision.AddSorted(tree);
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
                    treeModel.AllNowsViewModel.Nows.RemoveAll(x => x.Now.UUID == newItem.NowViewModel.UUID);
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
            foreach (var rd in revisions)
            {
                AppendRevision(rd);
                pendingTree.Children.Remove(rd);
            }
        }
    }
}
