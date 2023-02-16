using Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLib
{
    [Serializable]
    public class ResultPromptRule : DataAudit, IEquatable<ResultPromptRule>
    {
        private Guid? resultPromptUUID { get; set; }

        public ResultPromptRule() 
        { 
            UUID = Guid.NewGuid(); 
            CreatedDate = DateTime.Now; 
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            Rule = ResultPromptRuleType.optional;
        }

        public ResultPromptRule(ResultPrompt resultPrompt, Guid? resultDefinitionUUID) : this()
        {
            ResultPrompt = resultPrompt;
            ResultDefinitionUUID = resultDefinitionUUID;
        }

        public ResultPromptRule(Guid? resultPromptUUID, Guid? resultDefinitionUUID) : this()
        {
            this.resultPromptUUID = resultPromptUUID;
            ResultDefinitionUUID = resultDefinitionUUID;
        }

        public Guid? ResultDefinitionUUID { get; set; }

        public Guid? ResultPromptUUID
        {
            get
            {
                if (ResultPrompt == null)
                {
                    return resultPromptUUID;
                }
                resultPromptUUID = null;
                return ResultPrompt.UUID;
            }
            set
            {
                resultPromptUUID = value;
            }
        }

        [JsonIgnore]
        public Guid? ResultPromptMasterUUID
        {
            get
            {
                if (ResultPrompt == null)
                {
                    return null;
                }
                return ResultPrompt.MasterUUID == null ? ResultPrompt.UUID : ResultPrompt.MasterUUID;
            }
        }

        [JsonIgnore]
        public ResultPrompt ResultPrompt { get; set; }

        [JsonIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultPromptRuleType? Rule { get; set; }

        [JsonIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultPromptRuleType? RuleMags { get; set; }

        [JsonIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultPromptRuleType? RuleCrown { get; set; }

        [JsonIgnore]
        public int? PromptSequence { get; set; }

        [JsonIgnore]
        public List<string> UserGroups { get; set; }

        public ResultPromptRule Draft()
        {
            return MakeCopy(true);
        }

        public ResultPromptRule Copy()
        {
            return MakeCopy(false);
        }

        public ResultPromptRule CopyAsDraft()
        {
            var res = MakeCopy(false);

            //set as a draft
            res.PublishedStatus = DataLib.PublishedStatus.Draft;

            return res;
        }

        private ResultPromptRule MakeCopy(bool asDraft)
        {
            ResultPromptRule res = (ResultPromptRule)this.MemberwiseClone();
            res.UUID = Guid.NewGuid();
            res.LastModifiedDate = DateTime.Now;
            res.CreatedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;
            if (UserGroups != null)
            {
                res.UserGroups = new List<string>(UserGroups);
            }
            else
            {
                res.UserGroups = new List<string>();
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
            return Equals(obj as ResultPromptRule);
        }

        public bool Equals(ResultPromptRule other)
        {
            return other != null &&
                   Rule == other.Rule &&
                   EqualityComparer<int?>.Default.Equals(PromptSequence, other.PromptSequence) &&
                   UserGroupListsAreEqual(other.UserGroups);
        }

        public string GetChangedProperties(ResultPromptRule other)
        {
            if (other == null) { return null; }

            var sb = new StringBuilder();
            if (Rule != other.Rule)
            {
                sb.AppendLine(string.Format("Rule, from: '{0}' to: '{1}'", Rule.GetDescription(), other.Rule.GetDescription()));
            }

            if (!EqualityComparer<int?>.Default.Equals(PromptSequence, other.PromptSequence))
            {
                sb.AppendLine(string.Format("PromptSequence, from: '{0}' to: '{1}'", PromptSequence, other.PromptSequence));
            }

            if (!UserGroupListsAreEqual(other.UserGroups))
            {
                sb.AppendLine("From User Groups...");
                if (UserGroups == null || UserGroups.Count == 0)
                {
                    sb.AppendLine("'Empty'");
                }
                else
                {
                    sb.AppendLine(string.Join(", ", UserGroups));
                }
                sb.AppendLine("To User Groups...");
                if (other.UserGroups == null || other.UserGroups.Count == 0)
                {
                    sb.AppendLine("'Empty'");
                }
                else
                {
                    sb.AppendLine(string.Join(", ", other.UserGroups));
                }
            }

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;
            hashCode = hashCode * -1521134295 + Rule.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(PromptSequence);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(MasterUUID);
            hashCode = hashCode * -1521134295 + GetUserGroupListHashCode();
            return hashCode;
        }

        public static bool operator ==(ResultPromptRule rule1, ResultPromptRule rule2)
        {
            return EqualityComparer<ResultPromptRule>.Default.Equals(rule1, rule2);
        }

        public static bool operator !=(ResultPromptRule rule1, ResultPromptRule rule2)
        {
            return !(rule1 == rule2);
        }

        private bool UserGroupListsAreEqual(List<string> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.UserGroups == null || this.UserGroups.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.UserGroups == null)
            {
                return false;
            }

            return (this.UserGroups.Count == other.Count) && !this.UserGroups.Except(other).Any();
        }

        public int GetUserGroupListHashCode()
        {
            int hc = 0;
            if (UserGroups != null)
                foreach (var u in UserGroups)
                    hc ^= u.GetHashCode();
            return hc;
        }

        public void SynchroniseChanges(ResultPromptRule source)
        {
            Rule = source.Rule;
            PromptSequence = source.PromptSequence;
            UserGroups = source.UserGroups;
            LastModifiedDate = source.LastModifiedDate;
        }
    }
}