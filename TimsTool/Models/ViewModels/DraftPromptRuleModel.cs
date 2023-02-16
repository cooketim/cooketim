using DataLib;
using System.Collections.Generic;
using System.Linq;

namespace Models.ViewModels
{
    public class DraftPromptRuleModel
    {
        public DraftPromptRuleModel(ITreeModel treeModel, ResultPromptRuleViewModel source, List<DraftPromptModel> allDraftPrompts)
        {
            Rule = source;

            //set a draft prompt when the model has a draft prompt
            if (source.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                //determine if there is a draft prompt for this draft rule
                var matchedPrompt = allDraftPrompts.FirstOrDefault(x => x.Prompt.MasterUUID == source.ResultPromptViewModel.MasterUUID);

                if (matchedPrompt == null)
                {
                    DraftPrompt = new DraftPromptModel(treeModel, source.ResultPromptViewModel, allDraftPrompts);
                    allDraftPrompts.Add(DraftPrompt);
                }
                else
                {
                    DraftPrompt = matchedPrompt;
                }
            }

            var rules = treeModel.AllResultPromptViewModel.Prompts.Where(
                x => x.MasterUUID == source.MasterUUID).ToList();

            ExistingPublishedResultPromptRule = treeModel.AllResultPromptViewModel.Prompts.FirstOrDefault(
                x => x.MasterUUID == source.MasterUUID &&
                    (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending));

            if (ExistingPublishedResultPromptRule == null)
            {
                //draft must be a new rule item
                ChangeReport = string.Format("New prompt rule '{0}' added to result", source.ResultPromptViewModel.Label);
                IsRuleAdded = true;
            }
            else
            {
                var publishedData = ExistingPublishedResultPromptRule.ResultPromptRule;
                var draftData = source.ResultPromptRule;
                if (publishedData != draftData)
                {
                    //changes made, make a change report, set the result as changed
                    ChangeReport = publishedData.GetChangedProperties(draftData);
                    IsRuleChanged = true;
                }
            }

            if (DraftPrompt != null)
            {
                DraftPrompt.SetPromptChanges(ExistingPublishedResultPromptRule);
            }
        }

        public ResultPromptRuleViewModel Rule { get; }

        public bool IsPromptChanged 
        { 
            get
            {
                if (DraftPrompt != null) { return DraftPrompt.IsChanged; }
                return false;
            }
        }

        public bool IsPromptAdded
        {
            get
            {
                if (DraftPrompt != null) { return DraftPrompt.IsAdded; }
                return false;
            }
        }

        public bool IsChanged
        {
            get
            {
                return IsPromptChanged || IsRuleChanged || IsRuleAdded;
            }
        }

        public bool IsRuleChanged { get; set; }

        public bool IsRuleAdded { get; set; }

        public string ChangeReport { get; set; }

        public DraftPromptModel DraftPrompt { get; set; }

        public ResultPromptRuleViewModel ExistingPublishedResultPromptRule { get; private set; }

        internal void PrepareForPublication()
        {
            Rule.PublishedStatus = PublishedStatus.PublishedPending;

            // if the prompt is not changed, reset the rule reference to the published version of the prompt
            if (ExistingPublishedResultPromptRule != null && !IsPromptChanged && Rule.ResultPromptViewModel.PublishedStatus == PublishedStatus.Draft)
            {
                Rule.ResultPromptViewModel = ExistingPublishedResultPromptRule.ResultPromptViewModel;
                Rule.ResultPromptRule.ResultPrompt = ExistingPublishedResultPromptRule.ResultPromptViewModel.ResultPrompt;
            }

            if (ExistingPublishedResultPromptRule != null)
            {
                ExistingPublishedResultPromptRule.PublishedStatus = PublishedStatus.RevisionPending;
            }
        }
    }
}
