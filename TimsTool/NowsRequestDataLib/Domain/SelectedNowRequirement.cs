using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public sealed class SelectedNowRequirement
    {
        private NowRequirement nr;

        public SelectedNowRequirement(NowRequirement nr) { this.nr = nr; }

        public Guid? id { get => nr.UUID; }

        public Guid? resultDefinitionId { get => nr.ResultDefinition.UUID; }
        public string resultDefinition { get => nr.ResultDefinition.Label; }

        public Guid? parentNowRequirementId { get => nr.ParentNowRequirementUUID; }
        public string parentResultDefinition { get => nr.ParentNowRequirement == null ? null : nr.ParentNowRequirement.ResultDefinition.Label; }

        public Guid? nowId { get => nr.NOWUUID; }

        public bool mandatory { get => nr.Mandatory; }

        public bool primary { get => nr.PrimaryResult; }

        public int? sequence { get => nr.ResultDefinitionSequence; }

        public List<NowRequirementPromptRule> prompts { get => nr.NowRequirementPromptRules; }

        public List<NowRequirementText> nowRequirementText { get => nr.NowRequirementTextList; }

        public List<SelectedNowRequirement> nowRequirements
        {
            get
            {
                if (nr.NowRequirements == null) { return null; }
                if (nr.NowRequirements.Where(x => x.DeletedDate == null).Count() == 0) { return null; }

                return nr.NowRequirements.Where(x => x.DeletedDate == null).Select(x => new SelectedNowRequirement(x)).ToList();
            }
        }
    }
}
