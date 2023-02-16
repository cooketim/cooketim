using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DataLib.DataModel
{
    [Serializable]
    public class AllData
    {
        public List<ResultDefinitionRule> ResultDefinitionRules;
        public List<ResultDefinition> ResultDefinitions;
        public List<ResultDefinitionWordGroup> ResultDefinitionWordGroups;
        public List<ResultPromptWordGroup> ResultPromptWordGroups;
        public List<Now> Nows;
        public List<NowRequirement> NowRequirements;
        public List<NowSubscription> NowSubscriptions;
        public List<ResultPromptRule> ResultPromptRules;
        public List<FixedList> FixedLists;
        public Guid? ResultDefinitionSynonymCollectionId;
        public Guid? ResultPromptSynonymCollectionId;
        public List<Comment> Comments;
    }
}
