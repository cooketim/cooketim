using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NowsRequestDataLib.Domain;
using System.Linq;

namespace NowsRequestDataLib.NowDocument
{
    [Serializable]
    public class DefendantForDocRequest
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; }

        [JsonProperty(PropertyName = "middleName")]
        public string MiddleName { get; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; }

        [JsonProperty(PropertyName = "aliasNames")]
        public string[] AliasNames { get; }

        [JsonProperty(PropertyName = "occupation")]
        public string Occupatation { get; }

        [JsonProperty(PropertyName = "occupationWelsh")]
        public string OccupatationWelsh { get; }

        [JsonProperty(PropertyName = "dateOfBirth")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime DateOfBirth { get; }

        [JsonProperty(PropertyName = "isYouth")]
        public string IsYouth { get; }

        [JsonProperty(PropertyName = "address")]
        public NowAddress Address { get; }

        [JsonProperty(PropertyName = "prosecutingAuthorityReference")]
        public string ASN { get; private set; }

        [JsonProperty(PropertyName = "pncId")]
        public string PNCId { get; private set; }

        [JsonProperty(PropertyName = "mobileNumber")]
        public string MobileNumber { get; private set; }

        [JsonProperty(PropertyName = "landlineNumber")]
        public string PhoneNumber { get; private set; }

        [JsonProperty(PropertyName = "nationalInsuranceNumber")]
        public string NINO { get; private set; }

        [JsonProperty(PropertyName = "ethnicity")]
        public string Ethnicity { get; private set; }

        [JsonProperty(PropertyName = "selfDefinedEthnicity")]
        public string SelfDefinedEthnicity { get; private set; }

        [JsonProperty(PropertyName = "interpreterLanguageNeeds")]
        public string InterpreterLanguageNeeds { get; private set; }

        [JsonProperty(PropertyName = "specialNeeds")]
        public string SpecialRequirements { get; private set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; private set; }

        [JsonProperty(PropertyName = "nationality")]
        public string Nationality { get; private set; }

        [JsonProperty(PropertyName = "driverNumber")]
        public string DriverNumber { get; private set; }

        [JsonProperty(PropertyName = "solicitor")]
        public NowSolicitor Solicitor { get; set; }

        [JsonProperty(PropertyName = "defendantResults")]
        public ResultForDocumentRequest[] DefendantResults { get; private set; }

        public DefendantForDocRequest(Defendant defendant)
        {
            Name = defendant.Name;
            Title = defendant.Title;
            FirstName = defendant.FirstName;
            MiddleName = defendant.MiddleName;
            LastName = defendant.LastName;
            AliasNames = defendant.AliasNames == null ? null : defendant.AliasNames.ToArray();
            Occupatation = defendant.Occupation;
            OccupatationWelsh = defendant.OccupationWelsh;
            DateOfBirth = defendant.DateOfBirth;
            IsYouth = defendant.IsYouth ? "Y" : "N";
            Address = defendant.Address;
            ASN = defendant.ASN;
            PNCId = defendant.PNCId;
            MobileNumber = defendant.MobileNumber;
            PhoneNumber = defendant.PhoneNumber;
            NINO = defendant.NINO;
            Ethnicity = defendant.Ethnicity;
            SelfDefinedEthnicity = defendant.SelfDefinedEthnicity;
            InterpreterLanguageNeeds = defendant.InterpreterLanguageNeeds;
            SpecialRequirements = defendant.SpecialRequirements;
            Gender = defendant.Gender;
            Nationality = defendant.Nationality;
            DriverNumber = defendant.DriverNumber;
            Solicitor = new NowSolicitor(defendant.SolicitorName, defendant.SolicitorAddress);            

            //make some defendant results based on the defendant.DefendantResults
            SetDefendantResults(defendant);
        }

        private void SetDefendantResults(Defendant defendant)
        {
            if (defendant.DefendantResults == null || defendant.DefendantResults.Count == 0) { return; }

            var resp = new List<ResultForDocumentRequest>();
            foreach (var result in defendant.DefendantResults)
            {
                resp.Add(new ResultForDocumentRequest(result));
            }
            DefendantResults = resp.ToArray();
        }

        internal void SetASNVisible(bool includeDefendantASN)
        {
            if (!includeDefendantASN) { ASN = null; }
        }

        internal void SetEthnicityVisible(bool includeEthnicity)
        {
            if (!includeEthnicity) { Ethnicity = null; SelfDefinedEthnicity = null; }
        }

        internal void SetNationalityVisible(bool includeNationality)
        {
            if (!includeNationality) { Nationality = null; }
        }

        internal void SetGenderVisible(bool includeDefendantGender)
        {
            if (!includeDefendantGender) { Gender = null; }
        }

        internal void SetLandlineVisible(bool includeDefendantLandlineNumber)
        {
            if (!includeDefendantLandlineNumber) { PhoneNumber = null; }
        }

        internal void SetMobileVisible(bool includeDefendantMobileNumber)
        {
            if (!includeDefendantMobileNumber) { MobileNumber = null; }
        }

        internal void SetNINOVisible(bool includeDefendantNINO)
        {
            if (!includeDefendantNINO) { NINO = null; }
        }

        internal void SetNDriverNumberVisible(bool includeDriverNumber)
        {
            if (!includeDriverNumber) { DriverNumber = null; }
        }

        internal void SetPNCId(bool includePNCID)
        {
            if (!includePNCID) { PNCId = null; }
        }

        internal void SetSolicitorNameAddress(bool includeSolicitorsNameAddress)
        {
            if (!includeSolicitorsNameAddress) { Solicitor = null; }
        }
    }
}
