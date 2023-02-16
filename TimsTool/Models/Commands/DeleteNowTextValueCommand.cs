using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteNowTextValueCommand : ICommand { }

    public class DeleteNowTextValueCommand : IDeleteNowTextValueCommand
    {
        private ITreeModel treeModel;
        public DeleteNowTextValueCommand(ITreeModel treeModel)
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
            //Determine the selected tree item
            var selectedNow = treeModel.SelectedItem as NowTreeViewModel;
            if (selectedNow == null) { return; }

            //remove the current selection from the view model
            if (selectedNow.NowViewModel.SelectedTextValue != null)
            {
                selectedNow.NowViewModel.TextValues.Remove(selectedNow.NowViewModel.SelectedTextValue);
            }
        }
    }
}
