using Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataLib
{
    [Serializable]
    public sealed class NowSubscription : DataAudit
    {
        [JsonProperty]
        private List<Guid?> includedNOWIds { get; set; }

        [JsonProperty]
        private List<Guid?> excludedNOWIds { get; set; }

        [JsonProperty]
        private Guid? parentNowSubscriptionId { get; set; }

        //default constructor for serialisation
        public NowSubscription() { }

        /// <summary>
        /// Constructor for NOW or EDT or Informant/Court/Prison Register subscription
        /// </summary>
        /// <param name="isEDT"></param>
        public NowSubscription(bool isNow, bool isEDT, bool isInformantRegister, bool isCourtRegister, bool isPrisonCourtRegister)
        {
            UUID = Guid.NewGuid(); 
            CreatedDate = DateTime.Now; 
            LastModifiedDate = DateTime.Now;
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            NoRecipient = true;
            IsNow = isNow; 
            IsEDT = isEDT; 
            IsInformantRegister = isInformantRegister;
            IsCourtRegister = isCourtRegister; 
            IsPrisonCourtRegister = isPrisonCourtRegister;
            SubscriptionVocabulary = new SubscriptionVocabulary();

            if (isNow || isEDT)
            {
                IncludedNOWS = new List<Now>(); ExcludedNOWS = new List<Now>();
            }

            if (isEDT || isInformantRegister || isCourtRegister || isPrisonCourtRegister)
            {
                IsEmail = true;
                IsForDistribution = true;
            }

            SetName();
        }

        /// <summary>
        /// Constructor for child now subscription
        /// </summary>
        /// <param name="parent"></param>
        public NowSubscription(NowSubscription parent)
        {
            UUID = Guid.NewGuid(); 
            CreatedDate = DateTime.Now; 
            LastModifiedDate = DateTime.Now;
            CreatedUser = IdentityHelper.SignedInUser.Email;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            NowRecipient = new NowRecipient(this); 
            IncludedNOWS = new List<Now>(); 
            ExcludedNOWS = new List<Now>();
            IsNow = parent.IsNow;
            IsEDT = parent.IsEDT;
            IsForDistribution = parent.IsForDistribution;
            IsEmail = parent.IsEmail;
            ParentNowSubscription = parent;
            SubscriptionVocabulary = new SubscriptionVocabulary();
            PublishedStatus = parent.PublishedStatus;
            PublicationTags = new List<string>(parent.PublicationTags);
            SetName();
        }

        private void SetName()
        {
            Name = string.Format("New {0} Subscription", IsNow ? "Now" : (IsEDT ? "EDT" : (IsInformantRegister ? "Informant Register" : (IsCourtRegister ? "Court Register" : (IsPrisonCourtRegister ? "Prison Court Register" : "Unknown")))));
        }

        public string Name { get; set; }

        public bool IsNow { get; set; }

        public bool IsEDT { get; set; }

        public bool IsInformantRegister { get; set; }

        public bool IsCourtRegister { get; set; }

        public bool IsPrisonCourtRegister { get; set; }

        public NowRecipient NowRecipient { get; set; }

        public SubscriptionVocabulary SubscriptionVocabulary { get; set; }

        public bool IsFirstClassLetter { get; set; }

        public bool IsSecondClassLetter { get; set; }

        public bool IsCPSNotificationAPI { get; set; }

        public bool IsEmail { get; set; }

        public bool IsForDistribution { get; set; }

        public bool ApplySubscriptionRules { get; set; }

        #region Where is recipient sourced from?

        public bool CaseRecipient { get; set; }

        public bool SubscriptionRecipient { get; set; }

        public bool ResultRecipient { get; set; }

        public bool NoRecipient { get; set; }

        #endregion Where is recipient sourced from?

        #region case recipient details

        public bool CaseRecipientProsecution { get; set; }

        public bool CaseRecipientDefendant { get; set; }

        public bool CaseRecipientDefenceOrganisation { get; set; }

        public bool CaseRecipientCustodyLocation { get; set; }

        public bool CaseRecipientParentGuardian { get; set; }

        public bool CaseRecipientApplicant { get; set; }

        public bool CaseRecipientRespondent { get; set; }

        public bool CaseRecipientThirdParty { get; set; }

        #endregion case recipient details

        /// <summary>
        /// The user groups that apply to this subscription for now variants
        /// e.e. defence user group for the defence now variant, etc
        /// </summary>
        public List<string> UserGroupVariants { get; set; }

        [JsonIgnore]
        public string[] UserGroupVariantsStringArray
        {
            get
            {
                //if we dont have user group dont serialise them
                if (UserGroupVariants == null || UserGroupVariants.Count == 0) { return null; }

                //serialise as a string array
                return UserGroupVariants.ToArray();
            }
        }

        [JsonIgnore]
        public List<NowSubscription> ChildNowSubscriptions { get; set; }

        /// <summary>
        /// Informant Register
        /// </summary>
        public string InformantCode { get; set; }

        /// <summary>
        /// Court Register YOTs Organisation
        /// </summary>
        public string YOTsCode { get; set; }

        /// <summary>
        /// Court Register Selected Court Houses For Yot Org
        /// </summary>
        public List<string> SelectedCourtHouses { get; set; }

        [JsonIgnore]
        public string[] SelectedCourtHousesStringArray
        {
            get
            {
                //if we dont have court houses dont serialise them
                if (SelectedCourtHouses == null || SelectedCourtHouses.Count == 0) { return null; }

                //serialise as a string array
                return SelectedCourtHouses.ToArray();
            }
        }

        [JsonIgnore]
        public NowSubscription ParentNowSubscription { get; set; }

        public Guid? ParentNowSubscriptionId
        {
            get
            {
                if (ParentNowSubscription == null) 
                { 
                    if (parentNowSubscriptionId != null) { return parentNowSubscriptionId; }
                    return null; 
                }
                parentNowSubscriptionId = null;
                return ParentNowSubscription.UUID;
            }
            set
            {
                parentNowSubscriptionId = value;
            }
        }

        [JsonIgnore]
        public List<Now> IncludedNOWS { get; set; }

        public List<Guid?> IncludedNOWIds
        {
            get
            {
                if (IncludedNOWS == null || IncludedNOWS.Where(x => x.UUID != null).Count() == 0) 
                {
                    if (includedNOWIds != null) { return includedNOWIds; }
                    return null; 
                }

                includedNOWIds = null;

                return IncludedNOWS.Where(x => x.UUID != null).Select(x => x.UUID).ToList();
            }
            set
            {
                includedNOWIds = value;
            }
        }

        [JsonIgnore]
        public List<Now> ExcludedNOWS { get; set; }

        public List<Guid?> ExcludedNOWIds
        {
            get
            {
                if (ExcludedNOWS == null || ExcludedNOWS.Where(x => x.UUID != null).Count() == 0) 
                {
                    if (excludedNOWIds != null) { return excludedNOWIds; }
                    return null; 
                }

                excludedNOWIds = null;

                return ExcludedNOWS.Where(x => x.UUID != null).Select(x => x.UUID).ToList();
            }
            set
            {
                excludedNOWIds = value;
            }
        }        

        public string EmailTemplateName { get; set; }

        public string BilingualEmailTemplateName { get; set; }

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

        public NowSubscription Copy()
        {
            return MakeCopy(false);
        }

        public NowSubscription Draft()
        {
            return MakeCopy(true);
        }

        private string IncludedNowsReport(List<Now> other)
        {
            List<Now> currentNotOther = new List<Now>();
            List<Now> otherNotCurrent = new List<Now>();

            if (other == null || other.Count == 0)
            {
                if (IncludedNOWS == null || IncludedNOWS.Count == 0)
                {
                    return null;
                }

                currentNotOther.AddRange(IncludedNOWS);
            }
            else if(IncludedNOWS == null)
            {
                otherNotCurrent.AddRange(other);
            }
            else
            {
                var currentNotOtherIds = IncludedNOWS.Select(x=>x.MasterUUID ?? x.UUID).Except(other.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                currentNotOther.AddRange(from now in IncludedNOWS
                                         join nowId in currentNotOtherIds on (now.MasterUUID ?? now.UUID) equals nowId
                                         select now);

                var otherNotCurrentIds = other.Select(x => x.MasterUUID ?? x.UUID).Except(IncludedNOWS.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                otherNotCurrent.AddRange(from now in other
                                         join nowId in otherNotCurrentIds on (now.MasterUUID ?? now.UUID) equals nowId
                                         select now);
            }

            //generate a report
            if (currentNotOther.Any() || otherNotCurrent.Any())
            {
                var sb = new StringBuilder();                
                if (otherNotCurrent.Any())
                {
                    sb.AppendLine(string.Format("New Included {0}...", IsEDT ? "Edts" : "Nows"));
                    foreach (var now in otherNotCurrent.OrderBy(x => x.Name))
                    {
                        sb.AppendLine(now.Name);
                    }
                }
                if (currentNotOther.Any())
                {
                    sb.AppendLine(string.Format("Deleted Included {0}...", IsEDT ? "Edts" : "Nows"));
                    foreach (var now in currentNotOther.OrderBy(x => x.Name))
                    {
                        sb.AppendLine(now.Name);
                    }
                }
                return sb.ToString();
            }
            return null;
        }

        private string ExcludedNowsReport(List<Now> other)
        {
            List<Now> currentNotOther = new List<Now>();
            List<Now> otherNotCurrent = new List<Now>();

            if (other == null || other.Count == 0)
            {
                if (ExcludedNOWS == null || ExcludedNOWS.Count == 0)
                {
                    return null;
                }

                currentNotOther.AddRange(ExcludedNOWS);
            }
            else if (ExcludedNOWS == null)
            {
                otherNotCurrent.AddRange(other);
            }
            else
            {
                var currentNotOtherIds = ExcludedNOWS.Select(x => x.MasterUUID ?? x.UUID).Except(other.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                currentNotOther.AddRange(from now in ExcludedNOWS
                                         join nowId in currentNotOtherIds on (now.MasterUUID ?? now.UUID) equals nowId
                                         select now);

                var otherNotCurrentIds = other.Select(x => x.MasterUUID ?? x.UUID).Except(ExcludedNOWS.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                otherNotCurrent.AddRange(from now in other
                                         join nowId in otherNotCurrentIds on (now.MasterUUID ?? now.UUID) equals nowId
                                         select now);
            }

            //generate a report
            if (currentNotOther.Any() || otherNotCurrent.Any())
            {
                var sb = new StringBuilder();
                if (currentNotOther.Any())
                {
                    sb.AppendLine(string.Format("Deleted Excluded {0}...", IsEDT ? "Edts" : "Nows"));                    
                    foreach (var now in currentNotOther)
                    {
                        sb.AppendLine(now.Name);
                    }
                }
                if (otherNotCurrent.Any())
                {
                    sb.AppendLine(string.Format("New Excluded {0}...", IsEDT ? "Edts" : "Nows"));
                    foreach (var now in otherNotCurrent)
                    {
                        sb.AppendLine(now.Name);
                    }
                }

                return sb.ToString();
            }
            return null;
        }

        public string GetChangedProperties(NowSubscription other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = GetChangedProperties<NowSubscription>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.Append(simpleTypeChanges);
            }

            //add included nows changes
            var includedReport = IncludedNowsReport(other.IncludedNOWS);
            if (!string.IsNullOrEmpty(includedReport))
            {
                sb.AppendLine(includedReport);
            }

            //add excluded nows changes
            var excludedReport = ExcludedNowsReport(other.ExcludedNOWS);
            if (!string.IsNullOrEmpty(excludedReport))
            {
                sb.AppendLine(excludedReport);
            }

            //add recipient simple changes
            AppendRecipientChanges(sb, other);

            //add vocabulary simple changes
            AppendSubscriptionVocabularyChanges(sb, other);

            return Regex.Replace(sb.ToString().Trim(), @"(\r\n){2,}", "\r\n\r\n");
        }

        private void AppendRecipientChanges(StringBuilder sb, NowSubscription other)
        {
            if (NowRecipient == null && other.NowRecipient == null)
            {
                return;
            }

            if (NowRecipient != null && other.NowRecipient == null)
            {
                sb.AppendLine("Recipient details removed");
                return;
            }

            if (NowRecipient == null && other.NowRecipient != null)
            {
                sb.AppendLine("Recipient details added");
                return;
            }

            var changes = NowRecipient.GetChangedProperties(other.NowRecipient);
            if (!string.IsNullOrEmpty(changes))
            {
                sb.AppendLine(changes);
            }
        }

        private void AppendSubscriptionVocabularyChanges(StringBuilder sb, NowSubscription other)
        {
            if (SubscriptionVocabulary == null && other.SubscriptionVocabulary == null)
            {
                return;
            }

            if (SubscriptionVocabulary != null && other.SubscriptionVocabulary == null)
            {
                sb.AppendLine("Vocabulary details removed");
                return;
            }

            if (SubscriptionVocabulary == null && other.SubscriptionVocabulary != null)
            {
                sb.AppendLine("Vocabulary details added");
                return;
            }

            var changes = SubscriptionVocabulary.GetChangedProperties(other.SubscriptionVocabulary);
            if (!string.IsNullOrEmpty(changes))
            {
                sb.AppendLine(changes);
            }
        }

        public NowSubscription MakeCopy(bool asDraft)
        {
            NowSubscription res = (NowSubscription)MemberwiseClone();
            res.UUID = Guid.NewGuid();
            res.CreatedDate = DateTime.Now;
            res.LastModifiedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

            //Copy the child objects
            if (res.NowRecipient != null)
            {
                res.NowRecipient = res.NowRecipient.Copy();
                res.NowRecipient.NowSubscription = res;
            }
            if (res.SubscriptionVocabulary != null)
            {
                res.SubscriptionVocabulary = res.SubscriptionVocabulary.Copy();
            }

            //create a new collection for any child subscriptions
            res.ChildNowSubscriptions = new List<NowSubscription>();
            if (ChildNowSubscriptions != null)
            {
                res.ChildNowSubscriptions.AddRange(ChildNowSubscriptions.Where(x => x.DeletedDate == null));
            }

            //create a new collection for included nows
            res.IncludedNOWS = new List<Now>();
            if(IncludedNOWS != null)
            {
                res.IncludedNOWS.AddRange(IncludedNOWS.Where(x => x.DeletedDate == null));
            }

            //create a new collection for excluded nows
            res.ExcludedNOWS = new List<Now>();
            if (ExcludedNOWS != null)
            {
                res.ExcludedNOWS.AddRange(ExcludedNOWS.Where(x => x.DeletedDate == null));
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
                res.Name = res.Name + " - Copy";
            }

            return res;
        }        

        public void SynchroniseChanges(NowSubscription source)
        {
            Name = source.Name;
            IsNow = source.IsNow;
            IsEDT = source.IsEDT;
            IsInformantRegister = source.IsInformantRegister;
            IsCourtRegister = source.IsCourtRegister;
            IsPrisonCourtRegister = source.IsPrisonCourtRegister;
            ApplySubscriptionRules = source.ApplySubscriptionRules;
            NowRecipient = source.NowRecipient;
            SubscriptionVocabulary = source.SubscriptionVocabulary;
            IsFirstClassLetter = source.IsFirstClassLetter;
            IsSecondClassLetter = source.IsSecondClassLetter;
            IsEmail = source.IsEmail;
            IsForDistribution = source.IsForDistribution;
            UserGroupVariants = source.UserGroupVariants;
            ChildNowSubscriptions = source.ChildNowSubscriptions;
            InformantCode = source.InformantCode;
            YOTsCode = source.YOTsCode;
            SelectedCourtHouses = source.SelectedCourtHouses;
            ParentNowSubscription = source.ParentNowSubscription;
            IncludedNOWS = source.IncludedNOWS;
            ExcludedNOWS = source.ExcludedNOWS;
            EmailTemplateName = source.EmailTemplateName;
            BilingualEmailTemplateName = source.BilingualEmailTemplateName;
        }
    }
}