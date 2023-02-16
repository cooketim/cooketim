using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace DataLib
{
    [Serializable]
    public sealed class ResultDefinition : DataAudit, IEquatable<ResultDefinition>
    {
        public ResultDefinition() 
        { 
            UUID = Guid.NewGuid();
            CreatedDate = DateTime.Now; 
            CreatedUser = IdentityHelper.SignedInUser.Email;
            Label = "New Result Definition";
            ShortCode = "Code";
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
            Jurisdiction = "Both";
            Category = "Interim";
            Level = "Offence";
            PostHearingCustodyStatus = "A";
            IsRollupPrompts = true;
        }

        public string Label { get; set; }

        public string TriggeredApplicationCode { get; set; }

        public string ShortCode { get; set; }

        public string PointsDisqualificationCode { get; set; }        

        public string Level { get; set; }

        [JsonIgnore]
        public string LevelFirstLetter
        {
            get
            {
                if (string.IsNullOrEmpty(Level)) { return null; }

                return Level[0].ToString().ToUpper();
            }
        }

        public int? Rank { get; set; }

        //Determines the type of driviung test required when this result is made
        public DrivingTestTypeEnum? StipulatedDrivingTestType { get; set; }

        public bool IsAvailableForCourtExtract { get; set; }

        public bool Financial { get; set; }

        [JsonIgnore]
        public string FinancialYN
        {
            get => Financial ? "Y" : "N";
        }

        /// <summary>
        /// Determines that when this result is published it is published as a prompt when it forms part of a group
        /// </summary>
        public bool IsPublishedAsAPrompt { get; set; }

        /// <summary>
        /// Determines that when this result is within a group result that it is never included in any resulting i.e. only its child results are recorded/ordered in court
        /// </summary>
        public bool IsExcludedFromResults { get; set; }

        /// <summary>
        /// Determines that this result is not transformed i.e. made into a prompt, collapsed onto a parent, etc and is always published without transformation from anywhere within a group structure
        /// </summary>
        public bool IsAlwaysPublished { get; set; }

        /// <summary>
        /// Determines that when this result is within a group result, its prompts are rolled up to the parent result.  When this result is the parent then it is published as-is
        /// </summary>
        public bool IsRollupPrompts { get; set; }

        /// <summary>
        /// Determines that this result is additionally published for the purposes of NOWs generation.  Additional setting that is applicable to "publishedAsAPrompt" & "rollupPrompts"
        /// </summary>
        public bool IsPublishedForNows { get; set; }

        /// <summary>
        /// Determines that this result is set as a life duration
        /// </summary>
        public bool IsLifeDuration { get; set; }

        /// <summary>
        /// Enumerated values are CachableEnum
        /// </summary>
        public CacheableEnum? Cacheable { get; set; }

        /// <summary>
        /// The details of the DSL path setting where the data is stored and the cached is initiated from
        /// </summary>
        public string CacheDataPath { get; set; }

        [JsonIgnore]
        public List<ResultDefinitionRule> ResultDefinitionRules { get; set; }

        /// <summary>
        /// Determines that this result is boolean result
        /// </summary>
        public bool IsBooleanResult { get; set; }

        /// <summary>
        /// Determines that this result is required for judicial validation.  If a child result then the root must also be required for judicial validation
        /// </summary>
        public bool JudicialValidation { get; set; }

        /// <summary>
        /// The collection of definitions and their synonyms built during load based on ResultWordGroupsString
        /// but maintained through UI binding
        /// </summary>
        [JsonIgnore]
        public List<ResultWordGroup> WordGroups { get; set; }

        /// <summary>
        /// Serialisable and abbreviated version of WordGroups 
        /// </summary>
        [JsonIgnore]
        public List<ResultWordGroupJSON> WordGroupJSON
        {
            get
            {
                if (WordGroups == null || WordGroups.Count == 0) { return null; }
                return new List<ResultWordGroupJSON>(
                    (from resultWordGroup in WordGroups.Where(x=>x.ResultDefinitionWordGroups != null && x.ResultDefinitionWordGroups.Any()) select new ResultWordGroupJSON(resultWordGroup)).ToList());
            }
        }

        /// <summary>
        /// The comma separated string from initial data load used to drive the creation of the ResultWordGroups through matching
        /// </summary>
        [JsonIgnore]
        private List<List<string>> resultWordGroupsString;
        public List<List<string>> ResultWordGroupsString
        {
            get
            {
                if (WordGroups != null && WordGroups.Count > 0)
                {
                    var res = new List<List<string>>();
                    foreach(var wg in WordGroups)
                    {
                        var wgString = wg.ResultDefinitionWordGroups.Where(x => x.DeletedDate == null).Select(x => x.UUID.ToString()).ToList();
                        res.Add(wgString);
                    }
                    return res;
                }
                return resultWordGroupsString;
            }
            set
            {
                resultWordGroupsString = value;
            }
        }

        public string ReportSecondaryCJSCodes()
        {
            if (SecondaryCJSCodes != null && SecondaryCJSCodes.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var sc in SecondaryCJSCodes.OrderBy(x => x.CJSCode))
                {
                    sb.AppendLine(string.Format("{0} - {1}", sc.CJSCode, sc.Text));
                }
                return sb.Length == 0 ? "'Empty'" : sb.ToString();
            }
            return "'Empty'";
        }

        public string ReportResultWordGroups()
        {
            if (WordGroups != null && WordGroups.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var wgString in WordGroups.Select(x=>x.ResultDefinitionWordGroups.Where(y => y.DeletedDate == null).OrderBy(y => y.ResultDefinitionWord).Select(y => y.ResultDefinitionWord)))
                {
                    sb.AppendLine(string.Join(", ", wgString));
                }
                return sb.Length == 0 ? "'Empty'" : sb.ToString();
            }
            return "'Empty'";
        }

        public bool Adjournment { get; set; }

        [JsonIgnore]
        public string AdjournmentYN
        {
            get => Adjournment ? "Y" : "N";
        }

        public string Category { get; set; }

        [JsonIgnore]
        public string CategoryFirstLetter
        {
            get
            {
                if (string.IsNullOrEmpty(Category))
                {
                    return null;
                }

                return Category[0].ToString().ToUpper();
            }
        }

        public bool Urgent { get; set; }

        public bool Unscheduled { get; set; }

        public bool Convicted { get; set; }

        [JsonIgnore]
        public string ConvictedYN
        {
            get => Convicted ? "Y" : "N";
        }

        public bool D20 { get; set; }

        public string ResultDefinitionGroup { get; set; }

        public string DVLACode { get; set; }

        public string CJSCode { get; set; }

        public List<SecondaryCJSCode> SecondaryCJSCodes { get; set; }

        /// <summary>
        /// Serialisable an abbreviated version of Secondary CLS Codes 
        /// </summary>
        [JsonIgnore]
        public List<SecondaryCJSCodeJSON> SecondaryCJSCodesJSON
        {
            get
            {
                if (SecondaryCJSCodes == null || SecondaryCJSCodes.Count == 0) { return null; }
                return new List<SecondaryCJSCodeJSON>(
                    (from code in SecondaryCJSCodes select new SecondaryCJSCodeJSON(code)).ToList());
            }
        }

        public string Qualifier { get; set; }

        public string PostHearingCustodyStatus { get; set; }

        public string Jurisdiction { get; set; }

        [JsonIgnore]
        public string JurisdictionLetter
        {
            get
            {
                return Jurisdiction[0].ToString().ToUpper();
            }
        }

        public string LibraCode { get; set; }

        public string LCode { get; set; }

        public List<string> UserGroups { get; set; }

        [JsonIgnore]
        public string[] UserGroupStringArray
        {
            get
            {
                //if we have prompts then we do not need to have user groups for the result
                if (ResultPrompts != null && ResultPrompts.Where(x => x.DeletedDate == null).Count() > 0) { return null; }

                //if we dont have user groups we want an empty collection as the collection is mandatory on CPP
                if (UserGroups == null || UserGroups.Count == 0) { return new string[0]; }

                //We should have user groups for the result definition
                return UserGroups.ToArray();
            }
        }

        public bool? IsBusinessResult { get; set; }

        public string ResultTextTemplate { get; set; }

        public string DependantResultDefinitionGroup { get; set; }

        public string ResultWording { get; set; }

        public string WelshResultWording { get; set; }

        public string WelshLabel { get; set; }

        /// <summary>
        /// Determines that when this result is ordered that the proceedings for the given offence are terminated
        /// </summary>
        public bool TerminatesOffenceProceedings { get; set; }

        [JsonIgnore]
        public List<ResultPromptRule> ResultPrompts { get; set; }

        public List<ResultPromptJSON> MakeResultPromptsJSONList(List<string> featureToggles)
        {
            if (ResultPrompts == null || ResultPrompts.Where(x => x.DeletedDate == null).Count() == 0) { return null; }

            //make additional prompts for durations & wordGroups
            var res = MakeDurationAndWordGroupResultPrompts(featureToggles);

            //make additional prompts for name address parts
            res.AddRange(MakeNameAddressResultPrompts(featureToggles));

            //deal with scenario where there are not any durations or word groups or name address
            foreach (var prompt in ResultPrompts.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null))
            {
                if ((prompt.ResultPrompt.ResultPromptWordGroups == null || prompt.ResultPrompt.ResultPromptWordGroups.Count == 0) &&
                    (prompt.ResultPrompt.DurationElements == null || prompt.ResultPrompt.DurationElements.Count == 0) &&
                     prompt.ResultPrompt.PromptType != "NAMEADDRESS" &&
                     prompt.ResultPrompt.PromptType != "ADDRESS"
                   )
                {
                    res.Add(new ResultPromptJSON(this, prompt, featureToggles));
                }
            }

            return res;
        }

        public bool CanBeSubjectOfBreach { get; set; }

        public bool CanBeSubjectOfVariation { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [JsonIgnore]
        public string StartDateJSON
        {
            get
            {
                if (StartDate == null) { return null; }
                return StartDate.Value.ToString("yyyy-MM-dd");
            }
        }

        [JsonIgnore]
        public string EndDateJSON
        {
            get
            {
                if (EndDate == null) { return null; }
                return EndDate.Value.ToString("yyyy-MM-dd");
            }
        }

        public static List<ResultPromptPart> MakeNameParts(ResultPrompt prompt)
        {
            var res = new List<ResultPromptPart>();
            var referenceName = MakeReferenceName(prompt);

            if (string.IsNullOrEmpty(prompt.NameAddressType) || prompt.NameAddressType.ToLowerInvariant() == "both")
            {
                res.Add(new ResultPromptPart("OrganisationName", string.Format("{0}OrganisationName", referenceName), string.Format("{0} organisation name", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
                res.Add(new ResultPromptPart("Title", string.Format("{0}Title", referenceName), string.Format("{0} title", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
                res.Add(new ResultPromptPart("FirstName", string.Format("{0}FirstName", referenceName), string.Format("{0} first name", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
                res.Add(new ResultPromptPart("MiddleName", string.Format("{0}MiddleName", referenceName), string.Format("{0} middle name", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
                res.Add(new ResultPromptPart("LastName", string.Format("{0}LastName", referenceName), string.Format("{0} last name", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
            }

            if (prompt.NameAddressType.ToLowerInvariant() == "person")
            {
                res.Add(new ResultPromptPart("Title", string.Format("{0}Title", referenceName), string.Format("{0} title", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
                res.Add(new ResultPromptPart("FirstName", string.Format("{0}FirstName", referenceName), string.Format("{0} first name", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
                res.Add(new ResultPromptPart("MiddleName", string.Format("{0}MiddleName", referenceName), string.Format("{0} middle name", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
                res.Add(new ResultPromptPart("LastName", string.Format("{0}LastName", referenceName), string.Format("{0} last name", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
            }

            if (prompt.NameAddressType.ToLowerInvariant() == "organisation")
            {
                res.Add(new ResultPromptPart("OrganisationName", string.Format("{0}OrganisationName", referenceName), string.Format("{0} organisation name", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
            }
            return res;
        }

        public static List<ResultPromptPart> MakeAddressParts(ResultPrompt prompt)
        {
            var res = new List<ResultPromptPart>();
            var referenceName = MakeReferenceName(prompt);

            //make 5 address lines
            for (var i = 0; i < 5; i++)
            {
                res.Add(
                    new ResultPromptPart(string.Format("AddressLine{0}", i + 1), string.Format("{0}Address{1}", referenceName, i + 1), string.Format("{0} address line {1}", prompt.Label, i + 1), referenceName, prompt.NameAddressType, prompt.Hidden));
            }

            res.Add(new ResultPromptPart("PostCode", string.Format("{0}PostCode", referenceName), string.Format("{0} post code", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
            res.Add(new ResultPromptPart("EmailAddress1", string.Format("{0}EmailAddress1", referenceName), string.Format("{0} email address 1", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));
            res.Add(new ResultPromptPart("EmailAddress2", string.Format("{0}EmailAddress2", referenceName), string.Format("{0} email address 2", prompt.Label), referenceName, prompt.NameAddressType, prompt.Hidden));

            return res;
        }

        private List<ResultPromptJSON> MakeNameAddressResultPrompts(List<string> featureToggles)
        {
            var res = new List<ResultPromptJSON>();
            foreach (var prompt in ResultPrompts.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null && (x.ResultPrompt.PromptType == "NAMEADDRESS" || x.ResultPrompt.PromptType == "ADDRESS")))
            {
                var referenceName = MakeReferenceName(prompt.ResultPrompt);
                if (prompt.ResultPrompt.PromptType == "ADDRESS")
                {
                    res.AddRange(MakeAddressResultPrompts(prompt, featureToggles));
                }
                else
                {
                    var parts = MakeNameParts(prompt.ResultPrompt);
                    foreach (var part in parts)
                    {
                        res.Add(new ResultPromptJSON(
                                    this,
                                    prompt,
                                    part.ComponentPartName,
                                    part.ShortComponentPartName,
                                    part.Reference,
                                    part.Label,
                                    part.ReferenceName,
                                    featureToggles
                                    )
                           );
                    }
                    res.AddRange(MakeAddressResultPrompts(prompt, featureToggles));
                }
            }
            return res;
        }

        public static string MakeReferenceName(ResultPrompt prompt)
        {
            var label = string.IsNullOrEmpty(prompt.PromptReference) ? prompt.Label : prompt.PromptReference;
            //convert label to camel case
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");
            var text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(rgx.Replace(label, string.Empty)).Replace(" ", string.Empty);
            return $"{text.First().ToString().ToLowerInvariant()}{text.Substring(1)}";
        }

        private List<ResultPromptJSON> MakeAddressResultPrompts(ResultPromptRule prompt, List<string> featureToggles)
        {
            var res = new List<ResultPromptJSON>();
            var parts = MakeAddressParts(prompt.ResultPrompt);
            foreach (var part in parts)
            {
                res.Add(new ResultPromptJSON(
                                this,
                                prompt,
                                part.ComponentPartName,
                                part.ShortComponentPartName,
                                part.Reference,
                                part.Label,
                                part.ReferenceName,
                                featureToggles
                                )
                           );
            }
            return res;
        }

        private List<ResultPromptJSON> MakeDurationAndWordGroupResultPrompts(List<string> featureToggles)
        {
            var res = new List<ResultPromptJSON>();
            foreach (var prompt in ResultPrompts.Where(x => x.DeletedDate == null && x.ResultPrompt.DeletedDate == null))
            {
                var wordGroupsToProcess = new List<ResultPromptWordGroup>();
                if (prompt.ResultPrompt.ResultPromptWordGroups != null && prompt.ResultPrompt.ResultPromptWordGroups.Where(x=>x.DeletedDate == null).Count() > 0) { wordGroupsToProcess.AddRange(prompt.ResultPrompt.ResultPromptWordGroups.Where(x => x.DeletedDate == null)); }
                var durationsToProcess = new List<ResultPromptDuration>();
                if (prompt.ResultPrompt.DurationElements != null && prompt.ResultPrompt.DurationElements.Count > 0) { durationsToProcess.AddRange(prompt.ResultPrompt.DurationElements); }

                //create prompts for each of the duration elements that match any word groups
                var processedWordGroups = new List<ResultPromptWordGroup>();
                foreach (var duration in durationsToProcess)
                {
                    //look for matching word group
                    var matchedWG = wordGroupsToProcess.FirstOrDefault(x => x.ResultPromptWord.ToLowerInvariant() == duration.DurationElement.ToLowerInvariant());

                    if (matchedWG != null)
                    {
                        res.Add(new ResultPromptJSON(this, prompt, new string[] { matchedWG.ResultPromptWord }, duration, featureToggles));
                        processedWordGroups.Add(matchedWG);
                    }
                    else
                    {
                        //just duration on its own
                        res.Add(new ResultPromptJSON(this, prompt, null, duration, featureToggles));
                    }
                }

                //determine any remaining word groups
                var remainingWordGroups = new List<ResultPromptWordGroup>();
                foreach (var item in wordGroupsToProcess)
                {
                    //check if already processed
                    if (processedWordGroups.FirstOrDefault(x => x.UUID == item.UUID) == null)
                    {
                        remainingWordGroups.Add(item);
                    }
                }

                //create a final prompt for the remaining word groups
                if (remainingWordGroups.Count > 0)
                {
                    var wordGroups = remainingWordGroups.Select(x => x.ResultPromptWord).ToArray();
                    res.Add(new ResultPromptJSON(this, prompt, wordGroups, null, featureToggles));
                }
            }

            return res;
        }

        public ResultDefinition Draft()
        {
            var res = MakeCopy(true);

            //make new draft rules for each of the child results
            var newRules = new List<ResultDefinitionRule>();
            if (res.ResultDefinitionRules != null)
            {
                foreach (var rule in res.ResultDefinitionRules.Where(x => x.DeletedDate == null))
                {
                    var newRule = rule.Draft();
                    newRule.ParentUUID = res.UUID;
                    newRules.Add(newRule);
                }                
            }
            res.ResultDefinitionRules = newRules;

            //make new draft rules for each of the prompts
            var newPromptRules = new List<ResultPromptRule>();
            if (res.ResultPrompts != null)
            {
                foreach (var rule in res.ResultPrompts.Where(x => x.DeletedDate == null))
                {
                    var newRule = rule.Draft();
                    newRule.ResultDefinitionUUID = res.UUID;
                    newPromptRules.Add(newRule);
                }                
            }
            res.ResultPrompts = newPromptRules;

            return res;
        }

        public ResultDefinition Copy()
        {
            var res = MakeCopy(false);

            //make new copied rules for each of the child results
            if (res.ResultDefinitionRules != null)
            {
                var newRules = new List<ResultDefinitionRule>();
                foreach (var rule in res.ResultDefinitionRules.Where(x => x.DeletedDate == null))
                {
                    var newRule = rule.Copy();
                    newRule.ParentUUID = res.UUID;
                    newRules.Add(newRule);
                }
                res.ResultDefinitionRules = newRules;
            }

            //make new copied rules for each of the prompts
            if (res.ResultPrompts != null)
            {
                var newRules = new List<ResultPromptRule>();
                foreach (var rule in res.ResultPrompts.Where(x => x.DeletedDate == null))
                {
                    var newRule = rule.Copy();
                    newRule.ResultDefinitionUUID = res.UUID;
                    newRules.Add(newRule);
                }
                res.ResultPrompts = newRules;
            }

            return res;
        }

        public ResultDefinition CopyAsDraft()
        {
            var res = MakeCopy(false);

            //set as draft
            res.PublishedStatus = DataLib.PublishedStatus.Draft;

            //make new copied rules for each of the child results
            if (res.ResultDefinitionRules != null)
            {
                var newRules = new List<ResultDefinitionRule>();
                foreach (var rule in res.ResultDefinitionRules.Where(x => x.DeletedDate == null))
                {
                    var newRule = rule.CopyAsDraft();
                    newRule.ParentUUID = res.UUID;
                    newRules.Add(newRule);
                }
                res.ResultDefinitionRules = newRules;
            }

            //make new copied rules for each of the prompts
            if (res.ResultPrompts != null)
            {
                var newRules = new List<ResultPromptRule>();
                foreach (var rule in res.ResultPrompts.Where(x => x.DeletedDate == null))
                {
                    var newRule = rule.CopyAsDraft();
                    newRule.ResultDefinitionUUID = res.UUID;
                    newRules.Add(newRule);
                }
                res.ResultPrompts = newRules;
            }

            return res;
        }

        private ResultDefinition MakeCopy(bool asDraft)
        {
            ResultDefinition res = (ResultDefinition)MemberwiseClone();
            
            res.UUID = Guid.NewGuid();
            res.CreatedDate = DateTime.Now;
            res.LastModifiedDate = DateTime.Now;
            res.DeletedDate = null;
            res.DeletedUser = null;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

            if (UserGroups != null)
            {
                res.UserGroups = new List<string>(UserGroups);
            }
            else
            {
                res.UserGroups = new List<string>();
            }

            //remove the publication tags
            res.PublicationTags = new List<string>();

            //replace word group collection
            res.WordGroups = new List<ResultWordGroup>();
            if (WordGroups != null)
            {
                foreach (var wg in WordGroups)
                {
                    var newWG = new ResultWordGroup(res);
                    newWG.ResultDefinitionWordGroups.AddRange(wg.ResultDefinitionWordGroups);
                    res.WordGroups.Add(newWG);
                }
            }

            //replace secondary CJS Code
            res.SecondaryCJSCodes = new List<SecondaryCJSCode>();
            if (SecondaryCJSCodes != null)
            {                
                foreach (var sc in SecondaryCJSCodes)
                {
                    //make a new copy becuse the codes are not reused across result definitions in the same way as word groups
                    var newSC = sc.Copy();
                    res.SecondaryCJSCodes.Add(newSC);
                }
            }

            if (asDraft)
            {
                res.PublishedStatus = DataLib.PublishedStatus.Draft;
                if (res.MasterUUID == null)
                {
                    res.MasterUUID = MasterUUID == null ? UUID : MasterUUID;
                }
            }
            else
            {
                res.MasterUUID = res.UUID;
                res.Label = res.Label + " - Copy";
                res.ShortCode = res.ShortCode + " - Copy";
            }

            return res;
        }

        public bool SynchroniseChanges(ResultDefinition source)
        {
            if (source == this)
            {
                //rank must have changed as it is not included in the equality check
                Rank = source.Rank;
                LastModifiedDate = source.LastModifiedDate;
                LastModifiedUser = source.LastModifiedUser;
                return false;
            }

            Label = source.Label;
            ShortCode = source.ShortCode;
            PointsDisqualificationCode = source.PointsDisqualificationCode;
            Level = source.LevelFirstLetter;
            Rank = source.Rank;
            StipulatedDrivingTestType = source.StipulatedDrivingTestType;
            IsAvailableForCourtExtract = source.IsAvailableForCourtExtract;
            Financial = source.Financial;
            IsPublishedAsAPrompt = source.IsPublishedAsAPrompt;
            IsExcludedFromResults = source.IsExcludedFromResults;
            IsAlwaysPublished = source.IsAlwaysPublished;
            IsRollupPrompts = source.IsRollupPrompts;
            IsPublishedForNows = source.IsPublishedForNows;
            IsLifeDuration = source.IsLifeDuration;
            WordGroups = source.WordGroups;
            Adjournment = source.Adjournment;
            Category = source.Category;
            Urgent = source.Urgent;
            Unscheduled = source.Unscheduled;
            Convicted = source.Convicted;
            D20 = source.D20;
            ResultDefinitionGroup = source.ResultDefinitionGroup;
            DVLACode = source.DVLACode;
            CJSCode = source.CJSCode;
            Qualifier = source.Qualifier;
            PostHearingCustodyStatus = source.PostHearingCustodyStatus;
            Jurisdiction = source.JurisdictionLetter;
            LibraCode = source.LibraCode;
            LCode = source.LCode;
            UserGroups = source.UserGroups;
            ResultWording = source.ResultWording;
            WelshResultWording = source.WelshResultWording;
            WelshLabel = source.WelshLabel;
            TerminatesOffenceProceedings = source.TerminatesOffenceProceedings;
            Cacheable = source.Cacheable;
            CacheDataPath = source.CacheDataPath;
            LastModifiedDate = source.LastModifiedDate;

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResultDefinition);
        }

        public bool Equals(ResultDefinition other)
        {
            return other != null &&
                string.IsNullOrEmpty(GetChangedProperties<ResultDefinition>(this, other)) &&
                WordGroupListsAreEqual(other.WordGroups) &&
                UserGroupListsAreEqual(other.UserGroups) &&
                SecondaryCJSCodeListsAreEqual(other.SecondaryCJSCodes);
        }

        public string GetChangedProperties(ResultDefinition other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = GetChangedProperties<ResultDefinition>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.Append(simpleTypeChanges);
            }
            //now add reports for the complex types:
            if (!WordGroupListsAreEqual(other.WordGroups))
            {
                sb.AppendLine("From Word Groups...");
                sb.Append(ReportResultWordGroups());
                sb.AppendLine("To Word Groups...");
                sb.AppendLine(other.ReportResultWordGroups());
            }

            if (!UserGroupListsAreEqual(other.UserGroups))
            {
                sb.AppendLine("From User Groups...");
                if (UserGroups == null || UserGroups.Count == 0)
                {
                    sb.AppendLine("'Empty'");
                }
                else
                {
                    sb.AppendLine(string.Join(", ", UserGroups));
                }
                sb.AppendLine("To User Groups...");
                if (other.UserGroups == null || other.UserGroups.Count == 0)
                {
                    sb.AppendLine("'Empty'");
                }
                else
                {
                    sb.AppendLine(string.Join(",", other.UserGroups));
                }
            }

            if (!SecondaryCJSCodeListsAreEqual(other.SecondaryCJSCodes))
            {
                sb.AppendLine("From Secondary CJSCodes...");                
                sb.AppendLine(ReportSecondaryCJSCodes());
                sb.AppendLine("To Secondary CJSCodes...");
                sb.AppendLine(other.ReportSecondaryCJSCodes());
            }

            return sb.ToString();
        }

        private bool WordGroupListsAreEqual(List<ResultWordGroup> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.WordGroups == null || this.WordGroups.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.WordGroups == null)
            {
                return false;
            }
            //compare the two lists but based on just the word group names rather than synonym equality
            var currentWords = WordGroups.Select(x => x.WordGroupName);
            var otherWords = other.Select(x => x.WordGroupName);
            var currentNotOther = currentWords.Except(otherWords).ToList();
            var otherNotCurrent = otherWords.Except(currentWords).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }

        public int GetWordGroupListHashCode()
        {
            int hc = 0;
            if (WordGroups != null)
            {
                var reordered = WordGroups.OrderBy(x => x.WordGroupName);
                foreach (var w in reordered)
                    hc ^= w.GetHashCode();
            }
            return hc;
        }

        private bool UserGroupListsAreEqual(List<string> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.UserGroups == null || this.UserGroups.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.UserGroups == null)
            {
                return false;
            }

            //compare the two lists
            var currentNotOther = UserGroups.Except(other).ToList();
            var otherNotCurrent = other.Except(UserGroups).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }

        private bool SecondaryCJSCodeListsAreEqual(List<SecondaryCJSCode> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.SecondaryCJSCodes == null || this.SecondaryCJSCodes.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.SecondaryCJSCodes == null)
            {
                return false;
            }

            //compare the two lists
            var currentNotOther = SecondaryCJSCodes.Except(other).ToList();
            var otherNotCurrent = other.Except(SecondaryCJSCodes).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }

        public int GetUserGroupListHashCode()
        {
            int hc = 0;
            if (UserGroups != null)
            {
                var reordered = UserGroups.OrderBy(x => x);
                foreach (var u in reordered)
                    hc ^= u.GetHashCode();
            }
            return hc;
        }

        public int GetSecondaryCLSCodeListHashCode()
        {
            int hc = 0;
            if (SecondaryCJSCodes != null)
            {
                var reordered = SecondaryCJSCodes.OrderBy(x => x.CJSCode);
                foreach (var s in SecondaryCJSCodes)
                    hc ^= s.GetHashCode();
            }
            return hc;
        }

        public override int GetHashCode()
        {
            var hashCode = -1211238045;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Label);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PointsDisqualificationCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LevelFirstLetter);

            //discount the rank from any equality test
            //hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(Rank);

            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode((int?)StipulatedDrivingTestType);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsAvailableForCourtExtract);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(Financial);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsPublishedAsAPrompt);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsExcludedFromResults);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsAlwaysPublished);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsRollupPrompts);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsPublishedForNows);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(IsLifeDuration);
            hashCode = hashCode * -1521134295 + GetWordGroupListHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(Adjournment);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Category);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(Urgent);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(Unscheduled);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(Convicted);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(D20);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ResultDefinitionGroup);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DVLACode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CJSCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Qualifier);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PostHearingCustodyStatus);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(JurisdictionLetter);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LibraCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LCode);
            hashCode = hashCode * -1521134295 + GetUserGroupListHashCode();
            hashCode = hashCode * -1521134295 + GetSecondaryCLSCodeListHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ResultWording);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WelshResultWording);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WelshLabel);
            hashCode = hashCode * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(TerminatesOffenceProceedings);
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode((int?)Cacheable);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CacheDataPath);
            return hashCode;
        }

        public static bool operator ==(ResultDefinition rd1, ResultDefinition rd2)
        {
            return EqualityComparer<ResultDefinition>.Default.Equals(rd1, rd2);
        }

        public static bool operator !=(ResultDefinition rd1, ResultDefinition rd2)
        {
            return !(rd1 == rd2);
        }
    }

    [Serializable]
    public sealed class ResultDefinitionJSON
    {
        private ResultDefinition resultDefinition;
        private DateTime? patchDate;
        private List<string> featureToggles;

        public ResultDefinitionJSON(ResultDefinition resultDefinition, DateTime? patchDate, List<string> featureToggles) 
        { 
            this.resultDefinition = resultDefinition; 
            this.patchDate = patchDate;
            this.featureToggles = featureToggles;
        }

        public Guid? id { get { return resultDefinition.MasterUUID ?? resultDefinition.UUID; } }

        public string label { get { return resultDefinition.Label; } }

        public string triggeredApplicationCode
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x=>x.ToUpperInvariant().Contains("DD-16049")) != null)
                {
                    return resultDefinition.TriggeredApplicationCode;
                }
                return null;
            }
        }

        public string shortCode { get { return resultDefinition.ShortCode; } }

        public string pointsDisqualificationCode 
        { 
            get 
            {
                if (featureToggles != null)
                {
                    return resultDefinition.PointsDisqualificationCode;
                }
                return null;
            } 
        }

        public string level { get { return resultDefinition.LevelFirstLetter; } }

        public int? rank { get { return resultDefinition.Rank; } }

        public int? drivingTestStipulation
        { 
            get 
            {
                if (resultDefinition.StipulatedDrivingTestType == null || resultDefinition.StipulatedDrivingTestType == DrivingTestTypeEnum.noTest)
                {
                    return null;
                }
                return (int?)resultDefinition.StipulatedDrivingTestType;
            }
        }

        public bool? isBooleanResult { get { return resultDefinition.IsBooleanResult; } }

        /// <summary>
        /// Determines that this result is required for judicial validation.  If a child result then the root must also be required for judicial validation
        /// </summary>
        public bool? judicialValidation
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x=>x.ToUpperInvariant().Contains("CCT-1076")) != null)
                {
                    if (resultDefinition.JudicialValidation)
                    {
                        return true;
                    }
                }
                return null;
            }
        }

        public bool isAvailableForCourtExtract { get { return resultDefinition.IsAvailableForCourtExtract; } }

        public string financial { get { return resultDefinition.FinancialYN; } }

        public bool unscheduled { get { return resultDefinition.Unscheduled; } }

        public bool canBeSubjectOfBreach { get { return resultDefinition.CanBeSubjectOfBreach; } }

        public bool canBeSubjectOfVariation { get { return resultDefinition.CanBeSubjectOfVariation; } }

        public bool publishedAsAPrompt { get { return resultDefinition.IsPublishedAsAPrompt; } }

        public bool excludedFromResults { get { return resultDefinition.IsExcludedFromResults; } }

        public bool alwaysPublished { get { return resultDefinition.IsAlwaysPublished; } }

        public bool rollUpPrompts { get { return resultDefinition.IsRollupPrompts; } }

        public bool publishedForNows { get { return resultDefinition.IsPublishedForNows; } }

        public bool lifeDuration { get { return resultDefinition.IsLifeDuration; } }

        public List<ResultDefinitionRuleJSON> resultDefinitionRules
        {
            get
            {
                if (resultDefinition.ResultDefinitionRules == null || resultDefinition.ResultDefinitionRules.Where(x => x.DeletedDate == null).Count() == 0)
                {
                    return null;
                }
                var res = new List<ResultDefinitionRuleJSON>();

                foreach (var item in resultDefinition.ResultDefinitionRules.Where(x => x.DeletedDate == null))
                {
                    //add the child rules of the result to this result
                    res.Add(new ResultDefinitionRuleJSON(item,featureToggles));
                }
                return res;
            }
        }

        public List<ResultWordGroupJSON> wordGroups { get { return resultDefinition.WordGroupJSON; } }

        public string adjournment { get { return resultDefinition.AdjournmentYN; } }

        public string category { get { return resultDefinition.CategoryFirstLetter; } }

        public bool urgent { get { return resultDefinition.Urgent; } }

        public string convicted { get { return resultDefinition.ConvictedYN; } }

        public bool d20 { get { return resultDefinition.D20; } }

        public string resultDefinitionGroup { get { return resultDefinition.ResultDefinitionGroup; } }

        public string dvlaCode { get { return resultDefinition.DVLACode; } }

        public string cjsCode { get { return resultDefinition.CJSCode; } }

        public string qualifier { get { return resultDefinition.Qualifier; } }

        public string postHearingCustodyStatus { get { return resultDefinition.PostHearingCustodyStatus; } }

        public string jurisdiction { get { return resultDefinition.JurisdictionLetter; } }

        public string libraCode { get { return resultDefinition.LibraCode; } }

        public string lCode { get { return resultDefinition.LCode; } }

        public List<SecondaryCJSCodeJSON> secondaryCJSCodes 
        { 
            get 
            {
                return resultDefinition.SecondaryCJSCodesJSON;
            } 
        }

        /// <summary>
        /// Enumerated values are CachableEnum
        /// </summary>
        public CacheableEnum? cacheable
        {
            get
            {
                return resultDefinition.Cacheable;
            }
        }

        /// <summary>
        /// The details of the DSL path setting where the data is stored and the cached is initiated from
        /// </summary>
        public string cacheDataPath
        {
            get
            {
                return resultDefinition.CacheDataPath;
            }
        }

        public string[] userGroups { get { return resultDefinition.UserGroupStringArray; } }

        /// <summary>
        /// Indicates if the result is a business result and therefore available to be matched/parsed in Notepad
        /// </summary>
        public bool? isBusinessResult
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x=>x.ToUpperInvariant().Contains("CCT-1471")) != null)
                {
                    return resultDefinition.IsBusinessResult;
                }
                return null;
            }
        }

        /// <summary>
        /// Describes the template for rendering the result text
        /// </summary>
        public string resultTextTemplate
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x=>x.ToUpperInvariant().Contains("CCT-1326")) != null)
                {
                    return resultDefinition.ResultTextTemplate;
                }
                return null;
            }
        }

        /// <summary>
        /// Describes the dependent result definition group which informs the display sequence of results in CAAG, etc
        /// Where a dependant group is specified for a result any other published results that are within that group 
        /// must be displayed immediately after the result in-hand
        /// </summary>
        public string dependantResultDefinitionGroup
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1326")) != null)
                {
                    return resultDefinition.DependantResultDefinitionGroup;
                }
                return null;
            }
        }

        public string resultWording { get { return resultDefinition.ResultWording; } }

        public string welshResultWording { get { return resultDefinition.WelshResultWording; } }

        public string welshLabel { get { return resultDefinition.WelshLabel; } }

        public bool terminatesOffenceProceedings { get { return resultDefinition.TerminatesOffenceProceedings; } }

        public List<ResultPromptJSON> prompts { get { return resultDefinition.MakeResultPromptsJSONList(featureToggles); } }

        public string startDate { get => resultDefinition.StartDate.HasValue && patchDate.HasValue && resultDefinition.StartDate.Value.Date < patchDate.Value.Date ? null : resultDefinition.StartDateJSON; }
    }    
}