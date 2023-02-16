using Newtonsoft.Json;
using System;
using NowsRequestDataLib.Domain;
using System.Text;
using System.Linq;

namespace NowsRequestDataLib.NowDocument
{
    [Serializable]
    public class PromptForDocumentRequest
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; }

        [JsonProperty(PropertyName = "welshLabel")]
        public string WelshLabel { get; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; }

        [JsonProperty(PropertyName = "welshValue")]
        public string WelshValue { get; }

        [JsonProperty(PropertyName = "promptReference")]
        public string PromptReference { get; }

        [JsonProperty(PropertyName = "promptIdentifier")]
        public Guid? PromptIdentifier { get; }

        [JsonIgnore]
        public string PromptType { get; }

        public PromptForDocumentRequest(ResultPrompt prompt)
        {
            Label = prompt.Label;
            WelshLabel = prompt.WelshLabel;
            PromptIdentifier = prompt.PromptIdentifier;
            PromptType = prompt.PromptType;

            if (prompt.ResultAsAPrompt == null)
            {
                Value = prompt.Values[0].Value;
                WelshValue = prompt.Values[0].WelshValue;
            }
            else
            {
                var sb = new StringBuilder();

                foreach (var item in prompt.ResultAsAPrompt.SelectedResultPrompts.Where(x => !x.Hidden).OrderBy(x=>x.Sequence))
                {
                    if (sb.Length > 0) { sb.AppendLine(); }
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        sb.AppendFormat("{0}: {1}", item.Label, item.Value);
                    }
                    else
                    {
                        sb.Append(item.Label);
                    }                    
                }
                Value = sb.ToString();

                sb.Clear();
                foreach (var item in prompt.ResultAsAPrompt.SelectedResultPrompts.Where(x => !x.Hidden).OrderBy(x => x.Sequence))
                {
                    if (!string.IsNullOrEmpty(item.WelshValue))
                    {
                        if (sb.Length > 0) { sb.Append(", "); }
                        sb.AppendFormat("{0}: {1}", item.WelshLabel, item.WelshValue);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.WelshLabel))
                        {
                            if (sb.Length > 0) { sb.Append(", "); }
                            sb.Append(item.WelshLabel);
                        }
                    }
                }
                WelshValue = sb.ToString();
            }

            PromptReference = prompt.PromptReference;
        }
    }
}
