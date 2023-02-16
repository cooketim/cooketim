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
    public interface IDraftResultDefinitionWordGroupCommand : ICommand { }

    public class DraftResultDefinitionWordGroupCommand : IDraftResultDefinitionWordGroupCommand
    {
        private ITreeModel treeModel;
        public DraftResultDefinitionWordGroupCommand(ITreeModel treeModel)
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
            var source = parameter == null ? null : parameter as ResultDefinitionWordGroupTreeViewModel;

            //Base the source on the current selected item
            if (source == null)
            {
                source = treeModel.SelectedItem as ResultDefinitionWordGroupTreeViewModel;
            }

            if (source == null)
            {
                Log.Error("Result Word Group not sent for drafting");
                return;
            }

            DraftResultWordGroupTreeViewModel(source);
        }

        private void DraftResultWordGroupTreeViewModel(ResultDefinitionWordGroupTreeViewModel source)
        {
            //Draft a new deep copy of the ResultDefinitionWordGroupTreeViewModel 
            var newRdWgVm = source.ResultDefinitionWordGroupViewModel.Draft();

            //determine the parent result definition tree view
            var parentResultPromptTree = source.Parent;
            var parentResultTree = parentResultPromptTree.Parent as ResultDefinitionTreeViewModel;

            //Copy any publication tags from the root parent result to the draft result definition word group
            foreach (var tag in parentResultTree.ResultRuleViewModel.ChildResultDefinitionViewModel.PublicationTagsModel.SelectedPublicationTags)
            {
                newRdWgVm.PublicationTagsModel.AddTagCommand.Execute(tag);
            }

            //update any other draft results that use this result definition word group
            UpdateAllDraftResults(newRdWgVm);

            //reset the source to reflect the new view model
            source.ResetVM();
            treeModel.ResetDraftVisibility();
        }

        private void UpdateAllDraftResults(ResultDefinitionWordGroupViewModel newRdWgVm)
        {
            foreach (var rwg in newRdWgVm.ParentWordGroups.Where(x=>x.ParentResultDefinitionVM.PublishedStatus == PublishedStatus.Draft))
            {
                //apply the new draft result definition word group view model to the word group
                rwg.SetDraftResultDefinitionWordGroup(newRdWgVm);

                //get the draft tree item 
                var draftTreeViewItem = treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.First(
                    x => x.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == rwg.ParentResultDefinitionVM.UUID);

                //apply the new draft result definition word group view model to the child word group tree item on the given draft result definition tree view
                if (draftTreeViewItem.Children.Count > 0 && !draftTreeViewItem.HasDummyChild)
                {
                    var childWordGroupTreeItems = draftTreeViewItem.Children.Where(x => x.GetType() == typeof(ResultWordGroupTreeViewModel)).Select(x => x as ResultWordGroupTreeViewModel);
                    var matchedChildWG = childWordGroupTreeItems.First(x => x.ResultWordGroupViewModel.WordGroupName == rwg.WordGroupName);
                    matchedChildWG.ResultWordGroupViewModel = rwg;

                    //apply the new draft result definition word group view model to the child result definition word group tree item
                    if (matchedChildWG.Children.Count > 0 && !matchedChildWG.HasDummyChild)
                    {
                        var childRDWGTreeItems = matchedChildWG.Children.Where(x => x.GetType() == typeof(ResultDefinitionWordGroupTreeViewModel)).Select(x => x as ResultDefinitionWordGroupTreeViewModel);
                        var matchedChildRDWG = childRDWGTreeItems.First(x => x.ResultDefinitionWordGroupViewModel.MasterUUID == newRdWgVm.MasterUUID);
                        matchedChildRDWG.ResultDefinitionWordGroupViewModel = newRdWgVm;
                    }
                }
            }
        }
    }
}
