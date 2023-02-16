using DataLib;
using DataLib.DataModel;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Models.Commands
{
    public interface IValidateCommand : ICommand 
    {
        string ErrorMessages { get; }
    }

    public class ValidateCommand : IValidateCommand
    {
        private ITreeModel treeModel;
        public string ErrorMessages { get; private set; }
        public ValidateCommand(ITreeModel treeModel)
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
            // this command never raise the event, and
            // not using the WeakEvent pattern here can
            // cause memory leaks.  WeakEvent pattern is
            // not simple to implement, so why bother.
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            //reset errors as the output state that is set by this command
            ErrorMessages = null;

            //Get results and prompts to be validated
            var resultsToValidate = GetResultsToBeValidated();
            var resultPromptsToValidate = GetResultPromptsToBeValidated();
            var nowsToValidate = GetNOWSToBeValidated();
            var edtsToValidate = GetEDTSToBeValidated();

            //First enforce that any result definitions with single word groups have a matching short code
            resultsToValidate.ForEach(x => x.HasSingleWordGroup());

            //Now validate the data
            var sb = new StringBuilder();

            CheckResultDefinitionsPublishedStatus(sb, resultsToValidate);
            CheckResultDefinitionsEmptyShortCode(sb, resultsToValidate.Where(x=>x.PublishedStatus == PublishedStatus.Draft && x.DeletedDate == null).ToList());
            CheckResultDefinitionsDuplicateShortCode(sb, resultsToValidate.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.DeletedDate == null).ToList());
            CheckResultPromptsPromptType(sb, resultPromptsToValidate.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.DeletedDate == null).ToList());
            CheckResultPromptsSequence(sb, resultPromptsToValidate.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.DeletedDate == null).ToList());
            CheckNowHasDefinitions(sb, nowsToValidate, edtsToValidate);
            CheckNowHasRank(sb, nowsToValidate, edtsToValidate);
            CheckNowHasPrimaryRequirement(sb, nowsToValidate, edtsToValidate);
            CheckNowWithNowRequirementTextHasAReference(sb, nowsToValidate, edtsToValidate);

            if (sb.Length > 0)
            {
                ErrorMessages = sb.ToString();
            }
        }

        private List<ResultDefinitionViewModel> GetResultsToBeValidated()
        {
            return treeModel.AllResultDefinitionsViewModel.Definitions.Where(x => x.PublishedStatus == PublishedStatus.Draft || x.PublishedStatus == PublishedStatus.PublishedPending || x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
        }

        private List<ResultPromptRuleViewModel> GetResultPromptsToBeValidated()
        {
            return treeModel.AllResultPromptViewModel.Prompts.Where(x => x.PublishedStatus == PublishedStatus.Draft || x.PublishedStatus == PublishedStatus.PublishedPending || x.PublishedStatus == PublishedStatus.RevisionPending).ToList();
        }

        private List<NowViewModel> GetNOWSToBeValidated()
        {
            return treeModel.AllNowsViewModel.Nows.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.DeletedDate == null).ToList();
        }

        private List<NowViewModel> GetEDTSToBeValidated()
        {
            return treeModel.AllEDTsViewModel.EDTs.Where(x => x.PublishedStatus == PublishedStatus.Draft && x.DeletedDate == null).ToList();
        }

        private void CheckResultDefinitionsPublishedStatus(StringBuilder sb, List<ResultDefinitionViewModel> resultsToValidate)
        {
            //fix result definition rules and result prompt rules
            foreach (var rdv in resultsToValidate)
            {
                if (rdv.ResultDefinition.ResultDefinitionRules != null)
                {
                    foreach (var rdr in rdv.ResultDefinition.ResultDefinitionRules)
                    {
                        if (rdr.CalculatedPublishedStatus != rdv.ResultDefinition.CalculatedPublishedStatus)
                        {
                            sb.AppendLine(string.Format("Result Rule '{0}' cannot be publication status '{1}' when parent result is '{2}'", rdr.ResultDefinition.Label, rdr.CalculatedPublishedStatus, rdv.ResultDefinition.PublishedStatus));
                        }
                    }
                }

                if (rdv.ResultDefinition.ResultPrompts != null)
                {
                    foreach (var rpr in rdv.ResultDefinition.ResultPrompts)
                    {
                        if (rpr.CalculatedPublishedStatus != rdv.ResultDefinition.CalculatedPublishedStatus)
                        {
                            sb.AppendLine(string.Format("Prompt Rule '{0}' cannot be publication status '{1}' when parent result is '{2}'", rpr.ResultPrompt.Label, rpr.CalculatedPublishedStatus, rdv.ResultDefinition.PublishedStatus));
                        }
                    }
                }
            }
        }

        private void CheckResultPromptsPromptType(StringBuilder sb, List<ResultPromptRuleViewModel> resultPromptsToValidate)
        {
            var missingPromptTypes = resultPromptsToValidate.Where(x => x.ResultPromptViewModel != null && x.ResultPromptViewModel.UUID != null && x.DeletedDate == null)
                                                            .GroupBy(x => x.ResultPromptViewModel.UUID)
                                                            .Select(group => group.First()).Where(x => string.IsNullOrEmpty(x.ResultPromptViewModel.PromptType)).ToList();

            if (missingPromptTypes != null && missingPromptTypes.Count > 0)
            {
                sb.AppendLine("The following Result Prompts have empty prompt types which is mandatory ...");
                foreach (var item in missingPromptTypes)
                {
                    sb.AppendLine(string.Format("Result: {0} - Prompt: {1}", item.ResultDefinitionViewModel.Label, item.ResultPromptViewModel.Label));
                }
                sb.AppendLine();
            }
        }

        private void CheckResultPromptsSequence(StringBuilder sb, List<ResultPromptRuleViewModel> resultPromptsToValidate)
        {
            var missingSequences = resultPromptsToValidate.Where(x => x.ResultPromptViewModel != null && x.ResultPromptViewModel.UUID != null && x.DeletedDate == null && !x.PromptSequence.HasValue)
                                                          .GroupBy(x => x.ResultDefinitionViewModel.UUID).ToList();

            if (missingSequences.Count > 0)
            {
                sb.AppendLine("The following Result Prompts have missing prompt sequences ...");
            }

            foreach (var rdGroup in missingSequences)
            {
                var prompts = rdGroup.ToList();
                var rd = prompts[0].ResultDefinitionViewModel;
                sb.AppendLine(string.Format("Prompts for Result '{0}'...", rd.Label));
                
                foreach (var prompt in prompts)
                {
                    sb.AppendLine(prompt.ResultPromptViewModel.Label);
                }
                sb.AppendLine();
            }
        }

        private void CheckResultDefinitionsEmptyShortCode(StringBuilder sb, List<ResultDefinitionViewModel> resultsToValidate)
        {
            var emptyShortCodes = resultsToValidate.FindAll(x => string.IsNullOrEmpty(x.ShortCode) && x.DeletedDate == null);

            if (emptyShortCodes != null && emptyShortCodes.Count > 0)
            {
                sb.AppendLine("The following Result Definitions have empty short codes when short code is mandatory ...");
                emptyShortCodes.ForEach(x => sb.AppendLine(string.Format("{0}", x.Label)));
                sb.AppendLine();
            }
        }

        private void CheckNowHasDefinitions(StringBuilder sb, List<NowViewModel> nowsToValidate, List<NowViewModel> edtsToValidate)
        {
            var nowsWithoutRequirements = nowsToValidate.FindAll(x => x.NowRequirements == null || x.NowRequirements.Count == 0 && (x.DeletedDate == null));
            if (nowsWithoutRequirements != null && nowsWithoutRequirements.Count > 0)
            {
                sb.AppendLine("The following Nows require at least one now requirement");
                nowsWithoutRequirements.ForEach(x => sb.AppendLine(string.Format("{0}", x.Name)));
                sb.AppendLine();
            }

            var edtsWithoutRequirements = edtsToValidate.FindAll(x => x.NowRequirements == null || x.NowRequirements.Count == 0 && (x.DeletedDate == null));
            if (edtsWithoutRequirements != null && edtsWithoutRequirements.Count > 0)
            {
                sb.AppendLine("The following EDTs require at least one now requirement");
                edtsWithoutRequirements.ForEach(x => sb.AppendLine(string.Format("{0}", x.Name)));
                sb.AppendLine();
            }
        }

        private void CheckNowHasRank(StringBuilder sb, List<NowViewModel> nowsToValidate, List<NowViewModel> edtsToValidate)
        {
            var nowsWithoutRank = nowsToValidate.FindAll(x => x.Rank == null && x.DeletedDate == null);
            if (nowsWithoutRank != null && nowsWithoutRank.Count > 0)
            {
                sb.AppendLine("The following Nows require a Rank setting");
                nowsWithoutRank.ForEach(x => sb.AppendLine(string.Format("{0}", x.Name)));
                sb.AppendLine();
            }

            var edtsWithoutRank = edtsToValidate.FindAll(x => x.Rank == null && x.DeletedDate == null);
            if (edtsWithoutRank != null && edtsWithoutRank.Count > 0)
            {
                sb.AppendLine("The following EDTs require a Rank setting");
                edtsWithoutRank.ForEach(x => sb.AppendLine(string.Format("{0}", x.Name)));
                sb.AppendLine();
            }
        }

        private void CheckNowHasPrimaryRequirement(StringBuilder sb, List<NowViewModel> nowsToValidate, List<NowViewModel> edtsToValidate)
        {
            var nowsWithoutPrimaryReqs = new List<NowViewModel>();
            var edtsWithoutPrimaryReqs = new List<NowViewModel>();

            foreach (var item in nowsToValidate)
            {
                var nrs = item.AllNowRequirements;
                if (nrs.Count > 0)
                {
                    var primaryNR = nrs.FirstOrDefault(x => x.PrimaryResult && x.DeletedDate == null && x.NOWUUID == item.UUID);
                    if (primaryNR == null)
                    {
                        nowsWithoutPrimaryReqs.Add(item);
                    }
                }
            }

            foreach (var item in edtsToValidate)
            {
                var nrs = item.AllNowRequirements;
                if (nrs.Count > 0)
                {
                    var primaryNR = nrs.FirstOrDefault(x => x.PrimaryResult && x.DeletedDate == null && x.NOWUUID == item.UUID);
                    if (primaryNR == null)
                    {
                        edtsWithoutPrimaryReqs.Add(item);
                    }
                }
            }


            if (nowsWithoutPrimaryReqs.Count > 0)
            {
                sb.AppendLine("The following Nows require at least one 'Primary' now requirement");
                nowsWithoutPrimaryReqs.ForEach(x => sb.AppendLine(string.Format("{0}", x.Name)));
                sb.AppendLine();
            }

            if (edtsWithoutPrimaryReqs.Count > 0)
            {
                sb.AppendLine("The following EDTs require at least one 'Primary' now requirement");
                edtsWithoutPrimaryReqs.ForEach(x => sb.AppendLine(string.Format("{0}", x.Name)));
                sb.AppendLine();
            }
        }

        private void CheckNowWithNowRequirementTextHasAReference(StringBuilder sb, List<NowViewModel> nowsToValidate, List<NowViewModel> edtsToValidate)
        {
            var nowsWithoutReference = new List<NowViewModel>();
            var edtsWithoutReference = new List<NowViewModel>();

            foreach (var item in nowsToValidate)
            {
                var nrs = item.AllNowRequirements;
                if (nrs.Count > 0)
                {
                    var nrsWithNowText = nrs.Where(x => x.TextValues != null && x.TextValues.Count() > 0 && x.DeletedDate == null && x.NOWUUID == item.UUID);
                    if (nrsWithNowText.Any())
                    {
                        var textWithoutReference = nrsWithNowText.SelectMany(x => x.TextValues).FirstOrDefault(x => x.NowReference == null);
                        if (textWithoutReference != null)
                        {
                            nowsWithoutReference.Add(item);
                        }
                    }
                }
            }

            foreach (var item in edtsToValidate)
            {
                var nrs = item.AllNowRequirements;
                if (nrs.Count > 0)
                {
                    var nrsWithNowText = nrs.Where(x => x.TextValues != null && x.TextValues.Count() > 0 && x.DeletedDate == null && x.NOWUUID == item.UUID);
                    var textWithoutReference = nrsWithNowText.SelectMany(x => x.TextValues).FirstOrDefault(x => x.NowReference == null);
                    if (nrsWithNowText.Any())
                    {
                        if (textWithoutReference != null)
                        {
                            edtsWithoutReference.Add(item);
                        }
                    }
                }
            }

            if (nowsWithoutReference.Count > 0)
            {
                sb.AppendLine("The following Nows have Now Requirements that have text but without a corresponding 'text label'");
                nowsWithoutReference.ForEach(x => sb.AppendLine(string.Format("{0}", x.Name)));
                sb.AppendLine();
            }

            if (edtsWithoutReference.Count > 0)
            {
                sb.AppendLine("The following EDTs have Now Requirements that have text but without a corresponding 'text label'");
                edtsWithoutReference.ForEach(x => sb.AppendLine(string.Format("{0}", x.Name)));
                sb.AppendLine();
            }
        }

        private void CheckResultDefinitionsDuplicateShortCode(StringBuilder sb, List<ResultDefinitionViewModel> resultsToValidate)
        {
            var groupedDuplicates = resultsToValidate.Where(x => x.DeletedDate == null && !string.IsNullOrEmpty(x.ShortCode)).GroupBy(x => x.ShortCode).Where(x => x.ToList().Count > 1).ToList();
            foreach (var group in groupedDuplicates)
            {
                sb.AppendLine(Environment.NewLine);
                sb.AppendLine(string.Format("The following Result Definitions have duplicate short code:  '{0}'", group.Key));
                group.ToList().ForEach(x => sb.AppendLine(string.Format("{0}", x.Label)));
                sb.AppendLine();
            }
        }
    }
}
