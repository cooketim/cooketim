using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NowsRequestDataLib.Domain;
using System.Linq;

namespace NowsRequestDataLib.NowDocument
{
    [Serializable]
    public class ResultForDocumentRequest
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; }

        [JsonProperty(PropertyName = "welshLabel")]
        public string WelshLabel { get; }

        [JsonProperty(PropertyName = "resultWording")]
        public string ResultWording { get; }

        [JsonProperty(PropertyName = "welshResultWording")]
        public string WelshResultWording { get; }

        [JsonProperty(PropertyName = "resultIdentifier")]
        public Guid? ResultIdentifier { get; }

        [JsonIgnore]
        public bool PublishedForNOWs { get; }

        [JsonProperty(PropertyName = "resultDefinitionGroup")]
        public string ResultDefinitionGroup { get; }

        [JsonProperty(PropertyName = "prompts")]
        public List<PromptForDocumentRequest> Prompts { get; }

        [JsonProperty(PropertyName = "nowRequirementText")]
        public List<TextValue> TextValues { get; set; }

        [JsonProperty(PropertyName = "sequence")]
        public int? Sequence { get; set; }

        public ResultForDocumentRequest(Result result)
        {
            Label = result.ResultDefinitionLabel;
            WelshLabel = result.ResultDefinitionWelshLabel;
            ResultIdentifier = result.ResultDefinitionId;
            ResultDefinitionGroup = result.ResultDefinitionGroup;
            PublishedForNOWs = result.IsPublishedForNows;
            ResultWording = result.ResultDefinitionWording;
            WelshResultWording = result.ResultDefinitionWelshResultWording;

            //build the prompts

            if (result.ResultPrompts != null && result.ResultPrompts.Where(x=>!x.Hidden).Count() > 0)
            {
                var prompts = new List<PromptForDocumentRequest>();

                foreach(var prompt in result.ResultPrompts)
                {
                    if (prompt.Hidden) 
                    { 
                        continue; 
                    }
                    if (prompt.ResultAsAPrompt == null)
                    {
                        prompts.Add(new PromptForDocumentRequest(prompt));
                    }
                    else
                    {
                        if (prompt.ResultAsAPrompt.ResultPrompts != null && prompt.ResultAsAPrompt.ResultPrompts.Count > 0)
                        {
                            prompts.Add(new PromptForDocumentRequest(prompt));
                        }
                    }
                }
                Prompts = prompts;
            }

            //build the text values
            var textValues = new List<TextValue>();
            if (result.NowText != null && result.NowText.Count>0)
            {
                result.NowText.ForEach(x => textValues.Add(new TextValue(x.NowReference, x.NowText, x.NowWelshText)));
            }
            TextValues = textValues.Count == 0 ? null : textValues;
        }
    }
}
