using DataLib;
using Models.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDraftNowCommand : ICommand { }

    public class DraftNowCommand : IDraftNowCommand
    {
        private ITreeModel treeModel;
        public DraftNowCommand(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            // I intentionally left these empty because
            // this command never raises the event, and
            // not using the WeakEvent pattern here can
            // cause memory leaks.  WeakEvent pattern is
            // not simple to implement, so why bother.
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            var ptm = parameter == null ? null : parameter as PublicationTagsModel;

            if (ptm == null)
            {
                Log.Error("Publication Tags Model not sent for drafting");
                return;
            }

            //Determine the source
            var source = treeModel.SelectedItem as NowTreeViewModel;

            if (source == null)
            {
                Log.Error("Now or EDT not sent for drafting");
                return;
            }

            DraftNowTreeViewModel(source, ptm.SelectedPublicationTags.ToList(), true);
        }

        public NowTreeViewModel DraftNowTreeViewModel(NowTreeViewModel source, List<string> tags, bool withStorage)
        {
            //Draft a new deep copy of the NowTreeViewModel 
            var newItem = source.Draft(tags);

            //ensure parentage is null
            newItem.ResetParentage(null);

            if (withStorage)
            {
                SaveDraft(newItem, true);
            }

            return newItem;
        }

        public void SaveDraft(NowTreeViewModel newItem, bool withSelection)
        {
            //store the data
            StoreDataAndVMs(newItem, withSelection);

            //Set any existing draft now subscriptions with the new draft now
            UpdateDraftSubscriptions(newItem);
        }

        private void UpdateDraftEDTSubscriptions(NowTreeViewModel newItem)
        {
            var targetIncludedSubscriptions = from subs in treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions
                                              where subs.PublishedStatus == PublishedStatus.Draft &&
                                                    subs.IncludedNOWS != null &&
                                                    subs.IncludedNOWS.Any(x => x != null &&
                                                                              (x.MasterUUID ?? x.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID))
                                              select subs;

            foreach (var item in targetIncludedSubscriptions)
            {
                //replace the included edt in the view model
                var index = item.IncludedNOWS.FindIndex(x => (x.MasterUUID ?? x.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID));
                if (index == -1) { continue; }
                item.IncludedNOWS.RemoveAt(index);
                item.IncludedNOWS.Insert(index, newItem.NowViewModel.Now);

                //deal with the tree views
                var matchedSubTree = treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.FirstOrDefault(
                                                        x => (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == (item.MasterUUID ?? item.UUID));
                if (matchedSubTree == null) { continue; }

                //replace the item in the list of included edts
                var includedExcludedTrees = matchedSubTree.Children.Where(x => x.GetType() == typeof(NowSubscriptionNowsTreeViewModel)).Select(x => (NowSubscriptionNowsTreeViewModel)x).ToList();
                var includedTree = includedExcludedTrees.FirstOrDefault(x => x.IsIncludedNowsTree);
                if (includedTree == null) { continue; }
                var includedEdtTrees = includedTree.Children.Where(x => x.GetType() == typeof(NowTreeViewModel)).Select(x => (NowTreeViewModel)x).ToList();
                index = includedEdtTrees.FindIndex(x => (x.NowViewModel.MasterUUID ?? x.NowViewModel.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID));
                if (index == -1) { continue; }
                includedEdtTrees.RemoveAt(index);
                includedEdtTrees.Insert(index, newItem);
            }

            var targetExcludedSubscriptions = from subs in treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions
                                              where subs.PublishedStatus == PublishedStatus.Draft &&
                                                    subs.ExcludedNOWS != null && 
                                                    subs.ExcludedNOWS.Any(x => x != null &&
                                                                              (x.MasterUUID ?? x.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID))
                                              select subs;

            foreach (var item in targetExcludedSubscriptions)
            {
                //replace the excluded now in the view model
                var index = item.ExcludedNOWS.FindIndex(x => x.MasterUUID == newItem.NowViewModel.MasterUUID);
                if (index == -1) { continue; }
                item.ExcludedNOWS.RemoveAt(index);
                item.ExcludedNOWS.Insert(index, newItem.NowViewModel.Now);

                //deal with the tree views
                var matchedSubTree = treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.FirstOrDefault(
                                                        x => (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == (item.MasterUUID ?? item.UUID));
                if (matchedSubTree == null) { continue; }

                //replace the item in the list of included edts
                var includedExcludedTrees = matchedSubTree.Children.Where(x => x.GetType() == typeof(NowSubscriptionNowsTreeViewModel)).Select(x => (NowSubscriptionNowsTreeViewModel)x).ToList();
                var excludedTree = includedExcludedTrees.FirstOrDefault(x => !x.IsIncludedNowsTree);
                if (excludedTree == null) { continue; }
                var excludedEdtTrees = excludedTree.Children.Where(x => x.GetType() == typeof(NowTreeViewModel)).Select(x => (NowTreeViewModel)x).ToList();
                index = excludedEdtTrees.FindIndex(x => (x.NowViewModel.MasterUUID ?? x.NowViewModel.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID));
                if (index == -1) { continue; }
                excludedEdtTrees.RemoveAt(index);
                excludedEdtTrees.Insert(index, newItem);
            }
        }

        private void UpdateDraftNowSubscriptions(NowTreeViewModel newItem)
        {
            var targetIncludedSubscriptions = from subs in treeModel.AllNowSubscriptionViewModel.NowSubscriptions
                                              where subs.PublishedStatus == PublishedStatus.Draft &&
                                                    subs.IncludedNOWS != null && 
                                                    subs.IncludedNOWS.Any(x => x != null && 
                                                                              (x.MasterUUID ?? x.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID))
                                              select subs;

            foreach (var item in targetIncludedSubscriptions)
            {
                //replace the included now in the view model
                var index = item.IncludedNOWS.FindIndex(x => (x.MasterUUID ?? x.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID));
                if (index == -1) { continue; }
                item.IncludedNOWS.RemoveAt(index);
                item.IncludedNOWS.Insert(index, newItem.NowViewModel.Now);

                //deal with the tree views
                var matchedSubTree = treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.FirstOrDefault(
                                                        x => (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == (item.MasterUUID ?? item.UUID));
                if (matchedSubTree == null) { continue; }

                //replace the item in the list of included nows
                var includedExcludedTrees = matchedSubTree.Children.Where(x => x.GetType() == typeof(NowSubscriptionNowsTreeViewModel)).Select(x=> (NowSubscriptionNowsTreeViewModel)x).ToList();
                var includedTree = includedExcludedTrees.FirstOrDefault(x => x.IsIncludedNowsTree);
                if (includedTree == null) { continue; }
                var includedNowTrees = includedTree.Children.Where(x => x.GetType() == typeof(NowTreeViewModel)).Select(x => (NowTreeViewModel)x).ToList();
                index = includedNowTrees.FindIndex(x => (x.NowViewModel.MasterUUID ?? x.NowViewModel.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID));
                if (index == -1) { continue; }
                includedNowTrees.RemoveAt(index);
                includedNowTrees.Insert(index, newItem);
            }

            var targetExcludedSubscriptions = from subs in treeModel.AllNowSubscriptionViewModel.NowSubscriptions
                                              where subs.PublishedStatus == PublishedStatus.Draft &&
                                                    subs.ExcludedNOWS != null && 
                                                    subs.ExcludedNOWS.Any(x => x != null &&
                                                                              (x.MasterUUID ?? x.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID))
                                              select subs;

            foreach (var item in targetExcludedSubscriptions)
            {
                //replace the excluded now in the view model
                var index = item.ExcludedNOWS.FindIndex(x => (x.MasterUUID ?? x.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID));
                if (index == -1) { continue; }
                item.ExcludedNOWS.RemoveAt(index);
                item.ExcludedNOWS.Insert(index, newItem.NowViewModel.Now);

                //deal with the tree views
                var matchedSubTree = treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.FirstOrDefault(
                                                        x => (x.NowSubscriptionViewModel.MasterUUID ?? x.NowSubscriptionViewModel.UUID) == (item.MasterUUID ?? item.UUID));
                if (matchedSubTree == null) { continue; }

                //replace the item in the list of excvluded nows
                var includedExcludedTrees = matchedSubTree.Children.Where(x => x.GetType() == typeof(NowSubscriptionNowsTreeViewModel)).Select(x => (NowSubscriptionNowsTreeViewModel)x).ToList();
                var excludedTree = includedExcludedTrees.FirstOrDefault(x => !x.IsIncludedNowsTree);
                if (excludedTree == null) { continue; }
                var excludedNowTrees = excludedTree.Children.Where(x => x.GetType() == typeof(NowTreeViewModel)).Select(x => (NowTreeViewModel)x).ToList();
                index = excludedNowTrees.FindIndex(x => (x.NowViewModel.MasterUUID ?? x.NowViewModel.UUID) == (newItem.NowViewModel.MasterUUID ?? newItem.NowViewModel.UUID));
                if (index == -1) { continue; }
                excludedNowTrees.RemoveAt(index);
                excludedNowTrees.Insert(index, newItem);
            }
        }

        private void UpdateDraftSubscriptions(NowTreeViewModel newItem)
        {

            //Update the subscriptions with the new draft item
            if (newItem.NowViewModel.IsEDT)
            {
                UpdateDraftEDTSubscriptions(newItem);
            }
            else
            {
                UpdateDraftNowSubscriptions(newItem);
            }
        }

        private void StoreNowVMs(NowTreeViewModel newItem)
        {
            var matchedTree = treeModel.AllNowsTreeViewModel.Nows.FirstOrDefault(x => x.NowViewModel.UUID == newItem.NowViewModel.UUID);
            if (matchedTree == null)
            {
                treeModel.AllNowsTreeViewModel.Nows.Add(newItem);

                treeModel.AllNowsTreeViewModel.NowsDraft.AddSorted(newItem);

                //Store the now vm
                var matchedVM = treeModel.AllNowsViewModel.Nows.FirstOrDefault(x => x.UUID == newItem.NowViewModel.UUID);
                if (matchedVM == null)
                {
                    treeModel.AllNowsViewModel.Nows.Add(newItem.NowViewModel);
                }

                //Store the now data
                var matchedData = treeModel.AllData.Nows.FirstOrDefault(x => x.UUID == newItem.NowViewModel.UUID);
                if (matchedData == null)
                {
                    treeModel.AllData.Nows.Add(newItem.NowViewModel.Now);
                }
            }
        }

        private void StoreEdtVMs(NowTreeViewModel newItem)
        {
            var matchedTree = treeModel.AllEDTsTreeViewModel.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == newItem.NowViewModel.UUID);
            if (matchedTree == null)
            {
                treeModel.AllEDTsTreeViewModel.EDTs.Add(newItem);

                //add to the drafts collection
                treeModel.AllEDTsTreeViewModel.EdtsDraft.AddSorted(newItem);

                //Store the edt vm
                var matchedVM = treeModel.AllEDTsViewModel.EDTs.FirstOrDefault(x => x.UUID == newItem.NowViewModel.UUID);
                if (matchedVM == null)
                {
                    treeModel.AllEDTsViewModel.EDTs.Add(newItem.NowViewModel);
                }
            }
        }

        private void StoreNowRequirementDataAndVMs(List<NowRequirementViewModel> nowRequirements)
        {
            if (nowRequirements == null)
            {
                return;
            }

            foreach(var nr in nowRequirements)
            {
                var matchedVM = treeModel.AllNowRequirementsViewModel.NowRequirements.FirstOrDefault(x => x.UUID == nr.UUID);
                if (matchedVM == null)
                {
                    treeModel.AllNowRequirementsViewModel.NowRequirements.Add(nr);
                }

                var matchedData = treeModel.AllData.NowRequirements.FirstOrDefault(x => x.UUID == nr.UUID);
                if (matchedData == null)
                {
                    treeModel.AllData.NowRequirements.Add(nr.NowRequirement);
                }

                StoreNowRequirementDataAndVMs(nr.NowRequirements);
            }
        }

        private void StoreDataAndVMs(NowTreeViewModel newItem, bool withSelection)
        {
            //Store the tree view model
            if (newItem.NowViewModel.IsEDT)
            {
                StoreEdtVMs(newItem);
            }
            else
            {
                StoreNowVMs(newItem);
            }

            //store the now/edt data
            var matchedData = treeModel.AllData.Nows.FirstOrDefault(x => x.UUID == newItem.NowViewModel.UUID);
            if(matchedData == null)
            {
                treeModel.AllData.Nows.Add(newItem.NowViewModel.Now);
            }

            //store the now/edt requirements
            StoreNowRequirementDataAndVMs(newItem.NowViewModel.NowRequirements);

            //Select the new item
            if (withSelection)
            {
                newItem.IsSelected = true;
            }
        }
    }
}
