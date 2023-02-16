using Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLib
{
    [Serializable]
    public sealed class Now : DataAudit, IEquatable<Now>
    {
        public Now (bool isEDT = false)
        {
            UUID = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            NowRequirements = new List<NowRequirement>();
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            Jurisdiction = "Both";

            if (isEDT)
            {
                Name = "New EDT";
                TemplateName = "EDTs";
                IsEDT = true;
            }
            else
            {
                Name = "New NOW";
                TemplateName = "NoticeOrderWarrants";
                BilingualTemplateName = "BilingualNoticeOrderWarrants";
                IsStorageRequired = true;
            }
        }

        public string Name { get; set; }

        public string WelshNowName { get; set; }

        public string TemplateName { get; set; }

        public string SubTemplateName { get; set; }

        public string BilingualTemplateName { get; set; }

        public string Jurisdiction { get; set; }

        [JsonIgnore]
        public string JurisdictionLetter
        {
            get
            {
                if (string.IsNullOrEmpty(Jurisdiction))
                {
                    return null;
                }

                return Jurisdiction[0].ToString().ToUpper();
            }
        }

        public int? Rank { get; set; }

        public List<NowText> NowTextList { get; set; }

        public int? UrgentTimeLimitInMinutes { get; set; }

        public bool RemotePrintingRequired { get; set; }

        public bool Financial { get; set; }

        public bool IsStorageRequired { get; set; }

        public bool IsEDT { get; set; }

        public bool IncludeAllResults { get; set; }

        public bool IncludeCaseMarkers { get; set; }

        public bool IncludePNCID { get; set; }

        public bool IncludeConvictionStatus { get; set; }

        public bool IncludeNationality { get; set; }

        public bool IncludeDefendantASN { get; set; }

        public bool IncludeDefendantMobileNumber { get; set; }

        public bool IncludeDefendantLandlineNumber { get; set; }

        public bool IncludeDefendantNINO { get; set; }

        public bool IncludeDefendantEthnicity { get; set; }

        public bool IncludeDefendantGender { get; set; }

        public bool IncludeSolicitorsNameAddress { get; set; }

        public bool IncludeDVLAOffenceCode { get; set; }

        public bool IncludeDriverNumber { get; set; }

        public bool IncludePlea { get; set; }

        public bool IncludeVehicleRegistration { get; set; }

        public bool ApplyMDEOffenceFiltering { get; set; }

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

        [JsonIgnore]
        public List<NowRequirement> NowRequirements { get; set; }

        public List<NowRequirement> AllNowRequirements
        {
            get
            {
                var res = new List<NowRequirement>();

                if (NowRequirements != null)
                {
                    FlattenNowRequirements(NowRequirements, res);
                }

                return res;
            }
        }

        private void FlattenNowRequirements(List<NowRequirement> nowRequirements, List<NowRequirement> flattened)
        {
            if (nowRequirements == null) { return; }

            flattened.AddRange(nowRequirements);
            foreach (var nr in nowRequirements)
            {
                FlattenNowRequirements(nr.NowRequirements, flattened);
            }
        }

        public string EmailTemplateName { get; set; }

        public string BilingualEmailTemplateName { get; set; }

        public bool SynchroniseChanges(Now source)
        {
            if (source == this)
            {
                //rank must have changed as it is not included in the equality check
                Rank = source.Rank;
                LastModifiedDate = source.LastModifiedDate;
                LastModifiedUser = source.LastModifiedUser;
                return false;
            }
            
            Name = source.Name;
            WelshNowName = source.WelshNowName;
            TemplateName = source.TemplateName;
            SubTemplateName = source.SubTemplateName;
            Financial = source.Financial;
            BilingualTemplateName = source.BilingualTemplateName;
            Jurisdiction = source.Jurisdiction;
            Rank = source.Rank;
            NowTextList = source.NowTextList;
            UrgentTimeLimitInMinutes = source.UrgentTimeLimitInMinutes;
            RemotePrintingRequired = source.RemotePrintingRequired;
            IsStorageRequired = source.IsStorageRequired;
            IsEDT = source.IsEDT;
            IncludeAllResults = source.IncludeAllResults;
            IncludeCaseMarkers = source.IncludeCaseMarkers;
            IncludePNCID = source.IncludePNCID;
            IncludeConvictionStatus = source.IncludeConvictionStatus;
            IncludeNationality = source.IncludeNationality;
            IncludeDefendantASN = source.IncludeDefendantASN;
            IncludeDefendantMobileNumber = source.IncludeDefendantMobileNumber;
            IncludeDefendantLandlineNumber = source.IncludeDefendantLandlineNumber;
            IncludeDefendantNINO = source.IncludeDefendantNINO;
            IncludeDefendantEthnicity = source.IncludeDefendantEthnicity;
            IncludeDefendantGender = source.IncludeDefendantGender;
            IncludeSolicitorsNameAddress = source.IncludeSolicitorsNameAddress;
            IncludeDVLAOffenceCode = source.IncludeDVLAOffenceCode;
            IncludeDriverNumber = source.IncludeDriverNumber;
            IncludePlea = source.IncludePlea;
            IncludeVehicleRegistration = source.IncludeVehicleRegistration;
            EmailTemplateName = source.EmailTemplateName;
            BilingualEmailTemplateName = source.BilingualEmailTemplateName;
            LastModifiedDate = source.LastModifiedDate;
            LastModifiedUser = source.LastModifiedUser;
            return true;
        }        

        public override bool Equals(object obj)
        {
            return Equals(obj as Now);
        }

        public bool Equals(Now other)
        {
            return other != null &&
                //UUID == other.UUID &&
                string.Equals(Name, other.Name, StringComparison.InvariantCulture) &&
                string.Equals(WelshNowName, other.WelshNowName, StringComparison.InvariantCulture) &&
                string.Equals(TemplateName, other.TemplateName, StringComparison.InvariantCulture) &&
                string.Equals(SubTemplateName, other.SubTemplateName, StringComparison.InvariantCulture) &&
                string.Equals(BilingualTemplateName, other.BilingualTemplateName, StringComparison.InvariantCulture) &&
                string.Equals(Jurisdiction, other.Jurisdiction, StringComparison.InvariantCulture) &&
                Financial == other.Financial &&

                //discount the rank from any equality test
                //Rank == other.Rank &&

                NowTextListsAreEqual(other.NowTextList) &&
                UrgentTimeLimitInMinutes == other.UrgentTimeLimitInMinutes &&
                RemotePrintingRequired == other.RemotePrintingRequired &&
                IsStorageRequired == other.IsStorageRequired &&
                IsEDT == other.IsEDT &&
                IncludeAllResults == other.IncludeAllResults &&
                IncludeCaseMarkers == other.IncludeCaseMarkers &&
                IncludePNCID == other.IncludePNCID &&
                IncludeConvictionStatus == other.IncludeConvictionStatus &&
                IncludeNationality == other.IncludeNationality &&
                IncludeDefendantASN == other.IncludeDefendantASN &&
                IncludeDefendantMobileNumber == other.IncludeDefendantMobileNumber &&
                IncludeDefendantLandlineNumber == other.IncludeDefendantLandlineNumber &&
                IncludeDefendantNINO == other.IncludeDefendantNINO &&
                IncludeDefendantEthnicity == other.IncludeDefendantEthnicity &&
                IncludeDefendantGender == other.IncludeDefendantGender &&
                IncludeSolicitorsNameAddress == other.IncludeSolicitorsNameAddress &&
                IncludeDVLAOffenceCode == other.IncludeDVLAOffenceCode &&
                IncludeDriverNumber == other.IncludeDriverNumber &&
                IncludePlea == other.IncludePlea &&
                IncludeVehicleRegistration == other.IncludeVehicleRegistration &&
                string.Equals(EmailTemplateName, other.EmailTemplateName, StringComparison.InvariantCulture) &&
                string.Equals(BilingualEmailTemplateName, other.BilingualEmailTemplateName, StringComparison.InvariantCulture);
        }

        private bool NowTextListsAreEqual(List<NowText> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.NowTextList == null || this.NowTextList.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.NowTextList == null)
            {
                return false;
            }

            //compare the two lists
            var currentNotOther = NowTextList.Except(other).ToList();
            var otherNotCurrent = other.Except(NowTextList).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }

        public string ReportNowText()
        {
            if (NowTextList != null && NowTextList.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var nowText in NowTextList.OrderBy(x => x.Reference))
                {
                    sb.AppendLine(string.Format("{0} - {1}", nowText.Reference, nowText.Text));
                }
                return sb.Length == 0 ? "'Empty'" : sb.ToString();
            }
            return "'Empty'";
        }

        public int GetNowTextListHashCode()
        {
            int hc = 0;
            if (NowTextList != null)
                foreach (var t in NowTextList)
                    hc ^= t.GetHashCode();
            return hc;
        }

        public override int GetHashCode()
        {
            var hashCode = -1211238045;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(UUID);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(MasterUUID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WelshNowName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TemplateName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SubTemplateName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BilingualTemplateName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Jurisdiction);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(Financial);

            //discount the rank from any equality test
            //hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(Rank);

            hashCode = hashCode * -1521134295 + GetNowTextListHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(UrgentTimeLimitInMinutes);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(RemotePrintingRequired);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsStorageRequired);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsEDT);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeAllResults);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeCaseMarkers);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludePNCID);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeConvictionStatus);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeNationality);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeDefendantASN);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeDefendantMobileNumber);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeDefendantLandlineNumber);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeDefendantNINO);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeDefendantEthnicity);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeDefendantGender);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeSolicitorsNameAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeDVLAOffenceCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeDriverNumber);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludePlea);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IncludeVehicleRegistration);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(EmailTemplateName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BilingualEmailTemplateName);
            return hashCode;
        }

        public static bool operator ==(Now now1, Now now2)
        {
            return EqualityComparer<Now>.Default.Equals(now1, now2);
        }

        public static bool operator !=(Now now1, Now now2)
        {
            return !(now1 == now2);
        }

        public Now Copy()
        {
            return MakeCopy(false);
        }

        public Now Draft()
        {
            return MakeCopy(true);
        }

        private Now MakeCopy(bool asDraft)
        {
            Now res = (Now)this.MemberwiseClone();

            res.UUID = Guid.NewGuid();
            res.CreatedDate = DateTime.Now;
            res.LastModifiedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

            //remove the publication tags
            res.PublicationTags = new List<string>();


            //make new now requirements for each of the child now requirements
            if (res.NowRequirements != null)
            {
                var newReqs = new List<NowRequirement>();
                foreach (var nr in res.NowRequirements.Where(x => x.DeletedDate == null))
                {
                    var newNr = asDraft ? nr.Draft(res, null) : nr.Copy(res, null);
                    newReqs.Add(newNr);
                }
                res.NowRequirements = newReqs;
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
                res.Name = res.Name + " - Copy";
            }

            return res;
        }

        public string GetChangedProperties(Now other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = GetChangedProperties<Now>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.Append(simpleTypeChanges);
            }

            if (!NowTextListsAreEqual(other.NowTextList))
            {
                sb.AppendLine("From Now Text...");
                sb.AppendLine(ReportNowText());
                sb.AppendLine("To Now Text...");
                sb.AppendLine(other.ReportNowText());
            }

            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class NowJSON
    {
        private Now now;
        private DateTime? patchDate;

        public NowJSON(Now now, DateTime? patchDate) 
        { 
            this.now = now; 
            this.patchDate = patchDate;
        }

        public Guid? id { get => now.MasterUUID ?? now.UUID; }

        public string name { get => now.Name; }

        public string welshName { get => now.WelshNowName; }

        public string templateName { get => now.TemplateName; }

        public string subTemplateName { get => now.SubTemplateName; }

        public string bilingualTemplateName { get => now.BilingualTemplateName; }

        public string jurisdiction { get => now.JurisdictionLetter; }

        public bool financial { get => now.Financial; }

        public int? rank { get => now.Rank; }

        public List<NowText> nowTextList { get => now.NowTextList; }

        public int? urgentTimeLimitInMinutes { get => now.UrgentTimeLimitInMinutes; }

        public bool remotePrintingRequired { get => now.RemotePrintingRequired; }

        public bool storageRequired { get => now.IsStorageRequired; }

        public bool isEDT { get => now.IsEDT; }

        public bool includeAllResults { get => now.IncludeAllResults; }

        public bool includeCaseMarkers { get => now.IncludeCaseMarkers; }

        public bool includePNCID { get => now.IncludePNCID; }

        public bool includeConvictionStatus { get => now.IncludeConvictionStatus; }

        public bool includeNationality { get => now.IncludeNationality; }

        public bool includeDefendantASN { get => now.IncludeDefendantASN; }

        public bool includeDefendantMobileNumber { get => now.IncludeDefendantMobileNumber; }

        public bool includeDefendantLandlineNumber { get => now.IncludeDefendantLandlineNumber; }

        public bool includeDefendantNINO { get => now.IncludeDefendantNINO; }

        public bool includeDefendantEthnicity { get => now.IncludeDefendantEthnicity; }

        public bool includeDefendantGender { get => now.IncludeDefendantGender; }

        public bool includeSolicitorsNameAddress { get => now.IncludeSolicitorsNameAddress; }

        public bool includeDVLAOffenceCode { get => now.IncludeDVLAOffenceCode; }

        public bool includeDriverNumber { get => now.IncludeDriverNumber; }

        public bool includePlea { get => now.IncludePlea; }

        public bool includeVehicleRegistration { get => now.IncludeVehicleRegistration; }

        public bool applyMDEOffenceFiltering { get => now.ApplyMDEOffenceFiltering; }        

        public List<NowRequirementJSON> nowRequirements
        {
            get
            {
                if (now.NowRequirements == null) { return null; }
                if (now.NowRequirements.Where(x => x.DeletedDate == null).Count() == 0) { return null; }

                return now.NowRequirements.Where(x => x.DeletedDate == null).Select(x => new NowRequirementJSON(x, patchDate)).ToList();
            }
        }

        public string emailTemplateName { get => now.EmailTemplateName; }

        public string bilingualEmailTemplateName { get => now.BilingualEmailTemplateName; }

        public string startDate { get => now.StartDate.HasValue && patchDate.HasValue && now.StartDate.Value.Date < patchDate.Value.Date ? null : now.StartDateJSON; }

    }

    [Serializable]
    public sealed class Now251
    {
        private Now now;
        private bool excludeHistoricalStartDates;
        private DateTime? patchDate;

        public Now251(Now now, bool excludeHistoricalStartDates, DateTime? patchDate) { this.now = now; this.excludeHistoricalStartDates = excludeHistoricalStartDates; this.patchDate = patchDate; }

        public Guid? id { get => now.UUID; }

        public string name { get => now.Name; }

        public string welshName { get => now.WelshNowName; }

        public string templateName { get => now.TemplateName; }

        public string subTemplateName { get => now.SubTemplateName; }

        public string bilingualTemplateName { get => now.BilingualTemplateName; }

        public string jurisdiction { get => now.JurisdictionLetter; }

        public int? rank { get => now.Rank; }

        public string text
        {
            get
            {
                if (now.NowTextList != null && now.NowTextList.Count > 0)
                {
                    //251 supports only one set of text
                    return now.NowTextList[0].Text;
                }
                return null;
            }
        }

        public string welshText
        {
            get
            {
                if (now.NowTextList != null && now.NowTextList.Count > 0)
                {
                    //251 supports only one set of text
                    return now.NowTextList[0].WelshText;
                }
                return null;
            }
        }

        public int? urgentTimeLimitInMinutes { get => now.UrgentTimeLimitInMinutes; }

        public bool remotePrintingRequired { get => now.RemotePrintingRequired; }

        public List<NowRequirement251> resultDefinitions
        {
            get
            {
                if (now.NowRequirements == null || now.NowRequirements.Count == 0)
                {
                    return null;
                }

                var res = new List<NowRequirement251>();
                now.NowRequirements.ForEach(x => res.Add(new NowRequirement251(x)));
                return res;
            }
        }

        public string startDate { get => excludeHistoricalStartDates ? (now.StartDate.HasValue && patchDate.HasValue && now.StartDate.Value.Date < patchDate.Value.Date ? now.StartDateJSON : null) : now.StartDateJSON; }

        public string endDate { get => excludeHistoricalStartDates ? (now.StartDate.HasValue && patchDate.HasValue && now.StartDate.Value.Date < patchDate.Value.Date ? now.EndDateJSON : null) : now.EndDateJSON; }
    }

    [Serializable]
    public sealed class Now251DataPatch
    {
        public Guid? id { get; set; }

        public string name { get; set; }

        public string welshName { get; set; }

        public string templateName { get; set; }

        public string subTemplateName { get; set; }

        public string bilingualTemplateName { get; set; }

        public string jurisdiction { get; set; }

        public int? rank { get; set; }

        public string text { get; set; }

        public string welshText { get; set; }

        public int? urgentTimeLimitInMinutes { get; set; }

        public bool remotePrintingRequired { get; set; }

        public List<NowRequirement251DataPatch> resultDefinitions { get; set; }

    }
}