using DataLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class ProsecutionCase
    {
        [JsonProperty(PropertyName = "isApplication")]
        public bool IsApplication { get; }

        [JsonProperty(PropertyName = "caseId")]
        public Guid? CaseId { get; }

        [JsonProperty(PropertyName = "prosecutor")]
        public string Prosecutor { get; private set; }

        [JsonProperty(PropertyName = "prosecutorWelsh")]
        public string ProsecutorWelsh { get; private set; }

        [JsonProperty(PropertyName = "prosecutorAddress")]
        public NowAddress ProsecutorAddress { get; private set; }

        [JsonProperty(PropertyName = "prosecutorOUCode")]
        public string ProsecutorOUCode { get; private set; }

        [JsonProperty(PropertyName = "urn")]
        public string Urn { get; }

        [JsonProperty(PropertyName = "caseMarkers")]
        public List<string> CaseMarkers { get; private set; }

        [JsonProperty(PropertyName = "offences")]
        public List<Offence> Offences { get; private set; }

        [JsonProperty(PropertyName = "applicationType")]
        public string ApplicationType { get; private set; }

        [JsonProperty(PropertyName = "applicationLegislation")]
        public string ApplicationLegislation { get; private set; }

        [JsonProperty(PropertyName = "applicationTypeWelsh")]
        public string ApplicationTypeWelsh { get; private set; }

        [JsonProperty(PropertyName = "applicationCode")]
        public string ApplicationCode { get; private set; }

        [JsonProperty(PropertyName = "applicationLegislationWelsh")]
        public string ApplicationLegislationWelsh { get; private set; }

        [JsonProperty(PropertyName = "applicationReceivedDate")]
        public DateTime? ApplicationReceivedDate { get; private set; }

        [JsonProperty(PropertyName = "allegationOrComplaintStartDate")]
        public DateTime? AllegationOrComplaintStartDate { get; private set; }

        [JsonProperty(PropertyName = "allegationOrComplaintEndDate")]
        public DateTime? AllegationOrComplaintEndDate { get; private set; }

        [JsonProperty(PropertyName = "applicationParticulars")]
        public string ApplicationParticulars { get; private set; }

        [JsonProperty(PropertyName = "applicationParticularsWelsh")]
        public string ApplicationParticularsWelsh { get; private set; }

        [JsonProperty(PropertyName = "applicant")]
        public NowParty Applicant { get; }

        [JsonProperty(PropertyName = "respondents")]
        public List<NowParty> Respondents { get; }

        [JsonProperty(PropertyName = "thirdParties")]
        public List<NowParty> ThirdParties { get; }

        /// <summary>
        /// criminal case - when forApplication then the case represents the linked case and does is not required to be resulted
        /// </summary>
        /// <param name="urn"></param>
        /// <param name="orderDate"></param>
        /// <param name="prosecutor"></param>
        /// <param name="prosecutorOUCode"></param>
        /// <param name="prosecutorAddress"></param>
        /// <param name="applicationRequest"></param>
        public ProsecutionCase(bool isCrown, string urn, DateTime? orderDate, string prosecutor, string prosecutorOUCode, NowAddress prosecutorAddress, ApplicationRequest applicationRequest, Defendant defendant)
        {
            CaseId = Guid.NewGuid();
            SetCaseMarkers();
            Urn = urn;
            Prosecutor = prosecutor;
            ProsecutorWelsh = "Welsh ~ " + prosecutor;
            ProsecutorOUCode = prosecutorOUCode;
            Offences = new List<Offence>();
            this.ProsecutorAddress = prosecutorAddress;

            var startDate = orderDate.Value.AddDays(applicationRequest != null ? -165 : - 65).Date;
            var endDateDays = applicationRequest != null ? HearingResults.Random.Next(100, 160) : HearingResults.Random.Next(10, 60);
            var endDate = orderDate.Value.AddDays(endDateDays * -1).Date;

            if (applicationRequest != null)
            {
                var mojCharge = Charge.GetMOJCharge(startDate, endDate, applicationRequest.IsLinkedApplication, applicationRequest.IsLiveProceedings);
                var mojOffence = new Offence(mojCharge.Title, "Welsh ~ " + mojCharge.Title, mojCharge.Legislation, mojCharge.ModeOfTrial, mojCharge.Code, mojCharge.Wording, "Welsh ~ " + mojCharge.Wording, startDate, endDate, applicationRequest);

                if (applicationRequest.IsResentenceOrActivation)
                {
                    mojOffence.ConvictionDate = orderDate;
                }

                Offences.Add(mojOffence);

                //set the application details
                ApplicationType = mojOffence.Title;
                ApplicationCode = mojOffence.Code;
                ApplicationTypeWelsh = mojOffence.WelshTitle;
                ApplicationLegislation = mojOffence.Legislation;
                ApplicationLegislationWelsh = "Welsh~" + mojOffence.Legislation;
                ApplicationReceivedDate = orderDate.Value.AddDays(-5);
                if (!applicationRequest.IsLiveProceedings)
                {
                    AllegationOrComplaintStartDate = orderDate.Value.AddDays(-15);
                    AllegationOrComplaintEndDate = orderDate.Value.AddDays(-10);
                }
                ApplicationParticulars = mojOffence.Wording;
                ApplicationParticularsWelsh = mojOffence.WelshWording;

                if (applicationRequest.IsLiveProceedings || !applicationRequest.IsLinkedApplication)
                {
                    //is typically the defendant when live proceedings
                    Applicant = new NowParty
                    (
                        defendant.Name,
                        defendant.Address,
                        defendant.DateOfBirth,
                        defendant.MobileNumber,
                        defendant.PhoneNumber
                    );
                }
                else
                {
                    //is typically probation for a failure to comply
                    Applicant = new NowParty
                    (
                        string.Format("Probation Team For Breach Application '{0}'.  Can also be the defendant for variations and appeals", urn),
                        new NowAddress
                        (
                            "22 Hill Rise", "West Side", "Hertford", "Hertfordshire", null, "H14 7HV", "courtApplications@me.com", null
                        ),
                        null,
                        "07803 195765",
                        "01992 336787"
                    );
                }

                //overwrite the prosecutor details when not live proceedings
                if (!applicationRequest.IsLiveProceedings)
                {
                    Prosecutor = Applicant.Name;
                    ProsecutorWelsh = null;
                    ProsecutorOUCode = applicationRequest.IsLinkedApplication ? "ProbationCode" : null;
                    ProsecutorAddress = Applicant.Address;
                }

                if (applicationRequest.IsLiveProceedings || !applicationRequest.IsLinkedApplication)
                {
                    //typically no respondents listed or defence or prosecution
                    Respondents = new List<NowParty>(){ new NowParty
                    (
                        string.Format("Typically the opposing counsel will need to respond to application '{0}'.  Can also be null", urn),
                        new NowAddress
                        (
                            "22 Westbourne", "East Hill", "Hertford", "Hertfordshire", null, "H13 8YV", "courtApplications@me.com", null
                        ),
                        null,
                        "07801 444567",
                        "01992 333666"
                    ) };
                }
                else
                {
                    // is typically the defendant for a failure to comply
                    Respondents = new List<NowParty>(){ new NowParty
                    (
                        defendant.Name,
                        defendant.Address,
                        defendant.DateOfBirth,
                        defendant.MobileNumber,
                        defendant.PhoneNumber
                    ) };
                }

                if (applicationRequest.IsLiveProceedings)
                {
                    ThirdParties = new List<NowParty>(){ new NowParty
                    (
                        string.Format("Other 3rd party for application '{0}' to be notified e.g. Victim from original case", urn),
                        new NowAddress
                        (
                            "77 Eastville Road", "Bishops End", "Hertford", "Hertfordshire", null, "H7 5BV", "courtApplicationsThirdParties@me.com", null
                        ),
                        null,
                        "07866 235665",
                        "01992 567686"
                    ) };
                }

                IsApplication = true;

                return;
            }

            var allCharges = Charge.GetAllCharges(startDate, endDate);
            var numberOffences = applicationRequest != null ? 1 : HearingResults.Random.Next(2)+1;
            var indices = new List<int?>();
            var selectedCharges = new List<Charge>();

            if (numberOffences >= allCharges.Count)
            {
                numberOffences = allCharges.Count;
                selectedCharges = allCharges;
            }
            else
            {
                do
                {
                    var index = HearingResults.Random.Next(allCharges.Count);
                    if (indices.FirstOrDefault(x => x == index) == null)
                    {
                        if (isCrown && allCharges[index].ModeOfTrial == "S")
                        {
                            continue;
                        }
                        if (!isCrown && allCharges[index].ModeOfTrial == "I")
                        {
                            continue;
                        }
                        indices.Add(index);
                    }

                } while (indices.Count < numberOffences);

                foreach (var index in indices)
                {
                    selectedCharges.Add(allCharges[index.Value]);
                }
            }

            for (var i = 0; i < selectedCharges.Count; i++)
            {
                Offences.Add(new Offence(selectedCharges[i].Title, "Welsh ~ " + selectedCharges[i].Title, selectedCharges[i].Legislation, selectedCharges[i].ModeOfTrial, selectedCharges[i].Code, selectedCharges[i].Wording, "Welsh ~ " + selectedCharges[i].Wording, startDate, endDate, applicationRequest));
            }
        }

        private void SetCaseMarkers()
        {
            var items = new List<string>()
            {
                "AE", "CO", "DH", "DV", "FM", "FT", "HC", "HP", "HT", "LO", "NU", "PW", "RA", "RE", "RP", "TG", "AA", "AG", "AR", "CA", "DI", "IV", "MI", "PC", "PD", "SC", "VC", "VI", "WR"
            };

            var random = new Random();
            var numberItems = random.Next(1, 7);
            CaseMarkers = new List<string>();
            while (CaseMarkers.Count < numberItems)
            {
                var index = random.Next(0, items.Count - 1);
                if (!CaseMarkers.Contains(items[index]))
                {
                    CaseMarkers.Add(items[index]);
                }
            }
        }
    }
}
