using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface ICopyResultPromptCommand : ICommand { }

    public class CopyResultPromptCommand : ICopyResultPromptCommand
    {
        private ITreeModel treeModel;
        public CopyResultPromptCommand(ITreeModel treeModel)
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
            var selectedResultPrompt = treeModel.SelectedItem as ResultPromptTreeViewModel;

            //set the copied item, ready to be pasted
            selectedResultPrompt.PendingCopyModel = selectedResultPrompt == null ? null : selectedResultPrompt;
        }
    }
}
