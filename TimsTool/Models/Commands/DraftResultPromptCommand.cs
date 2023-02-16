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
    public interface IDraftResultPromptCommand : ICommand { }

    public class DraftResultPromptCommand : IDraftResultPromptCommand
    {
        private ITreeModel treeModel;
        public DraftResultPromptCommand(ITreeModel treeModel)
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
            var source = parameter == null ? null : parameter as ResultPromptTreeViewModel;

            //Base the source on the current selected item
            if (source == null)
            {
                source = treeModel.SelectedItem as ResultPromptTreeViewModel;
            }

            if (source == null)
            {
                Log.Error("Result Prompt not sent for drafting");
                return;
            }

            //Draft a new deep copy of the ResultPromptViewModel
            DraftResultPromptViewModel(source.ResultPromptRuleViewModel.ResultPromptViewModel);
        }

        public void DraftResultPromptViewModel(ResultPromptViewModel source)
        {
            //Draft a new deep copy of the ResultPromptViewModel 
            var newRPVM = source.Draft();

            UpdateAllDraftPromptRules(source, newRPVM);
        }

        private void UpdateAllDraftPromptRules(ResultPromptViewModel source, ResultPromptViewModel newRPVM)
        {
            //apply to all draft result prompt rules that do not have a draft prompt for this same prompt
            var rulesForUpdate = treeModel.AllResultPromptViewModel.Prompts.Where(x => x.PublishedStatus == PublishedStatus.Draft &&
                                                    x.ResultPromptViewModel.MasterUUID == source.MasterUUID &&
                                                 (  
                                                    x.ResultPromptViewModel.PublishedStatus == PublishedStatus.Published ||
                                                    x.ResultPromptViewModel.PublishedStatus == PublishedStatus.PublishedPending
                                                 ));
            foreach (var item in rulesForUpdate)
            {
                //apply the new draft result prompt
                item.SetDraftResultPrompt(newRPVM);
            }
        }
    }
}
