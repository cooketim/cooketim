using Models.ViewModels;
using System;
using System.Windows.Input;

namespace Models.Commands
{
    public interface ISetDeleteNowCommand : ICommand { }

    public class SetDeleteNowCommand : ISetDeleteNowCommand
    {
        private ITreeModel treeModel;
        public SetDeleteNowCommand(ITreeModel treeModel)
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

            var deletedDate = DateTime.Now;

            //set the deleted date on the Now            
            selectedNow.NowViewModel.DeletedDate = deletedDate;

            //set the deleted date on the now requirements and their prompt rules
            foreach (var nr in selectedNow.NowViewModel.AllNowRequirements)
            {
                nr.DeletedDate = deletedDate;

                if (nr.NowRequirementPromptRules != null)
                {
                    foreach (var nrpr in nr.NowRequirementPromptRules)
                    {
                        nrpr.DeletedDate = deletedDate;
                    }
                }
            }
        }
    }
}
