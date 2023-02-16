using DataLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class Offence
    {
        [JsonProperty(PropertyName = "offenceId")]
        public Guid OffenceId { get; }

        [JsonProperty(PropertyName = "originalCaseReference")]
        public string OriginalCaseReference { get; set; }

        [JsonProperty(PropertyName = "originalCaseId")]
        public Guid? OriginalCaseId { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; }

        [JsonProperty(PropertyName = "welshTitle")]
        public string WelshTitle { get; }

        [JsonProperty(PropertyName = "wording")]
        public string Wording { get; set; }

        [JsonProperty(PropertyName = "welshWording")]
        public string WelshWording { get; }

        [JsonProperty(PropertyName = "legislation")]
        public string Legislation { get; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "modeOfTrial")]
        public string ModeOfTrial { get; }

        [JsonProperty(PropertyName = "allocationDecision")]
        public string AllocationDecision { get; private set; }

        [JsonProperty(PropertyName = "isMOJOffence")]
        public bool IsMOJOffence { get; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime? StartDate { get; }

        [JsonProperty(PropertyName = "endDate")]
        public DateTime? EndDate { get; }

        [JsonProperty(PropertyName = "displayText")]
        public string DisplayText { get; }

        [JsonProperty(PropertyName = "convictionDate")]
        public DateTime? ConvictionDate { get; set; }

        [JsonProperty(PropertyName = "dvlaCode")]
        public string DVLACode { get; private set; }

        [JsonProperty(PropertyName = "plea")]
        public string Plea { get; }

        [JsonProperty(PropertyName = "vehicleRegistration")]
        public string VehicleRegistration { get; }

        [JsonProperty(PropertyName = "alcoholReadingAmount")]
        public int? AlcoholReadingAmount { get; }

        [JsonProperty(PropertyName = "alcoholReadingMethodCode")]
        public string AlcoholReadingMethodCode { get; }

        [JsonProperty(PropertyName = "alcoholReadingMethodDescription")]
        public string AlcoholReadingMethodDescription { get; }

        [JsonProperty(PropertyName = "offenceResults")]
        public List<Result> Results { get; internal set; }

        public Offence(string title, string welshTtitle, string legislation, string modeOfTrial, string code, string wording, string welshWording, DateTime? startDate, DateTime? endDate, ApplicationRequest applicationRequest)
        {
            OffenceId = Guid.NewGuid();
            Title = title;
            WelshTitle = welshTtitle;
            Legislation = legislation;
            ModeOfTrial = modeOfTrial;
            Code = code;
            Wording = wording;
            WelshWording = welshWording;
            StartDate = startDate;
            EndDate = endDate;

            if (applicationRequest == null)
            {
                DisplayText = string.Format("{0}{1}{2}{1}{3}", title, Environment.NewLine, legislation, wording);
                Plea = "Not Guilty";
                SetDVLACode();
                VehicleRegistration = "AJ18 MFY";

                AlcoholReadingMethodCode = "B";
                AlcoholReadingMethodDescription = "Breath";
                AlcoholReadingAmount = 20;
            }
            else
            {
                Plea = "Admitted/Denied, etc";
                IsMOJOffence = true;
            }
        }

        private void SetDVLACode()
        {
            var values = new List<string>()
            {
                "AC10", "AC20", "AC30", "BA10", "BA30", "BA40", "BA60", "CD10", "CD20", "CD30", "CD40", "CD50", "CD60", "CD70", "CD80", "CD90",
                "CU10", "CU20", "CU30", "CU40", "CU50", "CU80", "DD10", "DD40", "DD60", "DD80", "DD90", "DR10", "DR20", "DR30", "DR31", "DR40",
                "DR50", "DR60", "DR61", "DR70", "DG10", "DG40", "DG60", "DR80", "DR90", "IN10", "LC20", "LC30", "LC40", "LC50", "MS10", "MS20",
                "MS30", "MS50", "MS60", "MS70", "MS80", "MS90", "MW10", "PC10", "PC20", "PC30", "SP10", "SP20", "SP30", "SP40", "SP50", "TS10",
                "TS20", "TS30", "TS40", "TS50", "TS60", "TS70", "TT99", "UT50", "MR09", "MR19", "MR29", "MR39", "MR49", "MR59"
            };

            var index = HearingResults.Random.Next(values.Count - 1);
            DVLACode = values[index];
        }

        public void MakeAllocationDecision(bool isCrown)
        {
            if (string.IsNullOrEmpty(ModeOfTrial))
            {
                AllocationDecision = "No mode of trial";
            }

            if (ModeOfTrial == "S")
            {
                AllocationDecision = "Summary-only offence";
            }

            if (ModeOfTrial == "I")
            {
                AllocationDecision = "Indictable-only offence";
            }

            if (ModeOfTrial == "E")
            {
                if (isCrown)
                {
                    AllocationDecision = "Magistrates direct trial by jury";
                }
                else
                {
                    AllocationDecision = "Magistrates determine summary-only offence (Criminal Damage Offences & Active aggravated vehicle taking)";
                }                
            }
        }
    }
}

