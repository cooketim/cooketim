using DataLib;
using Newtonsoft.Json;
using NowsRequestDataLib.Domain;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace NowsRequestDataLib.NowDocument
{
    public class NowDocumentContent
    {
        public string orderName { get; set; }
        public string welshOrderName { get; set; }
        public OrderingCourt orderingCourt { get; set; }
        public string courtClerkName { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? orderDate { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? amendmentDate { get; set; }
        public OrderAddressee orderAddressee { get; set; }
        public DefendantForDocRequest defendant { get; set; }
        public NowParty parentGuardian { get; set; }
        public List<NowParty> applicants { get; set; }
        public List<NowParty> respondents { get; set; }
        public List<NowParty> thirdParties { get; set; }
        public FinancialOrderDetails financialOrderDetails { get; }
        public NextHearingCourtDetails nextHearingCourtDetails { get; set; }
        public bool? civilNow { get; set; }

        public Guid? requestId { get; }

        public ProsecutionCaseForDocumentRequest[] cases { get; set; }

        public ResultForDocumentRequest[] distinctResults { get; set; }

        public PromptForDocumentRequest[] distinctPrompts { get; set; }

        public string[] caseApplicationReferences { get; set; }

        public List<TextValue> nowText { get; set; }
        public List<TextValue> nowRequirementText { get; set; }

        public NowDocumentContent(string orderName, string welshOrderName, OrderingCourt orderingCourt, string courtClerkName,
                                    DateTime? orderDate, DateTime? amendmentDate, OrderAddressee orderAddressee, DefendantForDocRequest defendant, NowParty parentGuardian, 
                                    List<NowParty> applicants, List<NowParty> respondents, List<NowParty> thirdParties, 
                                    FinancialOrderDetails financialOrderDetails, NextHearingCourtDetails nextHearingCourtDetails, 
                                    ProsecutionCaseForDocumentRequest[] cases, ResultForDocumentRequest[] distinctResults, PromptForDocumentRequest[] distinctPrompts, Now now, bool civilNow)
        {
            this.orderName = orderName;
            this.welshOrderName = welshOrderName;
            this.orderingCourt = orderingCourt;
            this.courtClerkName = courtClerkName;
            this.orderDate = orderDate;
            this.amendmentDate = amendmentDate;
            this.orderAddressee = orderAddressee;
            this.defendant = defendant;
            this.parentGuardian = parentGuardian;
            this.financialOrderDetails = financialOrderDetails;

            if (financialOrderDetails != null)
            {
                requestId = Guid.NewGuid();
            }

            this.nextHearingCourtDetails = nextHearingCourtDetails;
            this.civilNow = civilNow;
            this.cases = cases;
            this.distinctResults = distinctResults;
            this.distinctPrompts = distinctPrompts;
            this.applicants = applicants;
            this.respondents = respondents;
            this.thirdParties = thirdParties;

            //build the case application references
            this.caseApplicationReferences = cases.ToList().Select(x => x.Urn).ToArray();

            //build the text values
            var textValues = new List<TextValue>();
            if (now.NowTextList != null && now.NowTextList.Count > 0)
            {
                now.NowTextList.ForEach(x => textValues.Add(new TextValue(x.Reference, x.Text, x.WelshText)));
            }
            nowText = textValues.Count == 0 ? null : textValues;

            //build the collection of text across all results
            SetNowResultDefinitionsText();

            //include/exclude dynamic content
            VaryContentForNow(now);
        }

        private void GetDistinctRequirements(List<NowRequirement> nowRequirements, List<Guid> distinctResults, List<Tuple<Guid, Guid>> distinctPrompts)
        {
            foreach (var nr in nowRequirements.Where(x => x.DeletedDate == null))
            {
                if (nr.DistinctResultTypes)
                {
                    var match = distinctResults.FirstOrDefault(x => x == nr.ResultDefinition.UUID.Value);
                    if (match == null)
                    {
                        distinctResults.Add(nr.ResultDefinition.UUID.Value);
                    }
                }

                if (nr.NowRequirementPromptRules != null && nr.NowRequirementPromptRules.Count > 0)
                {
                    foreach (var nrpr in nr.NowRequirementPromptRules.Where(x => x.DeletedDate == null))
                    {
                        var match = distinctPrompts.FirstOrDefault(x => x.Item1 == nr.ResultDefinition.UUID.Value && x.Item2 == nrpr.ResultPromptRule.ResultPrompt.UUID.Value);
                        if (match == null)
                        {
                            distinctPrompts.Add(new Tuple<Guid, Guid>(nr.ResultDefinition.UUID.Value, nrpr.ResultPromptRule.ResultPrompt.UUID.Value));
                        }
                    }
                }

                if (nr.NowRequirements != null && nr.NowRequirements.Count > 0)
                {
                    GetDistinctRequirements(nr.NowRequirements, distinctResults, distinctPrompts);
                }
            }
        }

        /// <summary>
        /// Set content null based on now settings
        /// </summary>
        /// <param name="now"></param>
        private void VaryContentForNow(Now now)
        {
            //case settings
            foreach(var pCase in cases)
            {
                pCase.SetCaseMarkersVisible(now.IncludeCaseMarkers);

                if (pCase.DefendantCaseOffences != null)
                {
                    //offence settings
                    foreach (var offence in pCase.DefendantCaseOffences)
                    {
                        offence.SetDVLACode(now.IncludeDVLAOffenceCode);
                        offence.SetCovictionStatus(now.IncludeConvictionStatus);
                        offence.SetVehicleRegistration(now.IncludeVehicleRegistration);
                        offence.SetPlea(now.IncludePlea);
                    }
                }
            }

            //defendant settings
            defendant.SetASNVisible(now.IncludeDefendantASN);
            defendant.SetEthnicityVisible(now.IncludeDefendantEthnicity);
            defendant.SetNationalityVisible(now.IncludeNationality);
            defendant.SetGenderVisible(now.IncludeDefendantGender);
            defendant.SetLandlineVisible(now.IncludeDefendantLandlineNumber);
            defendant.SetMobileVisible(now.IncludeDefendantMobileNumber);
            defendant.SetNINOVisible(now.IncludeDefendantNINO);
            defendant.SetNDriverNumberVisible(now.IncludeDriverNumber);
            defendant.SetPNCId(now.IncludePNCID);
            defendant.SetSolicitorNameAddress(now.IncludeSolicitorsNameAddress);

            ////offencesByResult settings
            //if (offencesByResult != null)
            //{
            //    foreach (var result in offencesByResult)
            //    {
            //        if (result.OffencesByPrompt != null)
            //        {
            //            foreach(var prompt in result.OffencesByPrompt)
            //            {
            //                if (prompt.Offences != null)
            //                {
            //                    //offence settings
            //                    foreach (var offence in prompt.Offences)
            //                    {
            //                        offence.SetDVLACode(now.IncludeDVLAOffenceCode);
            //                        offence.SetCovictionStatus(now.IncludeConvictionStatus);
            //                        offence.SetVehicleRegistration(now.IncludeVehicleRegistration);
            //                        offence.SetPlea(now.IncludePlea);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            ////offencesByPrompt settings
            //if (offencesByPrompt != null)
            //{
            //    foreach (var prompt in offencesByPrompt)
            //    {
            //        if (prompt.Offences != null)
            //        {
            //            //offence settings
            //            foreach (var offence in prompt.Offences)
            //            {
            //                offence.SetDVLACode(now.IncludeDVLAOffenceCode);
            //                offence.SetCovictionStatus(now.IncludeConvictionStatus);
            //                offence.SetVehicleRegistration(now.IncludeVehicleRegistration);
            //                offence.SetPlea(now.IncludePlea);
            //            }
            //        }
            //    }
            //}
        }

        private void SetNowResultDefinitionsText()
        {
            //build the text values based on text values across all cases
            var textValues = new List<TextValue>();

            foreach(var prosecutionCase in cases)
            {
                //collect defendant case text
                if (prosecutionCase.DefendantCaseOffences != null && prosecutionCase.DefendantCaseOffences.Length > 0)
                {
                    foreach(var dco in prosecutionCase.DefendantCaseOffences)
                    {
                        if (dco.Results != null && dco.Results.Length > 0)
                        {
                            foreach(var res in dco.Results)
                            {
                                if (res.TextValues != null && res.TextValues.Count > 0)
                                {
                                    foreach(var textItem in res.TextValues)
                                    {
                                        SetText(textValues, textItem);
                                    }
                                }
                            }
                        }
                    }
                }

                //collect defendant case result text
                if (prosecutionCase.DefendantCaseResults != null && prosecutionCase.DefendantCaseResults.Length > 0)
                {
                    foreach (var res in prosecutionCase.DefendantCaseResults)
                    {
                        if (res.TextValues != null && res.TextValues.Count > 0)
                        {
                            foreach (var textItem in res.TextValues)
                            {
                                SetText(textValues, textItem);
                            }
                        }
                    }
                }
            }

            nowRequirementText = textValues.Count == 0 ? null : textValues;
        }

        private void SetText(List<TextValue> textValues, TextValue textItem)
        {
            var match = textValues.FirstOrDefault(x => x.Label == textItem.Label);
            if(match == null)
            {
                textValues.Add(textItem);
            }
        }
    }
}
