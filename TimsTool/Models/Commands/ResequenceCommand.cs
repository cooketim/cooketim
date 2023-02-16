using System;
using Models.ViewModels;
using System.Windows.Input;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Linq;
using Serilog;

namespace Models.Commands
{
    public interface IResequenceCommand : ICommand { }

    public class ResequenceCommand : IResequenceCommand
    {
        private ITreeModel treeModel;
        public ResequenceCommand(ITreeModel treeModel)
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
            //resequence resultDefinitions
            var rank = 100;
            foreach (var rd in treeModel.AllResultDefinitionsViewModel.Definitions.Where(x=>x.ResultDefinition.DeletedDate == null).OrderBy(x => x.Rank).ThenBy(x => x.Label))
            {
                rd.ResultDefinition.Rank = rank;
                rank = rank + 100;
                
                //prompts
                //only set the prompts sequencing when there are not any child result definitions. as the user may wish to statically set the prompt sequencing
                if (rd.ResultDefinition.ResultDefinitionRules == null || rd.ResultDefinition.ResultDefinitionRules.Where(x=>x.DeletedDate == null).Count()==0)
                {
                    var promptSequence = 100;
                    if (rd.Prompts != null && rd.Prompts.Count > 0)
                    {
                        foreach (var rp in rd.Prompts.Where(x => x.ResultPromptRule.ResultPrompt.DeletedDate == null).OrderBy(x => x.PromptSequence).ThenBy(x => x.ResultPromptViewModel.Label))
                        {
                            rp.ResultPromptRule.PromptSequence = promptSequence;

                            promptSequence = promptSequence + 100;
                        }
                    }
                }                
            }

            //resequence nows
            var notices = treeModel.AllNowsViewModel.Nows.Where(x => !x.Now.IsEDT && x.Now.DeletedDate == null && x.Name.ToLowerInvariant().Contains("notice"));
            var orders = treeModel.AllNowsViewModel.Nows.Where(x => !x.Now.IsEDT && x.Now.DeletedDate == null && !x.Name.ToLowerInvariant().Contains("warrant") && !x.Name.ToLowerInvariant().Contains("notice"));
            var warrants = treeModel.AllNowsViewModel.Nows.Where(x => !x.Now.IsEDT && x.Now.DeletedDate == null && x.Name.ToLowerInvariant().Contains("warrant"));
            var edts = treeModel.AllEDTsViewModel.EDTs.Where(x => x.Now.IsEDT && x.Now.DeletedDate == null);
            var nowRank = 100;

            foreach(var now in notices.OrderBy(x=>x.Name))
            {
                now.Now.Rank = nowRank++;
            }

            nowRank = nowRank + 500;

            foreach (var now in orders.OrderBy(x => x.Name))
            {
                now.Now.Rank = nowRank++;
            }

            nowRank = nowRank + 500;

            foreach (var now in warrants.OrderBy(x => x.Name))
            {
                now.Now.Rank = nowRank++;
            }

            nowRank = nowRank + 500;

            foreach (var edt in edts.OrderBy(x => x.Name))
            {
                edt.Now.Rank = nowRank++;
            }

            foreach (var now in treeModel.AllNowsViewModel.Nows.Where(x=>x.Now.DeletedDate == null).OrderBy(x => x.Rank).ThenBy(x => x.Now.Name))
            {
                //resequence now requirements
                if (now.NowRequirements != null && now.NowRequirements.Count>0)
                {
                    var resultDefinitionSequence = 100;
                    foreach (var nr in now.NowRequirements.Where(x => x.NowRequirement.DeletedDate == null).OrderBy(x => x.NowRequirement.ResultDefinitionSequence).ThenBy(x => x.NowRequirement.ResultDefinition.Label))
                    {
                        nr.NowRequirement.ResultDefinitionSequence = resultDefinitionSequence;

                        resultDefinitionSequence = resultDefinitionSequence + 100;

                        //resequence the child now requirements
                        ResequenceNowRequirements(nr, ref resultDefinitionSequence);
                    }
                }
            }
        }

        private void ResequenceNowRequirements(NowRequirementViewModel nr, ref int resultDefinitionSequence)
        {
            if (nr.NowRequirements != null && nr.NowRequirements.Count > 0)
            {
                foreach (var childNR in nr.NowRequirements.Where(x => x.NowRequirement.DeletedDate == null)
                    .OrderBy(x => x.NowRequirement.ResultDefinitionSequence)
                    .ThenBy(x => x.NowRequirement.ResultDefinition.Label))
                {
                    childNR.NowRequirement.ResultDefinitionSequence = resultDefinitionSequence;

                    resultDefinitionSequence = resultDefinitionSequence + 100;

                    ResequenceNowRequirements(childNR, ref resultDefinitionSequence);
                }
            }
        }
    }
}
