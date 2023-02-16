using DataLib;
using Models.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IAddNewResultDefinitionCommand : ICommand { }
    public class AddNewResultDefinitionCommand : IAddNewResultDefinitionCommand
    {
        private ITreeModel treeModel;
        public AddNewResultDefinitionCommand(ITreeModel treeModel)
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
            var ptm = parameter == null ? null : parameter as PublicationTagsModel;
            Execute(ptm);
        }

        protected virtual void Execute(PublicationTagsModel ptm)
        {
            if (ptm == null)
            {
                Log.Error("Publication Tags Model not sent for add new Result Definition");
                return;
            }
            //make a new item and select it
            var newItem = MakeAndStoreResultDefinitionTreeViewModel(ptm);
            //reset any copied item
            newItem.PendingCopyModel = null;

            newItem.IsSelected = true;
        }

        protected ResultDefinitionTreeViewModel MakeAndStoreResultDefinitionTreeViewModel(PublicationTagsModel ptm)
        {
            //Make a new data item 
            var data = new ResultDefinition();

            //set the publication status
            data.PublishedStatus = PublishedStatus.Draft;
            data.IsNewItem = true;

            if (ptm != null && ptm.SelectedPublicationTags != null)
            {
                data.PublicationTags = ptm.SelectedPublicationTags.ToList();
            }

            var rdVM = new ResultDefinitionViewModel(treeModel, data,treeModel.AllResultPromptViewModel, treeModel.AllResultDefinitionWordGroupViewModel, 
                treeModel.UserGroups, treeModel.AllResultRuleViewModel);

            //ensure that label is unique
            var duplicateLabel = false;
            int i = 1;
            do
            {
                duplicateLabel = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.FirstOrDefault(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.Label == data.Label) != null;
                if (duplicateLabel)
                {
                    i++;
                    data.Label = string.Format("New Result Definition ({0})", i);
                    data.ShortCode = string.Format("Code ({0})", i);
                }

            } while (duplicateLabel);

            //Make new tree model
            var rule = new ResultRuleViewModel(treeModel, rdVM, null);
            var newItem = new ResultDefinitionTreeViewModel(treeModel, rule);

            //sort out the data store objects
            treeModel.AllResultRuleViewModel.Rules.Add(rule);
            treeModel.AllResultDefinitionsViewModel.Definitions.Add(rdVM);
            treeModel.AllData.ResultDefinitions.Add(data);

            //determine the index of the current selection 
            var selectedResultDefinition = GetTreeViewItemForSelectedItem();            
            var index = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.IndexOf(selectedResultDefinition);
            if (index < 0) { index = 0; }

            //determine the new rank based on the selected item
            if (selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank.HasValue)
            {
                if (index < treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.Count() -1)
                {
                    var nextItem = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft[index + 1];
                    if (nextItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank.HasValue)
                    {
                        SetRank(nextItem.ResultRuleViewModel.ChildResultDefinitionViewModel, selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel, data);                        
                    }
                }
                else
                {
                    data.Rank = selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Rank.Value + 100;
                }
            }

            //add to the tree of root definitions and all definitions
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.AddSorted(newItem);
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.AddSorted(newItem);

            return newItem;
        }

        private ResultDefinitionTreeViewModel GetTreeViewItemForSelectedItem()
        {
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

            if (selectedResultDefinition != null) { return selectedResultDefinition; }

            var selectedNowRequirement = treeModel.SelectedItem as NowRequirementTreeViewModel;

            if (selectedNowRequirement != null)
            {
                //find the associated result definition tree view model
                return treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.FirstOrDefault(x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.UUID == selectedNowRequirement.NowRequirementViewModel.ResultDefinition.UUID);
            }

            return null;
        }

        private void SetRank(ResultDefinitionViewModel nextItem, ResultDefinitionViewModel selectedItem, ResultDefinition data)
        {
            if (selectedItem.Rank.Value + 20 < nextItem.Rank.Value)
            {
                data.Rank = selectedItem.Rank.Value + 20;
                return;
            }

            if (selectedItem.Rank.Value + 10 < nextItem.Rank.Value)
            {
                data.Rank = selectedItem.Rank.Value + 10;
                return;
            }
            if (selectedItem.Rank.Value + 5 < nextItem.Rank.Value)
            {
                data.Rank = selectedItem.Rank.Value + 5;
                return;
            }

            int total = nextItem.Rank.Value + selectedItem.Rank.Value;
            decimal average = total > 0 ? total / 2 : 0;
            data.Rank = average == 0 ? null : (int?)Math.Round(average);
        }
    }
}
