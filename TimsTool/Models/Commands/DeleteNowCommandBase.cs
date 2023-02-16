using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public abstract class DeleteNowCommandBase : ICommand
    {
        private ITreeModel treeModel;
        public DeleteNowCommandBase(ITreeModel treeModel)
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
            //Determine the selected item
            var selectedNow = treeModel.SelectedItem as NowTreeViewModel;

            //determine if this is  a removal from a subscription
            if (selectedNow.Parent != null && selectedNow.Parent is NowSubscriptionNowsTreeViewModel)
            {
                RemoveNowFromSubscription(selectedNow);
            }
            else
            {

                //Removal of a NOW about to occur - Synchronise the Now Subscriptions
                SynchroniseNowSubscriptions(selectedNow.NowViewModel);

                //get the now requirement view models
                var nrVMs = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(x => x.NOWUUID == selectedNow.NowViewModel.UUID);

                //purge all the child now requirements
                treeModel.AllNowRequirementsViewModel.NowRequirements.RemoveAll(x => x.NOWUUID == selectedNow.NowViewModel.UUID);

                //When a draft version, physically remove any now requirements and the now
                if (selectedNow.NowViewModel.PublishedStatus == PublishedStatus.Draft)
                {
                    treeModel.AllData.NowRequirements.RemoveAll(x => x.NOWUUID == selectedNow.NowViewModel.UUID);
                    treeModel.AllData.Nows.RemoveAll(x => x.UUID == selectedNow.NowViewModel.UUID);
                }
                else
                {
                    //logically delete the data so that it may be included in the next data patch upload
                    nrVMs.ForEach(y => y.DeletedDate = DateTime.Now);
                    selectedNow.NowViewModel.DeletedDate = DateTime.Now;
                }

                Execute(selectedNow);
            }

            //reset the copied item
            selectedNow.PendingCopyModel = null;
        }

        protected abstract void Execute(NowTreeViewModel selectedItem);

        protected void SynchroniseNowSubscriptions(NowViewModel nowViewModel)
        {
            //treeModel.AllNowSubscriptionsTreeViewModel

            var nowSubscriptions = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FindAll(x => (x.IncludedNOWS != null && x.IncludedNOWS.FirstOrDefault(y => y.UUID == nowViewModel.UUID) != null ||
                                                                                                                 x.ExcludedNOWS != null && x.ExcludedNOWS.FirstOrDefault(y => y.UUID == nowViewModel.UUID) != null));
            var distinctNowSubscriptionIds = nowSubscriptions.Select(x => x.UUID).Distinct();

            foreach (var subscriptionId in distinctNowSubscriptionIds)
            {
                //Match the subscription tree view item and remove the Now from the Included or Excluded Nows
                var nowSubscriptionTreeItem = treeModel.AllNowSubscriptionsTreeViewModel.NowSubscriptions.FirstOrDefault(x => x.NowSubscriptionViewModel.UUID == subscriptionId);
                Type type = typeof(NowSubscriptionNowsTreeViewModel);
                var childrenToRemove = new List<NowTreeViewModel>();
                foreach (var child in nowSubscriptionTreeItem.Children)
                {
                    if (child.GetType() != type)
                    {
                        continue;
                    }

                    foreach (var item in child.Children)
                    {
                        if (item.GetType() != typeof(NowTreeViewModel))
                        {
                            continue;
                        }

                        var now = (NowTreeViewModel)item;

                        if (now.NowViewModel.UUID == nowViewModel.UUID)
                        {
                            childrenToRemove.Add(now);
                        }
                    }
                }

                //remove the children
                childrenToRemove.ForEach(x => nowSubscriptionTreeItem.Children.Remove(x));

                //set the parent Subscription as modified
                nowSubscriptionTreeItem.NowSubscriptionViewModel.LastModifiedDate = DateTime.Now;

                //update the view models and data
                var vm = treeModel.AllNowSubscriptionViewModel.NowSubscriptions.FirstOrDefault(x => x.UUID == subscriptionId);
                if (vm.IncludedNOWS != null)
                {
                    vm.IncludedNOWS.RemoveAll(x => x.UUID == nowViewModel.UUID);
                }
                if (vm.NowSubscription.IncludedNOWS != null)
                {
                    vm.NowSubscription.IncludedNOWS.RemoveAll(x => x.UUID == nowViewModel.UUID);
                }
                if (vm.ExcludedNOWS != null)
                {
                    vm.ExcludedNOWS.RemoveAll(x => x.UUID == nowViewModel.UUID);
                }
                if (vm.NowSubscription.ExcludedNOWS != null)
                {
                    vm.NowSubscription.ExcludedNOWS.RemoveAll(x => x.UUID == nowViewModel.UUID);
                }
            }
        }

        protected void RemoveNowFromSubscription(NowTreeViewModel selectedNow)
        {
            var parent = selectedNow.Parent as NowSubscriptionNowsTreeViewModel;

            //remove the now from the subscription
            int? deletePosition = null;
            for (var i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i] is NowTreeViewModel)
                {
                    var childNow = parent.Children[i] as NowTreeViewModel;
                    if (childNow.NowViewModel.UUID == selectedNow.NowViewModel.UUID)
                    {
                        deletePosition = i;
                        break;
                    }
                }
            }
            if (deletePosition != null)
            {
                parent.Children.RemoveAt(deletePosition.Value);
            }

            //remove from the view model
            if (parent.IsIncludedNowsTree)
            {
                parent.NowSubscriptionViewModel.IncludedNOWS.RemoveAll(x => x.UUID == selectedNow.NowViewModel.UUID);
            }
            else
            {
                parent.NowSubscriptionViewModel.ExcludedNOWS.RemoveAll(x => x.UUID == selectedNow.NowViewModel.UUID);
            }

            parent.NowSubscriptionViewModel.LastModifiedDate = DateTime.Now;

            //select the next node
            var index = deletePosition;
            if (parent.Children.Count > 0)
            {
                if (index > 0 && parent.Children.Count - 1 <= index)
                {
                    index = index - 1;
                }
                parent.Children[index.Value].IsSelected = true;
            }
            else
            {
                parent.Parent.IsSelected = true;
            }
        }
    }
}
