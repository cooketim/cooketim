using Newtonsoft.Json;
using System;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class NowAddress
    {
        [JsonProperty(PropertyName = "line1")]
        public string Line1 { get; }

        [JsonProperty(PropertyName = "line2")]
        public string Line2 { get; }

        [JsonProperty(PropertyName = "line3")]
        public string Line3 { get; }

        [JsonProperty(PropertyName = "line4")]
        public string Line4 { get; }

        [JsonProperty(PropertyName = "line5")]
        public string Line5 { get; }

        [JsonProperty(PropertyName = "postCode")]
        public string PostCode { get; }

        [JsonProperty(PropertyName = "emailAddress1")]
        public string EmailAddress1 { get; }

        [JsonProperty(PropertyName = "emailAddress2")]
        public string EmailAddress2 { get; }

        public NowAddress(string line1, string line2, string line3, string line4, string line5, string postCode, string emailAddress1, string emailAddress2)
        {
            Line1 = line1;
            Line2 = line2;
            Line3 = line3;
            Line4 = line4;
            Line5 = line5;
            PostCode = postCode;
            EmailAddress1 = emailAddress1;
            EmailAddress2 = emailAddress2;
        }
    }
}
