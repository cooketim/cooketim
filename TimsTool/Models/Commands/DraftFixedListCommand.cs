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
    public interface IDraftFixedListCommand : ICommand { }

    public class DraftFixedListCommand : IDraftFixedListCommand
    {
        private ITreeModel treeModel;
        public DraftFixedListCommand(ITreeModel treeModel)
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
            var source = parameter == null ? null : parameter as FixedListTreeViewModel;

            //Base the source on the current selected item
            if (source == null)
            {
                source = treeModel.SelectedItem as FixedListTreeViewModel;
            }

            if (source == null)
            {
                Log.Error("Fixed List not sent for drafting");
                return;
            }

            DraftFixedListTreeViewModel(source);
        }

        private void DraftFixedListTreeViewModel(FixedListTreeViewModel source)
        {
            //Draft a new deep copy of the FixedListTreeViewModel 
            var newFlVm = source.FixedListViewModel.Draft();

            //determine the parent result definition tree view
            var parentResultPromptTree = source.Parent;
            var parentResultTree = parentResultPromptTree.Parent as ResultDefinitionTreeViewModel;

            //Copy any publication tags from the root parent result to the draft fixed list
            foreach (var tag in parentResultTree.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTagsModel.SelectedPublicationTags)
            {
                newFlVm.PublicationTagsModel.AddTagCommand.Execute(tag);
            }

            //Set the new vm on the selected tree
            source.FixedListViewModel = newFlVm;

            //update any other draft result prompts that use this fixed list
            UpdateAllDraftResultPrompts(newFlVm, parentResultTree);

            //reset the source to reflect the new view model
            source.ResetVM();
            treeModel.ResetDraftVisibility();
        }

        private void UpdateAllDraftResultPrompts(FixedListViewModel newFlVm, ResultDefinitionTreeViewModel parentResultTree)
        {
            var matchedPromptRules = treeModel.AllResultPromptViewModel.Prompts.Where(
                    x => x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft &&
                         x.ResultPromptViewModel.FixedList != null &&
                         x.ResultPromptViewModel.FixedList.MasterUUID == newFlVm.MasterUUID);

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
                //set vm and data of the prompt
                dp.FixedList = newFlVm;

                //no need to set the data as modified because it simply has an association to fixed list by master uuid
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
