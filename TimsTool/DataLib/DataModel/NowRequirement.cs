using Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLib
{
    [Serializable]
    public sealed class NowRequirement : DataAudit
    {
        private bool excludeFromMDE;
        private Guid? parentNowRequirementUUID;
        private Guid? rootParentNowRequirementUUID;
        private Guid? resultDefinitionUUID;

        public NowRequirement() 
        { 
            UUID = Guid.NewGuid(); 
            CreatedDate = DateTime.Now; 
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            ResultDefinitionSequence = 0;
        }

        public NowRequirement(ResultDefinition rd, Now now, NowRequirement parentNr, NowRequirement rootNr) : this()
        {
            SetResultDefinitionAndPromptRules(rd);
            NOWUUID = now.UUID;
            PublishedStatus = now.PublishedStatus;
            ParentNowRequirement = parentNr;
            RootParentNowRequirement = rootNr;
            if (parentNr != null)
            {
                PublishedStatus = parentNr.PublishedStatus;
            }
            else if (rootNr != null)
            {
                PublishedStatus = rootNr.PublishedStatus;
            }
            else
            {
                PublishedStatus = now.PublishedStatus;
            }

            //Align the published status for the prompt rules
            if (NowRequirementPromptRules != null)
            {
                NowRequirementPromptRules.ForEach(x => x.PublishedStatus = PublishedStatus);
            }

            //make child now requirements for each child result definition rule
            SetChildNowRequirements(rd, now, 100);
        }

        public Guid? ParentNowRequirementUUID
        {
            get
            {
                if (ParentNowRequirement != null)
                {
                    return ParentNowRequirement.UUID;
                }
                return parentNowRequirementUUID;
            }
            set
            {
                parentNowRequirementUUID = value;
                ParentNowRequirement = null;
            }
        }

        public Guid? RootParentNowRequirementUUID
        {
            get
            {
                if (RootParentNowRequirement != null)
                {
                    return RootParentNowRequirement.UUID;
                }
                return rootParentNowRequirementUUID;
            }
            set
            {
                rootParentNowRequirementUUID = value;
                RootParentNowRequirement = null;
            }
        }

        public Guid? ResultDefinitionUUID
        {
            get
            {
                if (ResultDefinition != null)
                {
                    return ResultDefinition.UUID;
                }
                return resultDefinitionUUID;
            }
            set
            {
                resultDefinitionUUID = value;
            }
        }

        public Guid? NOWUUID { get; set; }

        public bool Mandatory { get; set; }

        public bool PrimaryResult { get; set; }

        public bool ExcludeFromMDE
        {
            get
            {
                if (RootParentNowRequirement == null)
                {
                    if (PrimaryResult)
                    {
                        return false;
                    }
                    return excludeFromMDE;
                }
                return RootParentNowRequirement.ExcludeFromMDE;
            }
            set
            {
                excludeFromMDE = value;
            }
        }

        [JsonIgnore]
        public ResultDefinition ResultDefinition { get; set; }

        public void ResetResultDefinitionReference(ResultDefinition rd)
        {
            ResultDefinition = rd;
        }

        public void SetResultDefinitionAndPromptRules(ResultDefinition rd)
        {
            if (rd == null) { return; }
            ResultDefinition = rd;
            var newRules = new List<NowRequirementPromptRule>();
            if (rd.ResultPrompts != null && rd.ResultPrompts.Where(x => x.DeletedDate == null).Count() > 0)
            {
                if (NowRequirementPromptRules == null) { NowRequirementPromptRules = new List<NowRequirementPromptRule>(); }

                foreach (var rpr in rd.ResultPrompts.Where(x => x.DeletedDate == null))
                {
                    //first see if we already have the rule
                    var match = NowRequirementPromptRules.FirstOrDefault(x => x.ResultPromptRule.ResultPromptMasterUUID == rpr.ResultPromptMasterUUID);
                    if (match == null)
                    {
                        var newItem = new NowRequirementPromptRule(rpr, this);
                        newRules.Add(newItem);
                    }
                    else
                    {
                        if (match.ResultPromptRule.ResultPromptUUID != rpr.ResultPromptUUID || match.ResultPromptRule.UUID != rpr.UUID)
                        {
                            match.ResultPromptRule = rpr;
                        }
                        newRules.Add(match);
                    }
                }
            }
            NowRequirementPromptRules = newRules;
        }

        [JsonIgnore]
        public NowRequirement ParentNowRequirement { get; set; }

        [JsonIgnore]
        public NowRequirement RootParentNowRequirement { get; set; }

        public List<NowRequirementPromptRule> NowRequirementPromptRules { get; set; }

        [JsonIgnore]
        public List<NowRequirement> NowRequirements { get; set; }


        public int? ResultDefinitionSequence { get; set; }


        public List<NowRequirementText> NowRequirementTextList { get; set; }

        /// <summary>
        /// When this is set true determines that when generating the NOW and multiple instances of results of the same type occur within the same NOW,
        /// only a single distinct instance for the result type should be included in the generation of the NOW.
        /// An example of where this would occur is The Community Requirement Results on a Community Order NOW i.e. the NOW should contain on one instance of each requirement type (across multiple offences)
        /// An additional example exists that follows a similar pattern ...
        /// Bail Conditions and Adjournment Reasons are defined as Results but in the rollup are made into prompts due to the rule setting "Make As A Prompt".
        /// When the NOW is generated these results will appear as prompts that must be treated as though the prompt requirement setting for  DistinctPromptTypes was true
        /// </summary>
        public bool DistinctResultTypes { get; set; }

        public DateTime? StartDate { get; set; }

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

        private void SetChildNowRequirements(ResultDefinition rd, Now now, int seq)
        {
            //make child now requirements for each child result definition rule
            if (rd.ResultDefinitionRules != null && rd.ResultDefinitionRules.Where(x => x.DeletedDate == null).Count() > 0)
            {
                var nrs = new List<NowRequirement>();
                foreach (var rdr in rd.ResultDefinitionRules.Where(x => x.DeletedDate == null))
                {
                    var nr = new NowRequirement(rdr.ResultDefinition, now, this, RootParentNowRequirement == null ? this : RootParentNowRequirement);
                    nr.ResultDefinitionSequence = seq;
                    seq = seq + 100;
                    nrs.Add(nr);
                }
                NowRequirements = nrs;
            }
        }

        public NowRequirement Draft(Now now, NowRequirement parentNr)
        {
            return MakeCopy(true, now, parentNr);
        }

        public NowRequirement Copy(Now now, NowRequirement parentNr)
        {
            return MakeCopy(false, now, parentNr);
        }

        private NowRequirement MakeCopy(bool asDraft, Now now, NowRequirement parentNr)
        {
            NowRequirement nowReq = (NowRequirement)this.MemberwiseClone();
            nowReq.NOWUUID = now.UUID;
            nowReq.ParentNowRequirement = parentNr;
            nowReq.UUID = Guid.NewGuid();
            nowReq.LastModifiedDate = DateTime.Now;
            nowReq.CreatedDate = DateTime.Now;
            nowReq.DeletedDate = null;
            nowReq.DeletedUser = null;
            nowReq.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            nowReq.CreatedUser = IdentityHelper.SignedInUser.Email;

            if (parentNr != null)
            {
                nowReq.PublishedStatus = parentNr.PublishedStatus;
            }
            else
            {
                nowReq.PublishedStatus = now.PublishedStatus;
            }

            if (asDraft)
            {
                nowReq.PublishedStatus = DataLib.PublishedStatus.Draft;
                if (nowReq.MasterUUID == null)
                {
                    nowReq.MasterUUID = MasterUUID == null ? UUID : MasterUUID;
                }
            }
            else
            {
                nowReq.MasterUUID = nowReq.UUID;
            }

            //deep copy of the NowRequirementPromptRules
            if (NowRequirementPromptRules != null && NowRequirementPromptRules.Where(x => x.DeletedDate == null).Where(x => x.ResultPromptRule.DeletedDate == null).Count() > 0)
            {
                var rules = new List<NowRequirementPromptRule>();
                foreach (var item in NowRequirementPromptRules.Where(x => x.DeletedDate == null).Where(x => x.ResultPromptRule.DeletedDate == null))
                {
                    var clone = asDraft ? item.Draft(nowReq) : item.Copy(nowReq);
                    rules.Add(clone);
                }
                nowReq.NowRequirementPromptRules = rules;
            }

            //deep copy of the NowRequirementText
            if (NowRequirementTextList != null)
            {
                nowReq.NowRequirementTextList = new List<NowRequirementText>();
                NowRequirementTextList.ForEach(x => nowReq.NowRequirementTextList.Add(x.Copy()));
            }

            //build a new collection of now requirements
            if (nowReq.NowRequirements != null)
            {
                var newReqs = new List<NowRequirement>();
                foreach (var nr in nowReq.NowRequirements.Where(x => x.DeletedDate == null))
                {                    
                    var newNr = asDraft ? nr.Draft(now, nowReq) : nr.Copy(now, nowReq);
                    newReqs.Add(newNr);
                }
                nowReq.NowRequirements = newReqs;
            }            

            return nowReq;
        }

        public bool SynchroniseChanges(NowRequirement source)
        {
            if (source == this)
            {
                //ResultDefinitionSequence must have changed as it is not included in the equality check
                ResultDefinitionSequence = source.ResultDefinitionSequence;
                LastModifiedDate = source.LastModifiedDate;
                LastModifiedUser = source.LastModifiedUser;
                return false;
            }

            Mandatory = source.Mandatory;
            PrimaryResult = source.PrimaryResult;
            ResultDefinitionSequence = source.ResultDefinitionSequence;
            DistinctResultTypes = source.DistinctResultTypes;
            NowRequirementTextList = source.NowRequirementTextList;
            LastModifiedDate = source.LastModifiedDate;
            LastModifiedUser = source.LastModifiedUser;
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NowRequirement);
        }

        public bool Equals(NowRequirement other)
        {
            return other != null &&
                //UUID == other.UUID &&
                Mandatory == other.Mandatory &&
                ExcludeFromMDE == other.ExcludeFromMDE &&
                PrimaryResult == other.PrimaryResult &&
                //discount the rank from any equality test
                //ResultDefinitionSequence == other.ResultDefinitionSequence &&

                DistinctResultTypes == other.DistinctResultTypes &&
                NowRequirementTextListsAreEqual(other.NowRequirementTextList);
        }

        private bool NowRequirementPromptRuleListsAreEqual(List<NowRequirementPromptRule> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.NowRequirementPromptRules == null || this.NowRequirementPromptRules.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.NowRequirementPromptRules == null)
            {
                return false;
            }

            //compare the two lists for delta entries
            var currentNotOther = NowRequirementPromptRules.Select(x=>x.MasterUUID == null ? x.UUID : x.MasterUUID).Except(other.Select(x => x.MasterUUID == null ? x.UUID : x.MasterUUID)).ToList();
            var otherNotCurrent = other.Select(x => x.MasterUUID == null ? x.UUID : x.MasterUUID).Except(NowRequirementPromptRules.Select(x => x.MasterUUID == null ? x.UUID : x.MasterUUID)).ToList();
            if (currentNotOther.Any() || otherNotCurrent.Any())
            {
                return false;
            }

            // compare the two lists for equality
            foreach (var item in NowRequirementPromptRules)
            {
                var id = item.MasterUUID == null ? item.UUID : item.MasterUUID;
                var matchedOther = other.First(x => x.UUID == id || x.MasterUUID == id);
                
                if(item.IsVariantData != matchedOther.IsVariantData)
                {
                    return false;
                }
            }

            return true;
        }

        public string ReportNowRequirementPromptRules()
        {
            if (NowRequirementPromptRules != null && NowRequirementPromptRules.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var nrPr in NowRequirementPromptRules.OrderBy(x => x.MasterUUID == null ? x.UUID : x.MasterUUID))
                {
                    sb.AppendLine(string.Format("{0} - Is Variant: {1}", nrPr.ResultPromptRule.ResultPrompt.Label, nrPr.IsVariantData));
                }
                return sb.Length == 0 ? "'Empty'" : sb.ToString();
            }
            return "'Empty'";
        }        

        private bool NowRequirementTextListsAreEqual(List<NowRequirementText> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.NowRequirementTextList == null || this.NowRequirementTextList.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.NowRequirementTextList == null)
            {
                return false;
            }

            //compare the two lists
            var currentNotOther = NowRequirementTextList.Except(other).ToList();
            var otherNotCurrent = other.Except(NowRequirementTextList).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }
        public string ReportNowRequirementText()
        {
            if (NowRequirementTextList != null && NowRequirementTextList.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var nrText in NowRequirementTextList.OrderBy(x => x.NowReference))
                {
                    sb.AppendLine(string.Format("{0} - {1}", nrText.NowReference, nrText.NowText));
                }
                return sb.Length == 0 ? "'Empty'" : sb.ToString();
            }
            return "'Empty'";
        }

        public int GetNowRequirementTextListHashCode()
        {
            int hc = 0;
            if (NowRequirementTextList != null)
                foreach (var t in NowRequirementTextList)
                    hc ^= t.GetHashCode();
            return hc;
        }

        public override int GetHashCode()
        {
            var hashCode = -1211238045;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(UUID);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(Mandatory);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(DistinctResultTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(ExcludeFromMDE);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(PrimaryResult);
            hashCode = hashCode * -1521134295 + GetNowRequirementTextListHashCode();

            //discount the ResultDefinitionSequence from any equality test
            //hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(ResultDefinitionSequence);

            return hashCode;
        }

        public static bool operator ==(NowRequirement nr1, NowRequirement nr2)
        {
            return EqualityComparer<NowRequirement>.Default.Equals(nr1, nr2);
        }

        public static bool operator !=(NowRequirement nr1, NowRequirement nr2)
        {
            return !(nr1 == nr2);
        }

        public string GetChangedProperties(NowRequirement other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = base.GetChangedProperties<NowRequirement>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.Append(simpleTypeChanges);
            }

            if (!NowRequirementTextListsAreEqual(other.NowRequirementTextList))
            {
                sb.AppendLine("From Now Requirement Text...");
                sb.AppendLine(ReportNowRequirementText());
                sb.AppendLine("To Now Requirement Text...");
                sb.AppendLine(other.ReportNowRequirementText());
            }

            //now deal with the prompt rules
            if (!NowRequirementPromptRuleListsAreEqual(other.NowRequirementPromptRules))
            {
                sb.AppendLine("From Now Requirement Prompt Rules...");
                sb.AppendLine(ReportNowRequirementPromptRules());
                sb.AppendLine("To Now Requirement Prompt Rules...");
                sb.AppendLine(other.ReportNowRequirementPromptRules());
            }

            return sb.ToString();
        }

    }

    [Serializable]
    public sealed class NowRequirementJSON
    {
        private NowRequirement nr;
        private DateTime? patchDate;

        public NowRequirementJSON(NowRequirement nr, DateTime? patchDate) 
        { 
            this.nr = nr; 
            this.patchDate = patchDate;
        }

        public Guid? id { get => nr.MasterUUID ?? nr.UUID; }

        public Guid? resultDefinitionId 
        {
            get
            {
                if (nr.ResultDefinition != null)
                {
                    return nr.ResultDefinition.MasterUUID ?? nr.ResultDefinition.UUID;
                }
                return null;
            }
        }

        public Guid? rootResultDefinitionId 
        {
            get
            {
                if (nr.RootParentNowRequirement != null)
                {
                    if (nr.RootParentNowRequirement.ResultDefinition != null)
                    {
                        return nr.RootParentNowRequirement.ResultDefinition.MasterUUID ?? nr.RootParentNowRequirement.ResultDefinition.UUID;
                    }
                }
                return null;
            }
        }

        public Guid? parentNowRequirementId { get => nr.ParentNowRequirementUUID; }

        public Guid? rootParentNowRequirementId { get => nr.RootParentNowRequirementUUID; }

        public Guid? nowId { get => nr.NOWUUID; }

        public bool mandatory { get => nr.Mandatory; }

        public bool primary { get => nr.PrimaryResult; }

        public bool excludeFromMDE { get => nr.ExcludeFromMDE; }

        public int? sequence { get => nr.ResultDefinitionSequence; }

        public bool distinctResultTypes { get => nr.DistinctResultTypes; }

        public List<NowRequirementPromptRuleJSON> prompts
        {
            get
            {
                if (nr.NowRequirementPromptRules == null) { return null; }
                if (nr.NowRequirementPromptRules.Where(x => x.DeletedDate == null).Count() == 0) { return null; }
                return nr.NowRequirementPromptRules.Where(x => x.DeletedDate == null).Select(x => new NowRequirementPromptRuleJSON(x)).ToList();
            } 
        }

        public List<NowRequirementText> nowRequirementText { get => nr.NowRequirementTextList; }

        public List<NowRequirementJSON> nowRequirements
        {
            get
            {
                if (nr.NowRequirements == null) { return null; }
                if (nr.NowRequirements.Where(x => x.DeletedDate == null).Count() == 0) { return null; }

                return nr.NowRequirements.Where(x => x.DeletedDate == null).Select(x => new NowRequirementJSON(x, patchDate)).ToList();
            }
        }

        //public string startDate { get => parentNowRequirementId == null && excludeHistoricalStartDates ? (nr.StartDate.HasValue && patchDate.HasValue && nr.StartDate.Value.Date < patchDate.Value.Date ? nr.StartDateJSON : null) : nr.StartDateJSON; }

    }

    [Serializable]
    public sealed class NowRequirement251
    {
        private NowRequirement nr;
        public NowRequirement251(NowRequirement nr) { this.nr = nr; }

        public Guid? id 
        { 
            get
            {
                if (nr.ResultDefinition != null)
                {
                    return nr.ResultDefinition.MasterUUID == null ? nr.ResultDefinition.UUID : nr.ResultDefinition.MasterUUID;
                }
                return null;
            }
        }

        public bool mandatory { get => nr.Mandatory; }

        public bool primary { get => nr.PrimaryResult; }

        public int? sequence { get => nr.ResultDefinitionSequence; }

        public string nowReference 
        { 
            get
            {
                if (nr.NowRequirementTextList != null && nr.NowRequirementTextList.Count>0)
                {
                    //251 supports only one set of dynamic text
                    return nr.NowRequirementTextList[0].NowReference;
                }
                return null;
            }
        }

        public string text
        {
            get
            {
                if (nr.NowRequirementTextList != null && nr.NowRequirementTextList.Count > 0)
                {
                    //251 supports only one set of dynamic text
                    return nr.NowRequirementTextList[0].NowText;
                }
                return null;
            }
        }

        public string welshText
        {
            get
            {
                if (nr.NowRequirementTextList != null && nr.NowRequirementTextList.Count > 0)
                {
                    //251 supports only one set of dynamic text
                    return nr.NowRequirementTextList[0].NowWelshText;
                }
                return null;
            }
        }

    }

    [Serializable]
    public sealed class NowRequirement251DataPatch
    {
        public Guid? id { get; set; }

        public bool mandatory { get; set; }

        public bool primary { get; set; }

        public int? sequence { get; set; }

        public string nowReference { get; set; }
        
        public string text { get; set; }
        
        public string welshText { get; set; }
        
    }
}