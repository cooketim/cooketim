using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Identity;
using Newtonsoft.Json;

namespace DataLib
{
    [Serializable]
    public class ResultPromptWordGroup : DataAudit, IEquatable<ResultPromptWordGroup>
    {
        public ResultPromptWordGroup() { 
            CreatedDate = DateTime.Now; 
            LastModifiedDate = DateTime.Now; 
            UUID = Guid.NewGuid(); 
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            ResultPromptWord = "New Result Prompt Word Group";
            Synonyms = new List<ResultPromptWordSynonym>();
        }

        public string ResultPromptWord { get; set; }

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
        public List<ResultPromptWordSynonym> Synonyms  { get; set; }

        private ResultPromptWordGroup MakeCopy(bool asDraft)
        {
            ResultPromptWordGroup res = (ResultPromptWordGroup)this.MemberwiseClone();
            res.UUID = Guid.NewGuid();
            res.LastModifiedDate = DateTime.Now;
            res.CreatedDate = DateTime.Now;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

            //Create a new collection of synonyms
            res.Synonyms = new List<ResultPromptWordSynonym>();
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

            //remove the publication tags
            res.PublicationTags = new List<string>();

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
                res.ResultPromptWord = res.ResultPromptWord + " - Copy";
            }

            return res;
        }

        public ResultPromptWordGroup Draft()
        {
            return MakeCopy(true);
        }

        public ResultPromptWordGroup Copy()
        {
            return MakeCopy(false);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResultPromptWordGroup);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ResultPromptWord);
            hashCode = hashCode * -1521134295 + GetSynonymListHashCode();
            return hashCode;
        }

        public bool Equals(ResultPromptWordGroup other)
        {
            return other != null &&
                   string.Equals(ResultPromptWord, other.ResultPromptWord, StringComparison.InvariantCulture) &&
                   SynonymListsAreEqual(other.Synonyms);
        }

        public static bool operator ==(ResultPromptWordGroup rpwg1, ResultPromptWordGroup rpwg2)
        {
            return EqualityComparer<ResultPromptWordGroup>.Default.Equals(rpwg1, rpwg2);
        }

        public static bool operator !=(ResultPromptWordGroup rpwg1, ResultPromptWordGroup rpwg2)
        {
            return !(rpwg1 == rpwg2);
        }

        public bool WordChanged(ResultPromptWordGroup other)
        {
            return !string.Equals(ResultPromptWord, other.ResultPromptWord);
        }

        private bool SynonymListsAreEqual(List<ResultPromptWordSynonym> other)
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

        public string GetChangedProperties(ResultPromptWordGroup other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = GetChangedProperties<ResultPromptWordGroup>(this, other);
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
                    sb.AppendLine(string.Join(", ", Synonyms.Select(x => x.Synonym)));
                }
                sb.AppendLine("To Synonyms...");
                if (other.Synonyms == null || other.Synonyms.Count == 0)
                {
                    sb.AppendLine("'Empty'");
                }
                else
                {
                    sb.AppendLine(string.Join(", ", other.Synonyms.Select(x => x.Synonym)));
                }
            }
            return sb.ToString();
        }
    }

    [Serializable]
    public class ResultPromptWordSynonym : DataAudit
    {
        public ResultPromptWordSynonym() {
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            CreatedUser = IdentityHelper.SignedInUser.Email;
            UUID = Guid.NewGuid();
            Synonym = "New Synonym";
        }

        public string Synonym { get; set; }

        internal object Copy()
        {
            return MakeCopy(false);
        }

        internal ResultPromptWordSynonym Draft()
        {
            return MakeCopy(true);
        }

        private ResultPromptWordSynonym MakeCopy(bool asDraft)
        {
            ResultPromptWordSynonym res = (ResultPromptWordSynonym)this.MemberwiseClone();
            res.UUID = Guid.NewGuid();
            res.CreatedDate = DateTime.Now;
            res.LastModifiedDate = DateTime.Now;
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
    }
}