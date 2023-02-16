using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using NowsRequestDataLib.Domain;

namespace NowsRequestDataLib.NowDocument
{
    [Serializable]
    public class ProsecutionCaseForDocumentRequest
    {

        [JsonProperty(PropertyName = "isApplication")]
        public bool IsApplication { get; }

        [JsonProperty(PropertyName = "reference")]
        public string Urn { get; }

        /// <summary>
        /// When the application is of type breach, the prosecutor will be probation
        /// </summary>
        [JsonProperty(PropertyName = "prosecutor")]
        public string Prosecutor { get; }

        [JsonProperty(PropertyName = "prosecutorWelsh")]
        public string ProsecutorWelsh { get; }

        [JsonProperty(PropertyName = "prosecutorOUCode")]
        public string ProsecutorOUCode { get; }

        [JsonProperty(PropertyName = "prosecutorAddress")]
        public NowAddress ProsecutorAddress { get; }

        [JsonProperty(PropertyName = "caseMarkers")]
        public string[] CaseMarkers { get; private set; }

        [JsonProperty(PropertyName = "applicationType")]
        public string ApplicationType { get; }

        [JsonProperty(PropertyName = "applicationLegislation")]
        public string ApplicationLegislation { get; }

        [JsonProperty(PropertyName = "applicationTypeWelsh")]
        public string ApplicationTypeWelsh { get; }

        [JsonProperty(PropertyName = "applicationLegislationWelsh")]
        public string ApplicationLegislationWelsh { get; }

        [JsonProperty(PropertyName = "applicationReceivedDate")]
        public DateTime? ApplicationReceivedDate { get; }

        [JsonProperty(PropertyName = "applicationParticulars")]
        public string ApplicationParticulars { get; }

        [JsonProperty(PropertyName = "applicationParticularsWelsh")]
        public string ApplicationParticularsWelsh { get; }

        [JsonProperty(PropertyName = "allegationOrComplaintStartDate")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? AllegationOrComplaintStartDate { get; }

        [JsonProperty(PropertyName = "allegationOrComplaintEndDate")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? AllegationOrComplaintEndDate { get; }

        [JsonProperty(PropertyName = "applicationCode")]
        public string ApplicationCode { get; }

        [JsonProperty(PropertyName = "defendantCaseResults")]
        public ResultForDocumentRequest[] DefendantCaseResults { get; set; }

        [JsonProperty(PropertyName = "defendantCaseOffences")]
        public DefendantCaseOffenceForDocumentRequest[] DefendantCaseOffences { get; set; }

        [JsonIgnore]
        public Guid? Id { get; }

        public ProsecutionCaseForDocumentRequest(ProsecutionCase prosecutionCase, Defendant defendant, bool isLiveProceedings)
        {
            Id = prosecutionCase.CaseId;
            Urn = prosecutionCase.Urn;
            Prosecutor = prosecutionCase.Prosecutor;
            ProsecutorWelsh = prosecutionCase.ProsecutorWelsh;
            ProsecutorOUCode = prosecutionCase.ProsecutorOUCode;
            ProsecutorAddress = prosecutionCase.ProsecutorAddress;
            IsApplication = prosecutionCase.IsApplication;
            ApplicationCode = prosecutionCase.ApplicationCode;
            ApplicationType = prosecutionCase.ApplicationType;
            ApplicationLegislation = prosecutionCase.ApplicationLegislation;
            ApplicationType = prosecutionCase.ApplicationType;
            ApplicationTypeWelsh = prosecutionCase.ApplicationTypeWelsh;
            ApplicationLegislationWelsh = prosecutionCase.ApplicationLegislationWelsh;
            ApplicationReceivedDate = prosecutionCase.ApplicationReceivedDate;
            ApplicationParticulars = prosecutionCase.ApplicationParticulars;
            ApplicationParticularsWelsh = prosecutionCase.ApplicationParticularsWelsh;
            AllegationOrComplaintStartDate = prosecutionCase.AllegationOrComplaintStartDate;
            AllegationOrComplaintEndDate = prosecutionCase.AllegationOrComplaintEndDate;

            CaseMarkers = prosecutionCase.CaseMarkers == null ? null : prosecutionCase.CaseMarkers.ToArray();

            //make some defendant case results based on the defendant.DefendantCaseResults
            SetDefendantCaseResults(prosecutionCase, defendant);

            //deal with the offence level results
            SetDefendantCaseOffences(prosecutionCase, isLiveProceedings);
        }

        public ProsecutionCaseForDocumentRequest(ProsecutionCase prosecutionCase, List<DefendantCaseOffenceForDocumentRequest> offences, ResultForDocumentRequest[] defendantCaseResults)
        {
            Id = prosecutionCase.CaseId;
            Urn = prosecutionCase.Urn;
            Prosecutor = prosecutionCase.Prosecutor;
            ProsecutorWelsh = prosecutionCase.ProsecutorWelsh;
            ProsecutorOUCode = prosecutionCase.ProsecutorOUCode;
            ProsecutorAddress = prosecutionCase.ProsecutorAddress;

            DefendantCaseOffences = offences.ToArray();
            DefendantCaseResults = defendantCaseResults;
        }

        public ProsecutionCaseForDocumentRequest(List<DefendantCaseOffenceForDocumentRequest> offences, Guid? id, ResultForDocumentRequest[] defendantCaseResults)
        {
            Id = id;
            Urn = offences[0].OriginalCaseReference;
            DefendantCaseOffences = offences.ToArray();
            DefendantCaseResults = defendantCaseResults;
        }

        private void SetDefendantCaseResults(ProsecutionCase prosecutionCase, Defendant defendant)
        {
            if (defendant.DefendantCaseResults == null) { return; }
            var results = defendant.DefendantCaseResults.Where(x => x.DefendantCaseId == prosecutionCase.CaseId).ToList();

            if (results.Count() == 0) { return; }

            var resp = new List<ResultForDocumentRequest>();
            foreach(var result in results)
            {
                resp.Add(new ResultForDocumentRequest(result));
            }
            DefendantCaseResults = resp.ToArray();
        }

        private void SetDefendantCaseOffences(ProsecutionCase prosecutionCase, bool isLiveProceedings)
        {
            //make some offence results based on the prosecutionCase.OffenceResults
            var defendantCaseOffences = new List<DefendantCaseOffenceForDocumentRequest>();

            foreach(var offence in prosecutionCase.Offences)
            {
                if (offence.Results != null || (offence.IsMOJOffence && !isLiveProceedings))
                {
                    var defendantCaseOffence = new DefendantCaseOffenceForDocumentRequest(offence);
                    defendantCaseOffences.Add(defendantCaseOffence);
                }
            }

            DefendantCaseOffences = defendantCaseOffences.Count == 0 ? null : defendantCaseOffences.ToArray();
        }

        internal void SetCaseMarkersVisible(bool includeCaseMarkers)
        {
            if (!includeCaseMarkers) { this.CaseMarkers = null; }
        }
    }
}
