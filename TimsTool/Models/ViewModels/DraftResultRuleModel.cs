using DataLib;
using System.Linq;

namespace Models.ViewModels
{
    public class DraftResultRuleModel
    {
        public DraftResultRuleModel(ITreeModel treeModel, ResultRuleViewModel source)
        {
            Rule = source;

            ExistingPublishedResultRule = treeModel.AllResultRuleViewModel.Rules.FirstOrDefault(
                x => x.MasterUUID == source.MasterUUID &&
                    (x.PublishedStatus == PublishedStatus.Published || x.PublishedStatus == PublishedStatus.PublishedPending));

            if (ExistingPublishedResultRule == null)
            {
                //draft must be a new item
                ChangeReport = string.Format("New Child Result '{0}'", source.ChildResultDefinitionViewModel.Label);
                IsRuleAdded = true;
            }
            else
            {
                var publishedData = ExistingPublishedResultRule.ResultDefinitionRule;
                var draftData = source.ResultDefinitionRule;

                if (publishedData != draftData)
                {
                    //changes made, make a change report, set the result as changed
                    ChangeReport = publishedData.GetChangedProperties(draftData);
                    IsRuleChanged = true;
                }
            }            
        }

        public ResultRuleViewModel Rule { get; }

        public bool IsChanged { get => IsRuleChanged || IsRuleAdded; }

        public bool IsRuleChanged { get; set; }

        public bool IsRuleAdded { get; set; }

        public string ChangeReport { get; set; }

        public ResultRuleViewModel ExistingPublishedResultRule { get; private set; }

        internal void PrepareForPublication()
        {
            Rule.PublishedStatus = PublishedStatus.PublishedPending;
            if (ExistingPublishedResultRule != null)
            {
                ExistingPublishedResultRule.PublishedStatus = PublishedStatus.RevisionPending;
            }
        }
    }
}
