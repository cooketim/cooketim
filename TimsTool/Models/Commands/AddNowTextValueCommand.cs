using DataLib;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IAddNowTextValueCommand : ICommand { }

    public class AddNowTextValueCommand : IAddNowTextValueCommand
    {
        private ITreeModel treeModel;
        public AddNowTextValueCommand(ITreeModel treeModel)
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

            //Make a new item 
            var data = new NowText(selectedNow.NowViewModel.Now);
            data.Reference = "newNowText";
            data.Text = "Enter new text here ...";
            var newItem = new NowTextViewModel(data, selectedNow.NowViewModel);

            //ensure that Now Reference is unique
            var duplicateValue = false;
            
            int i = 1;
            do
            {
                if (selectedNow.NowViewModel.TextValues != null)
                {
                    duplicateValue = selectedNow.NowViewModel.TextValues.FirstOrDefault(x => x.NowReference == data.Reference) != null;
                    if (duplicateValue)
                    {
                        i++;
                        data.Reference = string.Format("newNowText{0}", i);
                    }
                }

            } while (duplicateValue);

            //determine the index of the current selection
            var index = 0;
            if (selectedNow.NowViewModel.SelectedTextValue != null)
            {
                index = selectedNow.NowViewModel.TextValues.IndexOf(selectedNow.NowViewModel.SelectedTextValue);
                if (index == -1)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }
            }

            selectedNow.NowViewModel.TextValues.Insert(index, newItem);
        }
    }
}
