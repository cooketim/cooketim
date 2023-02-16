using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class NowDistribution
    {
        [JsonProperty(PropertyName = "emailAddress")]
        public string EmailAddress { get; }

        [JsonProperty(PropertyName = "email")]
        public bool? Email { get; }

        [JsonProperty(PropertyName = "emailContent")]
        public List<EmailRenderingVocabulary> EmailContent { get; }

        [JsonProperty(PropertyName = "firstClassLetter")]
        public bool? FirstClassLetter { get; }

        [JsonProperty(PropertyName = "emailTemplateName")]
        public string EmailTemplateName { get; }

        [JsonProperty(PropertyName = "bilingualEmailTemplateName")]
        public string BilingualEmailTemplateName { get; }

        [JsonProperty(PropertyName = "secondClassLetter")]
        public bool? SecondClassLetter { get; }

        public NowDistribution(string emailAddress, bool? firstClassLetter, bool? secondClassLetter, List<EmailRenderingVocabulary> emailContent)
        {
            EmailAddress = emailAddress;
            FirstClassLetter = firstClassLetter;
            SecondClassLetter = secondClassLetter;
            EmailContent = emailContent;

            if (emailContent != null)
            {
                Email = true;
                EmailTemplateName = "EmailTemplateName";
                BilingualEmailTemplateName = "BilingualEmailTemplateName";
            }
        }
    }

    [Serializable]
    public class EmailRenderingVocabulary
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; }
    }
}
