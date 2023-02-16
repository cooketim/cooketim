using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Models.ViewModels;

namespace Models.Commands
{
    public interface ICopyFixedListCommand : ICommand { }

    public class CopyFixedListCommand : ICopyFixedListCommand
    {
        private ITreeModel treeModel;
        public CopyFixedListCommand(ITreeModel treeModel)
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
            var selectedFixedList = treeModel.SelectedItem as FixedListTreeViewModel;

            //set the copied item, ready to be pasted
            selectedFixedList.PendingCopyModel = selectedFixedList == null ? null : selectedFixedList;
        }
    }
}
