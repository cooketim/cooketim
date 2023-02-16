using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class Defendant
    {
        [JsonProperty(PropertyName = "masterDefendantId")]
        public Guid DefendantId { get; }

        [JsonProperty(PropertyName = "asn")]
        public string ASN { get; }

        [JsonProperty(PropertyName = "pncId")]
        public string PNCId { get; }

        [JsonProperty(PropertyName = "mobileNumber")]
        public string MobileNumber { get; }

        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; }

        [JsonProperty(PropertyName = "nino")]
        public string NINO { get; }

        [JsonProperty(PropertyName = "ethnicity")]
        public string Ethnicity { get; private set; }

        [JsonProperty(PropertyName = "selfDefinedEthnicity")]
        public string SelfDefinedEthnicity { get; private set; }

        [JsonProperty(PropertyName = "interpreterLanguageNeeds")]
        public string InterpreterLanguageNeeds { get; private set; }

        [JsonProperty(PropertyName = "specialRequirements")]
        public string SpecialRequirements { get; private set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; private set; }

        [JsonProperty(PropertyName = "nationality")]
        public string Nationality { get; }

        [JsonProperty(PropertyName = "driverNumber")]
        public string DriverNumber { get; }

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

        [JsonProperty(PropertyName = "occupation")]
        public string Occupation { get; }

        [JsonProperty(PropertyName = "occupationWelsh")]
        public string OccupationWelsh { get; }

        [JsonProperty(PropertyName = "aliasNames")]
        public List<string> AliasNames { get; }

        [JsonProperty(PropertyName = "dateOfBirth")]
        public DateTime DateOfBirth { get; }

        [JsonProperty(PropertyName = "address")]
        public NowAddress Address { get; }

        [JsonProperty(PropertyName = "solicitorName")]
        public string SolicitorName { get; }

        [JsonProperty(PropertyName = "solicitorAddress")]
        public NowAddress SolicitorAddress { get; }

        [JsonProperty(PropertyName = "custodyAddress")]
        public NowAddress CustodyAddress { get; }

        [JsonProperty(PropertyName = "custodyName")]
        public string CustodyName { get; }

        [JsonProperty(PropertyName = "defendantResults")]
        public List<Result> DefendantResults { get; internal set; }

        [JsonProperty(PropertyName = "defendantCaseResults")]
        public List<Result> DefendantCaseResults { get; internal set; }

        [JsonProperty(PropertyName = "isYouth")]
        public bool IsYouth { get; }

        [JsonProperty(PropertyName = "parentGuardianAddress")]
        public NowAddress ParentGuardianAddress { get; }

        [JsonProperty(PropertyName = "parentGuardianName")]
        public string ParentGuardianName { get; }

        [JsonProperty(PropertyName = "parentGuardianDateOfBirth")]
        public DateTime? ParentGuardianDateOfBirth { get; }

        [JsonProperty(PropertyName = "appearedByVideoLink")]
        public bool AppearedByVideoLink { get; }

        [JsonProperty(PropertyName = "appearedInPerson")]
        public bool AppearedInPerson { get; set; }

        [JsonProperty(PropertyName = "inCustody")]
        public bool InCustody { get; }

        [JsonProperty(PropertyName = "custodyLocationIsPrison")]
        public bool CustodyLocationIsPrison { get; }

        [JsonProperty(PropertyName = "custodyLocationIsPolice")]
        public bool CustodyLocationIsPolice { get; }

        public Defendant(bool isYouth, bool appearedInPerson, bool custodyLocationIsPrison, bool custodyLocationIsPolice, bool standaloneCivilApplication)
        {
            DefendantId = Guid.NewGuid();
            IsYouth = isYouth;
            AppearedInPerson = appearedInPerson;
            AppearedByVideoLink = !appearedInPerson;
            CustodyLocationIsPrison = custodyLocationIsPrison;
            CustodyLocationIsPolice = custodyLocationIsPolice;
            InCustody = CustodyLocationIsPrison || CustodyLocationIsPolice;
            ASN = standaloneCivilApplication ? null : "20/105CE01/01245932H";
            MobileNumber = "07973 234788";
            PhoneNumber = "01279 659878";
            NINO = "NH569819B";
            Name = "Edward John Harrison";
            Title = "Mr";
            FirstName = "Edward";
            MiddleName = "John";
            LastName = "Harrison";
            Occupation = standaloneCivilApplication ? null : isYouth ? "Student" : "Electrician";
            OccupationWelsh = standaloneCivilApplication ? null : isYouth ? "Welsh ~ Student" : "Welsh ~ Electrician";
            AliasNames = standaloneCivilApplication ? null : new List<string>() { "Al Capone", "Baby Face Nelson", "Billy the Kid", "Jack the Ripper" };
            Address = new NowAddress
            (
                "32 Hen Park Avenue", "Little Hill", "Bishops Stortford", "Hertfordshire", null, "CM22 6WS", null, null
            );
            ParentGuardianName = "John William Harrison";
            ParentGuardianAddress = new NowAddress
            (
                "16 Bellows Drive", "Little Hill", "Bishops Stortford", "Hertfordshire", null, "CM20 8WS", null, null
            );
            DateOfBirth = isYouth ? DateTime.Now.AddYears(-16).Date : DateTime.Now.AddYears(-20).Date;
            ParentGuardianDateOfBirth = DateOfBirth.AddYears(-19).Date;
            if (!standaloneCivilApplication)
            {
                SetEthnicity();
                SetGender();
                SetInterpreterLanguageNeeds();
                SetSpecialRequirements();
            }
            Nationality = standaloneCivilApplication ? null : "GBR";
            PNCId = standaloneCivilApplication ? null : "20/4378549Z";
            DriverNumber = "012345678";

            SolicitorName = "Saul Goodman & Associates";
            SolicitorAddress = new NowAddress
            (
                "9800 Montgomery Blvd NE", "Albuquerque", "New Mexico", null, "NM4 8TY", null, null, null
            );
            if(CustodyLocationIsPrison)
            {
                CustodyName = "HM Prison Wormwood Scrubs";
                CustodyAddress = new NowAddress
                (
                    "160 Du Cane Rd", "London", null, null, null, "W12 0AN", "nows@HMPWormwordscrubs.co.uk", null
                );
            }
            if (CustodyLocationIsPolice)
            {
                CustodyName = "Paddington Green Police Station";
                CustodyAddress = new NowAddress
                (
                    "4 Harrow Rd", "Paddington", "London", null, null, "W2 1XJ", "nows@paddingtongreenpolicestn.co.uk", null
                );
            }
        }

        private void SetEthnicity()
        {
            var values = new List<string>()
            {
                "0 not recorded/ not known",
                "1 White - North European",
                "2 White - South European",
                "3 Black",
                "4 Asian",
                "5 Chinese, Japanese or any other South East Asian",
                "6 Arabic or North African"
            };

            var index1 = HearingResults.Random.Next(6);
            var index2 = HearingResults.Random.Next(6);
            Ethnicity = values[index1];
            SelfDefinedEthnicity = values[index2];
        }

        private void SetInterpreterLanguageNeeds()
        {
            var values = new List<string>()
            {
                "aar",
                "ace",
                "ada",
                "afr",
                "ara",
                "heb",
                "rom"
            };

            var index = HearingResults.Random.Next(6);
            InterpreterLanguageNeeds = values[index];
        }

        private void SetSpecialRequirements()
        {
            var values = new List<string>()
            {
                "wheel chair access",
                "learning difficulty",
                "hearing impairment",
                "visual impairment",
                "braille reader",
                "sign language",
                "vegan"
            };

            var index = HearingResults.Random.Next(6);
            SpecialRequirements = values[index];
        }

        private void SetGender()
        {
            var values = new List<string>()
            {
                "0 not known [not recorded i.e. enquiry as to gender may not have been made]",
                "1 male",
                "2 female",
                "9 not specified [indeterminate i.e. enquiry may have been made but gender was not determined]"
            };

            var index = HearingResults.Random.Next(3);
            Gender = values[index];
        }
    }
}
