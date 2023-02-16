using System;
using Models.ViewModels;
using System.Windows.Input;

namespace Models.Commands
{
    public interface ICopyEDTCommand : ICommand { }

    public class CopyEDTCommand : ICopyEDTCommand
    {
        private ITreeModel treeModel;
        public CopyEDTCommand(ITreeModel treeModel)
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
            var selectedEDT = treeModel.SelectedItem as NowTreeViewModel;

            //set the copied item, ready to be pasted
            selectedEDT.PendingCopyModel = selectedEDT == null ? null : selectedEDT;
        }
    }
}
