using DataLib;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IAddNewResultPromptCommand : ICommand { }

    public class AddNewResultPromptCommand : ResultPromptCommandBase, IAddNewResultPromptCommand
    {
        public AddNewResultPromptCommand(ITreeModel treeModel) : base(treeModel) { }
        

        protected override void ExecuteCommand()
        {
            //Make a new data item 
            var data = new ResultPrompt();
            var rpVM = new ResultPromptViewModel(treeModel, data, treeModel.AllResultPromptWordGroupViewModel);

            //set the publication status
            data.PublishedStatus = PublishedStatus.Draft;
            data.IsNewItem = true;

            //ensure that label is unique
            var duplicateLabel = false;
            int i = 1;
            do
            {
                duplicateLabel = treeModel.AllResultPromptViewModel.Prompts.FirstOrDefault(x => x.ResultPromptViewModel.Label == data.Label) != null;
                if (duplicateLabel)
                {
                    i++;
                    data.Label = string.Format("New Result Prompt ({0})", i);
                }

            } while (duplicateLabel);

            var rule = new ResultPromptRule(rpVM.ResultPrompt, Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);
            var ruleVM = new ResultPromptRuleViewModel(treeModel,rpVM, rule, treeModel.UserGroups, Parent.ResultRuleViewModel.ChildResultDefinitionViewModel);
            var newItem = new ResultPromptTreeViewModel(treeModel, ruleVM, Parent);

            //set the publication status
            rule.PublishedStatus = PublishedStatus.Draft;

            //add the prompt
            if (Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts == null) { Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts = new SilentObservableCollection<ResultPromptRuleViewModel>(); }
            Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.Add(ruleVM);
            

            //determine the position of the selected prompt
            int newIndex = 0;
            rule.PromptSequence = GetNextPromptSequence(out newIndex);
            Parent.Children.Insert(newIndex, newItem);

            //ensure that the pasted item is selected and the Parent is expanded
            Parent.IsExpanded = true;
            newItem.IsSelected = true;

            //reset any copied item
            newItem.PendingCopyModel = null;

            //sort out the other data store objects
            treeModel.AllResultPromptViewModel.Prompts.Add(ruleVM);
            treeModel.AllData.ResultPromptRules.Add(rule);

            //deal with any related NOWs
            SynchroniseNows(newItem.ResultPromptRuleViewModel);
        }
    }
}