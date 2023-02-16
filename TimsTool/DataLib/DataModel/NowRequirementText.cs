using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataLib
{
    [Serializable]
    public sealed class NowRequirementText : IEquatable<NowRequirementText>
    {
        private NowRequirement nowRequirement;
        public NowRequirementText(NowRequirement nowRequirement)
        {
            this.nowRequirement = nowRequirement;
        }

        [JsonIgnore]
        public NowRequirement NowRequirement
        {
            get
            {
                return nowRequirement;
            }
        }

        [JsonProperty(PropertyName = "nowReference")]
        public string NowReference { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string NowText { get; set; }

        [JsonProperty(PropertyName = "welshText")]
        public string NowWelshText { get; set; }

        public NowRequirementText Copy()
        {
            NowRequirementText nowReqText = (NowRequirementText)this.MemberwiseClone();
            
            return nowReqText;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NowRequirementText);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NowReference);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NowText);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NowWelshText);
            return hashCode;
        }

        public bool Equals(NowRequirementText other)
        {
            return other != null &&
                string.Equals(NowReference, other.NowReference, StringComparison.InvariantCulture) &&
                string.Equals(NowText, other.NowText, StringComparison.InvariantCulture) &&
                string.Equals(NowWelshText, other.NowWelshText, StringComparison.InvariantCulture);
        }

        public static bool operator ==(NowRequirementText text1, NowRequirementText text2)
        {
            return EqualityComparer<NowRequirementText>.Default.Equals(text1, text2);
        }

        public static bool operator !=(NowRequirementText text1, NowRequirementText text2)
        {
            return !(text1 == text2);
        }
    }
}