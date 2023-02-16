using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace DataLib
{
    [Serializable]
    public enum ResultDefinitionRuleType
    {
        optional = 0,
        mandatory = 1,
        atleastOneOf = 2,
        oneOf = 3
    }

    [Serializable]
    public sealed class ResultDefinitionRuleJSON
    {
        private ResultDefinitionRule resultDefinitionRule;
        private List<string> featureToggles;

        public ResultDefinitionRuleJSON(ResultDefinitionRule resultDefinitionRule, List<string> featureToggles) 
        { 
            this.resultDefinitionRule = resultDefinitionRule;
            this.featureToggles = featureToggles;
        }

        public Guid? childResultDefinitionId 
        { 
            get => resultDefinitionRule.ResultDefinition.MasterUUID ?? resultDefinitionRule.ResultDefinition.UUID;  
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultDefinitionRuleType? ruleType
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1635")) != null)
                {
                    //only return the common rule value when mags and crown rules are not set
                    if (resultDefinitionRuleMags == null && resultDefinitionRuleCrown == null)
                    {
                        return resultDefinitionRule.Rule;
                    }

                    //when there are different rules by jurisdiction, the common rule is not required
                    return null;
                }
                else
                {
                    //return the common rule if there is one, otherwise default to mags
                    if (resultDefinitionRule.Rule == null)
                    {
                        return resultDefinitionRule.RuleMags;
                    }
                    return resultDefinitionRule.Rule;
                }
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultDefinitionRuleType? resultDefinitionRuleMags
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1635")) != null)
                {
                    return resultDefinitionRule.RuleMags;
                }

                return null;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultDefinitionRuleType? resultDefinitionRuleCrown
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1635")) != null)
                {
                    return resultDefinitionRule.RuleCrown;
                }

                return null;
            }
        }
    }

    [Serializable]
    public sealed class ResultDefinitionRule : DataAudit, IEquatable<ResultDefinitionRule>
    {
        private Guid? childId = null;

        /// <summary>
        /// default constructor for serialisation
        /// </summary>
        public ResultDefinitionRule() { }

        public ResultDefinitionRule(Guid? parentId, ResultDefinition child, ResultDefinitionRuleType rule)
        {
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
            UUID = Guid.NewGuid();
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;

            ParentUUID = parentId;
            Rule = rule;
            ResultDefinition = child;
        }

        public Guid? ParentUUID { get; set; }

        public Guid? ChildResultDefinitionUUID
        {
            get
            {
                if (ResultDefinition != null) { return ResultDefinition.UUID; }
                return childId;
            }
            set
            {
                childId = value;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultDefinitionRuleType? Rule { get; set; }

        [JsonIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultDefinitionRuleType? RuleMags { get; set; }

        [JsonIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultDefinitionRuleType? RuleCrown { get; set; }

        [JsonIgnore]
        public ResultDefinition ResultDefinition { get; set; }

        [JsonIgnore]
        public DateTime? StartDate { get; set; }

        [JsonIgnore]
        public DateTime? EndDate { get; set; }

        [JsonIgnore]
        public string StartDateJSON
        {
            get
            {
                if (StartDate == null) { return null; }
                return StartDate.Value.ToString("yyyy-MM-dd");
            }
        }

        [JsonIgnore]
        public string EndDateJSON
        {
            get
            {
                if (EndDate == null) { return null; }
                return EndDate.Value.ToString("yyyy-MM-dd");
            }
        }

        public ResultDefinitionRule Draft()
        {
            return MakeCopy(true);
        }

        public ResultDefinitionRule Copy()
        {
            return MakeCopy(false);
        }

        public ResultDefinitionRule CopyAsDraft()
        {
            var res = MakeCopy(false);

            //set a draft status
            res.PublishedStatus = DataLib.PublishedStatus.Draft;
            return res;
        }

        private ResultDefinitionRule MakeCopy(bool asDraft)
        {
            ResultDefinitionRule res = (ResultDefinitionRule)this.MemberwiseClone();
            res.UUID = Guid.NewGuid();
            res.LastModifiedDate = DateTime.Now;
            res.CreatedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

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
            }

            return res;
        }

        public void SynchroniseChanges(ResultDefinitionRule source)
        {
            Rule = source.Rule;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResultDefinitionRule);
        }

        public bool Equals(ResultDefinitionRule other)
        {
            return other != null &&
                   Rule == other.Rule;
        }

        public string GetChangedProperties(ResultDefinitionRule other)
        {
            if (other == null) { return null; }

            if (Rule != other.Rule)
            {
                return string.Format("Rule, from: '{0}' to: '{1}'", Rule.GetDescription(), other.Rule.GetDescription());
            }

            return null;
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;
            hashCode = hashCode * -1521134295 + Rule.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(MasterUUID);
            return hashCode;
        }

        public static bool operator ==(ResultDefinitionRule rule1, ResultDefinitionRule rule2)
        {
            return EqualityComparer<ResultDefinitionRule>.Default.Equals(rule1, rule2);
        }

        public static bool operator !=(ResultDefinitionRule rule1, ResultDefinitionRule rule2)
        {
            return !(rule1 == rule2);
        }
    }
}