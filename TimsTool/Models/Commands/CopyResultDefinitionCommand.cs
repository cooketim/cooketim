using System;
using Models.ViewModels;
using System.Windows.Input;
using Serilog;

namespace Models.Commands
{
    public interface ICopyResultDefinitionCommand : ICommand { }

    public class CopyResultDefinitionCommand : ICopyResultDefinitionCommand
    {
        private ITreeModel treeModel;
        public CopyResultDefinitionCommand(ITreeModel treeModel)
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
            Log.Information("Starting copy result definition");
            //Determine the selected item
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

            //set the copied item, ready to be pasted
            Log.Information(string.Format("Copied {0}", selectedResultDefinition == null ? "NULL" : selectedResultDefinition.Label));
            selectedResultDefinition.PendingCopyModel = selectedResultDefinition == null ? null : selectedResultDefinition;
        }
    }
}
