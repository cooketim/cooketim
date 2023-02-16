using Newtonsoft.Json;
using System;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class TextValue
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; }

        [JsonProperty(PropertyName = "welshValue")]
        public string WelshValue { get; }

        public TextValue(string label, string value, string welshValue)
        {
            Label = label;
            Value = value;
            WelshValue = welshValue;
        }
    }
}
