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
    public interface IDraftResultPromptWordGroupCommand : ICommand { }

    public class DraftResultPromptWordGroupCommand : IDraftResultPromptWordGroupCommand
    {
        private ITreeModel treeModel;
        public DraftResultPromptWordGroupCommand(ITreeModel treeModel)
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
            //See if the source was sent as a paraneter
            var source = parameter == null ? null : parameter as ResultPromptWordGroupTreeViewModel;

            //Base the source on the current selected item
            if (source == null)
            {
                source = treeModel.SelectedItem as ResultPromptWordGroupTreeViewModel;
            }

            if (source == null)
            {
                Log.Error("Result Prompt Word Group not sent for drafting");
                return;
            }

            DraftResultPromptWordGroupTreeViewModel(source);
        }

        private void DraftResultPromptWordGroupTreeViewModel(ResultPromptWordGroupTreeViewModel source)
        {
            //Draft a new deep copy of the ResultPromptWordGroupTreeViewModel 
            var newRpwgVm = source.ResultPromptWordGroupViewModel.Draft();

            //determine the parent result definition tree view
            var parentResultPromptTree = source.Parent as ResultPromptTreeViewModel;
            var parentResultTree = parentResultPromptTree.Parent as ResultDefinitionTreeViewModel;

            //Copy any publication tags from the root parent result to the draft word group
            foreach (var tag in parentResultTree.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTagsModel.SelectedPublicationTags)
            {
                newRpwgVm.PublicationTagsModel.AddTagCommand.Execute(tag);
            }

            //Set the new vm on the selected tree
            source.ResultPromptWordGroupViewModel = newRpwgVm;

            //update any other draft result prompts that use this word group
            UpdateAllDraftResultPrompts(newRpwgVm, parentResultTree);

            //reset the source to reflect the new view model
            source.ResetVM();
            treeModel.ResetDraftVisibility();
        }

        private void UpdateAllDraftResultPrompts(ResultPromptWordGroupViewModel newRpwgVm, ResultDefinitionTreeViewModel parentResultTree)
        {
            var matchedPromptRules = treeModel.AllResultPromptViewModel.Prompts.Where(
                    x => x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft &&
                         x.ResultPromptViewModel.WordGroups != null &&
                         x.ResultPromptViewModel.WordGroups.Where(y=>y.MasterUUID == newRpwgVm.MasterUUID).Any());

            var matchedDraftPrompts = matchedPromptRules
                .GroupBy(x => x.ResultPromptViewModel.UUID)
                .Select(x => x.First())
                .Select(x => x.ResultPromptViewModel);

            var matchedDraftResults = matchedPromptRules
                .GroupBy(x => x.ResultDefinitionViewModel.UUID)
                .Select(x => x.First())
                .Select(x => x.ResultDefinitionViewModel);

            //update the prompt view models
            foreach (var dp in matchedDraftPrompts)
            {
                //replace the VM
                var matchedWG = dp.WordGroups.First(x => x.MasterUUID == newRpwgVm.MasterUUID);
                var index = dp.WordGroups.IndexOf(matchedWG);
                dp.WordGroups.RemoveAt(index);
                dp.WordGroups.Insert(index, newRpwgVm);

                //replace the data
                dp.ResultPrompt.ResultPromptWordGroups.Remove(matchedWG.ResultPromptWordGroup);
                dp.ResultPrompt.ResultPromptWordGroups.Add(newRpwgVm.ResultPromptWordGroup);

                //add prompt to the collection of parents
                var matched = newRpwgVm.ParentPrompts.FirstOrDefault(x => x.UUID == dp.UUID);
                if (matched == null) { newRpwgVm.ParentPrompts.Add(dp); }
            }            

            //deal with the parent prompt trees via the parent draft result trees
            foreach (var dr in matchedDraftResults.Where(x=>x.UUID != parentResultTree.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID))
            {
                var rootParentResultTree = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.FirstOrDefault(
                    x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == dr.UUID);

                if (rootParentResultTree != null)
                {
                    rootParentResultTree.ResetChildren();
                }
            }
        }
    }
}
