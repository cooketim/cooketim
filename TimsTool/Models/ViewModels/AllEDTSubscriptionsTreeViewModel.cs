using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using DataLib;

namespace Models.ViewModels
{
    public class AllEDTSubscriptionsTreeViewModel : SearchableTreeViewModel
    {
        private NowSubscriptionRevisionTreeViewModel pendingTree;

        public AllEDTSubscriptionsTreeViewModel(ITreeModel treeModel, AllEDTSubscriptionViewModel allEDTSubscriptions, AllEDTsTreeViewModel allEDTs)
        {
            this.treeModel = treeModel;
            //EDT subscriptions
            EDTSubscriptions = new SilentObservableCollection<NowSubscriptionTreeViewModel>(
                (from x in allEDTSubscriptions.EDTSubscriptions
                 select new NowSubscriptionTreeViewModel(treeModel, x, allEDTs)).OrderBy(x => x.NowSubscriptionViewModel.Name)
                .ToList());

            EDTSubscriptionsDraft = new SilentObservableCollection<NowSubscriptionTreeViewModel>(
                                        EDTSubscriptions.Where(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Draft
                                        ).OrderBy(x => x.NowSubscriptionViewModel.Name));

            EDTSubscriptionsPublished = new SilentObservableCollection<NowSubscriptionTreeViewModel>(
                                        EDTSubscriptions.Where(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Published || x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.PublishedPending
                                        ).OrderBy(x => x.NowSubscriptionViewModel.Name));

            // Create Pending Revision Tree
            EDTSubscriptionsRevision = new SilentObservableCollection<NowSubscriptionRevisionTreeViewModel>();
            pendingTree = new NowSubscriptionRevisionTreeViewModel(DateTime.MinValue, NowSubscriptionType.EDT,
                            EDTSubscriptions.Where(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.RevisionPending));
            EDTSubscriptionsRevision.AddSorted(pendingTree);

            //Create Revision Trees
            MakeRevisionTrees(EDTSubscriptions.Where(x => x.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Revision));

            staticSearchErrorText = "There were not any matching EDT Subscriptions for '{0}'.";
        }

        private void MakeRevisionTrees(IEnumerable<NowSubscriptionTreeViewModel> revisions)
        {
            foreach (var revisionGroup in
                            revisions
                                .GroupBy(x => x.NowSubscriptionViewModel.PublishedStatusDate.Date)
                                .OrderBy(x => x.Key))
            {
                var tree = new NowSubscriptionRevisionTreeViewModel(revisionGroup.Key, NowSubscriptionType.EDT, revisionGroup);
                EDTSubscriptionsRevision.AddSorted(tree);
            }
        }

        public SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptions { get; }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptionsDraft { get; }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptionsPublished { get; }
        public SilentObservableCollection<NowSubscriptionTreeViewModel> EDTSubscriptionsPublishedHistory { get; }
        public SilentObservableCollection<NowSubscriptionRevisionTreeViewModel> EDTSubscriptionsRevision { get; }

        public void AppendRevision(NowSubscriptionTreeViewModel newItem)
        {
            //deal with pending
            if (newItem.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.RevisionPending)
            {
                AppendRevisionPending(newItem);
                return;
            }
            //match to an existing root item
            var matchedRoot = EDTSubscriptionsRevision.ToList().FirstOrDefault(x => x.GroupDate == newItem.NowSubscriptionViewModel.PublishedStatusDate.Date);
            if (matchedRoot == null)
            {
                var tree = new NowSubscriptionRevisionTreeViewModel(newItem.NowSubscriptionViewModel.PublishedStatusDate, NowSubscriptionType.EDT, new List<NowSubscriptionTreeViewModel>() { newItem });
                EDTSubscriptionsRevision.AddSorted(tree);
            }
            else
            {
                matchedRoot.Children.AddSorted(newItem);
            }
        }

        public void AppendRevisionPending(NowSubscriptionTreeViewModel newItem)
        {
            //match to an child item under pending tree to ensure that the result appears only once
            var revisionTrees = pendingTree.Children.Where(x => x.GetType() == typeof(NowSubscriptionTreeViewModel)).Select(x => (NowSubscriptionTreeViewModel)x).ToList();
            var matchedTree = revisionTrees.FirstOrDefault(x => x.NowSubscriptionViewModel.MasterUUID == newItem.NowSubscriptionViewModel.MasterUUID);
            if (matchedTree == null)
            {
                pendingTree.Children.AddSorted(newItem);
            }
            else
            {
                //Duplicate revision pending is not allowed. The new revision pending has probably been made from a published pending and is not required and therefore
                //must be purged
                if (newItem.NowSubscriptionViewModel.UUID != matchedTree.NowSubscriptionViewModel.UUID)
                {
                    treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.RemoveAll(x => x.NowSubscription.UUID == newItem.NowSubscriptionViewModel.UUID);
                    treeModel.AllData.NowSubscriptions.RemoveAll(x => x.UUID == newItem.NowSubscriptionViewModel.UUID);
                }
            }
        }

        internal void ResetRevisionPending()
        {
            //get any revision items from the pending tree
            var revisions = new List<NowSubscriptionTreeViewModel>();
            if (pendingTree.Children != null)
            {
                foreach (var child in pendingTree.Children)
                {
                    var nowSubsChild = child as NowSubscriptionTreeViewModel;
                    if (nowSubsChild.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Revision)
                    {
                        revisions.Add(nowSubsChild);
                    }
                }
            }

            //add the revisions to the revisions tree and remove from pending
            foreach (var sub in revisions)
            {
                AppendRevision(sub);
                pendingTree.Children.Remove(sub);
            }
        }
    }
}
