using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Models.ViewModels;

namespace Models.Commands
{
    public interface IAddNewResultPromptWordGroupCommand : ICommand { }
    public class AddNewResultPromptWordGroupCommand : IAddNewResultPromptWordGroupCommand
    {
        private ITreeModel treeModel;
        public AddNewResultPromptWordGroupCommand(ITreeModel treeModel)
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
            //Make a new data item 
            var data = new ResultPromptWordGroup();
            var rpVM = new ResultPromptWordGroupViewModel(treeModel, data);

            //ensure that ResultPromptWord is unique
            var duplicateResultPromptWord = false;
            int i = 1;
            do
            {
                duplicateResultPromptWord = treeModel.AllResultPromptWordGroupViewModel.WordGroups.FirstOrDefault(x => x.ResultPromptWord.ToLowerInvariant() == data.ResultPromptWord.ToLowerInvariant()) != null;
                if (duplicateResultPromptWord)
                {
                    i++;
                    data.ResultPromptWord = string.Format("New Result Prompt Word Group ({0})", i);
                }

            } while (duplicateResultPromptWord);


            //Make a new tree item based on the selected tree item
            ResultPromptTreeViewModel parent;
            var selectedResultPromptWordGroup = treeModel.SelectedItem as ResultPromptWordGroupTreeViewModel;

            if (selectedResultPromptWordGroup == null)
            {
                //selected tree item must be the parent prompt
                parent = treeModel.SelectedItem as ResultPromptTreeViewModel;
            }
            else
            {
                //sibling selected
                parent = (ResultPromptTreeViewModel)selectedResultPromptWordGroup.Parent;               
            }
            var newItem = new ResultPromptWordGroupTreeViewModel(treeModel, rpVM, parent);

            //set parentage of the word group
            rpVM.ParentPrompts.Add(parent.ResultPromptRuleViewModel.ResultPromptViewModel);

            //add the ResultPromptWordGroup
            if (parent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups == null) { parent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups = new List<ResultPromptWordGroupViewModel>(); }
            parent.ResultPromptRuleViewModel.ResultPromptViewModel.WordGroups.Add(rpVM);
            if (parent.ResultPromptRuleViewModel.ResultPromptViewModel.ResultPrompt.ResultPromptWordGroups == null) { parent.ResultPromptRuleViewModel.ResultPromptViewModel.ResultPrompt.ResultPromptWordGroups = new List<ResultPromptWordGroup>(); }
            parent.ResultPromptRuleViewModel.ResultPromptViewModel.ResultPrompt.ResultPromptWordGroups.Add(data);

            parent.Children.Insert(0, newItem);
            

            //ensure that the pasted item is selected and the parent is expanded
            parent.IsExpanded = true;
            newItem.IsSelected = true;

            //reset any copied item
            newItem.PendingCopyModel = null;

            //sort out the other data store objects
            treeModel.AllResultPromptWordGroupViewModel.WordGroups.Add(rpVM);
            treeModel.AllData.ResultPromptWordGroups.Add(data);
        }
    }
}
