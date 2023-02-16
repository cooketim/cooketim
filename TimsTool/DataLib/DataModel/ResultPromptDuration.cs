using System;
using System.Collections.Generic;

namespace DataLib
{
    [Serializable]
    public sealed class ResultPromptDuration : IEquatable<ResultPromptDuration>
    {
        public string DurationElement { get; set; }
        public string WelshDurationElement { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResultPromptDuration);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DurationElement);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WelshDurationElement);
            return hashCode;
        }

        public bool Equals(ResultPromptDuration other)
        {
            return other != null &&
                string.Equals(DurationElement, other.DurationElement, StringComparison.InvariantCulture) &&
                string.Equals(WelshDurationElement, other.WelshDurationElement, StringComparison.InvariantCulture);
        }

        public static bool operator ==(ResultPromptDuration rpd1, ResultPromptDuration rpd2)
        {
            return EqualityComparer<ResultPromptDuration>.Default.Equals(rpd1, rpd2);
        }

        public static bool operator !=(ResultPromptDuration rpd1, ResultPromptDuration rpd2)
        {
            return !(rpd1 == rpd2);
        }
    }
}