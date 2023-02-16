using Models.ViewModels;
using System;
using System.Windows.Input;

namespace Models.Commands
{
    public interface ISetDeleteResultDefinitionCommand : ICommand { }

    public class SetDeleteResultDefinitionCommand : DeleteResultDefinitionCommand, ISetDeleteResultDefinitionCommand
    {
        public SetDeleteResultDefinitionCommand(ITreeModel treeModel) : base(treeModel) { }

        protected override void ExecuteCommand(object parameter)
        {
            //Determine the selected item
            var selectedResultDefinition = treeModel.SelectedItem as ResultDefinitionTreeViewModel;

            var deletedDate = DateTime.Now;

            //set the deleted date on the result            
            selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.DeletedDate = deletedDate;
            selectedResultDefinition.ResultRuleViewModel.ResetDeleted();

            //set the deleted date on the child result rules
            if (selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules != null)
            {
                foreach (var rdr in selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Rules)
                {
                    rdr.DeletedDate = deletedDate;
                }
            }

            //set the deleted date on the child result prompt rules
            if (selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts != null)
            {
                foreach (var rpr in selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel.Prompts)
                {
                    rpr.DeletedDate = deletedDate;
                }
            }

            ResetNowsAndSubscriptionsRootItem(selectedResultDefinition.ResultRuleViewModel.ChildResultDefinitionViewModel);
        }
    }
}
