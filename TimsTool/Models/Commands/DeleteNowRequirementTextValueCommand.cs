using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IDeleteNowRequirementTextValueCommand : ICommand { }

    public class DeleteNowRequirementTextValueCommand : IDeleteNowRequirementTextValueCommand
    {
        private ITreeModel treeModel;
        public DeleteNowRequirementTextValueCommand(ITreeModel treeModel)
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
            var selectedNowRequirement = treeModel.SelectedItem as NowRequirementTreeViewModel;
            if (selectedNowRequirement == null) { return; }

            //remove the current selection from the view model
            if (selectedNowRequirement.NowRequirementViewModel.SelectedTextValue != null)
            {
                selectedNowRequirement.NowRequirementViewModel.TextValues.Remove(selectedNowRequirement.NowRequirementViewModel.SelectedTextValue);
            }
        }
    }
}
