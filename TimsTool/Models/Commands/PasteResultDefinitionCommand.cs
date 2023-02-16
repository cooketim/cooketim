using DataLib;
using Models.ViewModels;
using Serilog;
using System.Linq;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IPasteResultDefinitionCommand : ICommand { }

    public class PasteResultDefinitionCommand : PasteResultDefinitionCommandBase, IPasteResultDefinitionCommand
    {
        private ITreeModel treeModel;
        public PasteResultDefinitionCommand(ITreeModel treeModel) : base(treeModel)
        {
            this.treeModel = treeModel;
        }
        protected override void Execute()
        {
            Log.Information("Start execution of paste of result definition");
            if (SelectedNowSubscription == null)
            {
                Log.Information("SelectedNowSubscription is null");
                if (SelectedResultDefinition == null)
                {
                    Log.Information("SelectedResultDefinition is null");
                    //now selected as target of result definition
                    PasteTarget(treeModel.SelectedItem as NowTreeViewModel);
                }
                else
                {
                    Log.Information("SelectedResultDefinition is NOT NULL");
                    //sibling result definition selected as target of paste
                    PasteTarget(SelectedResultDefinition);
                }
            }
            else
            {
                Log.Information("SelectedNowSubscription is NOT NULL");
                PasteNowSubscription();
            }

            //reset the copied item
            PendingCopyModel().PendingCopyModel = null;
        }

        private void PasteNowSubscription()
        {
            Log.Information("Start PasteNowSubscription()");
            var vm = new NowSubscriptionResultViewModel(PendingCopyModel().ResultRuleViewModel.ResultDefinitionRule.ResultDefinition, SelectedNowSubscription.NowSubscriptionViewModel);
            if (IsExcludedResults)
            {
                Log.Information(string.Format("PasteNowSubscription - Excluded Results. VM is {0}",vm == null ? "NULL" : "NOT NULL") );
                SelectedNowSubscription.NowSubscriptionViewModel.ExcludedResults.Add(vm);
            }
            else
            {
                Log.Information(string.Format("PasteNowSubscription - Included Results. VM is {0}", vm == null ? "NULL" : "NOT NULL"));
                SelectedNowSubscription.NowSubscriptionViewModel.IncludedResults.Add(vm);
            }
        }

        private ResultDefinitionTreeViewModel PendingCopyModel()
        {
            return treeModel.CopiedTreeViewModel as ResultDefinitionTreeViewModel;
        }

        private void PasteTarget(NowTreeViewModel targetParent)
        {
            // Make a new now requirement tree view item
            var newItem = MakeNowRequirementModel(PendingCopyModel().ResultRuleViewModel.ChildResultDefinitionViewModel, targetParent.NowViewModel, targetParent, null);

            AddNewItemToTree(targetParent, newItem);
        }

        private void PasteTarget(ResultDefinitionTreeViewModel target)
        {
            //compare the target parent with the source parent to determine if we are pasting a child to a root item or if we are pasting a sibling to root item
            if (target.Parent != null)
            {
                return;
            }

            //target and source are siblings, make a deep copy without parentage
            CopyResultDefinitionTreeViewModel(target, PendingCopyModel());
        }

        protected ResultDefinitionTreeViewModel CopyResultDefinitionTreeViewModel(ResultDefinitionTreeViewModel target, ResultDefinitionTreeViewModel source)
        {
            ResultDefinitionTreeViewModel newItem = null;
            //Copy and make a deep copy for a new item
            if (target.ResultRuleViewModel.ChildResultDefinitionViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                newItem = source.CopyAsDraft();
            }
            else
            {
                newItem = source.Copy();
            }

            SetData(newItem);

            //ensure parentage is null
            newItem.ResetParentage(null);            

            return newItem;
        }

        protected void SetData(ResultDefinitionTreeViewModel newItem)
        {
            //ensure that label is unique
            var duplicateLabel = false;
            int i = 2;
            {
                duplicateLabel = treeModel.AllResultDefinitionsViewModel.Definitions.FirstOrDefault(x => x.Label == newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Label) != null;
                if (duplicateLabel)
                {
                    i++;
                    newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Label = string.Format("{0} ({1})", newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Label, i);
                }

            } while (duplicateLabel) ;

            //store the view models that have been created as part of the copy
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitions.AddSorted(newItem);
            treeModel.AllResultDefinitionsTreeViewModel.ResultDefinitionRootsDraft.AddSorted(newItem);

            treeModel.AllResultRuleViewModel.Rules.Add(newItem.ResultRuleViewModel);

            if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null && newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules.Count > 0)
            {
                treeModel.AllResultRuleViewModel.Rules.AddRange(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules);
            }
            if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null && newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts.Count > 0)
            {
                treeModel.AllResultPromptViewModel.Prompts.AddRange(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts);
            }

            //store the new data objects that have been created as part of the copy
            treeModel.AllResultDefinitionsViewModel.Definitions.Add(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel);
            treeModel.AllData.ResultDefinitions.Add(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition);
            if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules != null &&
                newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules.Count > 0)
            {
                treeModel.AllData.ResultDefinitionRules.AddRange(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultDefinitionRules);
            }
            if (newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultPrompts != null &&
                newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultPrompts.Count > 0)
            {
                treeModel.AllData.ResultPromptRules.AddRange(newItem.ResultRuleViewModel.ChildResultDefinitionViewModel.ResultDefinition.ResultPrompts);
            }

            //Select the new item
            newItem.IsSelected = true;
        }
    }
}
