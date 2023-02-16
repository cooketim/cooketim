using DataLib;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public interface IDraftNowSubscriptionCommand : ICommand { }

    public class DraftNowSubscriptionCommand : IDraftNowSubscriptionCommand
    {
        private ITreeModel treeModel;
        public DraftNowSubscriptionCommand(ITreeModel treeModel)
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
            var source = treeModel.SelectedItem as NowSubscriptionTreeViewModel;

            if (source == null)
            {
                Log.Error("Subscription not sent for drafting");
                return;
            }

            DraftNowSubscriptionTreeViewModel(source, ptm.SelectedPublicationTags.ToList(), true);
        }

        public NowSubscriptionTreeViewModel DraftNowSubscriptionTreeViewModel(NowSubscriptionTreeViewModel source, List<string> tags, bool withStorage)
        {
            //Draft a new deep copy of the NowSubscriptionTreeViewModel 
            var newItem = source.Draft(tags);

            //ensure parentage is null
            newItem.ResetParentage(null);

            if (withStorage)
            {
                SaveDraft(newItem, true);
            }

            return newItem;
        }

        public void SaveDraft(NowSubscriptionTreeViewModel newItem, bool withSelection)
        {
            if (!SubscriptionTreeExists(newItem))
            {
                StoreTreeView(newItem);
            }

            //Store the now subscription vm
            if (!SubscriptionViewExists(newItem.NowSubscriptionViewModel))
            {
                StoreView(newItem.NowSubscriptionViewModel);
            }

            //store now subscription data
            var matchedData = treeModel.AllData.NowSubscriptions.FirstOrDefault(x => x.UUID == newItem.NowSubscriptionViewModel.UUID);
            if (matchedData == null)
            {
                treeModel.AllData.NowSubscriptions.Add(newItem.NowSubscriptionViewModel.NowSubscription);
                var exists = treeModel.AllData.NowSubscriptions.FirstOrDefault(x => x.UUID == newItem.NowSubscriptionViewModel.NowSubscription.UUID); 
            }

            //Select the new item
            if (withSelection)
            {
                newItem.IsSelected = true;
            }
        }

        private bool SubscriptionViewExists(NowSubscriptionViewModel item)
        {
            if (item.IsNow)
            {
                return treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            if (item.IsEDT)
            {
                return treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            if (item.IsCourtRegister)
            {
                return treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            if (item.IsPrisonCourtRegister)
            {
                return treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            if (item.IsInformantRegister)
            {
                return treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.FirstOrDefault(x => x.UUID == item.UUID) != null;
            }
            return false;
        }

        private bool SubscriptionTreeExists(NowSubscriptionTreeViewModel item)
        {
            if (item.NowSubscriptionViewModel.IsNow)
            {
                return treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.UUID == item.NowSubscriptionViewModel.UUID) != null;
            }
            if (item.NowSubscriptionViewModel.IsEDT)
            {
                return treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.UUID == item.NowSubscriptionViewModel.UUID) != null;
            }
            if (item.NowSubscriptionViewModel.IsCourtRegister)
            {
                return treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.UUID == item.NowSubscriptionViewModel.UUID) != null;
            }
            if (item.NowSubscriptionViewModel.IsPrisonCourtRegister)
            {
                return treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.UUID == item.NowSubscriptionViewModel.UUID) != null;
            }
            if (item.NowSubscriptionViewModel.IsInformantRegister)
            {
                return treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.UUID == item.NowSubscriptionViewModel.UUID) != null;
            }
            return false;
        }

        private void StoreView(NowSubscriptionViewModel newItem)
        {
            if (newItem.IsNow)
            {
                treeModel.AllNowSubscriptionViewModel.NowSubscriptions.Add(newItem);
                return;
            }

            if (newItem.IsEDT)
            {
                treeModel.AllEDTSubscriptionViewModel.EDTSubscriptions.Add(newItem);
                return;
            }

            if (newItem.IsCourtRegister)
            {
                treeModel.AllCourtRegisterSubscriptionViewModel.CourtRegisterSubscriptions.Add(newItem);
                return;
            }

            if (newItem.IsPrisonCourtRegister)
            {
                treeModel.AllPrisonCourtRegisterSubscriptionViewModel.PrisonCourtRegisterSubscriptions.Add(newItem);
                return;
            }

            if (newItem.IsInformantRegister)
            {
                treeModel.AllInformantRegisterSubscriptionViewModel.InformantRegisterSubscriptions.Add(newItem);
                return;
            }
        }

        private void StoreTreeView(NowSubscriptionTreeViewModel newItem)
        {
            if (newItem.NowSubscriptionViewModel.IsNow)
            {
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.Add(newItem);

                //add to the drafts collection
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsDraft.AddSorted(newItem);

                return;
            }

            if (newItem.NowSubscriptionViewModel.IsEDT)
            {
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.Add(newItem);

                //add to the drafts collection
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsDraft.AddSorted(newItem);

                return;
            }

            if (newItem.NowSubscriptionViewModel.IsCourtRegister)
            {
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.Add(newItem);

                //add to the drafts collection
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsDraft.AddSorted(newItem);

                return;
            }

            if (newItem.NowSubscriptionViewModel.IsPrisonCourtRegister)
            {
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.Add(newItem);

                //add to the drafts collection
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsDraft.AddSorted(newItem);

                return;
            }

            if (newItem.NowSubscriptionViewModel.IsInformantRegister)
            {
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.Add(newItem);

                //add to the drafts collection
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsDraft.AddSorted(newItem);

                return;
            }
        }
    }
}
