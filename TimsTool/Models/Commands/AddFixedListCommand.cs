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
    public interface IAddFixedListCommand : ICommand { }
    public class AddFixedListCommand : IAddFixedListCommand
    {
        private ITreeModel treeModel;
        public AddFixedListCommand(ITreeModel treeModel)
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
            //determine the parentage, only continue if the parent does not already have a fixed list
            var parent = treeModel.SelectedItem as ResultPromptTreeViewModel;

            if (parent.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList != null)
            {
                //parent already has a fixed list, do not continue with the add
                return;                
            }
            
            //Make a new data item 
            var data = new FixedList();
            var flVM = new FixedListViewModel(treeModel, data);


            //set the publication status
            data.PublishedStatus = PublishedStatus.Draft;
            data.IsNewItem = true;

            //ensure that Fixed List Label is unique
            var duplicate = false;
            int i = 1;
            do
            {
                duplicate = treeModel.AllFixedListViewModel.FixedLists.FirstOrDefault(x => x.Label == data.Label) != null;
                if (duplicate)
                {
                    i++;
                    data.Label = string.Format("New Fixed List ({0})", i);
                }

            } while (duplicate);


           
            var newItem = new FixedListTreeViewModel(treeModel, flVM, parent);

            //add the FixedList
            parent.ResultPromptRuleViewModel.ResultPromptViewModel.FixedList = flVM;
            parent.Children.Insert(0, newItem);
            
            //ensure that the pasted item is selected and the parent is expanded
            parent.IsExpanded = true;
            newItem.IsSelected = true;

            //reset any copied item
            newItem.PendingCopyModel = null;

            //sort out the other data store objects
            treeModel.AllFixedListViewModel.FixedLists.Add(flVM);
            treeModel.AllData.FixedLists.Add(data);
        }
    }
}
