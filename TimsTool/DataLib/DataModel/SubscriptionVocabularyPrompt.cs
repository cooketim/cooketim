using Newtonsoft.Json;
using System;

namespace DataLib
{
    [Serializable]
    public sealed class SubscriptionVocabularyPrompt
    {
        public SubscriptionVocabularyPrompt(ResultPromptRule rule)
        {
            if (rule == null || rule.ResultPrompt == null)
            {
                ResultPromptId = null;
                ResultPromptReference = null;
            }

            ResultPromptId = rule.ResultPrompt.MasterUUID ?? rule.ResultPrompt.UUID;
            ResultPromptReference = rule.ResultPrompt.PromptReference;
        }

        [JsonProperty(PropertyName = "resultPromptId")]
        public Guid? ResultPromptId { get; }

        [JsonProperty(PropertyName = "resultPromptReference")]
        public string ResultPromptReference { get; }
    }
}