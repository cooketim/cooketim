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
    public interface IAddNowRequirementTextValueCommand : ICommand { }

    public class AddNowRequirementTextValueCommand : IAddNowRequirementTextValueCommand
    {
        private ITreeModel treeModel;
        public AddNowRequirementTextValueCommand(ITreeModel treeModel)
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

            //Make a new item 
            var data = new NowRequirementText(selectedNowRequirement.NowRequirementViewModel.NowRequirement);
            data.NowReference = "newRequirementText";
            data.NowText = "Enter new text here ...";
            var newItem = new NowRequirementTextViewModel(data, selectedNowRequirement.NowRequirementViewModel);

            //ensure that Now Reference is unique
            var duplicateValue = false;
            
            int i = 1;
            do
            {
                if (selectedNowRequirement.NowRequirementViewModel.TextValues != null)
                {
                    duplicateValue = selectedNowRequirement.NowRequirementViewModel.TextValues.FirstOrDefault(x => x.NowReference == data.NowReference) != null;
                    if (duplicateValue)
                    {
                        i++;
                        data.NowReference = string.Format("newRequirementText{0}", i);
                    }
                }

            } while (duplicateValue);

            //determine the index of the current selection
            var index = 0;
            if (selectedNowRequirement.NowRequirementViewModel.SelectedTextValue != null)
            {
                index = selectedNowRequirement.NowRequirementViewModel.TextValues.IndexOf(selectedNowRequirement.NowRequirementViewModel.SelectedTextValue);
                if (index == -1)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }
            }

            selectedNowRequirement.NowRequirementViewModel.TextValues.Insert(index, newItem);
        }
    }
}
