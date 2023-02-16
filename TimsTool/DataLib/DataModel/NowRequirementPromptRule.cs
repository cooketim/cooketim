using Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataLib
{
    [Serializable]
    public sealed class NowRequirementPromptRule : DataAudit, IEquatable<NowRequirementPromptRule>
    {
        private Guid? parentNowRequirementUUID { get; set; }

        private Guid? resultPromptRuleUUID { get; set; }

        public NowRequirementPromptRule(ResultPromptRule rpr, NowRequirement parentNowRequirement) 
        {            
            UUID = Guid.NewGuid();
            CreatedDate = DateTime.Now; 
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            ResultPromptRule = rpr;
            ParentNowRequirement = parentNowRequirement;
            PublishedStatus = parentNowRequirement.PublishedStatus;
        }

        public NowRequirementPromptRule()
        {
        }

        [JsonIgnore]
        public ResultPromptRule ResultPromptRule { get; set; }
        public Guid? ResultPromptRuleUUID
        {
            get
            {
                if (ResultPromptRule != null)
                {
                    resultPromptRuleUUID = null;
                    return ResultPromptRule.UUID;
                }
                if (resultPromptRuleUUID != null) { return resultPromptRuleUUID; }
                return null;
            }
            set
            {
                resultPromptRuleUUID = value;
            }
        }

        [JsonIgnore]
        public NowRequirement ParentNowRequirement { get; set; }

        public Guid? ParentNowRequirementUUID
        {
            get
            {
                if (ParentNowRequirement != null)
                {
                    parentNowRequirementUUID = null;
                    return ParentNowRequirement.UUID;
                }
                if (parentNowRequirementUUID != null) { return parentNowRequirementUUID; }
                return null;
            }
            set
            {
                parentNowRequirementUUID = value;
            }
        }

        [JsonIgnore]
        public Guid? NOWUUID { get => ParentNowRequirement == null ? null : ParentNowRequirement.NOWUUID; }

        public bool IsVariantData { get; set; }

        /// <summary>
        /// When this is set true determines that when generating the NOW and multiple instances of prompts of the same type occur within the same NOW,
        /// only a single distinct instance for the prompt type should be included in the generation of the NOW.
        /// An example of where this would occur is Bail Exception and Bail Exception Reason i.e. the NOW should contain on one exception and exception reason (across multiple offences)
        /// </summary>
        public bool DistinctPromptTypes { get; set; }

        public NowRequirementPromptRule Draft(NowRequirement nr)
        {
            return MakeCopy(true, nr);
        }

        public NowRequirementPromptRule Copy(NowRequirement nr)
        {
            return MakeCopy(false, nr);
        }

        private NowRequirementPromptRule MakeCopy(bool asDraft, NowRequirement nr)
        {
            NowRequirementPromptRule res = (NowRequirementPromptRule)this.MemberwiseClone();
            res.CreatedDate = DateTime.Now;
            res.LastModifiedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;
            res.UUID = Guid.NewGuid();
            if (nr != null)
            {
                res.PublishedStatus = nr.PublishedStatus;
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
            }
            return res;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NowRequirementPromptRule);
        }

        public bool Equals(NowRequirementPromptRule other)
        {
            return other != null &&
                   IsVariantData == other.IsVariantData
                   &&
                   ResultPromptRule == other.ResultPromptRule &&
                   ParentNowRequirement == other.ParentNowRequirement;
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsVariantData);
            hashCode = hashCode * -1521134295 + ResultPromptRule.GetHashCode();
            hashCode = hashCode * -1521134295 + ParentNowRequirement.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(NowRequirementPromptRule rule1, NowRequirementPromptRule rule2)
        {
            return EqualityComparer<NowRequirementPromptRule>.Default.Equals(rule1, rule2);
        }

        public static bool operator !=(NowRequirementPromptRule rule1, NowRequirementPromptRule rule2)
        {
            return !(rule1 == rule2);
        }

        public void SynchroniseChanges(NowRequirementPromptRule localNrpr)
        {
            IsVariantData = localNrpr.IsVariantData;
            DistinctPromptTypes = localNrpr.DistinctPromptTypes;
            LastModifiedDate = localNrpr.LastModifiedDate;
        }
    }

    [Serializable]
    public sealed class NowRequirementPromptRuleJSON
    { 
        private NowRequirementPromptRule rule;
        public NowRequirementPromptRuleJSON(NowRequirementPromptRule rule) { this.rule = rule; }
   
        public Guid? promptId 
        {
            get
            {
                if (rule.ResultPromptRule == null)
                {
                    return null;
                }
                else
                {
                    return rule.ResultPromptRule.ResultPrompt.MasterUUID == null ? rule.ResultPromptRule.ResultPrompt.UUID : rule.ResultPromptRule.ResultPrompt.MasterUUID;
                }
            }
        }
        
        public bool variantData { get => rule.IsVariantData; }

        public bool distinctPromptTypes { get => rule.DistinctPromptTypes; }
    }
}