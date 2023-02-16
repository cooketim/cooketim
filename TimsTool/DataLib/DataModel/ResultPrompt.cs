using Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataLib
{
    [Serializable]
    public enum ResultPromptRuleType
    {
        optional = 0,
        mandatory = 1,
        oneOf = 2
    }

    [Serializable]
    public sealed class ResultPrompt : DataAudit, IEquatable<ResultPrompt>
    {
        public ResultPrompt() 
        { 
            UUID = Guid.NewGuid(); 
            CreatedDate = DateTime.Now; 
            CreatedUser = IdentityHelper.SignedInUser.Email;
            Label = "New Result Prompt";
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            Jurisdiction = "Both";
            PromptType = "TXT";
        }

        private FixedList fixedList;
        private List<string> resultPromptWordGroupsString;

        public string Label { get; set; }
        public string PromptType { get; set; }
        public string NameAddressType { get; set; }
        public bool NameEmail { get; set; }

        /// <summary>
        /// Identifies that the prompt is hidden from the Notepad but is included in published results
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Identifies that the prompt is associated to reference data
        /// </summary>
        public bool AssociateToReferenceData { get; set; }

        /// <summary>
        /// Determines that this result prompt is required for judicial validation.  The validation will only occur when the containing result definition is required to be validated
        /// </summary>
        public bool JudicialValidation { get; set; }

        /// <summary>
        /// Identifies that the prompt is a duration start date
        /// </summary>
        public bool IsDurationStartDate { get; set; }

        /// <summary>
        /// Identifies that the prompt is a duration end date
        /// </summary>
        public bool IsDurationEndDate { get; set; }

        public List<ResultPromptDuration> DurationElements { get; set; }

        public List<string> ResultPromptWordGroupsString 
        { 
            get
            {
                if (ResultPromptWordGroups != null && ResultPromptWordGroups.Count > 0)
                {
                    return ResultPromptWordGroups.Where(x => x.DeletedDate == null).Select(x => x.UUID.ToString()).ToList();
                }
                return resultPromptWordGroupsString;
            }
            set
            {
                resultPromptWordGroupsString = value;
            }
        }

        [JsonIgnore]
        public List<ResultPromptWordGroup> ResultPromptWordGroups { get; set; }

        public string ResultPromptGroup { get; set; }

        private Guid? fixedListUUID { get; set; }
        public Guid? FixedListUUID
        {
            get
            {
                if (fixedList != null && fixedList.DeletedDate == null)
                {
                    return fixedList.MasterUUID == null ? fixedList.UUID : fixedList.MasterUUID;
                }
                return fixedListUUID;
            }
            set
            {
                fixedListUUID = value;
            }
        }
        
        [JsonIgnore]
        public string FixedListLabel
        {
            get
            {
                if (fixedList != null && fixedList.DeletedDate == null)
                {
                    return fixedList.Label;
                }
                return null;
            }
        }
        public string Min { get; set; }
        public string Max { get; set; }
        public string Qual { get; set; }
        public string Wording { get; set; }
        public string WelshWording { get; set; }
        public string Jurisdiction { get; set; }
        public string PromptReference { get; set; }
        public string WelshLabel { get; set; }
        public bool IsAvailableForCourtExtract { get; set; }
        public bool Financial { get; set; }

        /// <summary>
        /// Enumerated values are CachableEnum
        /// </summary>
        public CacheableEnum? Cacheable { get; set; }

        /// <summary>
        /// The details of the DSL path setting where the data is stored and the cached is initiated from
        /// </summary>
        public string CacheDataPath { get; set; }

        public string RegularExpression { get; set; }

        /// <summary>
        /// Determines the duration sequence that this prompt represents i.e. the Primary, Seconday or Tertiary Sequence expressed as 1, 2, 3
        /// </summary>
        public int? DurationSequence { get; set; }

        [JsonIgnore]
        public FixedList FixedList
        {
            get => fixedList;
            set
            {
                fixedList = value;
            }
        }

        private ResultPrompt MakeCopy(bool asDraft)
        {
            ResultPrompt res = (ResultPrompt)this.MemberwiseClone();
            res.UUID = Guid.NewGuid();            
            res.CreatedDate = DateTime.Now;
            res.LastModifiedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

            //remove the publication tags
            res.PublicationTags = new List<string>();

            //replace word group collection
            res.ResultPromptWordGroups = new List<ResultPromptWordGroup>();
            if (ResultPromptWordGroups != null)
            {
                res.ResultPromptWordGroups.AddRange(ResultPromptWordGroups);
            }

            if (asDraft)
            {
                res.PublishedStatus = DataLib.PublishedStatus.Draft;
                if (res.MasterUUID == null)
                {
                    res.MasterUUID = MasterUUID ?? UUID;
                }
            }
            else
            {
                res.MasterUUID = res.UUID;
                res.PromptReference = string.Empty;
                res.Label = res.Label + " - Copy";
            }
            return res;
        }

        public ResultPrompt Copy()
        {
            return MakeCopy(false);
        }

        public ResultPrompt Draft()
        {
            return MakeCopy(true);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResultPrompt);
        }

        public bool Equals(ResultPrompt other)
        {
            if (other == null) { return false; }

            if (!string.IsNullOrEmpty(base.GetChangedProperties<ResultPrompt>(this, other)))
            {
                return false;
            }

            return DurationElementsAreEqual(other.DurationElements) && WordGroupListsAreEqual(other.ResultPromptWordGroups);
        }

        public string GetChangedProperties(ResultPrompt other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = base.GetChangedProperties<ResultPrompt>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.AppendLine(simpleTypeChanges);
            }

            //now add reports for the complex types:
            if (!DurationElementsAreEqual(other.DurationElements))
            {
                sb.AppendLine("From Duration Elements...");                
                sb.AppendLine(DurationElementsReport);
                sb.AppendLine();
                sb.AppendLine("To Duration Elements...");
                sb.AppendLine(other.DurationElementsReport);
            }

            if (!WordGroupListsAreEqual(other.ResultPromptWordGroups))
            {
                sb.AppendLine("From Word Synonyms...");
                sb.AppendLine(WordGroupsReport);
                sb.AppendLine();
                sb.AppendLine("To Word Synonyms...");
                sb.AppendLine(other.WordGroupsReport);
            }

            return sb.ToString();
        }

        private bool DurationElementsAreEqual(List<ResultPromptDuration> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.DurationElements == null || this.DurationElements.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.DurationElements == null)
            {
                return false;
            }

            //compare the two lists
            var currentNotOther = DurationElements.Except(other).ToList();
            var otherNotCurrent = other.Except(DurationElements).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }
        private bool WordGroupListsAreEqual(List<ResultPromptWordGroup> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.ResultPromptWordGroups == null || this.ResultPromptWordGroups.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.ResultPromptWordGroups == null)
            {
                return false;
            }

            //compare the two lists but based on just the words rather than synonym equality
            var currentWords = ResultPromptWordGroups.Select(x => x.ResultPromptWord);
            var otherWords = other.Select(x => x.ResultPromptWord);
            var currentNotOther = currentWords.Except(otherWords).ToList();
            var otherNotCurrent = otherWords.Except(currentWords).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }

        private string WordGroupsReport
        {
            get
            {
                if (ResultPromptWordGroups != null && ResultPromptWordGroups.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var wgString in ResultPromptWordGroups.Where(x => x.DeletedDate == null).OrderBy(x=>x.ResultPromptWord).Select(x => x.ResultPromptWord))
                    {
                        sb.AppendLine(string.Join(", ", wgString));
                    }
                    return sb.Length == 0 ? "'Empty'" : sb.ToString();
                }
                return "'Empty'";
            }
        }

        private string DurationElementsReport
        {
            get
            {
                if (DurationElements != null && DurationElements.Count > 0)
                {
                    var sb = new StringBuilder("Duration Elements...");
                    sb.AppendLine(string.Join(", ", DurationElements.Select(x=>x.DurationElement)));
                    return sb.ToString();
                }
                return null;
            }
        }

        public int GetDurationElementsHashCode()
        {
            int hc = 0;
            if (DurationElements != null)
                foreach (var d in DurationElements)
                    hc ^= d.GetHashCode();
            return hc;
        }

        public override int GetHashCode()
        {
            var hashCode = -1211238045;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Label);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PromptType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NameAddressType);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(NameEmail);
            hashCode = hashCode * -1521134295 + GetDurationElementsHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ResultPromptGroup);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FixedListLabel);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(FixedListUUID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Min);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Max);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Qual);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Wording);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WelshWording);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Jurisdiction);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PromptReference);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WelshLabel);
            hashCode = hashCode * -1521134295 + IsAvailableForCourtExtract.GetHashCode();
            hashCode = hashCode * -1521134295 + Financial.GetHashCode();
            hashCode = hashCode * -1521134295 + DurationSequence.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(Hidden);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(AssociateToReferenceData);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RegularExpression);
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode((int?)Cacheable);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CacheDataPath);
            return hashCode;
        }

        public static bool operator ==(ResultPrompt prompt1, ResultPrompt prompt2)
        {
            return EqualityComparer<ResultPrompt>.Default.Equals(prompt1, prompt2);
        }

        public static bool operator !=(ResultPrompt prompt1, ResultPrompt prompt2)
        {
            return !(prompt1 == prompt2);
        }

        public bool SynchroniseChanges(ResultPrompt source)
        {
            if (source == this)
            {
                //no changes
                return false;
            }

            Label = source.Label;
            PromptType = source.PromptType;
            NameAddressType = source.NameAddressType;
            NameEmail = source.NameEmail;
            DurationElements = source.DurationElements;
            ResultPromptGroup = source.ResultPromptGroup;
            FixedList = source.fixedList;
            Min = source.Min;
            Max = source.Max;
            Qual = source.Qual;
            WelshLabel = source.WelshLabel;
            WelshWording = source.WelshWording;
            Wording = source.Wording;
            Jurisdiction = source.Jurisdiction;
            PromptReference = source.PromptReference;
            IsAvailableForCourtExtract = source.IsAvailableForCourtExtract;
            Financial = source.Financial;
            DurationSequence = source.DurationSequence;
            Hidden = source.Hidden;
            AssociateToReferenceData = source.AssociateToReferenceData;
            RegularExpression = source.RegularExpression;
            Cacheable = source.Cacheable;
            CacheDataPath = source.CacheDataPath;
            LastModifiedDate = source.LastModifiedDate;

            return true;
        }
    }

    [Serializable]
    public class ResultPromptJSON
    {
        private ResultDefinition parentResult;
        private ResultPrompt prompt;
        private ResultPromptRule? promptRule;
        private string promptReference;
        private string referenceName;
        private string promptLabel;
        private string componentPartName;
        private string shortComponentPartName;
        private bool? cacheDataPathFound = null;
        private ResultPromptRuleType? rule;
        private ResultPromptRuleType? ruleMags;
        private ResultPromptRuleType? ruleCrown;
        private List<string> featureToggles;
        private string cachePath;

        public Guid? id
        {
            get => prompt.MasterUUID ?? prompt.UUID;
        }

        public string label
        {
            get => string.IsNullOrEmpty(promptLabel) ? prompt.Label : promptLabel;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultPromptRuleType? resultPromptRule
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1635")) != null)
                {
                    //only return the common rule value when mags and crown rules are not set
                    if (resultPromptRuleMags == null && resultPromptRuleCrown == null)
                    {
                        return rule == null ? promptRule.Rule : rule;
                    }

                    //when there are different rules by jurisdiction, the common rule is not required
                    return null;
                }
                else
                {
                    //return the common rule if there is one, otherwise default to mags
                    var res = rule == null ? promptRule.Rule : rule;
                    if (res == null)
                    {
                        // return the mags rule value, i.e. assume that the mags rule equates to the required common rule 
                        // note that the local variable may be set in the constructor as an override to the set data value i.e. for name address fields
                        return ruleMags == null ? promptRule.RuleMags : ruleMags;
                    }
                    return res;
                }
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultPromptRuleType? resultPromptRuleMags
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1635")) != null)
                {
                    return ruleMags == null ? promptRule.RuleMags : ruleMags;
                }

                return null;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultPromptRuleType? resultPromptRuleCrown
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1635")) != null)
                {
                    return ruleCrown == null ? promptRule.RuleCrown : ruleCrown;
                }

                return null;
            }
        }

        public string type
        {
            get => prompt.PromptType;
        }

        public string nameAddressType
        {
            get => ! string.IsNullOrEmpty(type) && type == "NAMEADDRESS" ? prompt.NameAddressType : null;
        }

        public bool nameEmail
        {
            get => !string.IsNullOrEmpty(type) && type == "NAMEADDRESS" ? prompt.NameEmail : false;
        }

        public string componentLabel
        {
            get => !string.IsNullOrEmpty(type) && type == "NAMEADDRESS" ? prompt.Label : null;
        }

        public string fixListLabel
        {
            get => !string.IsNullOrEmpty(type) && type == "NAMEADDRESS"  && prompt.AssociateToReferenceData ? string.Format("Select {0}",prompt.Label) : null;
        }

        public string referenceDataKey
        {
            get => prompt.AssociateToReferenceData ? (!string.IsNullOrEmpty(referenceName) ? referenceName : reference) : null;
        }

        public bool hidden
        {
            get 
            {
                if ((prompt.PromptType == "NAMEADDRESS" || prompt.PromptType == "ADDRESS") && !string.IsNullOrEmpty(componentPartName))
                {
                    if (!string.IsNullOrEmpty(nameAddressType) && nameAddressType.ToLowerInvariant() == "organisation")
                    {
                        if (componentPartName.ToLowerInvariant().Contains("first") || componentPartName.ToLowerInvariant().Contains("middle") || componentPartName.ToLowerInvariant().Contains("last") || componentPartName.ToLowerInvariant().Contains("title"))
                        {
                            return true;
                        }
                    }
                    if (!string.IsNullOrEmpty(nameAddressType) && nameAddressType.ToLowerInvariant() == "person")
                    {
                        if (componentPartName.ToLowerInvariant().Contains("organisation"))
                        {
                            //organisation name prompt must always be visible because it is used as a parent component within the Notepad UI.  Note - this is the case even when the address type is a person address
                            return false;
                        }
                    }
                    if (componentPartName.ToLowerInvariant().Contains("email"))
                    {
                        return true;
                    }
                }
                return prompt.Hidden;
            } 
        }

        public bool associateToReferenceData
        {
            get => prompt.AssociateToReferenceData;
        }

        public int? sequence
        {
            get => promptRule.PromptSequence;
        }

        public Guid? fixedListId
        {
            get => prompt.FixedListUUID;
        }

        public string duration
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Determines that this result prompt is required for judicial validation.  The validation will only occur when the containing result definition is required to be validated
        /// </summary>
        public bool? judicialValidation
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x=>x.ToUpperInvariant().Contains("CCT-1076")) != null)
                {
                    if (prompt.JudicialValidation && parentResult.JudicialValidation)
                    {
                        return true;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Identifies that the prompt is a duration start date
        /// </summary>
        public bool? isDurationStartDate 
        { 
            get
            {
                if (featureToggles != null)
                {
                    return prompt.IsDurationStartDate ? true : default(bool?);
                }
                return null;
            }
        }

        /// <summary>
        /// Identifies that the prompt is a duration end date
        /// </summary>
        public bool? isDurationEndDate
        {
            get
            {
                if (featureToggles != null)
                {
                    return prompt.IsDurationEndDate ? true : default(bool?);
                }
                return null;
            }
        }


        public string partName
        {
            get => componentPartName;
        }

        public string welshDuration
        {
            get;
            private set;
        }

        public string[] userGroups
        {
            get
            {
                if (promptRule.UserGroups == null || promptRule.UserGroups.Count == 0) { return new string[0]; }
                return promptRule.UserGroups.ToArray();
            }
        }

        public string[] wordGroup
        {
            get;
            private set;
        }

        public string courtExtract
        {
            get
            {
                if ((prompt.PromptType == "NAMEADDRESS" || prompt.PromptType == "ADDRESS") && !string.IsNullOrEmpty(componentPartName))
                {
                    if (componentPartName.ToLowerInvariant().Contains("email"))
                    {
                        return "N";
                    }
                    if (nameEmail && (componentPartName.ToLowerInvariant().Contains("line") || componentPartName.ToLowerInvariant().Contains("post")))
                    {
                        return "N";
                    }
                    if (!string.IsNullOrEmpty(nameAddressType) && nameAddressType.ToLowerInvariant() == "organisation")
                    {
                        if (componentPartName.ToLowerInvariant().Contains("first") || componentPartName.ToLowerInvariant().Contains("middle") || componentPartName.ToLowerInvariant().Contains("last") || componentPartName.ToLowerInvariant().Contains("title"))
                        {
                            return "N";
                        }
                    }
                    if (!string.IsNullOrEmpty(nameAddressType) && nameAddressType.ToLowerInvariant() == "person")
                    {
                        if (componentPartName.ToLowerInvariant().Contains("organisation"))
                        {
                            return "N";
                        }
                    }
                }
                if (prompt.Hidden)
                {
                    return "N";
                }
                return prompt.IsAvailableForCourtExtract ? "Y" : "N";
            }
        }

        public string financial
        {
            get => prompt.Financial ? "Y" : "N";
        }

        public string regularExpression
        {
            get
            {
                if ((prompt.PromptType == "NAMEADDRESS" || prompt.PromptType == "ADDRESS") && !string.IsNullOrEmpty(componentPartName) && componentPartName.ToLowerInvariant().Contains("email"))
                {
                    return "^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$";
                }
                return prompt.RegularExpression;
            }
        }

        /// <summary>
        /// Enumerated values are CachableEnum
        /// </summary>
        public CacheableEnum? cacheable 
        {
            get
            {
                if (cacheDataPathFound != null && !cacheDataPathFound.Value && !string.IsNullOrEmpty(prompt.CacheDataPath))
                {
                    //parsing did not match, set to "not cached"
                    return CacheableEnum.notCached;
                }

                //parsing matched, return the cacheable setting unaltered
                return prompt.Cacheable;
            }
        }

        /// <summary>
        /// The details of the DSL path setting where the data is stored and the cached is initiated from
        /// </summary>
        public string cacheDataPath 
        {
            get
            {
                if (cacheDataPathFound == null)
                {
                    //no parsing of the provided cache data path performed, just return the provided path
                    return prompt.CacheDataPath;
                }
                if (!cacheDataPathFound.Value)
                {
                    // parsing of the provided cache data path performed, key matching prompt reference not found, return null
                    return null;
                }
                //return the matched value from parsing the provided cache data path
                return cachePath;
            }
        }

        public string jurisdiction
        {
            get
            {
                return prompt.Jurisdiction[0].ToString().ToUpper();
            }
        }

        public string min
        {
            get => prompt.Min;
        }

        public string max
        {
            get => prompt.Max;
        }

        public string qual
        {
            get => prompt.Qual;
        }

        public string wording
        {
            get => prompt.Wording;
        }

        public string welshWording
        {
            get => prompt.WelshWording;
        }

        public string reference
        {
            get => string.IsNullOrEmpty(promptReference) ? prompt.PromptReference : promptReference;
        }

        public string welshLabel
        {
            //when label is overridden suppress the Welsh translation
            get => string.IsNullOrEmpty(promptLabel) ? prompt.WelshLabel : null;
        }

        public string resultPromptGroup
        {
            get => prompt.ResultPromptGroup;
        }

        public int? durationSequence
        {
            get => prompt.DurationSequence;
        }

        public ResultPromptJSON(ResultDefinition parentResult, ResultPromptRule promptRule, string[] wordGroup, ResultPromptDuration duration, List<string> featureToggles)
        {
            this.parentResult = parentResult;
            this.prompt = promptRule.ResultPrompt;
            this.promptRule = promptRule;
            this.duration = duration == null ? null : duration.DurationElement;
            this.welshDuration = duration == null ? null : duration.WelshDurationElement;
            this.wordGroup = wordGroup;
            this.featureToggles = featureToggles;
        }

        public ResultPromptJSON(ResultDefinition parentResult, ResultPromptRule promptRule, string componentPartName, 
            string shortComponentPartName, string reference, string label,
            string referenceName, List<string> featureToggles)
        {
            this.parentResult = parentResult;
            this.prompt = promptRule.ResultPrompt;
            this.promptRule = promptRule;
            this.componentPartName = componentPartName;
            this.shortComponentPartName = shortComponentPartName;
            promptReference = reference;
            promptLabel = label;
            if (promptRule.Rule != null)
            {
                //see if we have an expanded name address prompt
                if (!string.IsNullOrEmpty(componentPartName) && componentPartName == "AddressLine1")
                {
                    // address line 1 is mandatory
                    rule = ResultPromptRuleType.mandatory;
                }
                else if(promptRule.ResultPrompt.NameEmail && !string.IsNullOrEmpty(componentPartName) && componentPartName == "EmailAddress1")
                {
                    // email address 1 is mandatory for name email prompt
                    rule = ResultPromptRuleType.mandatory;
                }
                else
                {
                    rule = promptRule.Rule;
                }
            }
            if (promptRule.RuleMags != null)
            {
                //see if we have an expanded name address prompt
                if (!string.IsNullOrEmpty(componentPartName) && componentPartName == "AddressLine1")
                {
                    // address line 1 is mandatory
                    ruleMags = ResultPromptRuleType.mandatory;
                }
                else if (promptRule.ResultPrompt.NameEmail && !string.IsNullOrEmpty(componentPartName) && componentPartName == "EmailAddress1")
                {
                    // email address 1 is mandatory for name email prompt
                    ruleMags = ResultPromptRuleType.mandatory;
                }
                else
                {
                    ruleMags = promptRule.RuleMags;
                }
            }
            if (promptRule.RuleCrown != null)
            {
                //see if we have an expanded name address prompt
                if (!string.IsNullOrEmpty(componentPartName) && componentPartName == "AddressLine1")
                {
                    // address line 1 is mandatory
                    ruleCrown = ResultPromptRuleType.mandatory;
                }
                else if (promptRule.ResultPrompt.NameEmail && !string.IsNullOrEmpty(componentPartName) && componentPartName == "EmailAddress1")
                {
                    // email address 1 is mandatory for name email prompt
                    ruleCrown = ResultPromptRuleType.mandatory;
                }
                else
                {
                    ruleCrown = promptRule.RuleCrown;
                }
            }
            this.referenceName = referenceName;
            this.featureToggles = featureToggles;

            //set the cache mapping
            SetCacheDataPath();
        }

        public ResultPromptJSON(ResultDefinition parentResult, ResultPromptRule promptRule, List<string> featureToggles)
        {
            this.parentResult = parentResult;
            this.prompt = promptRule.ResultPrompt;
            this.promptRule = promptRule;
            this.featureToggles = featureToggles;
        }
        private void SetCacheDataPath()
        {
            cacheDataPathFound = false;
            if (!string.IsNullOrEmpty(prompt.CacheDataPath))
            {
                var parts = prompt.CacheDataPath.Split(',');
                foreach(var part in parts)
                {
                    var keyVal = part.Split(':');
                    if (keyVal.Length == 2 && keyVal[0].ToLowerInvariant().Trim() == reference.ToLowerInvariant())
                    {
                        cacheDataPathFound = true;
                        cachePath = keyVal[1].Trim();
                        break;
                    }
                }
            }
        }
    }
}