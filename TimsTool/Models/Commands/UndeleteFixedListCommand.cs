using Models.ViewModels;
using System;
using System.Windows.Input;
using System.Linq;
using DataLib;
using System.Collections.Generic;

namespace Models.Commands
{
    public interface IUndeleteFixedListCommand : ICommand { }
    public class UndeleteFixedListCommand : IUndeleteFixedListCommand
    {
        private ITreeModel treeModel;
        public UndeleteFixedListCommand(ITreeModel treeModel)
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

            //get the parent
            var parent = selectedFixedList.Parent as ResultPromptTreeViewModel;
            if (parent == null) { return; }

            //reinstate the data
            selectedFixedList.FixedListViewModel.DeletedDate = null;
            selectedFixedList.FixedListViewModel.LastModifiedDate = DateTime.Now;
            foreach (var flv in selectedFixedList.FixedListViewModel.FixedListValues)
            {
                flv.DeletedDate = null;
                flv.LastModifiedDate = DateTime.Now;
            }
            parent.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList = selectedFixedList.FixedListViewModel;

            //select the parent
            parent.IsSelected = true;
        }
    }
}
