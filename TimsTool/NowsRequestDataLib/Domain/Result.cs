using DataLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class Result
    {
        private ResultDefinition rd, parentRD;

        [JsonProperty(PropertyName = "treeLevel")]
        public int TreeLevel { get; set; }

        [JsonIgnore]
        public ResultDefinition ParentResultDefinition
        {
            get
            {
                if (NowRequirement != null)
                {
                    if (NowRequirement.ParentNowRequirement != null)
                    {
                        return NowRequirement.ParentNowRequirement.ResultDefinition;
                    }

                    return null;
                }
                return parentRD;
            }
        }

        [JsonIgnore]
        public List<NowRequirementText> NowText
        {
            // non now results never have now text
            get => NowRequirement != null ? NowRequirement.NowRequirementTextList : null;
        }

        [JsonIgnore]
        public bool DistinctResultTypes
        {
            // non now results are never required as distinct types
            get => NowRequirement != null ? NowRequirement.DistinctResultTypes : false;
        }

        [JsonIgnore]
        public NowRequirement NowRequirement { get; }

        [JsonProperty(PropertyName = "isMandatoryResult")]
        public bool IsMandatoryResult
        {
            get
            {
                if (ResultDefinitionRule != null)
                {
                    return ResultDefinitionRule.Rule == ResultDefinitionRuleType.mandatory;
                }

                if (NowRequirement != null)
                {
                    return NowRequirement.Mandatory;
                }
                
                //all non now results are mandatory
                return true;
            }
        }

        [JsonProperty(PropertyName = "resultDefinitionId")]
        public Guid? ResultDefinitionId 
        {
            get => ResultDefinition.UUID;
        }

        [JsonProperty(PropertyName = "label")]
        public string ResultDefinitionLabel
        {
            get => ResultDefinition.Label;
        }

        [JsonProperty(PropertyName = "welshLabel")]
        public string ResultDefinitionWelshLabel
        {
            get => ResultDefinition.WelshLabel;
        }

        [JsonProperty(PropertyName = "resultWording")]
        public string ResultDefinitionWording
        {
            get => ResultDefinition.ResultWording;
        }

        [JsonProperty(PropertyName = "welshResultWording")]
        public string ResultDefinitionWelshResultWording
        {
            get => ResultDefinition.WelshResultWording;
        }

        [JsonProperty(PropertyName = "resultDefinitionGroup")]
        public string ResultDefinitionGroup
        {
            get => ResultDefinition.ResultDefinitionGroup;
        }

        [JsonProperty(PropertyName = "rank")]
        public int? ResultDefinitionRank
        {
            get => ResultDefinition.Rank;
        }

        [JsonProperty(PropertyName = "resultLevel")]
        public string Level
        {
            get
            {
                return ResultDefinition.LevelFirstLetter;
            }
        }

        [JsonProperty(PropertyName = "isAlwaysPublished")]
        public bool IsAlwaysPublished
        {
            get
            {
                return ResultDefinition.IsAlwaysPublished;
            }
        }

        [JsonProperty(PropertyName = "isExcludedFromResults")]
        public bool IsExcludedFromResults
        {
            get
            {
                return ResultDefinition.IsExcludedFromResults;
            }
        }

        [JsonProperty(PropertyName = "isPublishedAsAPrompt")]
        public bool IsPublishedAsAPrompt
        {
            get
            {
                return ResultDefinition.IsPublishedAsAPrompt;
            }
        }

        [JsonProperty(PropertyName = "isRollupPrompts")]
        public bool IsRollupPrompts
        {
            get
            {
                return ResultDefinition.IsRollupPrompts;
            }
        }

        [JsonProperty(PropertyName = "isBooleanResult")]
        public bool IsBooleanResult
        {
            get
            {
                return ResultDefinition.IsBooleanResult;
            }
        }

        [JsonProperty(PropertyName = "isPublishedForNows")]
        public bool IsPublishedForNows
        {
            get
            {
                return ResultDefinition.IsPublishedForNows;
            }
        }

        [JsonProperty(PropertyName = "sequence")]
        public int? Sequence
        {
            get
            {
                return NowRequirement == null ? ResultDefinition.Rank : NowRequirement.ResultDefinitionSequence;
            }
        }

        [JsonProperty(PropertyName = "financial")]
        public bool Financial
        {
            get
            {
                return ResultDefinition.Financial;
            }
        }

        [JsonIgnore]
        public bool IsPrimary 
        {
            get => NowRequirement != null ? NowRequirement.PrimaryResult : false;
        }

        [JsonIgnore]
        public ResultDefinition ResultDefinition 
        {
            get => NowRequirement != null ? NowRequirement.ResultDefinition : rd;
        }

        [JsonIgnore]
        public ResultDefinition RootResultDefinition { get; }

        [JsonProperty(PropertyName = "rootResultDefinitionId")]
        public Guid? RootResultDefinitionId
        {
            get => RootResultDefinition.UUID;
        }

        [JsonProperty(PropertyName = "rootLabel")]
        public string RootResultDefinitionLabel
        {
            get => RootResultDefinition.Label;
        }

        [JsonProperty(PropertyName = "isSelected")]
        public bool IsSelected { get; internal set; }

        [JsonProperty(PropertyName = "isConvicted")]
        public bool IsConvicted { get; internal set; }

        [JsonProperty(PropertyName = "isPublished")]
        public bool IsPublished { get; internal set; }

        [JsonIgnore]
        public ResultDefinitionRule ResultDefinitionRule { get; }

        [JsonProperty(PropertyName = "resultPrompts")]
        public List<ResultPrompt> ResultPrompts { get; set; }

        [JsonProperty(PropertyName = "selectedResultPrompts")]
        public List<ResultPrompt> SelectedResultPrompts
        {
            get
            {
                if (ResultPrompts == null || ResultPrompts.Where(x => x.IsSelected).Count() == 0) { return null; }
                return ResultPrompts.Where(x => x.IsSelected).OrderBy(x => x.Sequence).ToList();
            }
        }

        [JsonProperty(PropertyName = "selectedDistinctResultPromptRequirements")]
        public List<ResultPrompt> SelectedDistinctResultPromptRequirements
        {
            get
            {
                if (SelectedResultPrompts == null || ResultPrompts.Where(x => x.IsSelected).Count() == 0) { return null; }
                return ResultPrompts.Where(x => x.IsSelected).OrderBy(x => x.Sequence).ToList();
            }
        }

        [JsonIgnore]
        public Guid? DefendantCaseId { get; internal set; }

        [JsonProperty(PropertyName = "isDistinctResultRequired")]
        public bool IsDistinctResultRequired
        {
            get => NowRequirement != null ? NowRequirement.DistinctResultTypes : false;
        }

        [JsonProperty(PropertyName = "convicted")]
        public bool Convicted 
        {
            get
            {
                return ResultDefinition.Convicted;
            }
        }

        internal Result(int treeLevel, ResultDefinition rootRD, NowRequirement nowRequirement, ResultDefinitionRule resultDefinitionRule)
        {
            TreeLevel = treeLevel;
            RootResultDefinition = rootRD;
            NowRequirement = nowRequirement;
            ResultDefinitionRule = resultDefinitionRule;
            MakeResultPrompts(nowRequirement.ResultDefinition.ResultPrompts);
        }

        internal Result(int treeLevel, ResultDefinition rootRD, ResultDefinition rd, ResultDefinitionRule resultDefinitionRule, ResultDefinition parentRD)
        {
            TreeLevel = treeLevel;
            RootResultDefinition = rootRD;
            this.rd = rd;
            this.parentRD = parentRD;
            ResultDefinitionRule = resultDefinitionRule;
            MakeResultPrompts(rd.ResultPrompts);
        }

        internal Result(ResultDefinition rd)
        {
            TreeLevel = 0;
            RootResultDefinition = rd;
            this.rd = rd;
            this.parentRD = null;
            ResultDefinitionRule = null;
            MakeResultPrompts(rd.ResultPrompts);
        }

        private void MakeResultPrompts(List<ResultPromptRule> resultPromptRules)
        {
            ResultPrompts = new List<ResultPrompt>();
            if (resultPromptRules != null)
            {
                foreach (var prompt in resultPromptRules.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null).OrderBy(y => y.PromptSequence))
                {
                    //deal with name address
                    if (!string.IsNullOrEmpty(prompt.ResultPrompt.PromptType) && (prompt.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" || prompt.ResultPrompt.PromptType.ToLowerInvariant() == "address"))
                    {
                        //clone some additional prompts
                        CloneNameAddressPrompts(prompt);
                    }
                    else
                    {
                        ResultPrompts.Add(new ResultPrompt(this, prompt));
                    }
                }
            }
        }

        private void CloneNameAddressPrompts(ResultPromptRule prompt)
        {
            ResultPromptRule ruleClone = null;
            DataLib.ResultPrompt promptClone = null;
            var referenceName = ResultDefinition.MakeReferenceName(prompt.ResultPrompt);
            int ctr = prompt.PromptSequence.Value;

            if (prompt.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress")
            {
                //make a organisation name prompt
                ruleClone = prompt.Copy();
                promptClone = ruleClone.ResultPrompt.Copy();
                promptClone.Label = string.Format("{0} organisation name", prompt.ResultPrompt.Label);
                promptClone.PromptReference = string.Format("{0}OrganisationName", referenceName);
                promptClone.UUID = prompt.ResultPromptUUID;
                ruleClone.ResultPrompt = promptClone;
                ruleClone.PromptSequence = ctr++;
                ResultPrompts.Add(new ResultPrompt(this, ruleClone));

                //make a title prompt
                ruleClone = prompt.Copy();
                promptClone = ruleClone.ResultPrompt.Copy();
                promptClone.Label = string.Format("{0} title", prompt.ResultPrompt.Label);
                promptClone.PromptReference = string.Format("{0}Title", referenceName);
                promptClone.UUID = prompt.ResultPromptUUID;
                ruleClone.ResultPrompt = promptClone;
                ruleClone.PromptSequence = ctr++;
                ResultPrompts.Add(new ResultPrompt(this, ruleClone));

                //make a first name prompt
                ruleClone = prompt.Copy();
                promptClone = ruleClone.ResultPrompt.Copy();
                promptClone.Label = string.Format("{0} first name", prompt.ResultPrompt.Label);
                promptClone.PromptReference = string.Format("{0}FirstName", referenceName);
                promptClone.UUID = prompt.ResultPromptUUID;
                ruleClone.ResultPrompt = promptClone;
                ruleClone.PromptSequence = ctr++;
                ResultPrompts.Add(new ResultPrompt(this, ruleClone));

                //make a middle name prompt
                ruleClone = prompt.Copy();
                promptClone = ruleClone.ResultPrompt.Copy();
                promptClone.Label = string.Format("{0} middle name", prompt.ResultPrompt.Label);
                promptClone.PromptReference = string.Format("{0}MiddleName", referenceName);
                promptClone.UUID = prompt.ResultPromptUUID;
                ruleClone.ResultPrompt = promptClone;
                ruleClone.PromptSequence = ctr++;
                ResultPrompts.Add(new ResultPrompt(this, ruleClone));

                //make a last name prompt
                ruleClone = prompt.Copy();
                promptClone = ruleClone.ResultPrompt.Copy();
                promptClone.Label = string.Format("{0} last name", prompt.ResultPrompt.Label);
                promptClone.PromptReference = string.Format("{0}LastName", referenceName);
                promptClone.UUID = prompt.ResultPromptUUID;
                ruleClone.ResultPrompt = promptClone;
                ruleClone.PromptSequence = ctr++;
                ResultPrompts.Add(new ResultPrompt(this, ruleClone));                
            }

            //make 5 address lines
            for (var i = 0; i < 5; i++)
            {
                ruleClone = prompt.Copy();
                promptClone = ruleClone.ResultPrompt.Copy();
                promptClone.Label = string.Format("{0} address line {1}", prompt.ResultPrompt.Label, i + 1);
                promptClone.PromptReference = string.Format("{0}Address{1}", referenceName, i + 1);
                promptClone.UUID = prompt.ResultPromptUUID;
                ruleClone.ResultPrompt = promptClone;
                ruleClone.Rule = ruleClone.Rule == ResultPromptRuleType.mandatory ? (i == 0 ? ResultPromptRuleType.mandatory : ResultPromptRuleType.optional) : ResultPromptRuleType.optional;
                ruleClone.PromptSequence = ctr++;
                ResultPrompts.Add(new ResultPrompt(this, ruleClone));
            }

            //make post code
            ruleClone = prompt.Copy();
            promptClone = ruleClone.ResultPrompt.Copy();
            promptClone.Label = string.Format("{0} post code", prompt.ResultPrompt.Label);
            promptClone.PromptReference = string.Format("{0}PostCode", referenceName);
            promptClone.UUID = prompt.ResultPromptUUID;
            ruleClone.ResultPrompt = promptClone;
            ruleClone.PromptSequence = ctr++;
            ResultPrompts.Add(new ResultPrompt(this, ruleClone));

            //make email 1 & 2
            ruleClone = prompt.Copy();
            promptClone = ruleClone.ResultPrompt.Copy();
            promptClone.Label = string.Format("{0} email address 1", prompt.ResultPrompt.Label);
            promptClone.PromptReference = string.Format("{0}Email1", referenceName);
            promptClone.UUID = prompt.ResultPromptUUID;
            ruleClone.ResultPrompt = promptClone;
            ruleClone.PromptSequence = ctr++;
            ResultPrompts.Add(new ResultPrompt(this, ruleClone));

            ruleClone = prompt.Copy();
            promptClone = ruleClone.ResultPrompt.Copy();
            promptClone.Label = string.Format("{0} email address 2", prompt.ResultPrompt.Label);
            promptClone.PromptReference = string.Format("{0}Email2", referenceName);
            promptClone.UUID = prompt.ResultPromptUUID;
            ruleClone.ResultPrompt = promptClone;
            ruleClone.PromptSequence = ctr++;
            ResultPrompts.Add(new ResultPrompt(this, ruleClone));
        }

        internal void AppendChildPrompts(Result item)
        {
            foreach (var prompt in item.ResultPrompts.OrderBy(x => x.Sequence))
            {
                ResultPrompts.Add(prompt);
            }

            //set the conviction status based on the child result when not already convicted
            if (!IsConvicted) { IsConvicted = item.ResultDefinition.Convicted; }
        }

        internal void MakeAsAPrompt(Result item)
        {
            var resultAsAPrompt = new ResultPrompt(this, item);
            ResultPrompts.Add(resultAsAPrompt);

            //set the conviction status based on the child result when not already convicted
            if (!IsConvicted) { IsConvicted = item.Convicted; }
        }

        internal Result CloneDefendantResult(Guid defendantId)
        {
            if (Level.ToLowerInvariant() != "d") { return null; }
            var resp = (Result)this.MemberwiseClone();

            resp.ResultPrompts = new List<ResultPrompt>();

            //filter the prompts for the passed defendant Id
            if (SelectedResultPrompts != null)
            {
                foreach (var prompt in SelectedResultPrompts.OrderBy(x => x.Sequence))
                {
                    //inspect prompt values
                    if (prompt.Values.Where(x => x.DefendantId == defendantId).Count() > 0)
                    {
                        var clone = prompt.CloneDefendantResultPrompt(defendantId);
                        if (clone != null) { resp.ResultPrompts.Add(clone); }
                    }
                }
            }

            if (resp.ResultPrompts.Count == 0) { resp.ResultPrompts = null; }

            return resp;
        }

        internal Result CloneDefendantCaseResult(Guid? caseId, Guid defendantId, bool forOffenceLevel)
        {
            if (forOffenceLevel && Level.ToLowerInvariant() != "o") { return null; }
            if (!forOffenceLevel && Level.ToLowerInvariant() != "c") { return null; }
            var resp = (Result)this.MemberwiseClone();

            resp.ResultPrompts = new List<ResultPrompt>();

            //filter the prompts for the passed caseId and defendantid
            if (SelectedResultPrompts != null)
            {
                foreach (var prompt in SelectedResultPrompts.OrderBy(x => x.Sequence))
                {
                    //inspect prompt values
                    if (prompt.Values.Where(x => x.CaseId == caseId && x.DefendantId == defendantId).Count() > 0)
                    {
                        var clone = prompt.CloneDefendantCaseResultPrompt(caseId);
                        if (clone != null)
                        {
                            resp.DefendantCaseId = caseId;
                            resp.ResultPrompts.Add(clone);
                        }
                    }
                }
            }

            if (resp.ResultPrompts.Count == 0) { resp.ResultPrompts = null; }

            return resp;
        }

        internal Result CloneOffenceResult(Guid? caseId, Guid offenceId, Guid defendantId)
        {
            if (Level.ToLowerInvariant() != "o") { return null; }
            var resp = (Result)this.MemberwiseClone();
            
            resp.ResultPrompts = new List<ResultPrompt>();

            //filter the prompts for the passed caseId and offence Id
            if (SelectedResultPrompts != null)
            {
                foreach (var prompt in SelectedResultPrompts.OrderBy(x => x.Sequence))
                {
                    //inspect prompt values
                    var promptValues = prompt.Values.Where(x => x.CaseId == caseId && x.OffenceId == offenceId && x.DefendantId == defendantId).ToList();
                    if (promptValues.Count() > 0)
                    {
                        var clone = prompt.CloneOffenceResultPrompt(caseId, offenceId);
                        if (clone != null) { resp.ResultPrompts.Add(clone); }
                    }

                    //include any results as prompt
                    if (prompt.ResultAsAPrompt != null)
                    {
                        var clone = prompt.CloneOffenceResultPrompt(caseId, offenceId);
                        if (clone != null) { resp.ResultPrompts.Add(clone); }
                    }
                }
            }

            if (resp.ResultPrompts.Count == 0) { resp.ResultPrompts = null; }

            return resp;
        }

        internal Result CloneAsAPrompt(Guid? caseId, Guid offenceId)
        {
            var resp = (Result)this.MemberwiseClone();
            var prompts = new List<ResultPrompt>();
            foreach (var prompt in resp.ResultPrompts)
            {
                var clone = prompt.CloneOffenceResultPrompt(caseId, offenceId);
                if (clone != null)
                {
                    prompts.Add(clone);
                }
            }
            resp.ResultPrompts = prompts;

            if (resp.ResultPrompts.Count == 0) { resp.ResultPrompts = null; }

            return resp;
        }

        internal bool DistinctPromptTypes(Guid? resultPromptUUID)
        {
            if (NowRequirement != null && NowRequirement.NowRequirementPromptRules != null && NowRequirement.NowRequirementPromptRules.Count > 0)
            {
                var nrpr = NowRequirement.NowRequirementPromptRules.FirstOrDefault(x => x.ResultPromptRule.ResultPrompt.UUID == resultPromptUUID);
                return nrpr == null ? false : nrpr.DistinctPromptTypes;
            }
            return false;
        }

        internal bool IsVariantData(Guid? resultPromptUUID)
        {
            if (NowRequirement != null && NowRequirement.NowRequirementPromptRules != null && NowRequirement.NowRequirementPromptRules.Count > 0)
            {
                var nrpr = NowRequirement.NowRequirementPromptRules.FirstOrDefault(x => x.ResultPromptRule.ResultPrompt.UUID == resultPromptUUID);
                return nrpr == null ? false : nrpr.IsVariantData;
            }
            return false;
        }
    }
}
