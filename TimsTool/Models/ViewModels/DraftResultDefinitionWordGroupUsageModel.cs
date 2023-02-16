namespace Models.ViewModels
{
    public class DraftResultDefinitionWordGroupUsageModel
    {
        public DraftResultDefinitionWordGroupUsageModel(DraftResultModel draftResult, DraftResultDefinitionWordGroupModel draftRdwg, ResultWordGroupViewModel rwg)
        {
            ChangedResultDefinitionWordGroup = draftRdwg;
            ParentResultWordGroupViewModel = rwg;
            Usage = draftResult;
            //set the change report on the parent result
            Usage.ChangeReport = string.Format("Changes to child result definition word group '{0}'", ChangedResultDefinitionWordGroup.DraftResultDefinitionWordGroupViewModel.ResultDefinitionWord);
        }

        public DraftResultModel Usage { get; }

        public DraftResultDefinitionWordGroupModel ChangedResultDefinitionWordGroup { get; }

        public ResultWordGroupViewModel ParentResultWordGroupViewModel { get; }
    }
}
