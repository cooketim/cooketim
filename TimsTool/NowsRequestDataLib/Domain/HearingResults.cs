using DataLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public partial class HearingResults
    {
        [JsonProperty(PropertyName = "hearingId")]
        public Guid? HearingId { get; }

        [JsonProperty(PropertyName = "defendant")]
        public Defendant Defendant { get; private set; }

        [JsonProperty(PropertyName = "parentGuardian")]
        public NowParty ParentGuardian { get; private set; }

        [JsonProperty(PropertyName = "applicant")]
        public List<NowParty> Applicants { get; private set; }

        [JsonProperty(PropertyName = "respondents")]
        public List<NowParty> Respondents { get; private set; }

        [JsonProperty(PropertyName = "thirdParties")]
        public List<NowParty> ThirdParties { get; private set; }

        [JsonProperty(PropertyName = "isCrown")]
        public bool IsCrown { get; private set; }

        [JsonProperty(PropertyName = "isYouth")]
        public bool IsYouth { get; private set; }

        [JsonProperty(PropertyName = "orderingCourt")]
        public OrderingCourt OrderingCourt { get; }

        [JsonProperty(PropertyName = "courtClerkName")]
        public string CourtClerkName { get; private set; }

        [JsonProperty(PropertyName = "orderDate")]
        public DateTime? OrderDate { get; private set; }

        [JsonProperty(PropertyName = "financialOrderDetails")]
        public FinancialOrderDetails FinancialOrderDetails { get; private set; }

        [JsonProperty(PropertyName = "nextHearingCourtDetails")]
        public NextHearingCourtDetails NextHearingCourtDetails { get; private set; }

        [JsonProperty(PropertyName = "civilNow")]
        public bool CivilNow { get; private set; }

        [JsonProperty(PropertyName = "cases")]
        public List<ProsecutionCase> Cases { get; private set; }

        [JsonProperty(PropertyName = "unresultedCases")]
        public List<ProsecutionCase> UnresultedCases { get; private set; }

        [JsonProperty(PropertyName = "informantAddress")]
        public NowAddress InformantAddress { get => Cases[0].ProsecutorAddress; }

        [JsonProperty(PropertyName = "informant")]
        public string Informant { get => Cases[0].Prosecutor; }

        [JsonIgnore]
        public List<NowRequirement> SelectedRootResults { get; private set; }

        [JsonProperty(PropertyName = "selectedNonNowResults")]
        public List<ResultDefinition> SelectedNonNowResults { get; private set; }

        [JsonProperty(PropertyName = "selectedRootNowRequirements")]
        public List<SelectedNowRequirement> SelectedRootNowRequirements
        {
            get
            {
                if (SelectedRootResults == null || SelectedRootResults.Count == 0) { return null; }

                var res = new List<SelectedNowRequirement>();
                SelectedRootResults.ForEach(x => res.Add(new SelectedNowRequirement(x)));
                return res;
            }
        }

        [JsonProperty(PropertyName = "results")]
        public List<Result> Results { get; private set; }

        [JsonProperty(PropertyName = "continuedProceedingsApplicationResults")]
        public List<Result> ContinuedProceedingsApplicationResults { get; private set; }

        [JsonProperty(PropertyName = "isLiveProceedings")]
        public bool IsLiveProceedings { get; private set; }

        [JsonProperty(PropertyName = "liveProceedingsApplicationResults")]
        public List<Result> LiveProceedingsApplicationResults { get; private set; }


        [JsonProperty(PropertyName = "selectedResults")]
        public List<Result> SelectedResults
        {
            get
            {
                if (Results == null || Results.Where(x => x.IsSelected).Count() == 0) { return null; }
                return Results.Where(x => x.IsSelected).OrderBy(x => x.TreeLevel).ToList();
            }
        }

        [JsonProperty(PropertyName = "publishedResults")]
        public List<Result> PublishedResults
        {
            get
            {
                if (Results == null || Results.Where(x => x.IsPublished).Count() == 0) { return null; }
                return Results.Where(x => x.IsPublished).OrderBy(x => x.TreeLevel).ToList();
            }
        }

        [JsonProperty(PropertyName = "orderAddressee")]
        public OrderAddressee OrderAddressee { get; private set; }

        [JsonProperty(PropertyName = "subscriberName")]
        public string SubscriberName { get; private set; }

        [JsonProperty(PropertyName = "urns")]
        public string[] URNs { get; private set; }

        public static Random RandomSwitch = new Random();

        public static Random Random = new Random();

        public HearingResults(Now now, List<FixedList> fixedLists, List<NowSubscription> subscriptions, bool includeAllOptionalData, string jurisdiction, List<ResultDefinition> allResults, List<NowRequirement> allNowRequirements, ApplicationRequest applicationRequest)
        {
            //Set crown or mags hearing
            IsCrown = false;
            if (string.IsNullOrEmpty(jurisdiction) || jurisdiction.ToLowerInvariant() == "both")
            {
                IsCrown = Switch();
            }
            else if (jurisdiction.ToLowerInvariant().StartsWith("crown"))
            {
                IsCrown = true;
            }

            //Determine if a civilNow
            CivilNow = applicationRequest != null && !applicationRequest.IsLinkedApplication;

            //Create the ordering court
            OrderingCourt = new OrderingCourt(IsCrown, Switch());

            //Initiate Court Proceedings
            SimulateCreateCase(applicationRequest);

            //Simulate initiate hearing
            HearingId = Guid.NewGuid();

            //make the results based on now requirements and the application requirements
            SimulateHearing(now, includeAllOptionalData, allResults, allNowRequirements, applicationRequest);

            //Randomly select the results based on optional/mandatory business rules.  Create the results values
            SimulateResulting(fixedLists, includeAllOptionalData, now);

            //publish the results
            SimulateShareResults(now, subscriptions);
        }

        private void SimulateCreateCase(ApplicationRequest applicationRequest)
        {
            IsYouth = Switch();

            CourtClerkName = "Alison Karen Mathews";
            OrderDate = DateTime.Now.Date;
            Cases = new List<ProsecutionCase>();
            UnresultedCases = new List<ProsecutionCase>();

            if (applicationRequest == null || applicationRequest.IsLinkedApplication)
            {
                //not an application request or is an application that is linked to a case, therefore a defendant from a case is required
                var inCustody = Switch();
                var policeCustody = inCustody ? Switch() : false;
                var prisonCustody = inCustody && !policeCustody ? true : false;
                var appearedInPerson = inCustody ? Switch() : true;
            
                Defendant = new Defendant(IsYouth, appearedInPerson, prisonCustody, policeCustody, false);
                
            }
            else
            {
                //application request that is not linked to a case i.e. is a standalone
                Defendant = new Defendant(IsYouth, true, false, false, true);
            }

            MakeParentGuardian();

            //when application is required a separate case will be created with a single moj offence
            MakeCriminalCases(OrderDate, applicationRequest);

            //Set application details for the hearing
            foreach(var pCase in Cases.Where(x=>x.IsApplication))
            {
                if (Applicants == null)
                {
                    Applicants = new List<NowParty>() { pCase.Applicant };
                }
                else
                {
                    Applicants.Add(pCase.Applicant);
                }

                if (pCase.Respondents != null && pCase.Respondents.Count > 0)
                {
                    if (Respondents == null)
                    {
                        Respondents = new List<NowParty>();
                        Respondents.AddRange(pCase.Respondents);
                    }
                    else
                    {
                        Respondents.AddRange(pCase.Respondents);
                    }
                }

                if (pCase.ThirdParties != null && pCase.ThirdParties.Count > 0)
                {
                    if (ThirdParties == null)
                    {
                        ThirdParties = new List<NowParty>();
                        ThirdParties.AddRange(pCase.ThirdParties);
                    }
                    else
                    {
                        ThirdParties.AddRange(pCase.ThirdParties);
                    }
                }
            }

            //in reality this would be taken from results
            NextHearingCourtDetails = new NextHearingCourtDetails();
            NextHearingCourtDetails.hearingDate = DateTime.Now.AddDays(73).ToString("dd/MM/yyyy");
            NextHearingCourtDetails.hearingTime = "10.30 AM";
            NextHearingCourtDetails.roomName = "Room 1";
            NextHearingCourtDetails.welshRoomName = "welsh~Room 1";
            NextHearingCourtDetails.courtAddress = new NowAddress("Liverpool Combined Court Centre", "Derby Square", "Liverpool", "Merseyside", null, "L2 1XA", null, null);

            if (IsCrown)
            {
                NextHearingCourtDetails.courtName = "Liverpool Crown Court";
            }
            else
            {
                NextHearingCourtDetails.courtName = "Liverpool Magistrates' Court";
            }
        }

        private void MakeParentGuardian()
        {
            if (!string.IsNullOrEmpty(Defendant.ParentGuardianName))
            {
                ParentGuardian = new NowParty
                                    (
                                        Defendant.ParentGuardianName,
                                        Defendant.ParentGuardianAddress,
                                        Defendant.ParentGuardianDateOfBirth,
                                        "07973 124875",
                                        "01922 769054"
                                    );
            }
        }

        private void MakeCriminalCases(DateTime? orderDate, ApplicationRequest applicationRequest)
        {
            if (applicationRequest != null)
            {
                if (applicationRequest.IsLinkedApplication)
                {
                    //application linked to two case URNs
                    URNs = new string[3] { "41C12017614", "51C12017615", "41C12017614, 51C12017615" };
                }
                else
                {
                    //an application reference
                    URNs = new string[1] { "SA00000001" };
                }    
            }
            else
            {
                var multipleURNs = Switch();
                URNs = multipleURNs ? new string[2] { "41C12017614", "51C12017615" } : new string[1] { "41C12017614" };
            }

            var bedsAddress = new NowAddress("Police HQ - Bedfordshire", "Woburn Road", "Kempston", "Bedfordshire", null, "MK43 9AX", "nows@bedfordshire.police.uk", null);
            var hertsAddress = new NowAddress("Hertfordshire Constabulary HQ", "Stanborough Rd", "Welwyn Garden City", "Hertfordshire", null, "AL8 6XF", "nows@hertfordshire.police.uk", null);

            for (int i = 0; i < URNs.Length; i++)
            {
                string urn = URNs[i];
                var applicationCase = i>=2 || (applicationRequest != null && !applicationRequest.IsLinkedApplication);
                Cases.Add(new ProsecutionCase(IsCrown, urn, orderDate, urn == "41C12017614" ? "Hertfordshire Constabulary" : "Bedfordshire Constabulary",
                                                    urn == "41C12017614" ? "OU1234" : "OU5678",
                                                    urn == "41C12017614" ? hertsAddress : bedsAddress,
                                                    applicationCase ? applicationRequest : null, Defendant
                                                    ));
            }

            //when the case is for an application linked to criminal cases, simulate the process of cloning the offences from prosecution cases
            if (applicationRequest != null && applicationRequest.IsLinkedApplication)
            {
                if (applicationRequest.IsResentenceOrActivation)
                {
                    var target = Cases.FirstOrDefault(x => x.IsApplication);

                    foreach (var pc in Cases.Where(x => !x.IsApplication))
                    {
                        foreach (var offence in pc.Offences)
                        {
                            offence.OriginalCaseReference = pc.Urn;
                            offence.OriginalCaseId = pc.CaseId;
                            //set the wording & code ready for resentencing
                            offence.Wording = string.Format("Resentence.  Original Code: '{0}', Original Details: '{1}'", offence.Code, offence.Wording);
                            offence.Code = "Resentence Code from app ref data";
                            offence.ConvictionDate = OrderDate;
                            target.Offences.Add(offence);
                        }
                    }
                }

                //remove the unwanted cases when not live proceedings
                if (!applicationRequest.IsLiveProceedings)
                {
                    UnresultedCases.AddRange(Cases.Where(x => !x.IsApplication));
                    Cases.RemoveAll(x => !x.IsApplication);
                }

                //see if the application has been heard at the same time as another case
                if (applicationRequest.IsHeardWithOtherCases)
                {
                    Cases.Add(new ProsecutionCase(IsCrown, "41C1201782", orderDate, "Hertfordshire Constabulary",
                                                    "OU1234",
                                                    hertsAddress,
                                                    null, Defendant));
                    var urns = new List<string>(URNs);
                    urns.Add("41C1201782");
                    URNs = urns.ToArray();
                }
            }
        }

        private void SimulateHearing(Now now, bool includeAllOptionalData, List<ResultDefinition> allResults, List<NowRequirement> allNowRequirements, ApplicationRequest applicationRequest)
        {
            //perform mode of trial
            foreach(var prosecutionCase in Cases.Where(x=>x.Offences != null))
            {
                foreach(var offence in prosecutionCase.Offences)
                {
                    offence.MakeAllocationDecision(IsCrown);
                }
            }

            //Select the results based on the primary results and the non primary results
            SelectResults(now, includeAllOptionalData, allResults, allNowRequirements);

            //Make a set of flattened results for the selected root Results
            Results = new List<Result>();

            //Make results based on the tree of results for each of the NOW Requirements
            MakeNOWResults();

            if (now.IncludeAllResults)
            {
                MakeNonNOWResults(allResults);
            }

            if (applicationRequest != null)
            {
                //deal with the application results

                if (applicationRequest.IsResentenceOrActivation)
                {
                    ContinuedProceedingsApplicationResults = new List<Result>();

                    var revokedOrder = allResults.FirstOrDefault(x => x.ShortCode == "OREV");

                    var result = new Result(revokedOrder);

                    ContinuedProceedingsApplicationResults.Add(result);
                }

                if (applicationRequest.IsLiveProceedings)
                {
                    IsLiveProceedings = true;
                    LiveProceedingsApplicationResults = new List<Result>();

                    //unknown result for this scenario ??????
                }
            }
        }

        private void MakeNOWResults()
        {
            Results = new List<Result>();
            foreach (var req in SelectedRootResults)
            {
                RecursivelyMakeNOWResults(req.ResultDefinition, req, 0);
            }

            //check that we have the full set of parents for any children that are primary results
            ResultParentage(0);

            //adjust the tree levels
            var rootRequirementGrouping = Results.GroupBy(x => x.RootResultDefinitionId).ToList();
            foreach (var rootResultGroup in rootRequirementGrouping)
            {
                var parentsForAdjustment = rootResultGroup.Where(x => x.TreeLevel < 0).ToList();
                if (parentsForAdjustment.Count > 0)
                {
                    int adjustment = parentsForAdjustment.Select(x => x.TreeLevel).Min() * -1;

                    //adjust the children first
                    var children = rootResultGroup.Where(x => x.ParentResultDefinition != null && x.TreeLevel == 0).ToList();
                    children.ForEach(x => x.TreeLevel = adjustment);

                    //adjust the parents
                    parentsForAdjustment.ForEach(x => x.TreeLevel = x.TreeLevel + adjustment);

                    //ensure that we have all of the results for each root
                    foreach (var root in parentsForAdjustment.Where(x => x.TreeLevel == 0))
                    {
                        RecursivelyMakeNOWResults(root.RootResultDefinition, root.NowRequirement, 0);
                    }
                }
            }
        }

        private void MakeNonNOWResults(List<ResultDefinition> allResults)
        {
            foreach (var rd in SelectedNonNowResults)
            {
                RecursivelyMakeNonNOWResults(rd, rd, null, 0, null, allResults);
            }

            ////check that we have the full set of parents for any children that are primary results
            //ResultParentage(0);

            ////adjust the tree levels
            //var rootRequirementGrouping = Results.Where(x=>x.NowRequirement == null).GroupBy(x => x.RootResultDefinitionId).ToList();
            //foreach (var rootResultGroup in rootRequirementGrouping)
            //{
            //    var parentsForAdjustment = rootResultGroup.Where(x => x.TreeLevel < 0).ToList();
            //    if (parentsForAdjustment.Count > 0)
            //    {
            //        int adjustment = parentsForAdjustment.Select(x => x.TreeLevel).Min() * -1;

            //        //adjust the children first
            //        var children = rootResultGroup.Where(x => x.ParentResultDefinition != null && x.TreeLevel == 0).ToList();
            //        children.ForEach(x => x.TreeLevel = adjustment);

            //        //adjust the parents
            //        parentsForAdjustment.ForEach(x => x.TreeLevel = x.TreeLevel + adjustment);

            //        //ensure that we have all of the results for each root
            //        foreach (var root in parentsForAdjustment.Where(x => x.TreeLevel == 0))
            //        {
            //            RecursivelyMakeNonNOWResults(root.RootResultDefinition, root.ResultDefinition, root.ParentResultDefinition, 0);
            //        }
            //    }
            //}
        }

        private void ResultParentage(int treeLevel)
        {
            var children = Results.Where(x => x.ParentResultDefinition != null && x.TreeLevel == treeLevel).ToList();

            if (children.Count > 0)
            {

                foreach (var child in children)
                {
                    ResultParent(child);
                }

                ResultParentage(treeLevel - 1);
            }
        }

        private void ResultParent(Result child)
        {
            var match = Results.FirstOrDefault(x => x.ResultDefinitionId == child.ResultDefinitionRule.ParentUUID
                                                 && x.RootResultDefinitionId == child.RootResultDefinitionId
            );

            if (match == null)
            {
                ResultDefinitionRule rule = null;
                if (child.NowRequirement.ParentNowRequirement.ParentNowRequirement != null)
                {
                    rule = child.NowRequirement.ParentNowRequirement.ParentNowRequirement.ResultDefinition.ResultDefinitionRules.FirstOrDefault(x => x.ResultDefinition.UUID == child.NowRequirement.ParentNowRequirement.ResultDefinition.UUID);
                }

                Results.Add(new Result(child.TreeLevel - 1, child.RootResultDefinition, child.NowRequirement.ParentNowRequirement, rule));
            }
        }

        private void SimulateResulting(List<FixedList> fixedLists, bool includeAllOptionalData, Now now)
        {
            //Select the results for each level based on the result definition rules
            SelectResultsBasedOnResultDefinitionRules(includeAllOptionalData);

            //Select the result prompts for selected result based on the result prompt rules
            SelectResultPrompts(includeAllOptionalData, now);

            //Select any continued proceedings results
            if (ContinuedProceedingsApplicationResults != null)
            {
                foreach (var item in ContinuedProceedingsApplicationResults)
                {
                    item.IsSelected = true;
                    SelectResultPromptsForResult(includeAllOptionalData, item);
                }                
            }

            //Select any live proceedings results
            if (LiveProceedingsApplicationResults != null)
            {
                foreach (var item in LiveProceedingsApplicationResults)
                {
                    item.IsSelected = true;
                    SelectResultPromptsForResult(includeAllOptionalData, item);
                }
            }

            //Generate result values and apply results to case, defendant and offence domain objects
            GenerateResultValues(fixedLists);

            //Set the conviction status
            SetConvictionStatus();
        }

        private void SetConvictionStatus()
        {
            Results.Where(x => x.IsSelected && x.ResultDefinition.Convicted).ToList().ForEach(x => x.IsConvicted = true);

            if(ContinuedProceedingsApplicationResults != null)
            {
                ContinuedProceedingsApplicationResults.Where(x => x.IsSelected && x.ResultDefinition.Convicted).ToList().ForEach(x => x.IsConvicted = true);
            }

            if (LiveProceedingsApplicationResults != null)
            {
                LiveProceedingsApplicationResults.Where(x => x.IsSelected && x.ResultDefinition.Convicted).ToList().ForEach(x => x.IsConvicted = true);
            }
        }

        private void SelectResultPrompts(bool includeAllOptionalData, Now now)
        {
            var defendantLevelResults = new List<Result>();
            var defendantCaseLevelResults = new List<Result>();
            foreach (var item in Results
                    .Where(x => x.IsSelected &&
                                x.ResultPrompts != null && x.ResultPrompts.Count > 0
                           ))
            {
                //if the result is a defendant level or a case defendant level result, select the same prompts across each duplicate instance to simulate that they are the same result
                if (item.ResultDefinition.LevelFirstLetter.ToLowerInvariant() == "d")
                {
                    var match = defendantLevelResults.FirstOrDefault(x => x.ResultDefinitionId == item.ResultDefinitionId);
                    if (match != null)
                    {
                        SelectSamePrompts(match, item, now);
                        continue;
                    }
                    else
                    {
                        defendantLevelResults.Add(item);
                    }
                }
                if (item.ResultDefinition.LevelFirstLetter.ToLowerInvariant() == "c")
                {
                    var match = defendantCaseLevelResults.FirstOrDefault(x => x.ResultDefinitionId == item.ResultDefinitionId);
                    if (match != null)
                    {
                        SelectSamePrompts(match, item, now);
                        continue;
                    }
                    else
                    {
                        defendantLevelResults.Add(item);
                    }
                }

                SelectResultPromptsForResult(includeAllOptionalData, item);
            }
        }

        private static void SelectResultPromptsForResult(bool includeAllOptionalData, Result item)
        {
            var optionalItems = item.ResultPrompts.Where(x => x.ResultPromptRule.Rule == ResultPromptRuleType.optional).ToList();
            var oneOfItems = item.ResultPrompts.Where(x => x.ResultPromptRule.Rule == ResultPromptRuleType.oneOf).ToList();
            var mandatoryItems = item.ResultPrompts.Where(x => x.ResultPromptRule.Rule == ResultPromptRuleType.mandatory).ToList();

            //first deal with optionality
            var optionalsRequired = true;
            if ((oneOfItems.Count > 0 || mandatoryItems.Count > 0) && !includeAllOptionalData) { optionalsRequired = Switch(); }
            if (optionalsRequired)
            {
                if (optionalItems != null && optionalItems.Count > 0)
                {
                    if (includeAllOptionalData)
                    {
                        optionalItems.ForEach(x => x.IsSelected = true);
                    }
                    else
                    {
                        //number of optionals
                        var numberItems = Random.Next(optionalItems.Count - 1) + 1;

                        //select random items
                        var indices = new List<int>();
                        while (indices.Count < numberItems)
                        {
                            var index = Random.Next(optionalItems.Count - 1);
                            if (!indices.Contains(index))
                            {
                                indices.Add(index);
                                optionalItems[index].IsSelected = true;
                            }
                        }
                    }
                }
            }

            //deal with the oneOfs
            if (oneOfItems != null && oneOfItems.Count > 0)
            {
                //select the OneOf
                var selectedIndex = Random.Next(oneOfItems.Count - 1);

                oneOfItems[selectedIndex].IsSelected = true;
            }

            //select the mandatory items
            mandatoryItems.ForEach(x => x.IsSelected = true);

            //deal with any incomplete address items
            var nameAddressItem = item.SelectedResultPrompts.FirstOrDefault(x => x.ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress");
            if (nameAddressItem != null)
            {
                var toSelect = item.ResultPrompts.FindAll(x => x.ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && !x.IsSelected).ToList();
                toSelect.ForEach(x => x.IsSelected = true);
            }

            var addressItem = item.SelectedResultPrompts.FirstOrDefault(x => x.ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "address");
            if (addressItem != null)
            {
                var toSelect = item.ResultPrompts.FindAll(x => x.ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "address" && !x.IsSelected);
                toSelect.ForEach(x => x.IsSelected = true);
            }
        }

        private void SelectSamePrompts(Result source, Result target, Now now)
        {
            foreach (var sourceItem in source.SelectedResultPrompts)
            {
                var sourcePromptType = sourceItem.ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant();
                ResultPrompt targetPrompt = null;
                if (sourcePromptType == "nameaddress" || sourcePromptType == "address")
                {
                    //match by prompt ref
                    targetPrompt = target.ResultPrompts.FirstOrDefault(x => x.ResultPromptRule.ResultPrompt.PromptReference == sourceItem.ResultPromptRule.ResultPrompt.PromptReference);
                    targetPrompt.IsSelected = true;
                }
                else
                {
                    targetPrompt = target.ResultPrompts.FirstOrDefault(x => x.ResultPromptRule.ResultPromptUUID == sourceItem.ResultPromptRule.ResultPromptUUID);
                    targetPrompt.IsSelected = true;
                }
            }
        }

        private void SelectResultsBasedOnResultDefinitionRules(bool includeAllOptionalData)
        {
            var resultsForJurisdiction = Results.Where(x => x.ResultDefinition.Jurisdiction.ToLowerInvariant().StartsWith("b") || x.ResultDefinition.Jurisdiction.ToLowerInvariant().StartsWith(IsCrown ? "c" : "m")).ToList();
            var rootRequirementGrouping = resultsForJurisdiction.GroupBy(x => x.RootResultDefinitionId).ToList();
            foreach (var rootResultGroup in rootRequirementGrouping)
            {
                var resultGrouping = rootResultGroup.GroupBy(x => x.TreeLevel).ToList();
                foreach (var group in resultGrouping)
                {
                    var optionalItems = group.Where(x => x.ResultDefinitionRule != null && x.ResultDefinitionRule.Rule == ResultDefinitionRuleType.optional).ToList();
                    var atleastOneOfItems = group.Where(x => x.ResultDefinitionRule != null && x.ResultDefinitionRule.Rule == ResultDefinitionRuleType.atleastOneOf).ToList();
                    var oneOfItems = group.Where(x => x.ResultDefinitionRule != null && x.ResultDefinitionRule.Rule == ResultDefinitionRuleType.oneOf).ToList();
                    var mandatoryItems = group.Where(x => x.ResultDefinitionRule != null && x.ResultDefinitionRule.Rule == ResultDefinitionRuleType.mandatory).ToList();
                    var parentOptionalItems = group.Where(x => x.ResultDefinitionRule == null && !x.IsMandatoryResult).ToList();
                    var parentMandatoryItems = group.Where(x => x.ResultDefinitionRule == null && x.IsMandatoryResult).ToList();

                    //first deal with optionality
                    var optionalsRequired = true;
                    if ((atleastOneOfItems.Count > 0 || oneOfItems.Count > 0 || mandatoryItems.Count > 0 || parentMandatoryItems.Count > 0) && !includeAllOptionalData) { optionalsRequired = Switch(); }
                    if (optionalsRequired)
                    {
                        if (optionalItems.Count > 0)
                        {
                            if (includeAllOptionalData)
                            {
                                optionalItems.ForEach(x=>SelectResult(x));
                            }
                            else
                            {
                                //number of optionals
                                var numberItems = Random.Next(optionalItems.Count-1)+1;

                                //select random items
                                var indices = new List<int>();
                                while (indices.Count < numberItems)
                                {
                                    var index = Random.Next(optionalItems.Count - 1);
                                    if (!indices.Contains(index))
                                    {
                                        indices.Add(index);
                                        SelectResult(optionalItems[index]);
                                    }
                                }
                            }
                        }

                        if (parentOptionalItems.Count > 0)
                        {
                            if (includeAllOptionalData)
                            {
                                parentOptionalItems.ForEach(x => SelectResult(x));
                            }
                            else
                            {
                                //number of optionals
                                var numberItems = Random.Next(parentOptionalItems.Count-1)+1;

                                //select random items
                                var indices = new List<int>();
                                while (indices.Count < numberItems)
                                {
                                    var index = Random.Next(parentOptionalItems.Count - 1);
                                    if (!indices.Contains(index))
                                    {
                                        indices.Add(index);
                                        SelectResult(parentOptionalItems[index]);
                                    }
                                }
                            }
                        }
                    }

                    //deal with the atleast oneOfs
                    if (atleastOneOfItems.Count > 0)
                    {
                        //group the at least oneOfs by their parents
                        var atleastOneOfItemGroups = atleastOneOfItems.GroupBy(x => x.ResultDefinitionRule.ParentUUID).ToList();
                        foreach (var atleastOneOfItemGroup in atleastOneOfItemGroups)
                        {
                            //number of at least oneOfs
                            var numberItems = Random.Next(atleastOneOfItemGroup.Count()-1)+1;

                            //select random items
                            var indices = new List<int>();
                            while (indices.Count < numberItems)
                            {
                                var index = Random.Next(atleastOneOfItemGroup.Count() - 1);
                                if (!indices.Contains(index))
                                {
                                    indices.Add(index);
                                    SelectResult(atleastOneOfItemGroup.ToArray()[index]);
                                }
                            }
                        }
                    }

                    //deal with the oneOfs
                    if (oneOfItems.Count > 0)
                    {
                        //group the oneOfs by their parents
                        var oneOfItemGroups = oneOfItems.GroupBy(x => x.ResultDefinitionRule.ParentUUID).ToList();
                        foreach (var oneOfItemGroup in oneOfItemGroups)
                        {
                            //select the one of item randomly
                            var index = Random.Next(oneOfItemGroup.Count() - 1);
                            SelectResult(oneOfItemGroup.ToArray()[index]);
                        }
                    }

                    //select the mandatory items
                    mandatoryItems.ForEach(x => SelectResult(x));
                    parentMandatoryItems.ForEach(x => SelectResult(x));
                }
            }
        }

        private void SelectResult(Result result)
        {
            //ensure that we only have one instance of a defendant result and a case defendant result selected
            if (SelectedResults == null || SelectedResults.Count == 0)
            {
                result.IsSelected = true;
                return;
            }
            if (result.ResultDefinition.LevelFirstLetter.ToLowerInvariant() == "d" || result.ResultDefinition.LevelFirstLetter.ToLowerInvariant() == "c")
            {
                var match = SelectedResults.FirstOrDefault(x => x.ResultDefinitionId == result.ResultDefinitionId);
                if (match == null)
                {
                    result.IsSelected = true;
                }
                return;
            }
            result.IsSelected = true;
        }

        private void GenerateResultValues(List<FixedList> fixedLists)
        {
            if (SelectedResults == null || SelectedResults.Count == 0)
            {
                //nothing to do
                return;
            }
            //create result prompt values for each defendant, case, offence combination
            var defendantLevelResults = new List<Result>();
            var defendantCaseLevelResults = new List<Result>();
            var selectedOffenceResults = SelectedResults.Where(x => x.Level.ToLowerInvariant() == "o").ToList();
            var selectedDefendantCaseResults = SelectedResults.Where(x => x.Level.ToLowerInvariant() == "c").ToList();
            var selectedDefendantResults = SelectedResults.Where(x => x.Level.ToLowerInvariant() == "d").ToList();
            var nonVariantPromptValues = new StringDictionary();
            foreach (var prosecutionCase in Cases)
            {
                if (prosecutionCase.IsApplication && (prosecutionCase.Offences == null || prosecutionCase.Offences.Count == 0))
                {
                    GenerateResultValuesForApplication(fixedLists, defendantLevelResults, defendantCaseLevelResults, selectedOffenceResults, selectedDefendantCaseResults, selectedDefendantResults, nonVariantPromptValues, prosecutionCase);
                }
                else
                {
                    GenerateResultValuesByOffence(fixedLists, defendantLevelResults, defendantCaseLevelResults, selectedOffenceResults, selectedDefendantCaseResults, selectedDefendantResults, nonVariantPromptValues, prosecutionCase);
                }
            }

            //create prompt values for the continuedProceedings
            if (ContinuedProceedingsApplicationResults != null)
            {
                foreach (var prosecutionCase in Cases.Where(x=>x.IsApplication))
                {
                    GenerateResultValuesForApplicationProceedings(fixedLists, ContinuedProceedingsApplicationResults, nonVariantPromptValues, prosecutionCase);
                }
            }

            //create prompt values for the liveProceedings
            if (LiveProceedingsApplicationResults != null)
            {
                foreach (var prosecutionCase in Cases.Where(x => x.IsApplication))
                {
                    GenerateResultValuesForApplicationProceedings(fixedLists, LiveProceedingsApplicationResults, nonVariantPromptValues, prosecutionCase);
                }
            }
        }

        private void GenerateResultValuesForApplicationProceedings(List<FixedList> fixedLists, List<Result> applicationProceedingsApplicationResults, StringDictionary nonVariantPromptValues, ProsecutionCase prosecutionCase)
        {
            foreach (var result in applicationProceedingsApplicationResults)
            {
                foreach (var offence in prosecutionCase.Offences.Where(x=>x.IsMOJOffence))
                {
                    if (result.SelectedResultPrompts != null)
                    {
                        foreach (var prompt in result.SelectedResultPrompts)
                        {
                            if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                            {
                                var key = GetKey(prompt);
                                if (nonVariantPromptValues.ContainsKey(key))
                                {
                                    prompt.AppendValue(Defendant, null, null, fixedLists, nonVariantPromptValues[key]);
                                }
                                else
                                {
                                    var val = prompt.AppendValue(Defendant, null, null, fixedLists, null);
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        nonVariantPromptValues.Add(key, val);
                                    }
                                }
                                continue;
                            }
                            prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                        }
                    }
                }
            }
        }

        private void GenerateResultValuesForApplication(List<FixedList> fixedLists, List<Result> defendantLevelResults, List<Result> defendantCaseLevelResults, List<Result> selectedOffenceResults, List<Result> selectedDefendantCaseResults, List<Result> selectedDefendantResults, StringDictionary nonVariantPromptValues, ProsecutionCase prosecutionCase)
        {
            //deal with defendant level results
            foreach (var result in selectedDefendantResults)
            {
                //only one instance of the defendant level result per defendant and we only have one defendant
                var match = defendantLevelResults.FirstOrDefault(x => x.ResultDefinitionId == result.ResultDefinitionId);
                if (match == null)
                {
                    if (result.SelectedResultPrompts != null)
                    {
                        foreach (var prompt in result.SelectedResultPrompts)
                        {
                            if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                            {
                                var key = GetKey(prompt);
                                if (nonVariantPromptValues.ContainsKey(key))
                                {
                                    prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, nonVariantPromptValues[key]);
                                }
                                else
                                {
                                    var val = prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, null);
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        nonVariantPromptValues.Add(key, val);
                                    }
                                }
                                continue;
                            }
                            prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, null);
                        }
                    }
                    defendantLevelResults.Add(result);
                    continue;
                }
            }

            //deal with defendant case level results
            foreach (var result in selectedDefendantCaseResults)
            {
                var match = defendantCaseLevelResults.FirstOrDefault(x => x.ResultDefinitionId == result.ResultDefinitionId);
                if (match == null)
                {
                    if (result.SelectedResultPrompts != null)
                    {
                        foreach (var prompt in result.SelectedResultPrompts)
                        {
                            if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                            {
                                var key = GetKey(prompt);
                                if (nonVariantPromptValues.ContainsKey(key))
                                {
                                    prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, nonVariantPromptValues[key]);
                                }
                                else
                                {
                                    var val = prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, null);
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        nonVariantPromptValues.Add(key, val);
                                    }
                                }
                                continue;
                            }
                            prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, null);
                        }
                    }
                    defendantCaseLevelResults.Add(result);
                    continue;
                }

                if (match.SelectedResultPrompts != null)
                {
                    //see if the match is for the same or a different case
                    foreach (var item in match.SelectedResultPrompts)
                    {
                        foreach (var promptValue in item.Values)
                        {
                            if (promptValue.CaseId == prosecutionCase.CaseId)
                            {
                                //already have this result for the defendant case
                                continue;
                            }
                        }
                    }
                }

                if (result.SelectedResultPrompts != null)
                {
                    //make new prompt values for the new defendant case combination
                    foreach (var prompt in result.SelectedResultPrompts)
                    {
                        if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                        {
                            var key = GetKey(prompt);
                            if (nonVariantPromptValues.ContainsKey(key))
                            {
                                prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, nonVariantPromptValues[key]);
                            }
                            else
                            {
                                var val = prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, null);
                                if (!string.IsNullOrEmpty(val))
                                {
                                    nonVariantPromptValues.Add(key, val);
                                }
                            }
                            continue;
                        }
                        prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, null);
                    }
                }
            }

            //deal with offence level results
            foreach (var result in selectedOffenceResults)
            {
                if (result.SelectedResultPrompts != null)
                {
                    foreach (var prompt in result.SelectedResultPrompts)
                    {                        
                        if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                        {
                            var key = GetKey(prompt);
                            if (nonVariantPromptValues.ContainsKey(key))
                            {
                                prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, nonVariantPromptValues[key]);
                            }
                            else
                            {
                                var val = prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, null);
                                if (!string.IsNullOrEmpty(val))
                                {
                                    nonVariantPromptValues.Add(key, val);
                                }
                            }
                            continue;
                        }
                        prompt.AppendValue(Defendant, prosecutionCase, null, fixedLists, null);
                    }
                }
            }
        }

        private void GenerateResultValuesByOffence(List<FixedList> fixedLists, List<Result> defendantLevelResults, List<Result> defendantCaseLevelResults, List<Result> selectedOffenceResults, List<Result> selectedDefendantCaseResults, List<Result> selectedDefendantResults, StringDictionary nonVariantPromptValues, ProsecutionCase prosecutionCase)
        {
            foreach (var offence in prosecutionCase.Offences)
            {
                //deal with defendant level results
                foreach (var result in selectedDefendantResults)
                {
                    //only one instance of the defendant level result per defendant and we only have one defendant
                    var match = defendantLevelResults.FirstOrDefault(x => x.ResultDefinitionId == result.ResultDefinitionId);
                    if (match == null)
                    {
                        if (result.SelectedResultPrompts != null)
                        {
                            foreach (var prompt in result.SelectedResultPrompts)
                            {
                                if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                                {
                                    var key = GetKey(prompt);
                                    if (nonVariantPromptValues.ContainsKey(key))
                                    {
                                        prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, nonVariantPromptValues[key]);
                                    }
                                    else
                                    {
                                        var val = prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                                        if (!string.IsNullOrEmpty(val))
                                        {
                                            nonVariantPromptValues.Add(key, val);
                                        }
                                    }
                                    continue;
                                }
                                prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                            }
                        }
                        defendantLevelResults.Add(result);
                        continue;
                    }
                }

                //deal with defendant case level results
                foreach (var result in selectedDefendantCaseResults)
                {
                    var match = defendantCaseLevelResults.FirstOrDefault(x => x.ResultDefinitionId == result.ResultDefinitionId);
                    if (match == null)
                    {
                        if (result.SelectedResultPrompts != null)
                        {
                            foreach (var prompt in result.SelectedResultPrompts)
                            {
                                if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                                {
                                    var key = GetKey(prompt);
                                    if (nonVariantPromptValues.ContainsKey(key))
                                    {
                                        prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, nonVariantPromptValues[key]);
                                    }
                                    else
                                    {
                                        var val = prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                                        if (!string.IsNullOrEmpty(val))
                                        {
                                            nonVariantPromptValues.Add(key, val);
                                        }
                                    }
                                    continue;
                                }
                                prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                            }
                        }
                        defendantCaseLevelResults.Add(result);
                        continue;
                    }

                    if (match.SelectedResultPrompts != null)
                    {
                        //see if the match is for the same or a different case
                        foreach (var item in match.SelectedResultPrompts)
                        {
                            foreach (var promptValue in item.Values)
                            {
                                if (promptValue.CaseId == prosecutionCase.CaseId)
                                {
                                    //already have this result for the defendant case
                                    continue;
                                }
                            }
                        }
                    }

                    if (result.SelectedResultPrompts != null)
                    {
                        //make new prompt values for the new defendant case combination
                        foreach (var prompt in result.SelectedResultPrompts)
                        {
                            if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                            {
                                var key = GetKey(prompt);
                                if (nonVariantPromptValues.ContainsKey(key))
                                {
                                    prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, nonVariantPromptValues[key]);
                                }
                                else
                                {
                                    var val = prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        nonVariantPromptValues.Add(key, val);
                                    }
                                }
                                continue;
                            }
                            prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                        }
                    }
                }

                //deal with offence level results
                foreach (var result in selectedOffenceResults)
                {
                    if (result.SelectedResultPrompts != null)
                    {
                        foreach (var prompt in result.SelectedResultPrompts)
                        {
                            if (prompt.IsVariantData && prompt.PromptIdentifier.HasValue)
                            {
                                var key = GetKey(prompt);
                                if (nonVariantPromptValues.ContainsKey(key))
                                {
                                    prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, nonVariantPromptValues[key]);
                                }
                                else
                                {
                                    var val = prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        nonVariantPromptValues.Add(key, val);
                                    }
                                }
                                continue;
                            }
                            prompt.AppendValue(Defendant, prosecutionCase, offence, fixedLists, null);
                        }
                    }
                }
            }
        }

        private string GetKey(ResultPrompt rp)
        {
            if (rp.PromptType == "NAMEADDRESS" || rp.PromptType == "ADDRESS")
            {
                return string.Format("{0}-{1}", rp.PromptIdentifier, rp.PromptReference);
            }
            return rp.PromptIdentifier.ToString();
        }

        private void SimulateShareResults(Now now, List<NowSubscription> subscriptions)
        {
            ////first decide on the subscriber, note published results will be null at this point
            //var subscription = FindSubscribingRecipient(now, subscriptions, PublishedResults);

            ////deselect results based on the variant required by the subscriber
            //if (subscription != null && subscription.UserGroupVariants != null && subscription.UserGroupVariants.Count > 0)
            //{
            //    DeselectResultsForRequiredVariant(subscription);
            //}

            //publish results based on rollup rules
            RollUpResults();

            //determine if a financial order is required based on published results
            SetFinancialOrder();

            //set the results on the domain objects
            ApplyResultsToDomain();

            //now decide on the subscriber
            var subscription = FindSubscribingRecipient(now, subscriptions, PublishedResults);

            //deselect results based on the variant required by the subscriber, does this impact the rollup?
            if (subscription != null && subscription.UserGroupVariants != null && subscription.UserGroupVariants.Count > 0)
            {
                DeselectResultsForRequiredVariant(subscription);
            }

            //orderAddressee = Orderaddressee.CreateInstance(isYouth, random),
            GenerateRecipientDetails(subscription);
        }

        private void DeselectResultsForRequiredVariant(NowSubscription subscription)
        {
            foreach(var userGroup in subscription.UserGroupVariants)
            {
                foreach(var res in SelectedResults)
                {
                    //deselect the result that matches the user group
                    if (res.ResultDefinition.UserGroups != null && res.ResultDefinition.UserGroups.Count > 0 && res.ResultDefinition.UserGroups.Contains(userGroup))
                    {
                        res.IsSelected = false;

                        //hide any prompts since they may have rolled up to a different result
                        foreach (var item in res.ResultPrompts)
                        {
                            item.Hide();
                        }
                        continue;
                    }

                    foreach(var item in res.ResultPrompts.Where(x=>x.UserGroups != null && x.UserGroups.Count>0))
                    {
                        if (item.UserGroups.Contains(userGroup))
                        {
                            item.Hide();
                        }
                    }
                }                
            }
        }

        private void GenerateRecipientDetails(NowSubscription subscription)
        {
            if (subscription == null)
            {
                OrderAddressee = OrderAddressee.BuildForNoSubscriber();
                SubscriberName = "No Subscriber";
                return;
            }

            SubscriberName = subscription.Name;

            if (subscription.NoRecipient)
            {
                OrderAddressee = null;
                return;
            }

            if (subscription.NowRecipient.RecipientFromCase)
            {
                if (subscription.NowRecipient.IsApplyDefendantDetails)
                {
                    OrderAddressee = new OrderAddressee(Defendant.Name, Defendant.Address);
                }

                if (subscription.NowRecipient.IsApplyDefenceOrganisationDetails)
                {
                    OrderAddressee = new OrderAddressee(Defendant.SolicitorName, Defendant.SolicitorAddress);
                }

                if (subscription.NowRecipient.IsApplyParentGuardianDetails && Defendant.IsYouth)
                {
                    OrderAddressee = new OrderAddressee(Defendant.ParentGuardianName, Defendant.ParentGuardianAddress);
                }

                if (subscription.NowRecipient.IsApplyProsecutionAuthorityDetails)
                {
                    OrderAddressee = new OrderAddressee(Informant, InformantAddress);
                }

                if (subscription.NowRecipient.IsApplyDefendantCustodyDetails)
                {
                    OrderAddressee = new OrderAddressee(Defendant.CustodyName, Defendant.CustodyAddress);
                }

                if (subscription.NowRecipient.IsApplyApplicantDetails && Applicants != null && Applicants.Count > 0)
                {
                    OrderAddressee = new OrderAddressee(Applicants[0].Name, Applicants[0].Address);
                }

                if (subscription.NowRecipient.IsApplyRespondentDetails && Respondents != null && Respondents.Count>0)
                {
                    OrderAddressee = new OrderAddressee(Respondents[0].Name, Respondents[0].Address);
                }

                if (subscription.NowRecipient.IsApplyThirdPartyDetails && ThirdParties != null && ThirdParties.Count > 0)
                {
                    OrderAddressee = new OrderAddressee(ThirdParties[0].Name, ThirdParties[0].Address);
                }

                return;
            }

            if (subscription.NowRecipient.RecipientFromSubscription)
            {
                var address = new NowAddress
                    (
                        subscription.NowRecipient.Address1, subscription.NowRecipient.Address2, subscription.NowRecipient.Address3,
                        subscription.NowRecipient.Address4, subscription.NowRecipient.Address5, subscription.NowRecipient.PostCode,
                        subscription.NowRecipient.EmailAddress1,
                        subscription.NowRecipient.EmailAddress2
                    );
                var name = MakeNameValue
                    (
                        subscription.NowRecipient.Title, subscription.NowRecipient.FirstName, subscription.NowRecipient.MiddleName,
                        subscription.NowRecipient.LastName, subscription.NowRecipient.OrganisationName
                    );
                OrderAddressee = new OrderAddressee(name, address);
                return;
            }

            if (subscription.NowRecipient.RecipientFromResults)
            {
                RecipientFromResults(subscription);
                return;
            }
        }

        private void RecipientFromResults(NowSubscription subscription)
        {
            //find each value
            string title = FindPromptValue(subscription.NowRecipient.TitleResultPromptId, "title");
            string firstName = FindPromptValue(subscription.NowRecipient.FirstNameResultPromptId, "firstname");
            string middleName = FindPromptValue(subscription.NowRecipient.MiddleNameResultPromptId, "middlename");
            string lastName = FindPromptValue(subscription.NowRecipient.LastNameResultPromptId, "lastname");
            string organisationName = FindPromptValue(subscription.NowRecipient.OrganisationNameResultPromptId, "organisationname");
            string address1 = FindPromptValue(subscription.NowRecipient.Address1ResultPromptId, "address1");
            string address2 = FindPromptValue(subscription.NowRecipient.Address2ResultPromptId, "address2");
            string address3 = FindPromptValue(subscription.NowRecipient.Address3ResultPromptId, "address3");
            string address4 = FindPromptValue(subscription.NowRecipient.Address4ResultPromptId, "address4");
            string address5 = FindPromptValue(subscription.NowRecipient.Address5ResultPromptId, "address5");
            string postCode = FindPromptValue(subscription.NowRecipient.PostCodeResultPromptId, "postcode");
            string emailAddress1 = FindPromptValue(subscription.NowRecipient.EmailAddress1ResultPromptId, "email1");
            string emailAddress2 = FindPromptValue(subscription.NowRecipient.EmailAddress2ResultPromptId, "email2");

            string name = MakeNameValue(title, firstName, middleName, lastName, organisationName);

            if (string.IsNullOrEmpty(name)) { name = "Missing Name Results"; }

            var missingAddress = string.IsNullOrEmpty(address1) && string.IsNullOrEmpty(address2) && string.IsNullOrEmpty(address3) && string.IsNullOrEmpty(address4)
                                    && string.IsNullOrEmpty(address5) && string.IsNullOrEmpty(postCode) && string.IsNullOrEmpty(emailAddress1) && string.IsNullOrEmpty(emailAddress2);

            if (missingAddress)
            {
                var address = new NowAddress
                    (
                        "Missing Address1 Result", "Missing Address2 Result", "Missing Address3 Result", "Missing Address4 Result", "Missing Address5 Result",
                        "Missing PostCode Result", "Missing EmailAddress1 Result", "Missing EmailAddress2 Result"
                    );
                OrderAddressee = new OrderAddressee(name, address);
            }
            else
            {
                var address = new NowAddress
                    (
                        address1, address2, address3, address4, address5, postCode, emailAddress1, emailAddress2
                    );
                OrderAddressee = new OrderAddressee(name, address);
            }
        }

        private string MakeNameValue(string title, string firstName, string middleName, string lastName, string organisationName)
        {
            if (!string.IsNullOrEmpty(organisationName)) { return organisationName; }

            string personName = null;
            if (!string.IsNullOrEmpty(title)) { personName = title; }
            if (!string.IsNullOrEmpty(firstName))
            {
                if (string.IsNullOrEmpty(personName)) { personName = firstName; }
                else { personName = personName + " " + firstName; }
            }
            if (!string.IsNullOrEmpty(middleName))
            {
                if (string.IsNullOrEmpty(personName)) { personName = middleName; }
                else { personName = personName + " " + middleName; }
            }
            if (!string.IsNullOrEmpty(lastName))
            {
                if (string.IsNullOrEmpty(personName)) { personName = lastName; }
                else { personName = personName + " " + lastName; }
            }

            return personName;
        }

        private string FindPromptValue(Guid? resultPromptId, string key)
        {
            foreach (var result in PublishedResults)
            {
                if (result.ResultPrompts != null)
                {
                    var matchingPrompts = result.ResultPrompts.Where(x => x.IsSelected && x.ResultPromptRule != null && x.ResultPromptRule.ResultPromptUUID == resultPromptId).ToList();
                    foreach (var prompt in matchingPrompts)
                    {
                        if (!string.IsNullOrEmpty(key) && (prompt.PromptType == "NAMEADDRESS" || prompt.PromptType == "ADDRESS"))
                        {
                            var matchedPv = prompt.Values.Where(x => !string.IsNullOrEmpty(x.Key) && x.Key.ToLowerInvariant().EndsWith(key.ToLowerInvariant())).ToList();
                            if (matchedPv.Count > 0)
                            {
                                return matchedPv[0].Value;
                            }
                            continue;
                        }
                        else
                        {
                            if (prompt.Values.Count>0)
                            {
                                return prompt.Values[0].Value;
                            }
                            continue;
                        }
                    }
                }
            }

            return null;
        }

        public static List<NowSubscription> GetSubscribers(Now now, List<NowSubscription> allSubscriptions, Defendant defendant, List<Result> publishedResults)
        {
            //scan the relevant subscriptions, starting with all parents
            var subscriptions = now.IsEDT ? allSubscriptions.Where(x => x.IsEDT && x.ParentNowSubscription == null && x.DeletedDate == null) : allSubscriptions.Where(x => x.IsNow && x.ParentNowSubscription == null && x.DeletedDate == null);

            var matchedSubscriptions = new List<NowSubscription>();

            //match subscription to the now
            foreach (var subscription in subscriptions)
            {
                var matched = MatchSubscription(now, subscription, defendant, publishedResults);
                if (matched != null) { matchedSubscriptions.Add(matched); }
            }

            return matchedSubscriptions;
        }

        private NowSubscription FindSubscribingRecipient(Now now, List<NowSubscription> allSubscriptions, List<Result> publishedResults)
        {
            var matchedSubscriptions = GetSubscribers(now, allSubscriptions, Defendant, publishedResults);

            if (matchedSubscriptions.Count>0)
            {
                var index = Random.Next(matchedSubscriptions.Count - 1);
                return matchedSubscriptions[index];
            }

            return null;
        }

        private static NowSubscription MatchSubscription(Now now, NowSubscription subscription, Defendant defendant, List<Result> publishedResults)
        {
            //see if the now is an included now
            if (subscription.IncludedNOWS != null && subscription.IncludedNOWS.Count > 0)
            {
                var match = subscription.IncludedNOWS.FirstOrDefault(x => x.UUID == now.UUID);
                if (match == null) { return null; }
            }

            //see if the subscriber has the now specifically excluded
            if (subscription.ExcludedNOWS != null && subscription.ExcludedNOWS.Count > 0)
            {
                //is the required now excluded?
                var match = subscription.ExcludedNOWS.FirstOrDefault(x => x.UUID == now.UUID);
                if (match != null) { return null; }
            }

            //Check the vocabulary
            if (!VocabularyOK(subscription, defendant, publishedResults)) { return null; }

            //Now is subscribed to by this subscriber
            //Either return the subscriber or find a child subscription that also subscrines to the NOW
            if (subscription.ChildNowSubscriptions != null && subscription.ChildNowSubscriptions.Where(x => x.DeletedDate == null).Count() > 0)
            {
                var lookForChild = Switch();
                if (lookForChild)
                {
                    //recursively build all the subscribing children
                    var subscribingChildren = new List<NowSubscription>();
                    foreach (var item in subscription.ChildNowSubscriptions.Where(x => x.DeletedDate == null))
                    {
                        RecursivelyFindSubscribingChildren(now, item, subscribingChildren, defendant, publishedResults);
                    }

                    if (subscribingChildren.Count > 0)
                    {
                        //randomly select a child
                        return subscribingChildren[Random.Next(subscribingChildren.Count - 1)];
                    }
                }
            }

            //return the parent subscription
            return subscription;
        }

        private static bool VocabularyOK(NowSubscription subscription, Defendant defendant, List<Result> publishedResults)
        {
            if (defendant == null) { return true; }
            if (subscription.SubscriptionVocabulary == null) { return true; }

            if (subscription.SubscriptionVocabulary.AdultDefendant && defendant.IsYouth) { return false; }

            if (subscription.SubscriptionVocabulary.YouthDefendant && !defendant.IsYouth) { return false; }

            if (subscription.SubscriptionVocabulary.CustodyLocationIsPolice && !defendant.CustodyLocationIsPolice) { return false; }

            if (subscription.SubscriptionVocabulary.CustodyLocationIsPrison && !defendant.CustodyLocationIsPrison) { return false; }

            if (subscription.SubscriptionVocabulary.InCustody && !defendant.InCustody) { return false; }

            if (subscription.SubscriptionVocabulary.AppearedByVideoLink && !defendant.AppearedByVideoLink) { return false; }

            if (subscription.SubscriptionVocabulary.AppearedInPerson && !defendant.AppearedInPerson) { return false; }

            if (subscription.SubscriptionVocabulary.IncludedPromptRules != null) 
            { 
                var promptsOK = CheckIncludedPromptRules(subscription.SubscriptionVocabulary.IncludedPromptRules, publishedResults);
                if (!promptsOK)
                {
                    return false;
                }
            }

            if (subscription.SubscriptionVocabulary.IncludedResults != null) 
            { 
                var resultsOK = CheckIncludedResults(subscription.SubscriptionVocabulary.IncludedResults, publishedResults);
                if (!resultsOK)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckIncludedPromptRules(List<ResultPromptRule> includedPromptRules, List<Result> publishedResults)
        {
            if (publishedResults == null) { return true; }
            foreach (var prompt in includedPromptRules)
            {
                var matched = false;
                foreach (var result in publishedResults.Where(x=>x.SelectedResultPrompts != null))
                {
                    var match = result.SelectedResultPrompts.FirstOrDefault(x => x.PromptIdentifier == prompt.ResultPromptUUID);
                    if (match != null)
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckIncludedResults(List<ResultDefinition> includedResults, List<Result> publishedResults)
        {
            if (publishedResults == null) { return true; }

            foreach (var res in includedResults)
            {
                var match = publishedResults.FirstOrDefault(x => x.ResultDefinitionId == res.UUID);
                if (match == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static void RecursivelyFindSubscribingChildren(Now now, NowSubscription child, List<NowSubscription> subscribingChildren, Defendant defendant, List<Result> publishedResults)
        {
            //see if the now is an included now
            if (child.IncludedNOWS != null && child.IncludedNOWS.Count > 0)
            {
                var match = child.IncludedNOWS.FirstOrDefault(x => x.UUID == now.UUID);
                if (match == null) { return; }
            }

            //see if the child has the now specifically excluded
            if (child.ExcludedNOWS != null && child.ExcludedNOWS.Count > 0)
            {
                //is the required now excluded?
                var match = child.ExcludedNOWS.FirstOrDefault(x => x.UUID == now.UUID);
                if (match != null) { return; }
            }

            //Check the vocabulary
            if (VocabularyOK(child, defendant, publishedResults))
            {
                //child is a subscriber 
                subscribingChildren.Add(child);

                //go to the next level of children
                if (child.ChildNowSubscriptions != null && child.ChildNowSubscriptions.Where(x => x.DeletedDate == null).Count() > 0)
                {
                    foreach (var item in child.ChildNowSubscriptions.Where(x => x.DeletedDate == null))
                    {
                        RecursivelyFindSubscribingChildren(now, child, subscribingChildren, defendant, publishedResults);
                    }
                }
            }

            return;
        }

        private void RollUpResults()
        {
            //Note - this simply rolls up the results.  The prompt values within each rollup will vary according to the defenant->case->offence combinations

            //Apply the business roll up rules to create the final set of published results
            var resultsForRollUp = SelectedResults.OrderByDescending(x => x.TreeLevel).ThenBy(x => x.Sequence).ToList();

            foreach (var item in resultsForRollUp)
            {
                //deal with items published for NOWs
                if (item.IsPublishedForNows && item.TreeLevel != 0)
                {
                    PublishResult(item);
                }

                //publish the root items
                if (item.TreeLevel == 0)
                {
                    PublishRoot(item);
                    continue;
                }

                if (item.IsBooleanResult)
                {
                    //move the prompts up one level
                    RollUpPrompts(item);
                    continue;
                }

                if (item.IsRollupPrompts)
                {
                    //move the prompts up one level
                    RollUpPrompts(item);
                    continue;
                }

                if (item.IsAlwaysPublished)
                {
                    PublishResult(item);
                    continue;
                }

                if (item.IsPublishedAsAPrompt)
                {
                    RollUpAsAPrompt(item);
                    continue;
                }
            }
        }

        private void PublishResult(Result item)
        {
            item.IsPublished = true;
        }

        private void SetFinancialOrder()
        {
            if (Results.FirstOrDefault(x => x.IsPublished && x.Financial) != null)
            {
                FinancialOrderDetails = new FinancialOrderDetails();
            }
        }

        private void ApplyResultsToDomain()
        {
            //deal with defendant level results
            foreach (var result in PublishedResults.Where(x => x.Level.ToLowerInvariant() == "d").OrderBy(x => x.Sequence))
            {
                var clone = result.CloneDefendantResult(Defendant.DefendantId);
                if (clone != null)
                {
                    if (Defendant.DefendantResults == null) { Defendant.DefendantResults = new List<Result>(); }
                    Defendant.DefendantResults.Add(clone);
                }
            }

            foreach (var prosecutionCase in Cases)
            {
                bool skipDefendantCaseResults = false;

                //if (prosecutionCase.IsApplication && LiveProceedingsApplicationResults != null)
                //{
                //    //when live proceedings, do not result (case level) the application case 
                //    //because the results will be entered against the linked case(s) that will always be listed at the same time
                //    skipDefendantCaseResults = true;
                //}
                
                if (!skipDefendantCaseResults)
                {
                    //deal with defendant case level results
                    foreach (var result in PublishedResults.Where(x => x.Level.ToLowerInvariant() == "c").OrderBy(x => x.Sequence))
                    {
                        var clone = result.CloneDefendantCaseResult(prosecutionCase.CaseId, Defendant.DefendantId, false);
                        if (clone != null)
                        {
                            if (Defendant.DefendantCaseResults == null) { Defendant.DefendantCaseResults = new List<Result>(); }
                            Defendant.DefendantCaseResults.Add(clone);
                        }
                    }
                }

                //deal with offence level results
                foreach (var offence in prosecutionCase.Offences)
                {
                    if (offence.IsMOJOffence)
                    {                      
                        //in continued proceedings, result the moj offence with application results 
                        //which will exist when the original offences have been resentenced
                        if (ContinuedProceedingsApplicationResults != null && ContinuedProceedingsApplicationResults.Count() > 0)
                        {
                            foreach (var result in ContinuedProceedingsApplicationResults.OrderBy(x => x.Sequence))
                            {
                                var clone = result.CloneOffenceResult(prosecutionCase.CaseId, offence.OffenceId, Defendant.DefendantId);
                                if (clone != null)
                                {
                                    if (offence.Results == null) { offence.Results = new List<Result>(); }
                                    offence.Results.Add(clone);
                                    if (result.IsConvicted)
                                    {
                                        offence.ConvictionDate = OrderDate;
                                    }
                                }
                            }

                            continue;
                        }

                        //in live proceedings, assume that the now results are also entered against the application, i.e. just continue

                        ////in live proceedings, result the moj offence with application results 
                        ////which will exist when the live linked cases are resulted
                        //if (LiveProceedingsApplicationResults != null)
                        //{
                        //    foreach (var result in LiveProceedingsApplicationResults.OrderBy(x => x.Sequence))
                        //    {
                        //        var clone = result.CloneOffenceResult(prosecutionCase.CaseId, offence.OffenceId, Defendant.DefendantId);
                        //        if (clone != null)
                        //        {
                        //            if (offence.Results == null) { offence.Results = new List<Result>(); }
                        //            offence.Results.Add(clone);
                        //            if (result.IsConvicted)
                        //            {
                        //                offence.ConvictionDate = OrderDate;
                        //            }
                        //        }
                        //    }

                        //    continue;
                        //}
                    }

                    foreach (var result in PublishedResults.Where(x => x.Level.ToLowerInvariant() == "o").OrderBy(x => x.Sequence))
                    {
                        var clone = result.CloneOffenceResult(prosecutionCase.CaseId, offence.OffenceId, Defendant.DefendantId);
                        if (clone != null)
                        {
                            if (offence.Results == null) { offence.Results = new List<Result>(); }
                            offence.Results.Add(clone);
                            if (result.IsConvicted)
                            {
                                offence.ConvictionDate = OrderDate;
                            }
                        }
                    }
                }
            }
        }

        private void RollUpAsAPrompt(Result item)
        {
            //find the parent
            var parentResult = Results.FirstOrDefault(x => x.ResultDefinitionId == item.ParentResultDefinition.UUID
                                                        && x.TreeLevel == item.TreeLevel - 1
                                                        && x.RootResultDefinitionId == item.RootResultDefinitionId
                                                        );

            //make result as a prompt
            parentResult.MakeAsAPrompt(item);
        }

        private void PublishRoot(Result item)
        {
            //ensure that the root can be published
            if (!item.ResultDefinition.IsExcludedFromResults)
            {
                PublishResult(item);
                return;
            }

            //root cannot be published, cascade back down
            foreach (var childAsRoot in Results.Where(x => x.IsSelected && x.TreeLevel == item.TreeLevel + 1))
            {
                PublishRoot(childAsRoot);
            }
        }

        private void RollUpPrompts(Result item)
        {
            var parentResult = Results.FirstOrDefault(x => x.ResultDefinitionId == item.ParentResultDefinition.UUID
                                                        && x.TreeLevel == item.TreeLevel - 1
                                                        && x.RootResultDefinitionId == item.RootResultDefinitionId
                                                        );

            if (parentResult == null)
            {
                var items = Results.Where(x => x.ResultDefinitionId == item.ParentResultDefinition.UUID).ToList();
                var rootedItems = items.Where(x => x.RootResultDefinitionId == item.RootResultDefinitionId).ToList();
                return;
            }

            parentResult.AppendChildPrompts(item);
        }

        private void RecursivelyMakeNOWResults(ResultDefinition rootRD, NowRequirement nowRequirement, int treeLevel)
        {
            //Find the result definition rule from the parent
            ResultDefinitionRule rule = null;
            if (nowRequirement.ParentNowRequirement != null && nowRequirement.ParentNowRequirement.ResultDefinition.ResultDefinitionRules != null)
            {
                //find the child result definition rule for the now requirement in hand
                rule = nowRequirement.ParentNowRequirement.ResultDefinition.ResultDefinitionRules.FirstOrDefault(x => x.ResultDefinition.UUID == nowRequirement.ResultDefinition.UUID);
            }

            var match = Results.FirstOrDefault(x => x.RootResultDefinitionId == rootRD.UUID
                                                            && x.TreeLevel == treeLevel
                                                            && x.ResultDefinitionId == nowRequirement.ResultDefinition.UUID);

            if (match == null)
            {
                Results.Add(new Result(treeLevel, rootRD, nowRequirement, rule));
            }

            if (nowRequirement.NowRequirements == null || nowRequirement.NowRequirements.Where(x => x.DeletedDate == null).Count() == 0) { return; }
            foreach (var item in nowRequirement.NowRequirements.Where(x => x.DeletedDate == null).OrderBy(x => x.ResultDefinitionSequence))
            {
                RecursivelyMakeNOWResults(rootRD, item, treeLevel + 1);
            }
        }

        private void RecursivelyMakeNonNOWResults(ResultDefinition rootRD, ResultDefinition rd, ResultDefinition parentRD, int treeLevel, ResultDefinitionRule rule, List<ResultDefinition> allResults)
        {
            var match = Results.FirstOrDefault(x => x.RootResultDefinitionId == rootRD.UUID
                                                            && x.TreeLevel == treeLevel
                                                            && x.ResultDefinitionId == rd.UUID);

            if (match == null)
            {
                Results.Add(new Result(treeLevel, rootRD, rd, rule, parentRD));
            }

            if (rd.ResultDefinitionRules == null || rd.ResultDefinitionRules.Where(x => x.DeletedDate == null).Count() == 0) { return; }
            foreach (var item in rd.ResultDefinitionRules.Where(x => x.DeletedDate == null))
            {
                var child = allResults.FirstOrDefault(x => x.UUID == item.ChildResultDefinitionUUID && x.DeletedDate == null);
                if (child != null)
                {
                    RecursivelyMakeNonNOWResults(rootRD, child, rd, treeLevel + 1, item, allResults);
                }
            }
        }

        private void SelectResults(Now now, bool includeAllOptionalData, List<ResultDefinition> allResults, List<NowRequirement> allNowRequirements)
        {
            SelectedRootResults = new List<NowRequirement>();
            var results = GetResults(now);

            if (results.Count == 0)
            {
                //switch jurisdiction and try again
                IsCrown = !IsCrown;
                results = GetResults(now);
            }

            if (includeAllOptionalData)
            {
                SelectedRootResults = results;
            }
            else
            {
                decimal start = decimal.Divide(results.Count, 2);
                var min = Math.Round(start, MidpointRounding.AwayFromZero);

                //first deal with primary results
                var primaryResults = results.Where(x => x.PrimaryResult).ToList();
                var numberItems = Random.Next(decimal.ToInt32(min), primaryResults.Count);
                //select random items
                var indices = new List<int>();
                while (indices.Count < numberItems)
                {
                    var index = Random.Next(primaryResults.Count - 1);
                    if (!indices.Contains(index))
                    {
                        indices.Add(index);
                        SelectedRootResults.Add(primaryResults[index]);
                    }
                }

                //now select the non primary 
                var nonPrimaryResults = results.Where(x => !x.PrimaryResult).ToList();
                numberItems = Random.Next(decimal.ToInt32(min), nonPrimaryResults.Count);
                //select random items
                indices = new List<int>();
                while (indices.Count < numberItems)
                {
                    var index = Random.Next(nonPrimaryResults.Count - 1);
                    if (!indices.Contains(index))
                    {
                        indices.Add(index);
                        SelectedRootResults.Add(nonPrimaryResults[index]);
                    }
                }
            }

            //deal with the requirement for non NOW results
            if (now.IncludeAllResults)
            {
                SelectNonNowResults(now, allResults, allNowRequirements);
            }            
        }

        private void SelectNonNowResults(Now now, List<ResultDefinition> allResults, List<NowRequirement> allNowRequirements)
        {
            SelectedNonNowResults = new List<ResultDefinition>();
            var nowResults = allNowRequirements.Where(x => x.NOWUUID == now.UUID).Select(x => x.ResultDefinition);

            var nonNowResults = allResults.Where(x =>
                                                     (x.Jurisdiction.ToLowerInvariant().StartsWith("b") ||
                                                      x.Jurisdiction.ToLowerInvariant().StartsWith(IsCrown ? "c" : "m")) &&
                                                      nowResults.All(y => y.UUID != x.UUID)).ToList();

            var nonNowResultsDefendantLevel = nonNowResults.Where(x => x.LevelFirstLetter.ToLowerInvariant() == "d").ToList();

            var nonNowResultsCaseDefendantLevel = nonNowResults.Where(x => x.LevelFirstLetter.ToLowerInvariant() == "c").ToList();

            var nonNowResultsOffenceLevel = nonNowResults.Where(x => x.LevelFirstLetter.ToLowerInvariant() == "o").ToList();

            //select a defendant level result
            var index = Random.Next(nonNowResultsDefendantLevel.Count - 1);
            SelectedNonNowResults.Add(nonNowResultsDefendantLevel[index]);

            //select a casedefendant level result
            index = Random.Next(nonNowResultsCaseDefendantLevel.Count - 1);
            SelectedNonNowResults.Add(nonNowResultsCaseDefendantLevel[index]);

            //select random offence level items
            var numberItems = Random.Next(2) + 1;
            var indices = new List<int>();
            while (indices.Count < numberItems)
            {
                index = Random.Next(nonNowResultsOffenceLevel.Count - 1);

                if (!indices.Contains(index))
                {
                    indices.Add(index);
                    SelectedNonNowResults.Add(nonNowResultsOffenceLevel[index]);
                }
            }
        }

        private List<NowRequirement> GetResults(Now now)
        {
            List<NowRequirement> res = new List<NowRequirement>();
            SelectRootResults(now.NowRequirements, res);
            return res;
        }

        private void SelectRootResults(List<NowRequirement> nowRequirements, List<NowRequirement> selectedResults)
        {
            var requirementsForJurisdiction = nowRequirements.Where(x =>
                                                x.ResultDefinition.Jurisdiction.ToLowerInvariant().StartsWith("b") ||
                                                x.ResultDefinition.Jurisdiction.ToLowerInvariant().StartsWith(IsCrown ? "c" : "m")).OrderBy(x => x.ResultDefinitionSequence).ToList();

            //first select the primary results
            foreach (var nr in requirementsForJurisdiction)
            {
                if (nr.PrimaryResult)
                {
                    var match = selectedResults.FirstOrDefault(x => x.ResultDefinition.UUID == nr.ResultDefinition.UUID);
                    if (match == null)
                    {
                        selectedResults.Add(nr);
                    }
                }
                else if (nr.NowRequirements != null && nr.NowRequirements.Count > 0)
                {
                    SelectRootResults(nr.NowRequirements, selectedResults);
                }
            }

            //now select the non primary root results
            foreach (var nr in requirementsForJurisdiction.Where(x=> !x.PrimaryResult && x.ParentNowRequirement == null))
            {
                var match = selectedResults.FirstOrDefault(x => x.ResultDefinition.UUID == nr.ResultDefinition.UUID);
                if (match == null)
                {
                    selectedResults.Add(nr);
                }
            }
        }

        public static bool Switch()
        {
            return RandomSwitch.NextDouble() > 0.5;
        }
    }
}
