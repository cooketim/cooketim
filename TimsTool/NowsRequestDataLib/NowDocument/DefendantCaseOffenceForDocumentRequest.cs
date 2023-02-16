using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NowsRequestDataLib.Domain;

namespace NowsRequestDataLib.NowDocument
{
    [Serializable]
    public class DefendantCaseOffenceForDocumentRequest
    {
        [JsonIgnore]
        public string OriginalCaseReference { get; set; }

        [JsonIgnore]
        public bool CourtApplicationOffence { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; }

        [JsonProperty(PropertyName = "welshTitle")]
        public string WelshTitle { get; }

        [JsonProperty(PropertyName = "wording")]
        public string Wording { get; }

        [JsonProperty(PropertyName = "welshWording")]
        public string WelshWording { get; }

        [JsonProperty(PropertyName = "legislation")]
        public string Legislation { get; }

        [JsonProperty(PropertyName = "startDate")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? StartDate { get; }

        [JsonProperty(PropertyName = "endDate")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? EndDate { get; }

        [JsonIgnore]
        public string DisplayText { get; }

        [JsonProperty(PropertyName = "convictionDate")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? ConvictionDate { get; set; }

        [JsonProperty(PropertyName = "convictionStatus")]
        public string ConvictionStatus { get; set; }

        [JsonProperty(PropertyName = "verdictType")]
        public string VerdictType { get; set; }

        [JsonProperty(PropertyName = "dvlaCode")]
        public string DVLACode { get; private set; }

        [JsonProperty(PropertyName = "plea")]
        public string Plea { get; private set; }

        [JsonProperty(PropertyName = "vehicleRegistration")]
        public string VehicleRegistration { get; private set; }

        [JsonProperty(PropertyName = "alcoholReadingAmount")]
        public int? AlcoholReadingAmount { get; }

        [JsonProperty(PropertyName = "alcoholReadingMethodCode")]
        public string AlcoholReadingMethodCode { get; }

        [JsonProperty(PropertyName = "alcoholReadingMethodDescription")]
        public string AlcoholReadingMethodDescription { get; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; }

        [JsonProperty(PropertyName = "modeOfTrial")]
        public string ModeOfTrial { get; }

        [JsonProperty(PropertyName = "allocationDecision")]
        public string AllocationDecision { get; private set; }

        [JsonProperty(PropertyName = "results")]
        public ResultForDocumentRequest[] Results { get; }

        [JsonIgnore]
        public Guid? Id { get; }

        public DefendantCaseOffenceForDocumentRequest(Offence offence)
        {
            Id = offence.OffenceId;
            OriginalCaseReference = offence.OriginalCaseReference;
            CourtApplicationOffence = offence.IsMOJOffence;
            Title = offence.Title;
            WelshTitle = offence.WelshTitle;
            Wording = offence.Wording;
            WelshWording = offence.WelshWording;
            Legislation = offence.Legislation;
            StartDate = offence.StartDate;
            EndDate = offence.EndDate;
            DisplayText = offence.DisplayText;
            ConvictionDate = offence.ConvictionDate;
            DVLACode = offence.DVLACode;
            Plea = offence.Plea;
            VehicleRegistration = offence.VehicleRegistration;
            AlcoholReadingAmount = offence.AlcoholReadingAmount;
            AlcoholReadingMethodDescription = offence.AlcoholReadingMethodDescription;
            AlcoholReadingMethodCode = offence.AlcoholReadingMethodCode;
            Code = offence.Code;
            ModeOfTrial = offence.ModeOfTrial;
            AllocationDecision = offence.AllocationDecision;
            SetVerdictType();
            if (offence.Results != null)
            {
                var results = new List<ResultForDocumentRequest>();
                offence.Results.ForEach(x => results.Add(new ResultForDocumentRequest(x)));
                Results = results.ToArray();
            }
        }

        private void SetVerdictType()
        {
            if (!ConvictionDate.HasValue)
            {
                SetNotGuiltyVerdict();
            }
            else
            {
                SetGuiltyVerdict();
            }
        }

        private void SetNotGuiltyVerdict()
        {
            var values = new List<string>()
            {
                "Found not guilty",
                "Found not guilty, by jury on judge's direction",
                "Found not guilty under section 17 Criminal Justice Act(1967): prosecution offer no evidence",
                "Found Not guilty - prosecution offers no evidence",
                "Found not guilty by reason of insanity",
                "Found not guilty by judge alone(under DVC&V Act 2004)"
            };

            var index = HearingResults.Random.Next(5);
            VerdictType = values[index];
        }

        private void SetGuiltyVerdict()
        {
            var values = new List<string>()
            {
                "Found guilty",
                "Found guilty, by jury on judge's direction",
                "Found guilty by judge alone  (under DVC&V Act 2004)",
                "Found not guilty but guilty of lesser/alternative offence not charged namely",
                "Found not guilty but guilty of lesser/alternative offence by judge's direction namely",
                "Found not guilty but guilty by Judge alone (under DVC&V Act 2004) of lesser/alternative offence not charged namely"
            };

            var index = HearingResults.Random.Next(5);
            VerdictType = values[index];
        }

        internal void SetDVLACode(bool includeDVLAOffenceCode)
        {
            if(!includeDVLAOffenceCode) { DVLACode = null; }
        }

        internal void SetCovictionStatus(bool includeConvictionStatus)
        {
            if (!includeConvictionStatus || CourtApplicationOffence) 
            { 
                ConvictionStatus = null;
                return;
            }

            if (!ConvictionDate.HasValue) 
            {
                ConvictionStatus = "NotConvicted";
            }
            else
            {
                ConvictionStatus = "Convicted";
            }
        }

        internal void SetPlea(bool includePlea)
        {
            if (!includePlea) { Plea = null; }
        }

        internal void SetVehicleRegistration(bool includeVehicleRegistration)
        {
            if (!includeVehicleRegistration) { VehicleRegistration = null; }
        }
    }
}
