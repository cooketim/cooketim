namespace Models.ViewModels
{
    public class DraftParentResultUsageModel
    {
        private DraftResultModel parentUsage;

        public DraftParentResultUsageModel(DraftResultModel childDraftResult, ResultDefinitionTreeViewModel existingParentPublishedResult)
        {
            ChildDraftResult = childDraftResult;
            ExistingParentPublishedResult = existingParentPublishedResult;
        }

        public bool IsChildResultChanged 
        {
            get => ChildDraftResult.IsChanged;
        }

        public DraftResultModel ChildDraftResult { get; }

        public DraftResultModel ParentUsage 
        {
            get => parentUsage;
            set
            {
                parentUsage = value;

                //set the change report on the parent
                parentUsage.ChangeReport = string.Format("Changes to child result '{0}'", ChildDraftResult.Result.ResultRuleViewModel.ChildResultDefinitionViewModel.Label);
            }
        }

        public ResultDefinitionTreeViewModel ExistingParentPublishedResult { get; }
    }
}
