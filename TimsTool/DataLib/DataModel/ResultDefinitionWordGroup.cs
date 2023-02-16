using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Identity;
using Newtonsoft.Json;

namespace DataLib
{ 
    [Serializable]
    public sealed class ResultWordGroupJSON
    {
        [JsonProperty(PropertyName = "wordGroup")]
        public List<string> ResultDefinitionWordGroupNames { get; set; }

        public ResultWordGroupJSON(ResultWordGroup group)
        {
            this.ResultDefinitionWordGroupNames = group.ResultDefinitionWordGroups.Select(x=>x.ResultDefinitionWord).ToList();
        }
    }

    [Serializable]
    public sealed class ResultWordGroupJSONDataPatch
    {
        public List<string> wordGroup { get; set; }
    }

    [Serializable]
    public sealed class ResultWordGroup : DataAudit, IEquatable<ResultWordGroup>
    {
        public ResultWordGroup(ResultDefinition resultDefinition)
        {
            ResultDefinition = resultDefinition;
            ResultDefinitionWordGroups = new List<ResultDefinitionWordGroup>();
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            CreatedUser = IdentityHelper.SignedInUser.Email;
        }

        public override Guid? UUID { get => ResultDefinition.UUID; }

        public override Guid? MasterUUID { get => ResultDefinition.MasterUUID; }

        public override PublishedStatus? PublishedStatus { get => ResultDefinition.PublishedStatus; }

        public override DateTime? PublishedStatusDate { get => ResultDefinition.PublishedStatusDate; }

        public override DateTime? OriginalPublishedDate { get => ResultDefinition.OriginalPublishedDate; }

        public override List<string> PublicationTags { get => ResultDefinition.PublicationTags; }

        public List<ResultDefinitionWordGroup> ResultDefinitionWordGroups { get; set; }

        public ResultDefinition ResultDefinition { get; set; }

        public string WordGroupName
        {
            get
            {
                if (ResultDefinitionWordGroups == null || ResultDefinitionWordGroups.Count == 0) { return null; }
                var orderedGroups = ResultDefinitionWordGroups.Where(x=>x.DeletedDate == null).OrderBy(x => x.ResultDefinitionWord).ToList();
                return string.Join(",", orderedGroups.Select(x => x.ResultDefinitionWord));
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResultWordGroup);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;
            hashCode = hashCode * -1521134295 + GetSynonymListHashCode();
            return hashCode;
        }

        public bool Equals(ResultWordGroup other)
        {
            return other != null &&
                   ResultDefinitionWordGroupsListsAreEqual(other.ResultDefinitionWordGroups);
        }

        private bool ResultDefinitionWordGroupsListsAreEqual(List<ResultDefinitionWordGroup> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.ResultDefinitionWordGroups == null || this.ResultDefinitionWordGroups.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.ResultDefinitionWordGroups == null)
            {
                return false;
            }

            //compare the two lists
            var currentNotOther = ResultDefinitionWordGroups.Except(other).ToList();
            var otherNotCurrent = other.Except(ResultDefinitionWordGroups).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }

        public int GetSynonymListHashCode()
        {
            int hc = 0;
            if (ResultDefinitionWordGroups != null)
                foreach (var r in ResultDefinitionWordGroups)
                    hc ^= r.GetHashCode();
            return hc;
        }

        public static bool operator ==(ResultWordGroup rwg1, ResultWordGroup rwg2)
        {
            return EqualityComparer<ResultWordGroup>.Default.Equals(rwg1, rwg2);
        }

        public static bool operator !=(ResultWordGroup rwg1, ResultWordGroup rwg2)
        {
            return !(rwg1 == rwg2);
        }
    }

    [Serializable]
    public sealed class ResultDefinitionWordGroup : DataAudit, IEquatable<ResultDefinitionWordGroup>
    {
        public ResultDefinitionWordGroup() { 
            CreatedDate = DateTime.Now; 
            LastModifiedDate = DateTime.Now; 
            UUID = Guid.NewGuid(); 
            Synonyms = new List<ResultDefinitionWordSynonym>();
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            CreatedUser = IdentityHelper.SignedInUser.Email;
            ResultDefinitionWord = "New Result Definition Word Group";
        }
        public string ResultDefinitionWord { get; set; }

        [JsonIgnore]
        public DateTime? StartDate { get; set; }

        [JsonIgnore]
        public DateTime? EndDate { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public string StartDateJSON
        {
            get
            {
                if (StartDate == null) { return null; }
                return StartDate.Value.ToString("yyyy-MM-dd");
            }
        }

        [JsonProperty(PropertyName = "endDate")]
        public string EndDateJSON
        {
            get
            {
                if (EndDate == null) { return null; }
                return EndDate.Value.ToString("yyyy-MM-dd");
            }
        }

        public List<ResultDefinitionWordSynonym> Synonyms  { get; set; }

        public ResultDefinitionWordGroup Copy()
        {
            return MakeCopy(false);
        }

        public void SynchroniseChanges(ResultDefinitionWordGroup source)
        {
            ResultDefinitionWord = source.ResultDefinitionWord;
            Synonyms = source.Synonyms;
            LastModifiedDate = source.LastModifiedDate;
            LastModifiedUser = source.LastModifiedUser;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResultDefinitionWordGroup);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ResultDefinitionWord);
            hashCode = hashCode * -1521134295 + GetSynonymListHashCode();
            return hashCode;
        }

        public bool Equals(ResultDefinitionWordGroup other)
        {
            return other != null &&
                   string.Equals(ResultDefinitionWord, other.ResultDefinitionWord, StringComparison.InvariantCulture) &&
                   SynonymListsAreEqual(other.Synonyms);
        }

        private bool SynonymListsAreEqual(List<ResultDefinitionWordSynonym> other)
        {
            if (other == null)
            {
                if (this.Synonyms == null)
                {
                    return true;
                }

                return false;
            }

            if (this.Synonyms == null)
            {
                return false;
            }

            //compare the two lists
            var currentNotOther = Synonyms.Except(other).ToList();
            var otherNotCurrent = other.Except(Synonyms).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }

        public int GetSynonymListHashCode()
        {
            int hc = 0;
            if (Synonyms != null)
            {
                var reordered = Synonyms.OrderBy(x => x.Synonym);
                foreach (var s in reordered)
                    hc ^= s.GetHashCode();
            }
            return hc;
        }

        public ResultDefinitionWordGroup MakeCopy(bool asDraft)
        {
            ResultDefinitionWordGroup res = (ResultDefinitionWordGroup)this.MemberwiseClone();
            res.UUID = Guid.NewGuid();
            res.CreatedDate = DateTime.Now;
            res.LastModifiedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

            //remove the publication tags
            res.PublicationTags = new List<string>();

            //Create a new collection of synonyms
            res.Synonyms = new List<ResultDefinitionWordSynonym>();
            foreach (var synonym in this.Synonyms)
            {
                if (asDraft)
                {
                    res.Synonyms.Add(synonym.Draft());
                }
                else
                {
                    res.Synonyms.Add(synonym);
                }                
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
                res.ResultDefinitionWord = res.ResultDefinitionWord + " - Copy";
            }

            return res;
        }

        public ResultDefinitionWordGroup Draft()
        {
            return MakeCopy(true);
        }

        public static bool operator ==(ResultDefinitionWordGroup rdwg1, ResultDefinitionWordGroup rdwg2)
        {
            return EqualityComparer<ResultDefinitionWordGroup>.Default.Equals(rdwg1, rdwg2);
        }

        public static bool operator !=(ResultDefinitionWordGroup rdwg1, ResultDefinitionWordGroup rdwg2)
        {
            return !(rdwg1 == rdwg2);
        }

        public bool WordChanged(ResultDefinitionWordGroup other)
        {
            return !string.Equals(ResultDefinitionWord, other.ResultDefinitionWord);
        }

        public string GetChangedProperties(ResultDefinitionWordGroup other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = GetChangedProperties<ResultDefinitionWordGroup>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.Append(simpleTypeChanges);
            }
            //now add reports for the complex types:
            if (!SynonymListsAreEqual(other.Synonyms))
            {
                sb.AppendLine("From Synonyms...");
                if (Synonyms == null || Synonyms.Count == 0)
                {
                    sb.AppendLine("'Empty'");
                }
                else
                {
                    sb.AppendLine(string.Join(", ", Synonyms.Select(x=>x.Synonym)));
                }
                sb.AppendLine("To Synonyms...");
                if (other.Synonyms == null || other.Synonyms.Count == 0)
                {
                    sb.AppendLine("'Empty'");
                }
                else
                {
                    sb.AppendLine(string.Join(", ", other.Synonyms.Select(x=>x.Synonym)));
                }
            }
            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class ResultDefinitionWordSynonym : DataAudit, IEquatable<ResultDefinitionWordSynonym>
    {
        public ResultDefinitionWordSynonym() { 
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            CreatedUser = IdentityHelper.SignedInUser.Email;
            UUID = Guid.NewGuid();
            Synonym = "New Synonym";
        }

        public string Synonym { get; set; }

        public void SynchroniseChanges(ResultDefinitionWordSynonym source)
        {
            Synonym = source.Synonym;
            LastModifiedDate = source.LastModifiedDate;
            LastModifiedUser = source.LastModifiedUser;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResultDefinitionWordSynonym);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Synonym);
            return hashCode;
        }

        public bool Equals(ResultDefinitionWordSynonym other)
        {
            return other != null &&
                   Synonym == other.Synonym;
        }

        internal ResultDefinitionWordSynonym Draft()
        {
            ResultDefinitionWordSynonym res = (ResultDefinitionWordSynonym)this.MemberwiseClone();
            res.UUID = Guid.NewGuid();
            res.CreatedDate = DateTime.Now;
            res.LastModifiedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

            res.PublishedStatus = DataLib.PublishedStatus.Draft;
            if (res.MasterUUID == null)
            {
                res.MasterUUID = MasterUUID ?? UUID;
            }

            return res;
        }

        public static bool operator ==(ResultDefinitionWordSynonym rdws1, ResultDefinitionWordSynonym rdws2)
        {
            return EqualityComparer<ResultDefinitionWordSynonym>.Default.Equals(rdws1, rdws2);
        }

        public static bool operator !=(ResultDefinitionWordSynonym rdws1, ResultDefinitionWordSynonym rdws2)
        {
            return !(rdws1 == rdws2);
        }

    }
}