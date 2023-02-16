using Models.ViewModels;
using System;
using System.Windows.Input;

namespace Models.Commands
{
    public interface ISetDeleteNowSubscriptionCommand : ICommand { }

    public class SetDeleteNowSubscriptionCommand : ISetDeleteNowSubscriptionCommand
    {
        private ITreeModel treeModel;
        public SetDeleteNowSubscriptionCommand(ITreeModel treeModel)
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
            var selectedNowSubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;

            var deletedDate = DateTime.Now;

            //set the deleted date on the Now Subscription           
            selectedNowSubscription.NowSubscriptionViewModel.DeletedDate = deletedDate;

            //set the deleted date on any child subscriptions
            if (selectedNowSubscription.NowSubscriptionViewModel.ChildNowSubscriptions != null)
            {
                foreach (var ns in selectedNowSubscription.NowSubscriptionViewModel.ChildNowSubscriptions)
                {
                    ns.DeletedDate = deletedDate;
                }
            }
        }
    }
}
