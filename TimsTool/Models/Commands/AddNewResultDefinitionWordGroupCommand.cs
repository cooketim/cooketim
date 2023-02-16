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
    public interface IAddNewResultDefinitionWordGroupCommand : ICommand { }
    
    public class AddNewResultDefinitionWordGroupCommand : IAddNewResultDefinitionWordGroupCommand
    {
        private ITreeModel treeModel;
        public AddNewResultDefinitionWordGroupCommand(ITreeModel treeModel)
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
            var data = new ResultDefinitionWordGroup();
            var rdVM = new ResultDefinitionWordGroupViewModel(treeModel, data);

            //ensure that ResultDefinitionWord is unique
            var duplicateResultDefinitionWord = false;
            int i = 1;
            do
            {
                duplicateResultDefinitionWord = treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.FirstOrDefault(x => x.ResultDefinitionWord.ToLowerInvariant() == data.ResultDefinitionWord.ToLowerInvariant()) != null;
                if (duplicateResultDefinitionWord)
                {
                    i++;
                    data.ResultDefinitionWord = string.Format("New Result Definition Word Group ({0})", i);
                }

            } while (duplicateResultDefinitionWord);


            //Make a new tree item based on the selected tree item
            ResultWordGroupTreeViewModel parent;
            var selectedResultDefinitionWordGroup = treeModel.SelectedItem as ResultDefinitionWordGroupTreeViewModel;

            if (selectedResultDefinitionWordGroup == null)
            {
                //selected tree item could be a word group i.e. the direct parent or a a result definition
                var parentResult = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

                if (parentResult == null)
                {
                    //parent word group selected
                    parent = treeModel.SelectedItem as ResultWordGroupTreeViewModel;
                }
                else
                {
                    //result definition selected, create a word group for the new result definition word group
                    var newResultWordGroup = new ResultWordGroup(parentResult.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition);
                    var newWgVm = new ResultWordGroupViewModel(newResultWordGroup, treeModel.AllResultDefinitionWordGroupViewModel);
                    newWgVm.ParentResultDefinitionVM = parentResult.ResultRuleViewModel.ChildResultDefinitionViewModel;
                    if (parentResult.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.WordGroups == null)
                    {
                        parentResult.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.WordGroups = new List<ResultWordGroup>();
                    }
                    parentResult.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.WordGroups.Add(newResultWordGroup);
                    parent = new ResultWordGroupTreeViewModel(treeModel, newWgVm, parentResult);

                    if (parentResult.IsExpanded)
                    {
                        parentResult.Children.Add(parent);
                    }
                    else
                    {
                        parentResult.IsExpanded = true;
                    }
                }
            }
            else
            {
                //sibling selected
                parent = (ResultWordGroupTreeViewModel)selectedResultDefinitionWordGroup.Parent;
            }

            //when required set the rdwg as a draft
            rdVM.PublishedStatus = PublishedStatus.Draft;
            data.IsNewItem = true;

            var newItem = new ResultDefinitionWordGroupTreeViewModel(treeModel, rdVM, parent);

            //add the ResultDefinitionWordGroup
            if (parent.ResultWordGroupViewModel.ResultDefinitionWordGroups == null) { parent.ResultWordGroupViewModel.ResultDefinitionWordGroups = new List<ResultDefinitionWordGroupViewModel>(); }
            parent.ResultWordGroupViewModel.ResultDefinitionWordGroups.Add(rdVM);
            if (parent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups == null) { parent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups = new List<ResultDefinitionWordGroup>(); }
            parent.ResultWordGroupViewModel.ResultWordGroup.ResultDefinitionWordGroups.Add(data);

            //add to the collection of parent views when required
            var match = rdVM.ParentWordGroups.FirstOrDefault(x => x.UUID == parent.ResultWordGroupViewModel.UUID);
            if (match == null)
            {
                rdVM.ParentWordGroups.Add(parent.ResultWordGroupViewModel);
            }

            //ensure that the pasted item is selected and the parent is expanded
            if (parent.IsExpanded)
            {
                parent.Children.Insert(0, newItem);
            }
            else
            {
                parent.IsExpanded = true;
            }
            
            newItem.IsSelected = true;

            //reset any copied item
            newItem.PendingCopyModel = null;

            //sort out the other data store objects
            treeModel.AllResultDefinitionWordGroupViewModel.WordGroups.Add(rdVM);
            treeModel.AllData.ResultDefinitionWordGroups.Add(data);

            //inform the result definition that there is a new word group so that it may determine if the short code needs to be edittable
            parent.ResultWordGroupViewModel.ParentResultDefinitionVM.SetWordGroupsChanged();
        }
    }
}
