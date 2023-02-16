using DataLib;
using DocumentFormat.OpenXml.Spreadsheet;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteNowSubscriptionCommand : ICommand { }

    public class DeleteNowSubscriptionCommand : IDeleteNowSubscriptionCommand
    {
        private ITreeModel treeModel;
        public DeleteNowSubscriptionCommand(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

        NowSubscriptionTreeViewModel selectedNowSubscription;

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
            //Determine the selected item
            selectedNowSubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            if (selectedNowSubscription.Parent != null)
            {
                RemoveFromParent(selectedNowSubscription);
            }
            else
            {
                RemoveFromRoot();
            }
        }

        /// <summary>
        /// remove a child definition from its parent definition
        /// </summary>
        /// <param name="resultDefinitionTreeView"></param>
        private void RemoveFromParent(NowSubscriptionTreeViewModel nowSubscriptionTreeView)
        {
            //no need to delete, just remove from parent
            var parent = nowSubscriptionTreeView.Parent as NowSubscriptionTreeViewModel;
            if(parent == null) { return; }

            //Set the parent as modified
            parent.NowSubscriptionViewModel.LastModifiedDate = DateTime.Now;

            //remove data from parent
            parent.NowSubscriptionViewModel.ChildNowSubscriptions.RemoveAll(x => x.UUID == nowSubscriptionTreeView.NowSubscriptionViewModel.UUID);
            parent.NowSubscriptionViewModel.NowSubscription.ChildNowSubscriptions.RemoveAll(x=>x.UUID == nowSubscriptionTreeView.NowSubscriptionViewModel.UUID);

            //remove tree view model from the parent
            parent.Children.Remove(nowSubscriptionTreeView);

            RecursivelyRemoveNowSubscription(nowSubscriptionTreeView.NowSubscriptionViewModel);
        }

        private void RecursivelyRemoveNowSubscription(NowSubscriptionViewModel nowSubscriptionViewModel)
        {
            //Find any tree items
            var treeItems = treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.Where(x=>x.NowSubscriptionViewModel.UUID == nowSubscriptionViewModel.UUID);
            foreach(var item in treeItems)
            {
                RemoveTreeView(item);
            }
            //remove the view from all view model
            treeModel.AllNowSubscriptionViewModel.NowSubscriptions.RemoveAll(x => x.UUID == nowSubscriptionViewModel.UUID);

            //delete the data
            treeModel.AllData.NowSubscriptions.RemoveAll(x => x.UUID == nowSubscriptionViewModel.UUID);

            //purge the children
            if (nowSubscriptionViewModel.ChildNowSubscriptions != null)
            {
                foreach(var child in nowSubscriptionViewModel.ChildNowSubscriptions)
                {
                    RecursivelyRemoveNowSubscription(child);
                }
            }
        }

        private void RemoveFromRoot()
        {
            //when required physically remove a draft item
            if (selectedNowSubscription.NowSubscriptionViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                //remove the view from all view model
                treeModel.AllNowSubscriptionViewModel.NowSubscriptions.RemoveAll(x => x.UUID == selectedNowSubscription.NowSubscriptionViewModel.UUID);

                //delete the data
                treeModel.AllData.NowSubscriptions.RemoveAll(x => x.UUID == selectedNowSubscription.NowSubscriptionViewModel.UUID);
            }
            else
            {
                //set the data as logically deleted
                selectedNowSubscription.NowSubscriptionViewModel.DeletedDate = DateTime.Now;
            }            

            //remove from the all tree view
            RemoveTreeView(selectedNowSubscription);

            //purge the children
            if (selectedNowSubscription.NowSubscriptionViewModel.ChildNowSubscriptions != null)
            {
                foreach (var child in selectedNowSubscription.NowSubscriptionViewModel.ChildNowSubscriptions)
                {
                    RecursivelyRemoveNowSubscription(child);
                }
            }
        }

        private void RemoveTreeView(NowSubscriptionTreeViewModel toRemove)
        {
            if (toRemove.NowSubscriptionViewModel.IsNow)
            {
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.Remove(toRemove);

                //remove from the drafts collection
                treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptionsDraft.Remove(toRemove);

                return;
            }

            if (toRemove.NowSubscriptionViewModel.IsEDT)
            {
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptions.Remove(toRemove);

                //remove from the drafts collection
                treeModel.AllEDTSubscriptionsTreeViewModel.EDTSubscriptionsDraft.Remove(toRemove);

                return;
            }

            if (toRemove.NowSubscriptionViewModel.IsCourtRegister)
            {
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptions.Remove(toRemove);

                //remove from the drafts collection
                treeModel.AllCourtRegisterSubscriptionsTreeViewModel.CourtRegisterSubscriptionsDraft.Remove(toRemove);

                return;
            }

            if (toRemove.NowSubscriptionViewModel.IsPrisonCourtRegister)
            {
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptions.Remove(toRemove);

                //remove from the drafts collection
                treeModel.AllPrisonCourtRegisterSubscriptionsTreeViewModel.PrisonCourtRegisterSubscriptionsDraft.Remove(toRemove);

                return;
            }

            if (toRemove.NowSubscriptionViewModel.IsInformantRegister)
            {
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptions.Remove(toRemove);

                //remove from the drafts collection
                treeModel.AllInformantRegisterSubscriptionsTreeViewModel.InformantRegisterSubscriptionsDraft.Remove(toRemove);

                return;
            }
        }
    }
}
