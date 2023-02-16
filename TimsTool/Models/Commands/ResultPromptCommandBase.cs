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
    public abstract class ResultPromptCommandBase : ICommand
    {
        protected ITreeModel treeModel;
        public ResultPromptCommandBase(ITreeModel treeModel)
        {
            this.treeModel = treeModel;
        }

        protected ResultDefinitionTreeViewModel Parent;
        protected ResultPromptTreeViewModel SelectedResultPrompt;
        protected NowSubscriptionTreeViewModel SelectedNowSubscription;
        protected bool IsExcludedPrompts;
        protected bool IsIncludedPrompts;

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
            //pasting to a Now Subscription?
            SelectedNowSubscription = treeModel.SelectedItem as NowSubscriptionTreeViewModel;
            if (SelectedNowSubscription != null)
            {
                //set included/excluded based on command parameter
                if (parameter == null) { return; }
                var includedExcluded = ((string)parameter).ToLowerInvariant();
                if (includedExcluded == "excluded")
                {
                    IsExcludedPrompts = true;
                    IsIncludedPrompts = false;
                }
                else
                {
                    IsIncludedPrompts = true;
                    IsExcludedPrompts = false;
                }

                Parent = null;
                SelectedResultPrompt = null;
            }
            else
            {
                var selectedResultPrompt = treeModel.SelectedItem as ResultPromptTreeViewModel;
                if (selectedResultPrompt == null)
                {
                    //Parent result definition selected
                    Parent = treeModel.SelectedItem as ResultDefinitionTreeViewModel;
                }
                else
                {
                    //sibling selected
                    SelectedResultPrompt = selectedResultPrompt;
                    Parent = (ResultDefinitionTreeViewModel)selectedResultPrompt.Parent;
                }

                SelectedNowSubscription = null;
                IsExcludedPrompts = false;
                IsIncludedPrompts = false;
            }

            ExecuteCommand();
        }

        protected abstract void ExecuteCommand();

        protected void SynchroniseNows(ResultPromptRuleViewModel newPromptRule)
        {
            //Find any now Requirements for the parent
            var matchedNowRequirements = treeModel.AllNowRequirementsViewModel.NowRequirements.FindAll(x => x.ResultDefinition.UUID == Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID);

            //When drafting mode then only deal with those requirements that are status draft
            matchedNowRequirements.RemoveAll(x => x.PublishedStatus != PublishedStatus.Draft);

            foreach (var nr in matchedNowRequirements)
            {
                //get the now or edt tree item
                var matchedNow = MatchNOWEDT(nr);
                if (matchedNow == null) { continue; }

                foreach (var child in matchedNow.Children)
                {
                    if (child is NowRequirementTreeViewModel)
                    {
                        var nrItem = child as NowRequirementTreeViewModel;
                        if (nrItem.NowRequirementViewModel.ResultDefinition.UUID == Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
                        {
                            //make a new child  Now Requirement Prompt Rule and add it to the vm and the data
                            var newPromptItem = MakeNowRequirementPromptRuleModel(newPromptRule, nrItem);
                            if (nrItem.NowRequirementViewModel.NowRequirementPromptRules == null) 
                            { 
                                nrItem.NowRequirementViewModel.NowRequirementPromptRules = new List<NowRequirementPromptRuleViewModel>(); 
                            }
                            nrItem.NowRequirementViewModel.NowRequirementPromptRules.Add(newPromptItem.NowRequirementPromptRuleViewModel);
                            nrItem.NowRequirementViewModel.LastModifiedDate = DateTime.Now;

                            if (nrItem.NowRequirementViewModel.NowRequirement.NowRequirementPromptRules == null) 
                            { 
                                nrItem.NowRequirementViewModel.NowRequirement.NowRequirementPromptRules = new List<NowRequirementPromptRule>(); 
                            }
                            nrItem.NowRequirementViewModel.NowRequirement.NowRequirementPromptRules.Add(newPromptItem.NowRequirementPromptRuleViewModel.NowRequirementPromptRule);
                            AddItemToTreeInSequence(nrItem, newPromptItem);
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

        private void AddItemToTreeInSequence(NowRequirementTreeViewModel parentItem, NowRequirementPromptRuleTreeViewModel newItem)
        {
            var index = 0;
            var added = false;
            foreach (var item in parentItem.Children)
            {
                if (item is NowRequirementPromptRuleTreeViewModel)
                {
                    var nrprItem = item as NowRequirementPromptRuleTreeViewModel;

                    if (nrprItem.NowRequirementPromptRuleViewModel.NowRequirementPromptRule.ResultPromptRule.PromptSequence >= newItem.NowRequirementPromptRuleViewModel.ResultPromptRule.PromptSequence)
                    {
                        parentItem.Children.Insert(index, newItem);
                        added = true;
                        break;
                    }
                }

                index++;
            }
            if (!added)
            {
                parentItem.Children.Add(newItem);
            }
        }

        private NowRequirementPromptRuleTreeViewModel MakeNowRequirementPromptRuleModel(ResultPromptRuleViewModel promptRuleVM, NowRequirementTreeViewModel targetParent)
        {
            //make a new data item
            var data = new NowRequirementPromptRule(promptRuleVM.ResultPromptRule, targetParent.NowRequirementViewModel.NowRequirement);

            var nrprVM = new NowRequirementPromptRuleViewModel(data, promptRuleVM, targetParent.NowRequirementViewModel, treeModel);

            //make a new tree item for the new now requirement
            return new NowRequirementPromptRuleTreeViewModel(treeModel, nrprVM, targetParent);
        }

        protected int? GetNextPromptSequence(out int newIndex)
        {
            newIndex = 0;
            int index;
            //determine the position of the selected prompt
            if (SelectedResultPrompt == null || Parent.Children.Count == 0)
            {
                if (Parent.Children.Count == 0)
                {
                    return 10;
                }
                //find the first prompt child
                for (index = 0; index < Parent.Children.Count; index++)
                {
                    var child = Parent.Children[index];
                    if (child is ResultPromptTreeViewModel)
                    {
                        var tvm = child as ResultPromptTreeViewModel;
                        if (tvm.ResultPromptRuleViewModel.PromptSequence.HasValue && tvm.ResultPromptRuleViewModel.PromptSequence.Value > 0)
                        {
                            return (int)Math.Round((decimal)tvm.ResultPromptRuleViewModel.PromptSequence.Value / 2);
                        }

                        break;
                    }
                }

                return null;
            }

            index = Parent.Children.IndexOf(SelectedResultPrompt);

            newIndex = index + 1;

            //try to set the prompt sequence for the new item
            if (SelectedResultPrompt.ResultPromptRuleViewModel.PromptSequence == null) { return null; }
            if (index < Parent.Children.Count - 1)
            {
                var nextChild = Parent.Children[index + 1] as ResultPromptTreeViewModel;
                if (nextChild == null || nextChild.ResultPromptRuleViewModel.PromptSequence == null)
                {
                    return SelectedResultPrompt.ResultPromptRuleViewModel.PromptSequence.Value + 100;
                }

                return FindIntermediatePoint(nextChild.ResultPromptRuleViewModel, SelectedResultPrompt.ResultPromptRuleViewModel);
            }

            return SelectedResultPrompt.ResultPromptRuleViewModel.PromptSequence.Value + 100;
        }

        private int? FindIntermediatePoint(ResultPromptRuleViewModel nextItem, ResultPromptRuleViewModel selectedItem)
        {
            if (selectedItem.PromptSequence.Value + 20 < nextItem.PromptSequence.Value)
            {
                return selectedItem.PromptSequence.Value + 20;
            }

            if (selectedItem.PromptSequence.Value + 10 < nextItem.PromptSequence.Value)
            {
                return selectedItem.PromptSequence.Value + 10;
            }
            if (selectedItem.PromptSequence.Value + 5 < nextItem.PromptSequence.Value)
            {
                return selectedItem.PromptSequence.Value + 5;
            }

            int total = nextItem.PromptSequence.Value + selectedItem.PromptSequence.Value;
            decimal average = total > 0 ? total / 2 : 0;
            return average == 0 ? null : (int?)Math.Round(average);
        }
    }
}