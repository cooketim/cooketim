using System;
using System.Collections.Generic;
namespace DataLib
{
    [Serializable]
    public sealed class SecondaryCJSCode : IEquatable<SecondaryCJSCode>
    {
        public string CJSCode { get; set; }

        public string Text { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as SecondaryCJSCode);
        }

        public override int GetHashCode()
        {
            var hashCode = -1211238045;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CJSCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            return hashCode;
        }

        public bool Equals(SecondaryCJSCode other)
        {
            return other != null &&
                string.Equals(CJSCode, other.CJSCode, StringComparison.InvariantCulture) &&
                string.Equals(Text, other.Text, StringComparison.InvariantCulture);
        }

        public SecondaryCJSCode()
        {
            CJSCode = "1000";
            Text = "new text (code 1000)";
        }

        public static bool operator ==(SecondaryCJSCode c1, SecondaryCJSCode c2)
        {
            return EqualityComparer<SecondaryCJSCode>.Default.Equals(c1, c2);
        }

        public static bool operator !=(SecondaryCJSCode c1, SecondaryCJSCode c2)
        {
            return !(c1 == c2);
        }

        public SecondaryCJSCode Copy()
        {
            return (SecondaryCJSCode)this.MemberwiseClone();
        }
    }

    [Serializable]
    public sealed class SecondaryCJSCodeJSON
    {
        private SecondaryCJSCode secondaryCJSCode;

        public SecondaryCJSCodeJSON(SecondaryCJSCode secondaryCJSCode)
        {
            this.secondaryCJSCode = secondaryCJSCode;
        }

        public string cjsCode { get { return secondaryCJSCode.CJSCode; } }

        public string text { get { return secondaryCJSCode.Text; } }

    }
}