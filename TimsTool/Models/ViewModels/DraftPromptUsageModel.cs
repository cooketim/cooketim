namespace Models.ViewModels
{
    public class DraftPromptUsageModel
    {
        public DraftPromptUsageModel(DraftPromptModel draftPrompt, ResultDefinitionTreeViewModel existingPublishedResult)
        {
            DraftPrompt = draftPrompt;
            ExistingPublishedResult = existingPublishedResult;
        }

        public DraftPromptModel DraftPrompt { get; }

        public DraftResultModel Usage { get; set; }

        public ResultDefinitionTreeViewModel ExistingPublishedResult { get; }
    }
}
