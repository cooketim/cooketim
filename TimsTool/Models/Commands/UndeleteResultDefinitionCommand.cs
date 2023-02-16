using Models.ViewModels;
using System;
using System.Windows.Input;
using System.Linq;
using DataLib;
using System.Collections.Generic;

namespace Models.Commands
{
    public interface IUndeleteResultDefinitionCommand : ICommand { }
    public class UndeleteResultDefinitionCommand : IUndeleteResultDefinitionCommand
    {
        private ITreeModel treeModel;
        public UndeleteResultDefinitionCommand(ITreeModel treeModel)
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
            //Determine the selected item
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
            if (selectedResultDefinition == null) { return; }

            //get the parent
            var parent = selectedResultDefinition.Parent as ResultDefinitionTreeViewModel;

            //reinstate the data
            selectedResultDefinition.ResultRuleViewModel.DeletedDate = null;
            selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.DeletedDate = null;

            //When dealing with a child result definition, set the parent as modified otherwise set this item as modified
            if (parent == null)
            {
                //undeleting the root
                selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.LastModifiedDate = DateTime.Now;

                //Synchronise the Nows
                SynchroniseNows(selectedResultDefinition, parent, false);
            }
            else
            {
                //undeleting a child
                parent.ResultRuleViewModel.ChildResultDefinitionViewModel.LastModifiedDate = DateTime.Now;

                //ensure that the root item is also undeleted
                var rootItem = treeModel.AllResultRuleViewModel.Rules.First
                       (
                            x => x.ResultDefinitionRule.ChildResultDefinitionUUID == selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                            &&
                            x.ParentUUID == null
                       );

                if (rootItem.DeletedDate != null)
                {
                    rootItem.DeletedDate = null;

                    //Synchronise the Nows with additional parent undelete processing
                    SynchroniseNows(selectedResultDefinition, parent, true);
                }
                else
                {
                    //Synchronise the Nows
                    SynchroniseNows(selectedResultDefinition, parent, false);
                }
            }            
        }

        private void SynchroniseNows(ResultDefinitionTreeViewModel selectedResultDefinition, ResultDefinitionTreeViewModel parent, bool withParentUndelete)
        {
            var matchedNowRequirements = new List<NowRequirementViewModel>();
            if (parent == null || withParentUndelete)
            {
                //Find any root now Requirements for the selected result definition
                matchedNowRequirements.AddRange(treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll
                                (
                                    x => x.ResultDefinition.UUID == selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                                    && x.ParentNowRequirement == null
                                ));
            }
            else
            {
                //Find any child now requirements for the selected result definition
                matchedNowRequirements.AddRange(treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll
                                (
                                    x => x.ResultDefinition.UUID == selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                                    && x.ParentNowRequirement != null && x.ParentNowRequirement.ResultDefinition.UUID == parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID
                                ));
            }

            foreach (var nowGroup in matchedNowRequirements.GroupBy(x=>x.NOWUUID))
            {
                var matchedNow = MatchNOWEDT(nowGroup.First());
                if (matchedNow != null)
                {
                    matchedNow.NowViewModel.LastModifiedDate = DateTime.Now;

                    foreach (var nr in nowGroup.ToList())
                    {
                        //undelete the data
                        nr.DeletedDate = null;

                        if (nr.ParentNowRequirement == null)
                        {
                            SetNowRequirement(matchedNow, nr);
                        }
                        else
                        {
                            //match the now requirement to create a new treeview item
                            RecursivelyMatchChildNowRequirement(matchedNow.Children, nr);
                        }                        
                    }
                }                
            }            
        }

        private NowTreeViewModel MatchNOWEDT(NowRequirementViewModel nr)
        {
            var match = treeModel.Nows.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
            if (match != null) { return match; }
            return treeModel.EDTs.FirstOrDefault(x => x.NowViewModel.UUID == nr.NOWUUID);
        }

        private void RecursivelyMatchChildNowRequirement(SilentObservableCollection<TreeViewItemViewModel> children, NowRequirementViewModel vmToUndelete)
        {
            foreach (var parent in children)
            {
                if (parent is NowRequirementTreeViewModel)
                {
                    var nrItem = parent as NowRequirementTreeViewModel;

                    if (nrItem == null)
                    {
                        continue;
                    }

                    //test if we have matched the parent of the now requirement to undelete
                    if (nrItem.NowRequirementViewModel.ResultDefinition.UUID == vmToUndelete.ParentNowRequirement.ResultDefinition.UUID)
                    {
                        //determine if the matched parent already has the required child
                        var childAlreadyInPlace = false;
                        foreach(var child in parent.Children)
                        {
                            if (child is NowRequirementTreeViewModel)
                            {
                                var childNrItem = child as NowRequirementTreeViewModel;

                                if (childNrItem == null)
                                {
                                    continue;
                                }

                                if (childNrItem.NowRequirementViewModel.ResultDefinition.UUID == vmToUndelete.NowRequirement.ResultDefinition.UUID)
                                {
                                    childAlreadyInPlace = true;
                                    break;
                                }
                            }
                        }

                        if (!childAlreadyInPlace)
                        {
                            SetNowRequirement(nrItem, vmToUndelete);
                            break;
                        }
                    }                  

                    RecursivelyMatchChildNowRequirement(nrItem.Children, vmToUndelete);
                }
            }
        }

        private void SetNowRequirement(NowTreeViewModel parentNowItem, NowRequirementViewModel vmToUndelete)
        {
            var childItem = new NowRequirementTreeViewModel(treeModel, vmToUndelete, parentNowItem);
            InsertChildItem(parentNowItem.Children, childItem);
        }

        private void SetNowRequirement(NowRequirementTreeViewModel parentNrItem, NowRequirementViewModel vmToUndelete)
        {
            var childItem = new NowRequirementTreeViewModel(treeModel, vmToUndelete, parentNrItem);
            InsertChildItem(parentNrItem.Children, childItem);
        }

        private static void InsertChildItem(SilentObservableCollection<TreeViewItemViewModel> children, NowRequirementTreeViewModel newChildItem)
        {
            //find the relevant position to insert
            var index = 0;
            var childAdded = false;
            var maxSeq = 0;
            foreach (var child in children.ToList())
            {
                if (child is NowRequirementTreeViewModel)
                {
                    var nrItem = child as NowRequirementTreeViewModel;
                    if (nrItem.NowRequirementViewModel.ResultDefinitionSequence <= newChildItem.NowRequirementViewModel.ResultDefinitionSequence)
                    {
                        var diff = Math.Abs(newChildItem.NowRequirementViewModel.ResultDefinitionSequence.Value - nrItem.NowRequirementViewModel.ResultDefinitionSequence.Value);
                        if (diff == 0)
                        {
                            newChildItem.NowRequirementViewModel.ResultDefinitionSequence = nrItem.NowRequirementViewModel.ResultDefinitionSequence + 10;
                        }
                        else if (diff > 100)
                        {
                            newChildItem.NowRequirementViewModel.ResultDefinitionSequence = nrItem.NowRequirementViewModel.ResultDefinitionSequence + 10;
                        }
                        children.Insert(index+1, newChildItem);
                        childAdded = true;
                        break;
                    }

                    if (nrItem.NowRequirementViewModel.ResultDefinitionSequence > maxSeq)
                    {
                        maxSeq = nrItem.NowRequirementViewModel.ResultDefinitionSequence.Value;
                    }
                }
                index++;
            }
            if (!childAdded)
            {
                newChildItem.NowRequirementViewModel.ResultDefinitionSequence = maxSeq+100;
                children.Add(newChildItem);
            }
        }    
    }
}