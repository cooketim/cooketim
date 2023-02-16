using DataLib;
using NowsRequestDataLib.Domain;
using System;
using System.Collections.Generic;
using NowsRequestDataLib.NowDocument;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace NowsRequestDataLib
{
    public class NowDocumentRequestGenerator
    {
        public NowData GetNowRequestData(Now now, List<FixedList> fixedLists, List<NowSubscription> subscriptions, bool includeAllOptionalData, string jurisdiction, List<ResultDefinition> allRootResults, List<NowRequirement> allNowRequirements, ApplicationRequest applicationRequest)
        {
            //make some hearing results
            var hearingResults = new HearingResults(now, fixedLists, subscriptions, includeAllOptionalData, string.IsNullOrEmpty(jurisdiction) ? now.Jurisdiction : jurisdiction, allRootResults, allNowRequirements, applicationRequest);

            //make a now document request
            var req = BuildNowDocumentRequest(hearingResults, now);
            var fileName = MakeFileName(now.Name);
            string requestName = "nowRequest";
            if (applicationRequest != null && applicationRequest.IsLinkedApplication)
            {
                requestName = "applnNowRequest";
            }
            else if (applicationRequest != null)
            {
                requestName = "standaloneApplnNowRequest";
            }

            var nowDocumentRequestFileNameSuffix = string.Format("_{0}_{1}_{2}_{3}.json", hearingResults.IsCrown ? "CROWN" : "MAGS", requestName, includeAllOptionalData ? string.Empty : "SampledData", DateTime.Now.ToString("yyyy-MM-dd"));
            var nowDocumentRequestErrorsFileName = string.Format("errors_{0}_{1}_{2}_{3}.txt", hearingResults.IsCrown ? "CROWN" : "MAGS", requestName, includeAllOptionalData ? string.Empty : "SampledData", DateTime.Now.ToString("yyyy-MM-dd"));

            var hearingResultsFileNameSuffix = string.Format("_{0}_hearingResults_{1}_{2}.json", hearingResults.IsCrown ? "CROWN" : "MAGS", includeAllOptionalData ? string.Empty : "SampledData", DateTime.Now.ToString("yyyy-MM-dd"));

            return new NowData()
            {
                NowDocumentRequest = req,
                NowDocumentRequestFileName = fileName,
                NowDocumentRequestFileNameSuffix = nowDocumentRequestFileNameSuffix,
                NowDocumentRequestErrorsFileName = nowDocumentRequestErrorsFileName,

                HearingResults = hearingResults,
                HearingResultsFileName = fileName,
                HearingResultsFileNameSuffix = hearingResultsFileNameSuffix
            };
        }

        public string MakeFileName(string nowName)
        {
            //remove unwanted characters
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");
            var name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(rgx.Replace(nowName, string.Empty)).Replace(" ", string.Empty);
            return name;
        }

        private NowDocumentRequest BuildNowDocumentRequest(HearingResults hearingResults, Now now)
        {
            //create the prosecutioncases
            List<ProsecutionCaseForDocumentRequest> cases = new List<ProsecutionCaseForDocumentRequest>();
            var heardCases = hearingResults.Cases.OrderBy(x => !x.IsApplication).ToList();
            heardCases.ForEach(x => cases.Add(new ProsecutionCaseForDocumentRequest(x, hearingResults.Defendant, hearingResults.IsLiveProceedings)));

            //create the distinct set of results
            List<ResultForDocumentRequest> distinctResults = new List<ResultForDocumentRequest>();

            if (hearingResults.Defendant.DefendantResults != null)
            {
                BuildDistinctResult(distinctResults, hearingResults.Defendant.DefendantResults);
            }

            if (hearingResults.Defendant.DefendantCaseResults != null)
            {
                BuildDistinctResult(distinctResults, hearingResults.Defendant.DefendantCaseResults);
            }

            foreach (var pCase in heardCases)
            {
                if (pCase.Offences != null)
                {
                    foreach (var offence in pCase.Offences.Where(x => x.Results != null && x.Results.Count > 0))
                    {
                        BuildDistinctResult(distinctResults, offence.Results);
                    }
                }
            }

            //create the distinct set of prompts
            List<PromptForDocumentRequest> distinctPrompts = new List<PromptForDocumentRequest>();
            if (hearingResults.Defendant.DefendantResults != null)
            {
                foreach(var result in hearingResults.Defendant.DefendantResults)
                {
                    if (result.ResultPrompts != null)
                    {
                        BuildDistinctPrompt(distinctPrompts, result.ResultPrompts);
                    }
                }  
            }

            if (hearingResults.Defendant.DefendantCaseResults != null)
            {
                foreach (var result in hearingResults.Defendant.DefendantCaseResults)
                {
                    if (result.ResultPrompts != null)
                    {
                        BuildDistinctPrompt(distinctPrompts, result.ResultPrompts);
                    }
                }
            }

            foreach (var pCase in heardCases)
            {
                if (pCase.Offences != null)
                {
                    foreach (var offence in pCase.Offences.Where(x => x.Results != null && x.Results.Count > 0))
                    {
                        foreach (var result in offence.Results)
                        {
                            if (result.ResultPrompts != null)
                            {
                                BuildDistinctPrompt(distinctPrompts, result.ResultPrompts);
                            }
                        }
                    }
                }
            }

            //transform the application cases into multiple cases where the application case has offences that are not application offences
            var applicationCase = cases.FirstOrDefault(x => x.IsApplication);
            var additionalCaseIds = new List<Guid?>();
            if (applicationCase != null)
            {
                var offences = applicationCase.DefendantCaseOffences.ToList();
                var nonApplicationOffences = offences.Where(x => !x.CourtApplicationOffence).ToList();
                if (nonApplicationOffences.Count() > 0)
                {
                    foreach (var offenceGroup in nonApplicationOffences.GroupBy(x=>x.OriginalCaseReference))
                    {
                        var id = Guid.NewGuid();
                        var matchedCase = hearingResults.UnresultedCases.FirstOrDefault(x=>x.Urn == offenceGroup.Key);
                        if (matchedCase == null)
                        {
                            var pCase = new ProsecutionCaseForDocumentRequest(offenceGroup.ToList(), id, applicationCase.DefendantCaseResults);
                            additionalCaseIds.Add(id);
                            cases.Add(pCase);
                        }
                        else
                        {
                            var pCase = new ProsecutionCaseForDocumentRequest(matchedCase, offenceGroup.ToList(), applicationCase.DefendantCaseResults);
                            additionalCaseIds.Add(id);
                            cases.Add(pCase);
                        }                      
                    }
                }

                applicationCase.DefendantCaseResults = null;
                applicationCase.DefendantCaseOffences = offences.Where(x => x.CourtApplicationOffence).ToArray();
            }

            //create the document content
            var content = new NowDocumentContent
                (
                    now.Name,
                    now.WelshNowName,
                    hearingResults.OrderingCourt,
                    hearingResults.CourtClerkName,
                    hearingResults.OrderDate,
                    null,
                    hearingResults.OrderAddressee,
                    new DefendantForDocRequest(hearingResults.Defendant),
                    hearingResults.ParentGuardian,
                    hearingResults.Applicants,
                    hearingResults.Respondents,
                    hearingResults.ThirdParties,
                    hearingResults.FinancialOrderDetails,
                    hearingResults.NextHearingCourtDetails,
                    cases.ToArray(),
                    distinctResults != null && distinctResults.Count> 0 ? distinctResults.ToArray() : null,
                    distinctPrompts != null && distinctPrompts.Count> 0 ? distinctPrompts.ToArray() : null,
                    now,
                    hearingResults.CivilNow
                );

            //return the document request
            var req = new NowDocumentRequest
                (
                    hearingResults.Cases.Where(x=>!x.IsApplication).Select(x=>x.CaseId).ToList(),
                    hearingResults.Cases.Where(x => x.IsApplication).Select(x => x.CaseId).ToList(),
                    hearingResults.HearingId,
                    hearingResults.Defendant.DefendantId,
                    now.UUID,
                    new NowDistribution(null, true, null, null),
                    null,
                    content,
                    now.TemplateName,
                    now.SubTemplateName,
                    now.BilingualTemplateName,
                    hearingResults.SubscriberName,
                    now.IsStorageRequired
                );

            if (additionalCaseIds.Count>0)
            {
                if (req.cases == null)
                {
                    req.cases = new List<Guid?>();
                }
                req.cases.AddRange(additionalCaseIds);
            }

            return req;
        }

        private void BuildDistinctResult(List<ResultForDocumentRequest> distinctResults, List<Result> results)
        {
            foreach (var result in results.Where(x => x.IsDistinctResultRequired))
            {
                var match = distinctResults.FirstOrDefault(x => x.ResultIdentifier == result.ResultDefinitionId);

                if (match == null)
                {
                    distinctResults.Add(new ResultForDocumentRequest(result));
                }
                else
                {
                    //ensure that all result prompts are available
                    if (result.ResultPrompts != null && result.ResultPrompts.Count > 0)
                    {
                        foreach (var prompt in result.ResultPrompts.Where(x => !x.Hidden))
                        {
                            //determine if the prompt already exists
                            var matchedPrompt = match.Prompts.FirstOrDefault(x => x.PromptIdentifier == prompt.PromptIdentifier);
                            if (matchedPrompt != null)
                            {
                                //when a address or nameAddress type try matching again by prompt reference
                                var promptType = matchedPrompt.PromptType == null ? null : matchedPrompt.PromptType.ToLowerInvariant();
                                if (!string.IsNullOrEmpty(promptType) && (promptType == "nameaddress" || promptType == "address"))
                                {
                                    matchedPrompt = match.Prompts.FirstOrDefault(x => x.PromptReference == prompt.PromptReference);
                                }
                            }

                            if (matchedPrompt == null)
                            {
                                match.Prompts.Add(new PromptForDocumentRequest(prompt));
                            }
                        }
                    }

                    //ensure that all result text is available
                    if (result.NowText != null && result.NowText.Count>0)
                    {
                        if (match.TextValues == null)
                        {
                            match.TextValues = new List<TextValue>();
                        }
                        foreach(var textItem in result.NowText)
                        {
                            var matchedText = match.TextValues.FirstOrDefault(x => x.Label == textItem.NowReference);
                            if (matchedText == null)
                            {
                                match.TextValues.Add(new TextValue(textItem.NowReference, textItem.NowText, textItem.NowWelshText));
                            }
                        }
                    }
                }
            }
        }

        private void BuildDistinctPrompt(List<PromptForDocumentRequest> distinctPrompts, List<Domain.ResultPrompt> resultPrompts)
        {
            var distinctReq = resultPrompts.Where(x => x.IsDistinctPromptRequired && !x.Hidden).ToList(); 

            foreach (var prompt in distinctReq)
            {
                PromptForDocumentRequest matchedPrompt = null;
                var promptType = prompt.PromptType == null ? null : prompt.PromptType.ToLowerInvariant();
                if (promptType == "nameaddress" || promptType == "address")
                {
                    matchedPrompt = distinctPrompts.FirstOrDefault(x => x.PromptIdentifier == prompt.PromptIdentifier && x.PromptReference == prompt.PromptReference && x.Value == prompt.Value);
                }
                else
                {
                    //result as a prompt will not have a prompt reference
                    matchedPrompt = distinctPrompts.FirstOrDefault(x => x.PromptIdentifier == prompt.PromptIdentifier && x.Value == prompt.Value);
                }

                if (matchedPrompt == null)
                {
                    distinctPrompts.Add(new PromptForDocumentRequest(prompt));
                }
            }
        }
    }
}
