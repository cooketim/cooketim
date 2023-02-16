using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NowsRequestDataLib.Domain;

namespace NowsRequestDataLib.NowDocument
{
    [Serializable]
    public class NowOffence
    {
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

        [JsonProperty(PropertyName = "civilOffence")]
        public bool CivilOffence { get; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime? StartDate { get; }

        [JsonProperty(PropertyName = "endDate")]
        public DateTime? EndDate { get; }

        [JsonIgnore]
        public string DisplayText { get; }

        [JsonProperty(PropertyName = "convictionDate")]
        public DateTime? ConvictionDate { get; set; }

        [JsonProperty(PropertyName = "convictionStatus")]
        public string ConvictionStatus { get; set; }

        [JsonProperty(PropertyName = "dvlaCode")]
        public string DVLACode { get; private set; }

        [JsonProperty(PropertyName = "plea")]
        public string Plea { get; private set; }

        [JsonProperty(PropertyName = "vehicleRegistration")]
        public string VehicleRegistration { get; private set; }

        [JsonIgnore]
        public Guid? Id { get; }

        [JsonProperty(PropertyName = "caseReference")]
        public string CaseReference { get; }

        public NowOffence(Offence offence, string caseReference)
        {
            Id = offence.OffenceId;
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
            CaseReference = caseReference;
        }

        internal void SetDVLACode(bool includeDVLAOffenceCode)
        {
            if (!includeDVLAOffenceCode) { DVLACode = null; }
        }

        internal void SetCovictionStatus(bool includeConvictionStatus)
        {
            if (!includeConvictionStatus || CivilOffence)
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
