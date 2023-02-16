using Models.ViewModels;
using System;
using System.Windows.Input;
using System.Linq;
using DataLib;
using System.Collections.Generic;

namespace Models.Commands
{
    public interface IUndeleteResultPromptWordGroupCommand : ICommand { }
    public class UndeleteResultPromptWordGroupCommand : IUndeleteResultPromptWordGroupCommand
    {
        private ITreeModel treeModel;
        public UndeleteResultPromptWordGroupCommand(ITreeModel treeModel)
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
            var selectedResultPromptWordGroup = treeModel.SelectedItem as ResultPromptWordGroupTreeViewModel;

            //get the parent
            var parent = selectedResultPromptWordGroup.Parent as ResultPromptTreeViewModel;
            if (parent == null) { return; }

            //reinstate the data
            selectedResultPromptWordGroup.ResultPromptWordGroupViewModel.DeletedDate = null;
            foreach (var synonym in selectedResultPromptWordGroup.ResultPromptWordGroupViewModel.Synonyms)
            {
                synonym.DeletedDate = null;
                synonym.LastModifiedDate = DateTime.Now;
            }

            parent.ResultPromptRuleViewModel.ResultPromptViewModel.LastModifiedDate = DateTime.Now;

            //select the parent
            parent.IsSelected = true;
        }
    }
}
