using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class ResultPromptValue
    {
        [JsonIgnore]
        public ResultPrompt ResultPrompt { get; }

        [JsonIgnore]
        public Defendant Defendant { get; }

        [JsonProperty(PropertyName = "masterDefendantId")]
        public Guid? DefendantId { get => Defendant.DefendantId; }

        [JsonIgnore]
        public ProsecutionCase ProsecutionCase { get; }

        [JsonProperty(PropertyName = "caseId")]
        public Guid? CaseId { get => ProsecutionCase.CaseId; }

        [JsonIgnore]
        public Offence Offence { get; }

        [JsonProperty(PropertyName = "offenceId")]
        public Guid? OffenceId { get => Offence?.OffenceId; }

        private string promptValue;

        [JsonProperty(PropertyName = "value")]
        public string Value
        {
            get
            {
                if (ResultPrompt.ResultPromptRule != null) 
                {
                    if (ResultPrompt.ResultPromptRule.ResultPrompt.Hidden)
                    {
                        return null;
                    }

                    return promptValue;
                }

                //derive a value from each of the result prompts in the result
                var builder = new StringBuilder();
                foreach (var prompt in ResultPrompt.ResultAsAPrompt.ResultPrompts.Where(x => x.IsSelected).OrderBy(x => x.Sequence))
                {
                    foreach(var promptValue in prompt.Values.Where(x=>!x.Hidden))
                    {
                        if (builder.Length > 0) { builder.AppendLine(); }
                        builder.AppendFormat("{0}: {1}", promptValue.Label, promptValue.Value);
                    }
                }
                return builder.ToString();
            }
        }

        [JsonProperty(PropertyName = "welshValue")]
        public string WelshValue
        {
            get
            {
                if (Value == null) { return null; }

                //derive a value from each of the result prompts in the result
                return "welsh-" + Value;
            }
        }

        [JsonProperty(PropertyName = "key")]
        public string Key { get; }

        [JsonProperty(PropertyName = "promptReference")]
        public string PromptReference
        {
            get
            {
                if (ResultPrompt.ResultPromptRule != null) { return ResultPrompt.ResultPromptRule.ResultPrompt.PromptReference; }

                return null;
            }
        }

        [JsonProperty(PropertyName = "label")]
        public string Label
        {
            get
            {
                if (ResultPrompt.ResultPromptRule != null) { return ResultPrompt.ResultPromptRule.ResultPrompt.Label; }

                return ResultPrompt.ResultAsAPrompt.ResultDefinitionRule.ResultDefinition.Label;
            }
        }

        [JsonProperty(PropertyName = "hidden")]
        public bool Hidden
        {
            get
            {
                if (ResultPrompt.ResultPromptRule != null) { return ResultPrompt.ResultPromptRule.ResultPrompt.Hidden; }

                return false;
            }
        }

        [JsonProperty(PropertyName = "associateToReferenceData")]
        public bool AssociateToReferenceData
        {
            get
            {
                if (ResultPrompt.ResultPromptRule != null) { return ResultPrompt.ResultPromptRule.ResultPrompt.AssociateToReferenceData; }

                return false;
            }
        }

        [JsonProperty(PropertyName = "promptIdentifier")]
        public Guid? PromptIdentifier
        {
            get
            {
                if (ResultPrompt.ResultPromptRule != null) { return ResultPrompt.ResultPromptRule.ResultPrompt.UUID; }

                return ResultPrompt.ResultAsAPrompt.ResultDefinitionRule.ResultDefinition.UUID;
            }
        }

        [JsonProperty(PropertyName = "promptType")]
        public string PromptType
        {
            get
            {
                if (ResultPrompt.ResultPromptRule != null) { return ResultPrompt.ResultPromptRule.ResultPrompt.PromptType; }

                return null;
            }
        }

        [JsonProperty(PropertyName = "welshLabel")]
        public string WelshLabel
        {
            get
            {
                if (ResultPrompt.ResultPromptRule != null) { return ResultPrompt.ResultPromptRule.ResultPrompt.WelshLabel; }

                return ResultPrompt.ResultAsAPrompt.ResultDefinitionRule.ResultDefinition.WelshLabel;
            }
        }

        public ResultPromptValue(ResultPrompt resultPrompt, Defendant defendant, ProsecutionCase prosecutionCase, Offence offence, string key, string val)
        {
            ResultPrompt = resultPrompt;
            Defendant = defendant;
            ProsecutionCase = prosecutionCase;
            Offence = offence;
            Key = key == null ? string.Empty : key.Trim().ToLowerInvariant();
            promptValue = val;
        }
    }
}
