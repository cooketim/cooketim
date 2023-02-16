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
    public interface IPasteResultPromptCommand : ICommand { }

    public class PasteResultPromptCommand : ResultPromptCommandBase, IPasteResultPromptCommand
    {
        public PasteResultPromptCommand(ITreeModel treeModel) : base (treeModel)
        {
            this.treeModel = treeModel;
        }
        protected override void ExecuteCommand()
        {
            if (SelectedNowSubscription == null)
            {
                PasteTarget();
            }
            else
            {
                PasteNowSubscription();
            }

            //reset the copied item
            PendingCopyModel().PendingCopyModel = null;
        }

        private void PasteNowSubscription()
        {
            var vm = new NowSubscriptionPromptViewModel(PendingCopyModel().ResultPromptRuleViewModel.ResultPromptRule, SelectedNowSubscription.NowSubscriptionViewModel);
            if (IsExcludedPrompts)
            {
                SelectedNowSubscription.NowSubscriptionViewModel.ExcludedPrompts.Add(vm);
            }
            else
            {
                SelectedNowSubscription.NowSubscriptionViewModel.IncludedPrompts.Add(vm);
            }
        }

        private ResultPromptTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as ResultPromptTreeViewModel;
        }

        private void PasteTarget()
        {
            //compare the target parent with the source parent to determine if they are siblings
            var sourceParent = PendingCopyModel().Parent as ResultDefinitionTreeViewModel;
            if (sourceParent == null) { return; }

            ResultPromptTreeViewModel newItem = null;

            if (Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID == sourceParent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID)
            {
                //target and source are siblings of the same parent, make a deep copy for the same parent
                newItem = CopySourceItem();
            }
            else
            {
                //make a shallow clone for a different parent
                newItem = CloneSourceItem();
            }

            //deal with any related NOWs
            SynchroniseNows(newItem.ResultPromptRuleViewModel);
        }

        private ResultPromptTreeViewModel CloneSourceItem()
        {
            //determine if the target parent already has the source item requested to be copied
            var child = Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts == null ? null : Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.FirstOrDefault(x => x.ResultPromptViewModel.UUID == PendingCopyModel().ResultPromptRuleViewModel.ResultPromptViewModel.UUID);
            if (child != null)
            {
                //deep copy is required for the target parent
                return CopySourceItem();
            }

            //Make a cloned copy ready to reset the parentage
            var newItem = PendingCopyModel().Clone();          

            PaseteNewItem(newItem);

            return newItem;
        }

        private ResultPromptTreeViewModel CopySourceItem()
        {
            //Make a deep copy for a new item 
            var newItem = PendingCopyModel().Copy();
            var label = newItem.ResultPromptRuleViewModel.ResultPromptViewModel.Label;
            var labelToMatch = label;

            //ensure that label is unique
            var duplicateLabel = false;
            int i = 1;
            do
            {
                var match = treeModel.AllResultPromptViewModel.Prompts.FirstOrDefault(x => x.ResultPromptViewModel.Label == labelToMatch);
                duplicateLabel = match != null;
                if (duplicateLabel)
                {
                    i++;
                    labelToMatch = string.Format("{0} ({1})",label, i);
                }

            } while (duplicateLabel);

            newItem.ResultPromptRuleViewModel.ResultPromptViewModel.Label = labelToMatch;

            PaseteNewItem(newItem);

            return newItem;
        }

        private void PaseteNewItem(ResultPromptTreeViewModel toPasteResultPrompt)
        {
            //when required set the rule as draft
            toPasteResultPrompt.ResultPromptRuleViewModel.PublishedStatus = PublishedStatus.Draft;

            //ensure correct parentage for the new item being pasted
            toPasteResultPrompt.ResetParentage(Parent);
            toPasteResultPrompt.ResultPromptRuleViewModel.ResultPromptRule.ResultDefinitionUUID = Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.UUID;
            toPasteResultPrompt.ResultPromptRuleViewModel.ResultDefinitionViewModel = Parent.ResultRuleViewModel.ChildResultDefinitionViewModel;

            //add the prompt and move it to the selected index
            if (Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts == null) { Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts = new SilentObservableCollection<ResultPromptRuleViewModel>(); }
            Parent.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.Add(toPasteResultPrompt.ResultPromptRuleViewModel);

            //determine the position of the selected prompt
            int newIndex = 0;
            toPasteResultPrompt.ResultPromptRuleViewModel.PromptSequence = GetNextPromptSequence(out newIndex);
            Parent.Children.Insert(newIndex, toPasteResultPrompt);

            //ensure that the pasted item is selected and the parent is expanded
            Parent.IsExpanded = true;
            toPasteResultPrompt.IsSelected = true;

            //store the new data objects that have been created as part of the copy
            treeModel.AllResultPromptViewModel.Prompts.Add(toPasteResultPrompt.ResultPromptRuleViewModel);
            treeModel.AllData.ResultPromptRules.Add(toPasteResultPrompt.ResultPromptRuleViewModel.ResultPromptRule);
        }
    }
}
