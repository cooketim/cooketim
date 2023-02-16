using System.Linq;

namespace DataLib
{
    public class ResultPromptPart
    {
        private string nameAddressType;
        private bool isHidden;
        private string shortComponentPartName;
        public ResultPromptPart(string componentPartName, string reference, string label, string referenceName, string nameAddressType, bool isHidden)
        {
            ComponentPartName = componentPartName;
            Reference = reference;
            Label = label;
            ReferenceName = referenceName;
            this.nameAddressType = nameAddressType;
            this.isHidden = isHidden;

            if (componentPartName.StartsWith("AddressLine"))
            {
                shortComponentPartName = componentPartName.Replace("AddressLine", "address");
            }

            if (componentPartName.StartsWith("OrganisationName"))
            {
                shortComponentPartName = componentPartName.Replace("OrganisationName", "name");
            }
        }

        public string ReferenceName { get; }

        public string ComponentPartName { get; }

        public string ShortComponentPartName 
        { 
            get
            {
                return string.IsNullOrEmpty(shortComponentPartName) ?  $"{ComponentPartName.First().ToString().ToLowerInvariant()}{ComponentPartName.Substring(1)}" : shortComponentPartName;
            }
        }

        public string Reference { get; }

        public string Label { get; }

        public bool Hidden
        {
            get
            {
                if (!string.IsNullOrEmpty(nameAddressType) && nameAddressType.ToLowerInvariant() == "organisation")
                {
                    if (ComponentPartName.ToLowerInvariant().Contains("first") || ComponentPartName.ToLowerInvariant().Contains("middle") || ComponentPartName.ToLowerInvariant().Contains("last") || ComponentPartName.ToLowerInvariant().Contains("title"))
                    {
                        return true;
                    }
                }
                if (!string.IsNullOrEmpty(nameAddressType) && nameAddressType.ToLowerInvariant() == "person")
                {
                    if (ComponentPartName.ToLowerInvariant().Contains("organisation"))
                    {
                        return true;
                    }
                }
                if (ComponentPartName.ToLowerInvariant().Contains("email"))
                {
                    return true;
                }
                return isHidden;
            }
        }
    }
}