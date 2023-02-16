using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataLib
{
    [Serializable]
    public sealed class NowText : IEquatable<NowText>
    {
        //default constructor for serialisation
        public NowText() { }

        private Now now;
        public NowText(Now now)
        {
            this.now = now;
        }

        public NowText(Now now, string reference, string text, string welshText)
        {
            this.now = now;
            this.Reference = reference;
            this.Text = text;
            this.WelshText = welshText;
        }

        [JsonIgnore]
        public Now Now
        {
            get
            {
                return now;
            }
        }

        [JsonProperty(PropertyName = "nowReference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "welshText")]
        public string WelshText { get; set; }

        public NowText Copy()
        {
            NowText nowText = (NowText)this.MemberwiseClone();

            return nowText;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NowText);
        }

        public bool Equals(NowText other)
        {
            return other != null &&
                string.Equals(Reference, other.Reference, StringComparison.InvariantCulture) &&
                string.Equals(Text, other.Text, StringComparison.InvariantCulture) &&
                string.Equals(WelshText, other.WelshText, StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Reference);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WelshText);
            return hashCode;
        }

        public static bool operator ==(NowText text1, NowText text2)
        {
            return EqualityComparer<NowText>.Default.Equals(text1, text2);
        }

        public static bool operator !=(NowText text1, NowText text2)
        {
            return !(text1 == text2);
        }
    }
}