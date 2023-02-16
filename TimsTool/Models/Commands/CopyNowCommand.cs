using System;
using Models.ViewModels;
using System.Windows.Input;

namespace Models.Commands
{
    public interface ICopyNowCommand : ICommand { }

    public class CopyNowCommand : ICopyNowCommand
    {
        private ITreeModel treeModel;
        public CopyNowCommand(ITreeModel treeModel)
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

            //set the copied item, ready to be pasted
            selectedNow.PendingCopyModel = selectedNow == null ? null : selectedNow;
        }
    }
}
