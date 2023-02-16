using Newtonsoft.Json;
using System;
using System.Text;

namespace DataLib
{
    [Serializable]
    public sealed class NowRecipient : Audittable
    {
        //default constructor for serialisation
        public NowRecipient() { }

        public NowRecipient(NowSubscription subscription) { NowSubscription = subscription; }

        [JsonIgnore]
        public NowSubscription NowSubscription { get; set; }

        public bool RecipientFromCase { get; set; }

        public bool RecipientFromSubscription { get; set; }

        public bool RecipientFromResults { get; set; }

        public bool IsApplyDefenceOrganisationDetails { get; set; }

        public bool IsApplyParentGuardianDetails { get; set; }

        public bool IsApplyDefendantDetails { get; set; }

        public bool IsApplyDefendantCustodyDetails { get; set; }

        public bool IsApplyProsecutionAuthorityDetails { get; set; }

        /// <summary>
        /// Application applicant
        /// </summary>
        public bool IsApplyApplicantDetails { get; set; }       

        /// <summary>
        /// Application respondent
        /// </summary>
        public bool IsApplyRespondentDetails { get; set; }

        /// <summary>
        /// Application 3rd parties
        /// </summary>
        public bool IsApplyThirdPartyDetails { get; set; }

        #region Name
        public string Title { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string OrganisationName { get; set; }

        public ResultPromptRule TitleResultPrompt { get; set; }

        public ResultPromptRule FirstNameResultPrompt { get; set; }

        public ResultPromptRule MiddleNameResultPrompt { get; set; }

        public ResultPromptRule LastNameResultPrompt { get; set; }

        public ResultPromptRule OrganisationNameResultPrompt { get; set; }

        [JsonIgnore]
        [UnAudittable]
        public Guid? TitleResultPromptId
        {
            get
            {
                if (TitleResultPrompt == null) { return null; }
                return TitleResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? FirstNameResultPromptId
        {
            get
            {
                if (FirstNameResultPrompt == null) { return null; }
                return FirstNameResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? MiddleNameResultPromptId
        {
            get
            {
                if (MiddleNameResultPrompt == null) { return null; }
                return MiddleNameResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? LastNameResultPromptId
        {
            get
            {
                if (LastNameResultPrompt == null) { return null; }
                return LastNameResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? OrganisationNameResultPromptId
        {
            get
            {
                if (OrganisationNameResultPrompt == null) { return null; }
                return OrganisationNameResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string TitleResultPromptReference
        {
            get
            {
                if (TitleResultPrompt == null || TitleResultPrompt.ResultPrompt == null) { return null; }
                if (TitleResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(TitleResultPrompt.ResultPrompt);
                    return string.Format("{0}Title", referenceName);
                }
                return TitleResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string FirstNameResultPromptReference
        {
            get
            {
                if (FirstNameResultPrompt == null || FirstNameResultPrompt.ResultPrompt == null) { return null; }
                if (FirstNameResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(FirstNameResultPrompt.ResultPrompt);
                    return string.Format("{0}FirstName", referenceName);
                }
                return FirstNameResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string MiddleNameResultPromptReference
        {
            get
            {
                if (MiddleNameResultPrompt == null || MiddleNameResultPrompt.ResultPrompt == null) { return null; }
                if (MiddleNameResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(MiddleNameResultPrompt.ResultPrompt);
                    return string.Format("{0}MiddleName", referenceName);
                }
                return MiddleNameResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string LastNameResultPromptReference
        {
            get
            {
                if (LastNameResultPrompt == null || LastNameResultPrompt.ResultPrompt == null) { return null; }
                if (LastNameResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(LastNameResultPrompt.ResultPrompt);
                    return string.Format("{0}LastName", referenceName);
                }
                return LastNameResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string OrganisationNameResultPromptReference
        {
            get
            {
                if (OrganisationNameResultPrompt == null || OrganisationNameResultPrompt.ResultPrompt == null) { return null; }
                if (OrganisationNameResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(OrganisationNameResultPrompt.ResultPrompt);
                    return string.Format("{0}OrganisationName", referenceName);
                }
                return OrganisationNameResultPrompt.ResultPrompt.PromptReference;
            }
        }

        #endregion Name

        #region Address
        
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        public string Address4 { get; set; }

        public string Address5 { get; set; }

        public string PostCode { get; set; }

        public string EmailAddress1 { get; set; }

        public string EmailAddress2 { get; set; }

        public ResultPromptRule Address1ResultPrompt { get; set; }
                
        public ResultPromptRule Address2ResultPrompt { get; set; }

        public ResultPromptRule Address3ResultPrompt { get; set; }

        public ResultPromptRule Address4ResultPrompt { get; set; }

        public ResultPromptRule Address5ResultPrompt { get; set; }

        public ResultPromptRule PostCodeResultPrompt { get; set; }

        public ResultPromptRule EmailAddress1ResultPrompt { get; set; }

        public ResultPromptRule EmailAddress2ResultPrompt { get; set; }

        [JsonIgnore]
        [UnAudittable]
        public Guid? Address1ResultPromptId 
        { 
            get
            {
                if (Address1ResultPrompt == null) { return null; }
                return Address1ResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? Address2ResultPromptId
        {
            get
            {
                if (Address2ResultPrompt == null) { return null; }
                return Address2ResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? Address3ResultPromptId
        {
            get
            {
                if (Address3ResultPrompt == null) { return null; }
                return Address3ResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? Address4ResultPromptId
        {
            get
            {
                if (Address4ResultPrompt == null) { return null; }
                return Address4ResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? Address5ResultPromptId
        {
            get
            {
                if (Address5ResultPrompt == null) { return null; }
                return Address5ResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? PostCodeResultPromptId
        {
            get
            {
                if (PostCodeResultPrompt == null) { return null; }
                return PostCodeResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? EmailAddress1ResultPromptId
        {
            get
            {
                if (EmailAddress1ResultPrompt == null) { return null; }
                return EmailAddress1ResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public Guid? EmailAddress2ResultPromptId
        {
            get
            {
                if (EmailAddress2ResultPrompt == null) { return null; }
                return EmailAddress2ResultPrompt.ResultPromptUUID;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string Address1ResultPromptReference
        {
            get
            {
                if (Address1ResultPrompt == null || Address1ResultPrompt.ResultPrompt == null) { return null; }
                if (Address1ResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS" || Address1ResultPrompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(Address1ResultPrompt.ResultPrompt);
                    return string.Format("{0}Address1", referenceName);
                }
                return Address1ResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string Address2ResultPromptReference
        {
            get
            {
                if (Address2ResultPrompt == null || Address2ResultPrompt.ResultPrompt == null) { return null; }
                if (Address2ResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS" || Address2ResultPrompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(Address2ResultPrompt.ResultPrompt);
                    return string.Format("{0}Address2", referenceName);
                }
                return Address2ResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string Address3ResultPromptReference
        {
            get
            {
                if (Address3ResultPrompt == null || Address3ResultPrompt.ResultPrompt == null) { return null; }
                if (Address3ResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS" || Address3ResultPrompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(Address3ResultPrompt.ResultPrompt);
                    return string.Format("{0}Address3", referenceName);
                }
                return Address3ResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string Address4ResultPromptReference
        {
            get
            {
                if (Address4ResultPrompt == null || Address4ResultPrompt.ResultPrompt == null) { return null; }
                if (Address4ResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS" || Address4ResultPrompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(Address4ResultPrompt.ResultPrompt);
                    return string.Format("{0}Address4", referenceName);
                }
                return Address4ResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string Address5ResultPromptReference
        {
            get
            {
                if (Address5ResultPrompt == null || Address5ResultPrompt.ResultPrompt == null) { return null; }
                if (Address5ResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS" || Address5ResultPrompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(Address5ResultPrompt.ResultPrompt);
                    return string.Format("{0}Address5", referenceName);
                }
                return Address5ResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string PostCodeResultPromptReference
        {
            get
            {
                if (PostCodeResultPrompt == null || PostCodeResultPrompt.ResultPrompt == null) { return null; }
                if (PostCodeResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS" || PostCodeResultPrompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(PostCodeResultPrompt.ResultPrompt);
                    return string.Format("{0}PostCode", referenceName);
                }
                return PostCodeResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string EmailAddress1ResultPromptReference
        {
            get
            {
                if (EmailAddress1ResultPrompt == null || EmailAddress1ResultPrompt.ResultPrompt == null) { return null; }
                if (EmailAddress1ResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS" || EmailAddress1ResultPrompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(EmailAddress1ResultPrompt.ResultPrompt);
                    return string.Format("{0}EmailAddress1", referenceName);
                }
                return EmailAddress1ResultPrompt.ResultPrompt.PromptReference;
            }
        }

        [JsonIgnore]
        [UnAudittable]
        public string EmailAddress2ResultPromptReference
        {
            get
            {
                if (EmailAddress2ResultPrompt == null || EmailAddress2ResultPrompt.ResultPrompt == null) { return null; }
                if (EmailAddress2ResultPrompt.ResultPrompt.PromptType == "NAMEADDRESS" || EmailAddress2ResultPrompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    var referenceName = ResultDefinition.MakeReferenceName(EmailAddress2ResultPrompt.ResultPrompt);
                    return string.Format("{0}EmailAddress2", referenceName);
                }
                return EmailAddress2ResultPrompt.ResultPrompt.PromptReference;
            }
        }

        #endregion Address

        public bool IsPromptSetForNameAddress(Guid? promptId)
        {
            if (TitleResultPromptId != null && TitleResultPromptId == promptId) { return true; }
            if (FirstNameResultPromptId != null && FirstNameResultPromptId == promptId) { return true; }
            if (MiddleNameResultPromptId != null && MiddleNameResultPromptId == promptId) { return true; }
            if (LastNameResultPromptId != null && LastNameResultPromptId == promptId) { return true; }
            if (OrganisationNameResultPromptId != null && OrganisationNameResultPromptId == promptId) { return true; }
            if (Address1ResultPromptId != null && Address1ResultPromptId == promptId) { return true; }
            if (Address1ResultPromptId != null && Address1ResultPromptId == promptId) { return true; }
            if (Address2ResultPromptId != null && Address2ResultPromptId == promptId) { return true; }
            if (Address3ResultPromptId != null && Address3ResultPromptId == promptId) { return true; }
            if (Address4ResultPromptId != null && Address4ResultPromptId == promptId) { return true; }
            if (Address5ResultPromptId != null && Address5ResultPromptId == promptId) { return true; }
            if (PostCodeResultPromptId != null && PostCodeResultPromptId == promptId) { return true; }
            if (EmailAddress1ResultPromptId != null && EmailAddress1ResultPromptId == promptId) { return true; }
            if (EmailAddress2ResultPromptId != null && EmailAddress2ResultPromptId == promptId) { return true; }
            return false;
        }

        public bool IsPromptRuleSetForNameAddress(ResultPromptRule rule)
        {
            if (TitleResultPrompt != null && (TitleResultPrompt.MasterUUID ?? TitleResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (FirstNameResultPrompt != null && (FirstNameResultPrompt.MasterUUID ?? FirstNameResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (MiddleNameResultPrompt != null && (MiddleNameResultPrompt.MasterUUID ?? MiddleNameResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (LastNameResultPrompt != null && (LastNameResultPrompt.MasterUUID ?? LastNameResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (OrganisationNameResultPrompt != null && (OrganisationNameResultPrompt.MasterUUID ?? OrganisationNameResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (Address1ResultPrompt != null && (Address1ResultPrompt.MasterUUID ?? Address1ResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (Address2ResultPrompt != null && (Address2ResultPrompt.MasterUUID ?? Address2ResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (Address3ResultPrompt != null && (Address3ResultPrompt.MasterUUID ?? Address3ResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (Address4ResultPrompt != null && (Address4ResultPrompt.MasterUUID ?? Address4ResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (Address5ResultPrompt != null && (Address5ResultPrompt.MasterUUID ?? Address5ResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (PostCodeResultPrompt != null && (PostCodeResultPrompt.MasterUUID ?? PostCodeResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (EmailAddress1ResultPrompt != null && (EmailAddress1ResultPrompt.MasterUUID ?? EmailAddress1ResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            if (EmailAddress2ResultPrompt != null && (EmailAddress2ResultPrompt.MasterUUID ?? EmailAddress2ResultPrompt.UUID) == (rule.MasterUUID ?? rule.UUID)) { return true; }
            return false;
        }

        public string GetChangedProperties(NowRecipient other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = GetChangedProperties<NowRecipient>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.Append(simpleTypeChanges);
            }

            //compare the prompt rules and report differences
            ReportResultPromptRuleDelta(TitleResultPrompt, other.TitleResultPrompt, sb, "Title");
            ReportResultPromptRuleDelta(FirstNameResultPrompt, other.FirstNameResultPrompt, sb, "First Name");
            ReportResultPromptRuleDelta(MiddleNameResultPrompt, other.MiddleNameResultPrompt, sb, "Middle Name");
            ReportResultPromptRuleDelta(LastNameResultPrompt, other.LastNameResultPrompt, sb, "Last Name");
            ReportResultPromptRuleDelta(Address1ResultPrompt, other.Address1ResultPrompt, sb, "Address Line 1");
            ReportResultPromptRuleDelta(Address2ResultPrompt, other.Address2ResultPrompt, sb, "Address Line 2");
            ReportResultPromptRuleDelta(Address3ResultPrompt, other.Address3ResultPrompt, sb, "Address Line 3");
            ReportResultPromptRuleDelta(Address4ResultPrompt, other.Address4ResultPrompt, sb, "Address Line 4");
            ReportResultPromptRuleDelta(Address5ResultPrompt, other.Address5ResultPrompt, sb, "Address Line 5");
            ReportResultPromptRuleDelta(PostCodeResultPrompt, other.PostCodeResultPrompt, sb, "Post Code");
            ReportResultPromptRuleDelta(EmailAddress1ResultPrompt, other.EmailAddress1ResultPrompt, sb, "Email Address 1");
            ReportResultPromptRuleDelta(EmailAddress2ResultPrompt, other.EmailAddress2ResultPrompt, sb, "Email Address 2");

            return sb.ToString();
        }

        private void ReportResultPromptRuleDelta(ResultPromptRule fromRule, ResultPromptRule toRule, StringBuilder sb, string typeName)
        {
            if (fromRule == null)
            {
                if (toRule == null)
                {
                    return;
                }

                sb.AppendLine(string.Format("{0} recipient prompt rule added", typeName));
                return;
            }

            if (toRule == null)
            {
                sb.AppendLine(string.Format("{0} recipient prompt rule removed", typeName));
                return;
            }

            if ((fromRule.MasterUUID ?? fromRule.UUID) != (toRule.MasterUUID ?? toRule.UUID))
            {
                sb.AppendLine(string.Format("{0} recipient prompt rule changed", typeName));
                return;
            }
        }

        public NowRecipient Copy()
        {
            NowRecipient res = (NowRecipient)MemberwiseClone();
            return res;
        }
    }

    [Serializable]
    public sealed class NowRecipientJSON
    {
        private NowRecipient nowRecipient;
        public NowRecipientJSON(NowRecipient nowRecipient) { this.nowRecipient = nowRecipient; }

        public Guid? nowSubscription { get => nowRecipient.NowSubscription.MasterUUID ?? nowRecipient.NowSubscription.UUID; }

        public bool recipientFromCase { get => nowRecipient.RecipientFromCase; }

        public bool recipientFromSubscription { get => nowRecipient.RecipientFromSubscription; }

        public bool recipientFromResults { get => nowRecipient.RecipientFromResults; }

        public bool isApplyDefenceOrganisationDetails { get => nowRecipient.IsApplyDefenceOrganisationDetails; }

        public bool isApplyParentGuardianDetails { get => nowRecipient.IsApplyParentGuardianDetails; }

        public bool isApplyDefendantDetails { get => nowRecipient.IsApplyDefendantDetails; }

        public bool isApplyDefendantCustodyDetails { get => nowRecipient.IsApplyDefendantCustodyDetails; }

        public bool isApplyProsecutionAuthorityDetails { get => nowRecipient.IsApplyProsecutionAuthorityDetails; }

        /// <summary>
        /// Application applicant
        /// </summary>
        public bool isApplyApplicantDetails { get => nowRecipient.IsApplyApplicantDetails; }

        /// <summary>
        /// Application respondent
        /// </summary>
        public bool isApplyRespondentDetails { get => nowRecipient.IsApplyRespondentDetails; }

        /// <summary>
        /// Application 3rd parties
        /// </summary>
        public bool isApplyThirdPartyDetails { get => nowRecipient.IsApplyThirdPartyDetails; }

        #region Name
        public string title { get => nowRecipient.Title; }
        public string firstName { get => nowRecipient.FirstName; }

        public string middleName { get => nowRecipient.MiddleName; }

        public string lastName { get => nowRecipient.LastName; }

        public string organisationName { get => nowRecipient.OrganisationName; }

        public Guid? titleResultPromptId { get => nowRecipient.TitleResultPromptId; }

        public Guid? firstNameResultPromptId { get => nowRecipient.FirstNameResultPromptId; }

        public Guid? middleNameResultPromptId { get => nowRecipient.MiddleNameResultPromptId; }

        public Guid? lastNameResultPromptId { get => nowRecipient.LastNameResultPromptId; }

        public Guid? organisationNameResultPromptId { get => nowRecipient.OrganisationNameResultPromptId; }

        public string titleResultPromptReference { get => nowRecipient.TitleResultPromptReference; }

        public string firstNameResultPromptReference { get => nowRecipient.FirstNameResultPromptReference; }

        public string middleNameResultPromptReference { get => nowRecipient.MiddleNameResultPromptReference; }

        public string lastNameResultPromptReference { get => nowRecipient.LastNameResultPromptReference; }

        public string organisationNameResultPromptReference { get => nowRecipient.OrganisationNameResultPromptReference; }

        #endregion Name

        #region Address
        public string address1 { get => nowRecipient.Address1; }

        public string address2 { get => nowRecipient.Address2; }

        public string address3 { get => nowRecipient.Address3; }

        public string address4 { get => nowRecipient.Address4; }

        public string address5 { get => nowRecipient.Address5; }

        public string postCode { get => nowRecipient.PostCode; }

        public string emailAddress1 { get => nowRecipient.EmailAddress1; }

        public string emailAddress2 { get => nowRecipient.EmailAddress2; }

        public Guid? address1ResultPromptId { get => nowRecipient.Address1ResultPromptId; }

        public Guid? address2ResultPromptId { get => nowRecipient.Address2ResultPromptId; }

        public Guid? address3ResultPromptId { get => nowRecipient.Address3ResultPromptId; }

        public Guid? address4ResultPromptId { get => nowRecipient.Address4ResultPromptId; }

        public Guid? address5ResultPromptId { get => nowRecipient.Address5ResultPromptId; }

        public Guid? postCodeResultPromptId { get => nowRecipient.PostCodeResultPromptId; }

        public Guid? emailAddress1ResultPromptId { get => nowRecipient.EmailAddress1ResultPromptId; }

        public Guid? emailAddress2ResultPromptId { get => nowRecipient.EmailAddress2ResultPromptId; }

        public string address1ResultPromptReference { get => nowRecipient.Address1ResultPromptReference; }

        public string address2ResultPromptReference { get => nowRecipient.Address2ResultPromptReference; }

        public string address3ResultPromptReference { get => nowRecipient.Address3ResultPromptReference; }

        public string address4ResultPromptReference { get => nowRecipient.Address4ResultPromptReference; }

        public string address5ResultPromptReference { get => nowRecipient.Address5ResultPromptReference; }

        public string postCodeResultPromptReference { get => nowRecipient.PostCodeResultPromptReference; }

        public string emailAddress1ResultPromptReference { get => nowRecipient.EmailAddress1ResultPromptReference; }

        public string emailAddress2ResultPromptReference { get => nowRecipient.EmailAddress2ResultPromptReference; }

        #endregion Address
    }
}