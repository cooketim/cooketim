using DataAccessLayer;
using DataAccessLayer.Models;
using DataLib;
using DataLib.DataModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace SupportTooling
{
    public class TreeServices
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings();
        private List<DataAccessLayer.Models.ResultDefinition> entitiesRD = new List<DataAccessLayer.Models.ResultDefinition>();
        private List<DataAccessLayer.Models.ResultDefinitionRule> entitiesRDR = new List<DataAccessLayer.Models.ResultDefinitionRule>();
        private List<DataAccessLayer.Models.ResultDefinitionWordGroup> entitiesRDWG = new List<DataAccessLayer.Models.ResultDefinitionWordGroup>();
        private List<DataAccessLayer.Models.ResultPrompt> entitiesPrompt = new List<DataAccessLayer.Models.ResultPrompt>();
        private List<DataAccessLayer.Models.ResultPromptRule> entitiesRPR = new List<DataAccessLayer.Models.ResultPromptRule>();
        private List<DataAccessLayer.Models.ResultPromptWordGroup> entitiesRPWG = new List<DataAccessLayer.Models.ResultPromptWordGroup>();
        private List<DataAccessLayer.Models.FixedList> entitiesFL = new List<DataAccessLayer.Models.FixedList>();
        private List<DataAccessLayer.Models.Now> entitiesNow = new List<DataAccessLayer.Models.Now>();
        private List<DataAccessLayer.Models.NowRequirement> entitiesNR = new List<DataAccessLayer.Models.NowRequirement>();
        private List<DataAccessLayer.Models.NowSubscription> entitiesSub = new List<DataAccessLayer.Models.NowSubscription>();
        private List<DataAccessLayer.Models.Comment> entitiesComment = new List<DataAccessLayer.Models.Comment>();

        private FileService fs = new FileService();

        public void Apply1635_1399Changes(string filePath)
        {
            var data = fs.GetFileData(filePath);

            //1635 - imprisonment reasons prompt Rule for Imprisonment ...
            var rule = data.ResultPromptRules.First(x => x.UUID == new Guid("c0a468c9-cd0a-4bbf-83cd-39cec21af078"));
            rule.RuleMags = ResultPromptRuleType.mandatory;
            rule.RuleCrown = ResultPromptRuleType.optional;
            rule.Rule = null;

            //1635 - resons for custody prompt Rule for Imprisonment ...
            rule = data.ResultPromptRules.First(x => x.UUID == new Guid("4f556054-7c69-4085-8682-e0a179a32ff8"));
            rule.RuleMags = ResultPromptRuleType.mandatory;
            rule.RuleCrown = ResultPromptRuleType.optional;
            rule.Rule = null;

            //1399 - data fix result definitions appear to be sharing the publication tags collection, ensure that each result
            // has its own collection
            foreach (var res in data.ResultDefinitions.Where(x=>x.PublicationTags != null && x.PublicationTags.Any()))
            {
                var publicationTags = new List<string>();
                publicationTags.AddRange(res.PublicationTags);
                res.PublicationTags = publicationTags;
            }

            //1399 - set new vocab on Major Creditor NOW Subscription
            var dateToSet = DateTime.Now;
            var nowSubs = data.NowSubscriptions.First(x => x.UUID == new Guid("6c851d6c-7502-4101-aaf8-225c2647befa"));
            nowSubs.SubscriptionVocabulary.ProsecutorMajorCreditor = true;
            nowSubs.LastModifiedDate = dateToSet;
            nowSubs.LastModifiedUser = "tim.cooke@solirius.com";

            //1399 - create a new clone of Major Creditor NOW Subscription, setting distrubution by major creditor
            var cloneNowSubs = nowSubs.MakeCopy(false);
            cloneNowSubs.Name = "Major Creditor (Non Prosecutor)";
            cloneNowSubs.IsForDistribution = true;
            cloneNowSubs.IsSecondClassLetter = true;
            cloneNowSubs.ApplySubscriptionRules = true;
            cloneNowSubs.SubscriptionVocabulary.NonProsecutorMajorCreditor = true;
            cloneNowSubs.SubscriptionVocabulary.ProsecutorMajorCreditor = false;
            cloneNowSubs.ResultRecipient = true;
            cloneNowSubs.NowRecipient.RecipientFromResults = true;

            var resultPromptRule = data.ResultPromptRules.First(x => x.UUID == new Guid("14390512-1040-448d-a4c8-47ddca51cdbb"));
            cloneNowSubs.NowRecipient.OrganisationNameResultPrompt = resultPromptRule;
            cloneNowSubs.NowRecipient.Address1ResultPrompt = resultPromptRule;
            cloneNowSubs.NowRecipient.Address2ResultPrompt = resultPromptRule;
            cloneNowSubs.NowRecipient.Address3ResultPrompt = resultPromptRule;
            cloneNowSubs.NowRecipient.Address4ResultPrompt = resultPromptRule;
            cloneNowSubs.NowRecipient.Address5ResultPrompt = resultPromptRule;
            cloneNowSubs.NowRecipient.PostCodeResultPrompt = resultPromptRule;
            cloneNowSubs.NowRecipient.EmailAddress1ResultPrompt = resultPromptRule;
            cloneNowSubs.NowRecipient.EmailAddress2ResultPrompt = resultPromptRule; 
            cloneNowSubs.LastModifiedUser = "tim.cooke@solirius.com";
            cloneNowSubs.CreatedUser = "tim.cooke@solirius.com";
            cloneNowSubs.LastModifiedDate = dateToSet;
            cloneNowSubs.CreatedDate = dateToSet;
            cloneNowSubs.PublishedStatus = PublishedStatus.Published;
            cloneNowSubs.PublishedStatusDate = dateToSet;

            data.NowSubscriptions.Add(cloneNowSubs);


            //1399 - remove the fixed list as it is being replaced by reference data
            data.FixedLists.RemoveAll(x => x.UUID == resultPromptRule.ResultPrompt.FixedListUUID);

            //1399 - make the result prompt for the major creditor a name and address prompt, add a new tag to the compensation result and the costs result
            resultPromptRule.ResultPrompt.PromptType = "NAMEADDRESS";
            resultPromptRule.ResultPrompt.NameAddressType = "Organisation";
            resultPromptRule.ResultPrompt.AssociateToReferenceData = true;
            resultPromptRule.ResultPrompt.FixedList = null;
            resultPromptRule.ResultPrompt.FixedListUUID = null;
            resultPromptRule.LastModifiedUser = "tim.cooke@solirius.com";
            resultPromptRule.LastModifiedDate = dateToSet;

            var compRes = data.ResultDefinitions.First(x => x.UUID == new Guid("592cd760-ced6-443a-a298-4f5e55fd70e4"));
            compRes.PublicationTags.Add("CCT-1399");

            var costsRes = data.ResultDefinitions.First(x => x.UUID == new Guid("e46d0207-3606-44b4-8c68-c34d1522506a"));
            costsRes.PublicationTags.Add("CCT-1399");

            //write the file
            fs.WriteFile(filePath, data);
        }

        public void FixDupeNRIds(string filePath)
        {
            var data = fs.GetFileData(filePath);

            //check for dupe now requirements
            var nrs = new List<DataLib.NowRequirement>();
            var dupeNrs = new List<DataLib.NowRequirement>();
            var nowDupes = new List<DataLib.Now>();
            foreach (var nr in data.NowRequirements)
            {
                var existing = nrs.FirstOrDefault(x => x.UUID == nr.UUID);

                if (existing != null)
                {
                    //ensure that the requirement is for the same now, parent requirement, root requirement
                    if (existing.NOWUUID == nr.NOWUUID && existing.ParentNowRequirementUUID == nr.ParentNowRequirementUUID && existing.RootParentNowRequirementUUID == nr.RootParentNowRequirementUUID)
                    {
                        dupeNrs.Add(nr);
                        if (nowDupes.FirstOrDefault(x => x.UUID == nr.NOWUUID) == null)
                        {
                            var now = data.Nows.First(x => x.UUID == nr.NOWUUID);
                            nowDupes.Add(now);
                        }
                    }
                    else
                    {
                        nr.UUID = Guid.NewGuid();
                        nrs.Add(nr);
                    }                    
                }
                else
                {
                    nrs.Add(nr);
                }
            }

            data.NowRequirements = nrs;

            //write the file
            fs.WriteFile(filePath, data);

        }

        public void FixMissingNrs(string filePath)
        {
            var data = fs.GetFileData(filePath);

            SynchroniseNowRequirements(data);

            //write the file
            fs.WriteFile(filePath, data);
        }

        private void SynchroniseNowRequirements(AllData data)
        {
            var toDelete = new List<DataLib.NowRequirement>();
            foreach (var now in data.Nows.Where(x => x.DeletedDate == null))
            {
                if (now.UUID == new Guid("16bc8913-dd84-47c6-805c-6d56dd6e276e"))
                {
                    var one = 1;
                }
                if (now.NowRequirements != null)
                {
                    var allNowRequirements = new List<DataLib.NowRequirement>();
                    allNowRequirements.AddRange(now.NowRequirements);                  
                    foreach (var nr in now.NowRequirements.Where(x => x.DeletedDate == null))
                    {
                        RecursivelyFixMissingNowRequirement(data, now, nr, allNowRequirements);
                    }

                    //ensure that we dont have more now requirements in the global collection than in the now
                    var onlyInGlobal = data.NowRequirements.Where(x => x.NOWUUID == now.UUID && x.DeletedDate == null).Except(allNowRequirements.Where(x => x.DeletedDate == null)).ToList();
                    toDelete.AddRange(onlyInGlobal);
                }
            }

            //determine if there are any now requirements that dont have corresponding NOWs
            foreach (var nrGroup in data.NowRequirements.GroupBy(x => x.NOWUUID))
            {
                var now = data.Nows.FirstOrDefault(x => x.UUID == nrGroup.Key);
                if (now == null)
                {
                    toDelete.AddRange(nrGroup.ToList());
                }
            }

            //remove unwanted now requirements
            foreach (var item in toDelete)
            {
                data.NowRequirements.RemoveAll(x => x.UUID == item.UUID);
            }
        }

        private void RecursivelyFixMissingNowRequirement(AllData data, DataLib.Now now, DataLib.NowRequirement nr, List<DataLib.NowRequirement> allNowRequirements)
        {
            var guidToMatch = new Guid("74aa683d-d7cf-44cd-8824-7b9081576105");
            if (nr.UUID == guidToMatch)
            {
                var one = 1;

                var matches = data.NowRequirements.Where(x => x.UUID == guidToMatch).ToList();
            }
            var guidToMatchAoe = new Guid("27faea22-d651-4be1-8e1a-efceac0f1d14");
            if (nr.UUID == guidToMatchAoe)
            {
                var one = 1;
            }
            //remove alien children
            if (nr.NowRequirements != null)
            {
                nr.NowRequirements.RemoveAll(x => x.ParentNowRequirementUUID != null && x.ParentNowRequirementUUID != nr.UUID);
            }

            if (nr.ResultDefinition.ResultDefinitionRules != null)
            {
                foreach (var rdr in nr.ResultDefinition.ResultDefinitionRules.Where(x => x.DeletedDate == null))
                {
                    var childNr = nr.NowRequirements == null ? null : nr.NowRequirements.FirstOrDefault(x => x.ResultDefinition.UUID == rdr.ResultDefinition.UUID);
                    if (childNr == null)
                    {
                        //check the global collection
                        childNr = data.NowRequirements.FirstOrDefault(x => x.ResultDefinition.UUID == rdr.ResultDefinition.UUID && x.NOWUUID == nr.NOWUUID && x.ParentNowRequirement != null && x.ParentNowRequirement.UUID == nr.UUID);
                        if (childNr == null)
                        {
                            var rootNr = nr.RootParentNowRequirement == null ? nr : nr.RootParentNowRequirement;
                            childNr = new DataLib.NowRequirement(rdr.ResultDefinition, now, nr, rootNr);
                            childNr.CreatedDate = now.LastModifiedDate.Value;
                            childNr.CreatedUser = now.LastModifiedUser;
                            childNr.LastModifiedDate = now.LastModifiedDate;
                            childNr.LastModifiedUser = now.LastModifiedUser;
                            childNr.PublishedStatus = now.PublishedStatus;
                            childNr.PublishedStatusDate = now.PublishedStatusDate;
                            childNr.OriginalPublishedDate = now.OriginalPublishedDate;
                            data.NowRequirements.Add(childNr);
                        }
                        else
                        {
                            childNr.DeletedDate = null;
                        }

                        if (nr.NowRequirements == null)
                        {
                            nr.NowRequirements = new List<DataLib.NowRequirement>();
                        }
                        nr.NowRequirements.Add(childNr);
                    }
                    allNowRequirements.AddRange(nr.NowRequirements);
                    RecursivelyFixMissingNowRequirement(data, now, childNr, allNowRequirements);
                }
            }
        }

        public void TreeFileTo_DB(string filePath)
        {
            var data = fs.GetFileData(filePath);

            ClearAll();
            SetAllData(data);
        }

        public void DBTo_TreeFile(string filePath)
        {
            GetDataFromDB();
            var data = BuildTreeData();
            fs.WriteFile(filePath, data);
        }

        private AllData BuildTreeData()
        {
            //serialisation settings
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;

            var res = new AllData();
            BuildRDs(res);
            BuildResultPrompts(res);
            BuildNows(res);
            return res;
        }
        private void BuildNows(AllData data)
        {
            //build the NOWs
            data.Nows = new List<DataLib.Now>();
            foreach (var entity in entitiesNow)
            {
                var now = JsonConvert.DeserializeObject<DataLib.Now>(entity.Payload, settings);
                now.CreatedUser = entity.CreatedUser;
                now.DeletedUser = entity.DeletedUser;
                now.LastModifiedUser = entity.LastModifiedUser;
                now.Added = false;
                now.UpdatedOrDeleted = false;
                data.Nows.Add(now);
            }

            //build NOW Requirement data
            data.NowRequirements = new List<DataLib.NowRequirement>();
            foreach (var entity in entitiesNR)
            {
                var nowRequirement = JsonConvert.DeserializeObject<DataLib.NowRequirement>(entity.Payload, settings);
                nowRequirement.CreatedUser = entity.CreatedUser;
                nowRequirement.DeletedUser = entity.DeletedUser;
                nowRequirement.LastModifiedUser = entity.LastModifiedUser;
                nowRequirement.Added = false;
                nowRequirement.UpdatedOrDeleted = false;

                //ensure tha the now exists
                var matchedNow = data.Nows.FirstOrDefault(x => x.UUID == nowRequirement.NOWUUID);
                if (matchedNow == null)
                {
                    continue;
                }

                //reset the result definition reference
                var rd = data.ResultDefinitions.FirstOrDefault(x => x.UUID == nowRequirement.ResultDefinitionUUID);
                if (rd != null)
                {
                    data.NowRequirements.Add(nowRequirement);
                    nowRequirement.ResetResultDefinitionReference(rd);
                }

                //reset the parent reference and the prompt rule references for each of the prompt rules
                if (nowRequirement.NowRequirementPromptRules != null)
                {
                    foreach (var nrpr in nowRequirement.NowRequirementPromptRules)
                    {
                        nrpr.ParentNowRequirement = nowRequirement;

                        var match = data.ResultPromptRules.FirstOrDefault(x => x.UUID == nrpr.ResultPromptRuleUUID);
                        nrpr.ResultPromptRule = match;
                    }
                    nowRequirement.NowRequirementPromptRules.RemoveAll(x => x.ResultPromptRule == null);
                }

                //when a parent now requirement, assign to the NOW
                if (nowRequirement.ParentNowRequirementUUID == null)
                {
                    var now = data.Nows.FirstOrDefault(x => x.UUID == nowRequirement.NOWUUID);
                    if (now == null)
                    {
                        data.NowRequirements.Remove(nowRequirement);
                        continue;
                    }
                    if (now.NowRequirements == null) { now.NowRequirements = new List<DataLib.NowRequirement>(); }
                    now.NowRequirements.Add(nowRequirement);
                }
            }

            //rebuild the now requirement hierachy for each of the child now requirements
            foreach (var nr in data.NowRequirements.Where(x => x.ParentNowRequirementUUID != null))
            {
                var parent = data.NowRequirements.FirstOrDefault(x => x.UUID == nr.ParentNowRequirementUUID);
                nr.ParentNowRequirement = parent;
                if (parent == null)
                {
                    nr.DeletedDate = DateTime.Now;
                    continue;
                }

                nr.RootParentNowRequirement = parent.ParentNowRequirement == null ? parent : parent.RootParentNowRequirement;

                if (parent.NowRequirements == null) { parent.NowRequirements = new List<DataLib.NowRequirement>(); }
                parent.NowRequirements.Add(nr);
            }

            //build NOW Subscription data
            data.NowSubscriptions = new List<DataLib.NowSubscription>();
            foreach (var entity in entitiesSub)
            {
                var nowSubscription = JsonConvert.DeserializeObject<DataLib.NowSubscription>(entity.Payload, settings);
                nowSubscription.CreatedUser = entity.CreatedUser;
                nowSubscription.DeletedUser = entity.DeletedUser;
                nowSubscription.LastModifiedUser = entity.LastModifiedUser;
                nowSubscription.Added = false;
                nowSubscription.UpdatedOrDeleted = false;
                data.NowSubscriptions.Add(nowSubscription);

                //reset the references for any included and excluded nows
                if (nowSubscription.IsNow || nowSubscription.IsEDT)
                {
                    nowSubscription.IncludedNOWS = new List<DataLib.Now>();
                    nowSubscription.ExcludedNOWS = new List<DataLib.Now>();
                }

                if (nowSubscription.IncludedNOWIds != null && nowSubscription.IncludedNOWIds.Count > 0)
                {
                    foreach (var nowId in nowSubscription.IncludedNOWIds)
                    {
                        var now = data.Nows.FirstOrDefault(x => x.UUID == nowId);
                        if (now != null)
                        {
                            nowSubscription.IncludedNOWS.Add(now);
                        }
                    }
                }

                if (nowSubscription.ExcludedNOWIds != null && nowSubscription.ExcludedNOWIds.Count > 0)
                {
                    foreach (var nowId in nowSubscription.ExcludedNOWIds)
                    {
                        var now = data.Nows.FirstOrDefault(x => x.UUID == nowId);
                        if (now != null)
                        {
                            nowSubscription.ExcludedNOWS.Add(now);
                        }
                    }
                }

                //reset the recipient references
                if (nowSubscription.NowRecipient != null)
                {
                    nowSubscription.NowRecipient.NowSubscription = nowSubscription;

                    nowSubscription.NowRecipient.TitleResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.TitleResultPrompt, data);
                    nowSubscription.NowRecipient.FirstNameResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.FirstNameResultPrompt, data);
                    nowSubscription.NowRecipient.MiddleNameResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.MiddleNameResultPrompt, data);
                    nowSubscription.NowRecipient.LastNameResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.LastNameResultPrompt, data);
                    nowSubscription.NowRecipient.OrganisationNameResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.OrganisationNameResultPrompt, data);

                    nowSubscription.NowRecipient.Address1ResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.Address1ResultPrompt, data);
                    nowSubscription.NowRecipient.Address2ResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.Address2ResultPrompt, data);
                    nowSubscription.NowRecipient.Address3ResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.Address3ResultPrompt, data);
                    nowSubscription.NowRecipient.Address4ResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.Address4ResultPrompt, data);
                    nowSubscription.NowRecipient.Address5ResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.Address5ResultPrompt, data);
                    nowSubscription.NowRecipient.PostCodeResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.PostCodeResultPrompt, data);
                    nowSubscription.NowRecipient.EmailAddress1ResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.EmailAddress1ResultPrompt, data);
                    nowSubscription.NowRecipient.EmailAddress2ResultPrompt = ResetPromptRule(nowSubscription.NowRecipient.EmailAddress2ResultPrompt, data);
                }

                //reset the SubscriptionVocabulary references for prompt rules etc
                if (nowSubscription.SubscriptionVocabulary != null)
                {
                    if (nowSubscription.SubscriptionVocabulary.IncludedPromptRules != null)
                    {
                        var includedPromptRules = new List<DataLib.ResultPromptRule>();
                        foreach (var item in nowSubscription.SubscriptionVocabulary.IncludedPromptRules)
                        {
                            var match = ResetPromptRule(item, data);
                            if (match != null) { includedPromptRules.Add(match); }
                        }
                        nowSubscription.SubscriptionVocabulary.IncludedPromptRules = includedPromptRules;
                    }

                    if (nowSubscription.SubscriptionVocabulary.ExcludedPromptRules != null)
                    {
                        var excludedPromptRules = new List<DataLib.ResultPromptRule>();
                        foreach (var item in nowSubscription.SubscriptionVocabulary.ExcludedPromptRules)
                        {
                            var match = ResetPromptRule(item, data);
                            if (match != null) { excludedPromptRules.Add(match); }
                        }
                        nowSubscription.SubscriptionVocabulary.ExcludedPromptRules = excludedPromptRules;
                    }
                }
            }

            //rebuild the now subscription hierachy for each child now subscription
            foreach (var sub in data.NowSubscriptions.Where(x => x.ParentNowSubscriptionId != null))
            {
                sub.ParentNowSubscription = data.NowSubscriptions.FirstOrDefault(x => x.UUID == sub.ParentNowSubscriptionId);
                if (sub.ParentNowSubscription != null)
                {
                    if (sub.ParentNowSubscription.ChildNowSubscriptions == null) { sub.ParentNowSubscription.ChildNowSubscriptions = new List<DataLib.NowSubscription>(); }
                    sub.ParentNowSubscription.ChildNowSubscriptions.Add(sub);
                }
            }

            data.Nows.ForEach(x => ResetNow(x));
            data.NowRequirements.ForEach(x => ResetNowRequirement(x));
            data.NowSubscriptions.ForEach(x => ResetNowSubscription(x));
        }

        private void ResetNow(DataLib.Now now)
        {
            if (now.NowRequirements != null && !now.NowRequirements.Any())
            {
                now.NowRequirements = null;
            }
            ResetEntity(now);
        }

        private void ResetNowRequirement(DataLib.NowRequirement nr)
        {
            if (nr.NowRequirements != null && !nr.NowRequirements.Any())
            {
                nr.NowRequirements = null;
            }
            ResetEntity(nr);
        }

        private void ResetNowSubscription(DataLib.NowSubscription ns)
        {
            ns.IncludedNOWIds = null;
            ns.ExcludedNOWIds = null;
            if(ns.IncludedNOWS != null && !ns.IncludedNOWS.Any())
            {
                ns.IncludedNOWS = null;
            }
            if (ns.ExcludedNOWS != null && !ns.ExcludedNOWS.Any())
            {
                ns.ExcludedNOWS = null;
            }
            ResetEntity(ns);
        }

        private DataLib.ResultPromptRule ResetPromptRule(DataLib.ResultPromptRule promptRule, AllData data)
        {
            if (promptRule == null) { return null; }

            return data.ResultPromptRules.FirstOrDefault(x => x.ResultDefinitionUUID == promptRule.ResultDefinitionUUID && x.ResultPromptUUID == promptRule.ResultPromptUUID);
        }

        private void BuildResultPrompts(AllData data)
        {
            var distinctPrompts = new List<DataLib.ResultPrompt>();

            //build result prompt data
            foreach (var entity in entitiesPrompt)
            {
                var prompt = JsonConvert.DeserializeObject<DataLib.ResultPrompt>(entity.Payload, settings);
                prompt.CreatedUser = entity.CreatedUser;
                prompt.DeletedUser = entity.DeletedUser;
                prompt.LastModifiedUser = entity.LastModifiedUser;
                prompt.Added = false;
                prompt.UpdatedOrDeleted = false;
                distinctPrompts.Add(prompt);
            }

            //build the result prompt rules
            data.ResultPromptRules = new List<DataLib.ResultPromptRule>();
            foreach (var entity in entitiesRPR)
            {
                var rd = data.ResultDefinitions.FirstOrDefault(x => x.UUID == entity.ResultDefinitionUuid);

                if (rd == null) { continue; }

                var rpr = new DataLib.ResultPromptRule()
                {
                    CreatedUser = entity.CreatedUser,
                    DeletedUser = entity.DeletedUser,
                    LastModifiedUser = entity.LastModifiedUser,
                    ResultDefinitionUUID = entity.ResultDefinitionUuid,
                    PromptSequence = entity.PromptSequence,
                    UserGroups = entity.UserGroups == null || entity.UserGroups.Count() == 0 ? null : entity.UserGroups.Split(',').ToList(),
                    Rule = entity.Rule == null ? null : (DataLib.ResultPromptRuleType)Enum.Parse(typeof(DataLib.ResultPromptRuleType), entity.Rule, true),
                    RuleMags = entity.RuleMags == null ? null : (DataLib.ResultPromptRuleType)Enum.Parse(typeof(DataLib.ResultPromptRuleType), entity.RuleMags, true),
                    RuleCrown = entity.RuleMags == null ? null : (DataLib.ResultPromptRuleType)Enum.Parse(typeof(DataLib.ResultPromptRuleType), entity.RuleCrown, true)
                };
                rpr.Added = false;
                rpr.UpdatedOrDeleted = false;
                rpr.ResultPrompt = distinctPrompts.FirstOrDefault(x => x.UUID == entity.ResultPromptUuid);
                if (rpr.ResultPrompt != null)
                {
                    data.ResultPromptRules.Add(rpr);

                    if (rd.ResultPrompts == null) { rd.ResultPrompts = new List<DataLib.ResultPromptRule>(); }
                    rd.ResultPrompts.Add(rpr);
                }
            }

            //build the result prompt word groups
            data.ResultPromptWordGroups = new List<DataLib.ResultPromptWordGroup>();
            foreach (var entity in entitiesRPWG)
            {
                var rpwg = JsonConvert.DeserializeObject<DataLib.ResultPromptWordGroup>(entity.Payload, settings);
                rpwg.CreatedUser = entity.CreatedUser;
                rpwg.DeletedUser = entity.DeletedUser;
                rpwg.LastModifiedUser = entity.LastModifiedUser;
                rpwg.Added = false;
                rpwg.UpdatedOrDeleted = false;

                //set the result prompts that require this word
                bool promptsSet = false;
                var resultPrompts = distinctPrompts.Where(x => x.ResultPromptWordGroupsString != null && x.ResultPromptWordGroupsString.Any() && x.ResultPromptWordGroupsString.Contains(rpwg.UUID.Value.ToString()));
                foreach(var prompt in resultPrompts)
                {
                    if (prompt.ResultPromptWordGroups == null || prompt.ResultPromptWordGroups.Count == 0) { prompt.ResultPromptWordGroups = new List<DataLib.ResultPromptWordGroup>(); }
                    prompt.ResultPromptWordGroups.Add(rpwg);
                    promptsSet = true;
                }

                if (promptsSet) { data.ResultPromptWordGroups.Add(rpwg); }
            }

            //build the fixed lists
            data.FixedLists = new List<DataLib.FixedList>();
            foreach (var entity in entitiesFL)
            {
                var fl = JsonConvert.DeserializeObject<DataLib.FixedList>(entity.Payload, settings);
                fl.CreatedUser = entity.CreatedUser;
                fl.DeletedUser = entity.DeletedUser;
                fl.LastModifiedDate = entity.LastModifiedDate;
                fl.LastModifiedUser = entity.LastModifiedUser;
                fl.Added = false;
                fl.UpdatedOrDeleted = false;                

                //set the result prompts that require this word
                bool promptsSet = false;
                foreach (var prompt in distinctPrompts.Where(x => x.FixedListUUID != null && x.FixedListUUID == fl.UUID))
                {
                    promptsSet = true;
                    prompt.FixedList = fl;
                }

                if (promptsSet) { data.FixedLists.Add(fl); }
            }

            data.ResultPromptRules.ForEach(x => ResetEntity(x));
            data.ResultPromptWordGroups.ForEach(x => ResetEntity(x));
            distinctPrompts.ForEach(x => ResetRp(x));
            data.FixedLists.ForEach(x => ResetEntity(x));
        }

        private void ResetRp(DataLib.ResultPrompt rp)
        {
            rp.ResultPromptWordGroupsString = null;
            ResetEntity(rp);
        }

        private void BuildRDs(AllData data)
        {
            //build the result definition word groups
            data.ResultDefinitionWordGroups = new List<DataLib.ResultDefinitionWordGroup>();
            foreach (var entity in entitiesRDWG)
            {
                var rdwg = JsonConvert.DeserializeObject<DataLib.ResultDefinitionWordGroup>(entity.Payload, settings);
                rdwg.CreatedUser = entity.CreatedUser;
                rdwg.DeletedUser = entity.DeletedUser;
                rdwg.LastModifiedUser = entity.LastModifiedUser;
                rdwg.Added = false;
                rdwg.UpdatedOrDeleted = false;
                data.ResultDefinitionWordGroups.Add(rdwg);
            }

            //build result definition data
            data.ResultDefinitions = new List<DataLib.ResultDefinition>();
            foreach (var entity in entitiesRD)
            {
                var rd = JsonConvert.DeserializeObject<DataLib.ResultDefinition>(entity.Payload, settings);
                rd.CreatedUser = entity.CreatedUser;
                rd.DeletedUser = entity.DeletedUser;
                rd.LastModifiedUser = entity.LastModifiedUser;
                rd.Added = false;
                rd.UpdatedOrDeleted = false;
                data.ResultDefinitions.Add(rd);

                //set the word groups when required
                if(rd.ResultWordGroupsString != null && rd.ResultWordGroupsString.Any())
                {
                    rd.WordGroups = new List<DataLib.ResultWordGroup>();
                    foreach (var wordGroup in rd.ResultWordGroupsString)
                    {
                        var rwg = new DataLib.ResultWordGroup(rd);
                        foreach (var rdwgId in wordGroup)
                        {
                            if (string.IsNullOrEmpty(rdwgId)) { continue; }
                            Guid id = new Guid(rdwgId);
                            var match = data.ResultDefinitionWordGroups.First(x => x.UUID == id);
                            rwg.ResultDefinitionWordGroups.Add(match);
                        }
                        if (rwg.ResultDefinitionWordGroups.Any())
                        {
                            rd.WordGroups.Add(rwg);
                        }
                    }
                }
            }

            //build result definition rule data
            data.ResultDefinitionRules = new List<DataLib.ResultDefinitionRule>();
            foreach (var entity in entitiesRDR)
            {
                var rdr = new DataLib.ResultDefinitionRule()
                {
                    CreatedUser = entity.CreatedUser,
                    DeletedUser = entity.DeletedUser,
                    LastModifiedUser = entity.LastModifiedUser,
                    MasterUUID = entity.MasterUuid,
                    UUID = entity.Uuid,
                    ParentUUID = entity.ParentUuid,
                    Rule = entity.Rule == null ? null : (DataLib.ResultDefinitionRuleType)Enum.Parse(typeof(DataLib.ResultDefinitionRuleType), entity.Rule, true),
                    RuleMags = entity.RuleMags == null ? null : (DataLib.ResultDefinitionRuleType)Enum.Parse(typeof(DataLib.ResultDefinitionRuleType), entity.RuleMags, true),
                    RuleCrown = entity.RuleCrown == null ? null : (DataLib.ResultDefinitionRuleType)Enum.Parse(typeof(DataLib.ResultDefinitionRuleType), entity.RuleCrown, true)
                };

                rdr.Added = false;
                rdr.UpdatedOrDeleted = false;

                var rd = data.ResultDefinitions.FirstOrDefault(x => x.UUID == entity.ChildResultDefinitionUuid);
                rdr.ResultDefinition = rd;

                //add the rule to the parent result definition
                var parentRD = data.ResultDefinitions.FirstOrDefault(x => x.UUID == rdr.ParentUUID);
                if (parentRD != null)
                {
                    if (parentRD.ResultDefinitionRules == null) { parentRD.ResultDefinitionRules = new List<DataLib.ResultDefinitionRule>(); }
                    parentRD.ResultDefinitionRules.Add(rdr);
                    data.ResultDefinitionRules.Add(rdr);
                }
            }

            data.ResultDefinitions.ForEach(x => ResetRd(x));
            data.ResultDefinitionWordGroups.ForEach(x => ResetEntity(x));
            data.ResultDefinitionRules.ForEach(x => ResetEntity(x));
        }

        private void ResetRd(DataLib.ResultDefinition rd)
        {
            rd.ResultWordGroupsString = null;
            ResetEntity(rd);
        }

        private void ResetEntity(DataLib.DataAudit entity)
        {
            if (entity.UUID == entity.MasterUUID)
            {
                entity.MasterUUID = null;
                if (entity.PublishedStatus == PublishedStatus.Published)
                {
                    entity.PublishedStatus = null;
                }
            }
        }

        private void GetDataFromDB()
        {
            using (var unitOfWork = new UnitOfWork(new TimsToolContext()))
            {
                entitiesRD = unitOfWork.ResultDefinitions.GetAll().ToList();
                entitiesRDR = unitOfWork.ResultDefinitionRules.GetAll().ToList();
                entitiesRDWG = unitOfWork.ResultDefinitionWordGroups.GetAll().ToList();
                //entitiesRWG = unitOfWork.ResultWordGroups.GetAll().ToList();
                entitiesPrompt = unitOfWork.ResultPrompts.GetAll().ToList();
                entitiesRPR = unitOfWork.ResultPromptRules.GetAll().ToList();
                entitiesRPWG = unitOfWork.ResultPromptWordGroups.GetAll().ToList();
                //entitiesRPW = unitOfWork.ResultPromptWords.GetAll().ToList();
                entitiesFL = unitOfWork.FixedLists.GetAll().ToList();
                entitiesNow = unitOfWork.Nows.GetAll().ToList();
                entitiesNR = unitOfWork.NowRequirements.GetAll().ToList();
                entitiesSub = unitOfWork.NowSubscriptions.GetAll().ToList();
                entitiesComment = unitOfWork.Comments.GetAll().ToList();
            }
        }

        private void ClearAll()
        {
            using (var unitOfWork = new UnitOfWork(new TimsToolContext(180)))
            {
                unitOfWork.Comments.Clear();
                unitOfWork.FixedLists.Clear();
                unitOfWork.Nows.Clear();
                unitOfWork.NowRequirements.Clear();
                unitOfWork.NowSubscriptions.Clear();
                unitOfWork.ResultDefinitions.Clear();
                unitOfWork.ResultDefinitionRules.Clear();
                unitOfWork.ResultDefinitionWordGroups.Clear();
                unitOfWork.ResultPrompts.Clear();
                unitOfWork.ResultPromptRules.Clear();
                unitOfWork.ResultPromptWordGroups.Clear();
                unitOfWork.Complete();
            }
        }

        private void SetAllData(AllData data)
        {
            //serialisation settings
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;

            //prepare all data entities
            PrepareRDEntitiesAll(data);
            PrepareResultPromptEntitiesAll(data);
            PrepareNowEnitiesAll(data);
            PrepareCommentEntitiesAll(data);

            using (var unitOfWork = new UnitOfWork(new TimsToolContext()))
            {
                //set the entities
                unitOfWork.FixedLists.AddRange(entitiesFL);
                unitOfWork.Nows.AddRange(entitiesNow);
                unitOfWork.NowRequirements.AddRange(entitiesNR);
                unitOfWork.NowSubscriptions.AddRange(entitiesSub);
                unitOfWork.ResultDefinitions.AddRange(entitiesRD);
                unitOfWork.ResultDefinitionRules.AddRange(entitiesRDR);
                unitOfWork.ResultDefinitionWordGroups.AddRange(entitiesRDWG);
                //unitOfWork.ResultWordGroups.AddRange(entitiesRWG);
                unitOfWork.ResultPrompts.AddRange(entitiesPrompt);
                unitOfWork.ResultPromptRules.AddRange(entitiesRPR);
                unitOfWork.ResultPromptWordGroups.AddRange(entitiesRPWG);

                try
                {
                    // Attempt to save changes to the database
                    unitOfWork.Complete();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
                {
                    //foreach (var entry in ex.Entries)
                    //{
                    //    if (entry.Entity is Person)
                    //    {
                    //        var proposedValues = entry.CurrentValues;
                    //        var databaseValues = entry.GetDatabaseValues();

                    //        foreach (var property in proposedValues.Properties)
                    //        {
                    //            var proposedValue = proposedValues[property];
                    //            var databaseValue = databaseValues[property];

                    //            // TODO: decide which value should be written to database
                    //            // proposedValues[property] = <value to be saved>;
                    //        }

                    //        // Refresh original values to bypass next concurrency check
                    //        entry.OriginalValues.SetValues(databaseValues);
                    //    }
                    //    else
                    //    {
                    //        throw new NotSupportedException(
                    //            "Don't know how to handle concurrency conflicts for "
                    //            + entry.Metadata.Name);
                    //    }
                    //}
                }
            }
        }

        private void SetData(AllData data)
        {
            //serialisation settings
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;

            //prepare a change set
            var flUpdated = data.FixedLists.Where(x => x.UpdatedOrDeleted && !x.Added);
            var nowUpdated = data.Nows.Where(x => x.UpdatedOrDeleted && !x.Added);
            var nowRequirementUpdated = data.NowRequirements.Where(x => x.UpdatedOrDeleted && !x.Added);
            var nowSubscriptionUpdated = data.NowSubscriptions.Where(x => x.UpdatedOrDeleted && !x.Added);
            var rdUpdated = data.ResultDefinitions.Where(x => x.UpdatedOrDeleted && !x.Added);
            var rdRuleUpdated = data.ResultDefinitionRules.Where(x => x.UpdatedOrDeleted && !x.Added);
            var rdWGUpdated = data.ResultDefinitionWordGroups.Where(x => x.UpdatedOrDeleted && !x.Added);
            var distinctPrompts = data.ResultPromptRules.GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt);
            var rpUpdated = distinctPrompts.Where(x => x.UpdatedOrDeleted && !x.Added);
            var rprUpdated = data.ResultPromptRules.Where(x => x.UpdatedOrDeleted && !x.Added);
            var commentUpdated = data.Comments.Where(x => x.UpdatedOrDeleted && !x.Added);

            //prepare added entities
            PrepareRDEntitiesAdded(data);
            PrepareResultPromptEntitiesAdded(data);
            PrepareNowEnitiesAdded(data);
            PrepareCommentEntitiesAdded(data);

            using (var unitOfWork = new UnitOfWork(new TimsToolContext()))
            {
                //set the entities
                SetComments(unitOfWork, entitiesComment, commentUpdated);
                SetFixedLists(unitOfWork, entitiesFL, flUpdated);
                SetNows(unitOfWork, entitiesNow, nowUpdated);
                SetNowRequirements(unitOfWork, entitiesNR, nowRequirementUpdated);
                SetNowSubscriptions(unitOfWork, entitiesSub, nowSubscriptionUpdated);
                SetResultDefinitions(unitOfWork, entitiesRD, rdUpdated);
                SetResultDefinitionRules(unitOfWork, entitiesRDR, rdRuleUpdated);
                SetResultDefinitionWordGroups(unitOfWork, entitiesRDWG, rdWGUpdated);
                SetResultPrompts(unitOfWork, entitiesPrompt, rpUpdated);
                SetResultPromptRules(unitOfWork, entitiesRPR, rprUpdated);

                try
                {
                    // Attempt to save changes to the database
                    unitOfWork.Complete();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
                {
                    //foreach (var entry in ex.Entries)
                    //{
                    //    if (entry.Entity is Person)
                    //    {
                    //        var proposedValues = entry.CurrentValues;
                    //        var databaseValues = entry.GetDatabaseValues();

                    //        foreach (var property in proposedValues.Properties)
                    //        {
                    //            var proposedValue = proposedValues[property];
                    //            var databaseValue = databaseValues[property];

                    //            // TODO: decide which value should be written to database
                    //            // proposedValues[property] = <value to be saved>;
                    //        }

                    //        // Refresh original values to bypass next concurrency check
                    //        entry.OriginalValues.SetValues(databaseValues);
                    //    }
                    //    else
                    //    {
                    //        throw new NotSupportedException(
                    //            "Don't know how to handle concurrency conflicts for "
                    //            + entry.Metadata.Name);
                    //    }
                    //}
                }

                //reset the modified flags for updated items
                flUpdated.ToList().ForEach(x => x.ResetTracking());
                nowUpdated.ToList().ForEach(x => x.ResetTracking());
                nowRequirementUpdated.ToList().ForEach(x => x.ResetTracking());
                nowSubscriptionUpdated.ToList().ForEach(x => x.ResetTracking());
                rdUpdated.ToList().ForEach(x => x.ResetTracking());
                rdRuleUpdated.ToList().ForEach(x => x.ResetTracking());
                rdWGUpdated.ToList().ForEach(x => x.ResetTracking());
                rpUpdated.ToList().ForEach(x => x.ResetTracking());
                rprUpdated.ToList().ForEach(x => x.ResetTracking());
            }
        }

        private void PrepareNowEnitiesAdded(AllData data)
        {
            //prepare NOW data
            foreach (var now in data.Nows.Where(x => x.Added))
            {
                AddNow(now);
            }

            //prepare NOW Requirement
            foreach (var nr in data.NowRequirements.Where(x => x.Added))
            {
                AddNowRequirement(nr);
            }

            //prepare NOW Subscription data
            foreach (var sub in data.NowSubscriptions.Where(x => x.Added))
            {
                AddNowSubscription(sub);
            }
        }

        private void PrepareCommentEntitiesAdded(AllData data)
        {
            //prepare Comment data
            foreach (var comment in data.Comments.Where(x => x.Added))
            {
                AddComment(comment);
            }
        }

        private void PrepareNowEnitiesAll(AllData data)
        {
            //prepare NOW data
            foreach (var now in data.Nows)
            {
                AddNow(now);
            }

            //prepare NOW Requirement
            foreach (var nr in data.NowRequirements)
            {
                AddNowRequirement(nr);
            }

            //prepare NOW Subscription data
            foreach (var sub in data.NowSubscriptions)
            {
                AddNowSubscription(sub);
            }
        }

        private void PrepareCommentEntitiesAll(AllData data)
        {
            //prepare Comment data
            foreach (var comment in data.Comments)
            {
                AddComment(comment);
            }
        }

        private void AddNowSubscription(DataLib.NowSubscription? sub)
        {
            entitiesSub.Add(new DataAccessLayer.Models.NowSubscription()
            {
                CreatedDate = sub.CreatedDate,
                CreatedUser = sub.CreatedUser,
                DeletedDate = sub.DeletedDate,
                DeletedUser = sub.DeletedUser,
                LastModifiedDate = sub.LastModifiedDate ?? sub.CreatedDate,
                LastModifiedUser = sub.LastModifiedUser ?? sub.CreatedUser,
                Uuid = sub.UUID.Value,
                MasterUuid = sub.MasterUUID ?? sub.UUID.Value,
                PublishedStatus = sub.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), sub.PublishedStatus),
                PublishedStatusDate = sub.PublishedStatusDate,
                PublicationTags = (sub.PublicationTags == null || !sub.PublicationTags.Any()) ? null : string.Join(",", sub.PublicationTags),
                Payload = JsonConvert.SerializeObject(sub, settings)
            });
        }

        private void AddNowRequirement(DataLib.NowRequirement? nr)
        {
            entitiesNR.Add(new DataAccessLayer.Models.NowRequirement()
            {
                CreatedDate = nr.CreatedDate,
                CreatedUser = nr.CreatedUser,
                DeletedDate = nr.DeletedDate,
                DeletedUser = nr.DeletedUser,
                LastModifiedDate = nr.LastModifiedDate ?? nr.CreatedDate,
                LastModifiedUser = nr.LastModifiedUser ?? nr.CreatedUser,
                Uuid = nr.UUID.Value,
                MasterUuid = nr.MasterUUID ?? nr.UUID.Value,
                PublishedStatus = nr.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), nr.PublishedStatus),
                PublishedStatusDate = nr.PublishedStatusDate,
                PublicationTags = (nr.PublicationTags == null || !nr.PublicationTags.Any()) ? null : string.Join(",", nr.PublicationTags),
                ResultDefinitionUuid = nr.ResultDefinition.UUID.Value,
                Nowuuid = nr.NOWUUID.Value,
                ParentNowRequirementUuid = nr.ParentNowRequirementUUID,
                RootParentNowRequirementUuid = nr.RootParentNowRequirementUUID,
                Payload = JsonConvert.SerializeObject(nr, settings)
            });
        }

        private void AddNow(DataLib.Now? now)
        {
            entitiesNow.Add(new DataAccessLayer.Models.Now()
            {
                CreatedDate = now.CreatedDate,
                CreatedUser = now.CreatedUser,
                DeletedDate = now.DeletedDate,
                DeletedUser = now.DeletedUser,
                LastModifiedDate = now.LastModifiedDate ?? now.CreatedDate,
                LastModifiedUser = now.LastModifiedUser ?? now.CreatedUser,
                Uuid = now.UUID.Value,
                MasterUuid = now.MasterUUID ?? now.UUID.Value,
                PublishedStatus = now.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), now.PublishedStatus),
                PublishedStatusDate = now.PublishedStatusDate,
                PublicationTags = (now.PublicationTags == null || !now.PublicationTags.Any()) ? null : string.Join(",", now.PublicationTags),
                Payload = JsonConvert.SerializeObject(now, settings)
            });
        }

        private void AddComment(DataLib.Comment? comment)
        {
            entitiesComment.Add(new DataAccessLayer.Models.Comment()
            {
                CreatedDate = comment.CreatedDate,
                CreatedUser = comment.CreatedUser,
                DeletedDate = comment.DeletedDate,
                DeletedUser = comment.DeletedUser,
                LastModifiedDate = comment.LastModifiedDate ?? comment.CreatedDate,
                LastModifiedUser = comment.LastModifiedUser ?? comment.CreatedUser,
                Uuid = comment.UUID.Value,
                MasterUuid = comment.MasterUUID ?? comment.UUID.Value,
                PublishedStatus = comment.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), comment.PublishedStatus),
                PublishedStatusDate = comment.PublishedStatusDate,
                PublicationTags = (comment.PublicationTags == null || !comment.PublicationTags.Any()) ? null : string.Join(",", comment.PublicationTags),
                ParentMasterUuid = comment.ParentMasterUUID.Value,
                ParentUuid = comment.ParentUUID.Value,
                Note = comment.Note,
            });
        }
        

        private void PrepareRDEntitiesAll(AllData data)
        {
            //prepare result defintion data
            foreach (var rd in data.ResultDefinitions)
            {
                AddResultDefinition(rd);
            }

            //prepare the result definition rules
            foreach (var rdr in data.ResultDefinitionRules)
            {
                var matchedParentRD = data.ResultDefinitions.FirstOrDefault(x => x.UUID == rdr.ParentUUID);
                if (matchedParentRD == null) { continue; }

                var matchedChild = data.ResultDefinitions.FirstOrDefault(x => x.UUID == rdr.ChildResultDefinitionUUID);
                if (matchedChild == null) { continue; }

                AddResultDefinitionRule(rdr);
            }

            //prepare the result definition word groups
            foreach (var rdwg in data.ResultDefinitionWordGroups)
            {
                AddResultDefinitionWordGroups(rdwg);
            }
        }

        private void PrepareRDEntitiesAdded(AllData data)
        {
            //prepare result defintion data
            foreach (var rd in data.ResultDefinitions.Where(x => x.Added))
            {
                AddResultDefinition(rd);
            }

            //prepare the result definition rules
            foreach (var rdr in data.ResultDefinitionRules.Where(x => x.Added))
            {
                var matchedParentRD = data.ResultDefinitions.FirstOrDefault(x => x.UUID == rdr.ParentUUID);
                if (matchedParentRD == null) { continue; }

                var matchedChild = data.ResultDefinitions.FirstOrDefault(x => x.UUID == rdr.ChildResultDefinitionUUID);
                if (matchedChild == null) { continue; }

                AddResultDefinitionRule(rdr);
            }

            //prepare the result definition word groups
            foreach (var rdwg in data.ResultDefinitionWordGroups.Where(x => x.Added))
            {
                AddResultDefinitionWordGroups(rdwg);
            }

            //prepare the word groups
            foreach (var rdwg in data.ResultDefinitionWordGroups.Where(x => x.Added))
            {
                AddResultDefinitionWordGroups(rdwg);
            }
        }

        private void AddResultDefinitionWordGroups(DataLib.ResultDefinitionWordGroup? rdwg)
        {
            entitiesRDWG.Add(new DataAccessLayer.Models.ResultDefinitionWordGroup()
            {
                CreatedDate = rdwg.CreatedDate,
                CreatedUser = rdwg.CreatedUser,
                DeletedDate = rdwg.DeletedDate,
                DeletedUser = rdwg.DeletedUser,
                LastModifiedDate = rdwg.LastModifiedDate ?? rdwg.CreatedDate,
                LastModifiedUser = rdwg.LastModifiedUser ?? rdwg.CreatedUser,
                Uuid = rdwg.UUID.Value,
                MasterUuid = rdwg.MasterUUID ?? rdwg.UUID.Value,
                PublishedStatus = rdwg.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), rdwg.PublishedStatus),
                PublishedStatusDate = rdwg.PublishedStatusDate,
                Payload = JsonConvert.SerializeObject(rdwg, settings)
            });
        }

        private void AddResultDefinitionRule(DataLib.ResultDefinitionRule? rdr)
        {
            entitiesRDR.Add(new DataAccessLayer.Models.ResultDefinitionRule()
            {
                CreatedDate = rdr.CreatedDate,
                CreatedUser = rdr.CreatedUser,
                DeletedDate = rdr.DeletedDate,
                DeletedUser = rdr.DeletedUser,
                LastModifiedDate = rdr.LastModifiedDate ?? rdr.CreatedDate,
                LastModifiedUser = rdr.LastModifiedUser ?? rdr.CreatedUser,
                Uuid = rdr.UUID.Value,
                MasterUuid = rdr.MasterUUID ?? rdr.UUID.Value,
                PublishedStatus = rdr.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), rdr.PublishedStatus),
                PublishedStatusDate = rdr.PublishedStatusDate,
                PublicationTags = (rdr.PublicationTags == null || !rdr.PublicationTags.Any()) ? null : string.Join(",", rdr.PublicationTags),
                ChildResultDefinitionUuid = rdr.ChildResultDefinitionUUID.Value,
                ParentUuid = rdr.ParentUUID.Value,
                Rule = rdr.Rule == null ? null : Enum.GetName(typeof(DataLib.ResultDefinitionRuleType), rdr.Rule.Value),
                RuleMags = rdr.RuleMags == null ? null : Enum.GetName(typeof(DataLib.ResultDefinitionRuleType), rdr.RuleMags.Value),
                RuleCrown = rdr.RuleCrown == null ? null : Enum.GetName(typeof(DataLib.ResultDefinitionRuleType), rdr.RuleCrown.Value)
            });
        }

        private void AddResultDefinition(DataLib.ResultDefinition? rd)
        {
            string rwgString = null;

            if (rd.ResultWordGroupsString != null)
            {
                var rwgList = new List<string>();
                foreach (var rwg in rd.ResultWordGroupsString)
                {
                    rwgList.Add(string.Join(",", rwg));
                }
                rwgString = string.Join(";", rwgList);
            }
            
            //if (rd.WordGroups != null)
            //{
            //    foreach(var wg in rd.WordGroups)
            //    {
            //        entitiesRWG.Add(new DataAccessLayer.Models.ResultWordGroup()
            //            {
            //                ResultDefinitionUuid = rd.UUID.Value,
            //                WordGroupName = wg.WordGroupName
            //            }
            //        );
            //    }
            //}

            entitiesRD.Add(new DataAccessLayer.Models.ResultDefinition()
            {
                CreatedDate = rd.CreatedDate,
                CreatedUser = rd.CreatedUser,
                DeletedDate = rd.DeletedDate,
                DeletedUser = rd.DeletedUser,
                LastModifiedDate = rd.LastModifiedDate ?? rd.CreatedDate,
                LastModifiedUser = rd.LastModifiedUser ?? rd.CreatedUser,
                Uuid = rd.UUID.Value,
                ResultDefinitionWordGroupIds = rwgString,
                MasterUuid = rd.MasterUUID ?? rd.UUID.Value,
                PublishedStatus = rd.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), rd.PublishedStatus),
                PublishedStatusDate = rd.PublishedStatusDate,
                PublicationTags = (rd.PublicationTags == null || !rd.PublicationTags.Any()) ? null : string.Join(",", rd.PublicationTags),
                Payload = JsonConvert.SerializeObject(rd, settings)
            });
        }

        private void PrepareResultPromptEntitiesAdded(AllData data)
        {
            var distinctPrompts = data.ResultPromptRules.Where(x => x.ResultPrompt.Added).GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt);

            //prepare result prompt data
            foreach (var prompt in distinctPrompts)
            {
                AddResultPrompt(prompt);
            }

            //prepare the result prompt rules
            foreach (var rpr in data.ResultPromptRules.Where(x => x.Added))
            {
                var rd = data.ResultDefinitions.FirstOrDefault(x => x.UUID == rpr.ResultDefinitionUUID);
                if (rd == null) { continue; }

                AddResultPromptRule(rpr);
            }

            //prepare the result prompt word groups 
            foreach (var rpwg in data.ResultPromptWordGroups.Where(x => x.Added))
            {
                AddResultPromptWordGroup(rpwg);
            }

            //prepare the fixed lists
            foreach (var fl in data.FixedLists.Where(x => x.Added))
            {
                AddFixedList(fl);
            }
        }

        private void PrepareResultPromptEntitiesAll(AllData data)
        {
            var distinctPrompts = data.ResultPromptRules.GroupBy(x => x.ResultPromptUUID).Select(x => x.First()).Select(x => x.ResultPrompt);

            //prepare result prompt data
            foreach (var prompt in distinctPrompts)
            {
                AddResultPrompt(prompt);
            }

            //prepare the result prompt rules
            foreach (var rpr in data.ResultPromptRules)
            {
                var rd = data.ResultDefinitions.FirstOrDefault(x => x.UUID == rpr.ResultDefinitionUUID);
                if (rd == null) { continue; }

                AddResultPromptRule(rpr);
            }

            //prepare the result prompt word groups 
            foreach (var rpwg in data.ResultPromptWordGroups)
            {
                AddResultPromptWordGroup(rpwg);
            }

            //prepare the fixed lists
            foreach (var fl in data.FixedLists)
            {
                AddFixedList(fl);
            }
        }

        private void AddFixedList(DataLib.FixedList? fl)
        {
            entitiesFL.Add(new DataAccessLayer.Models.FixedList()
            {
                CreatedDate = fl.CreatedDate,
                CreatedUser = fl.CreatedUser,
                DeletedDate = fl.DeletedDate,
                DeletedUser = fl.DeletedUser,
                LastModifiedDate = fl.LastModifiedDate ?? fl.CreatedDate,
                LastModifiedUser = fl.LastModifiedUser ?? fl.CreatedUser,
                Uuid = fl.UUID.Value,
                MasterUuid = fl.MasterUUID ?? fl.UUID.Value,
                PublishedStatus = fl.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), fl.PublishedStatus),
                PublishedStatusDate = fl.PublishedStatusDate,
                PublicationTags = (fl.PublicationTags == null || !fl.PublicationTags.Any()) ? null : string.Join(",", fl.PublicationTags),
                Payload = JsonConvert.SerializeObject(fl, settings)
            });
        }

        private void AddResultPromptWordGroup(DataLib.ResultPromptWordGroup? rpwg)
        {
            entitiesRPWG.Add(new DataAccessLayer.Models.ResultPromptWordGroup()
            {
                CreatedDate = rpwg.CreatedDate,
                CreatedUser = rpwg.CreatedUser,
                DeletedDate = rpwg.DeletedDate,
                DeletedUser = rpwg.DeletedUser,
                LastModifiedDate = rpwg.LastModifiedDate ?? rpwg.CreatedDate,
                LastModifiedUser = rpwg.LastModifiedUser ?? rpwg.CreatedUser,
                Uuid = rpwg.UUID.Value,
                MasterUuid = rpwg.MasterUUID ?? rpwg.UUID.Value,
                PublishedStatus = rpwg.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), rpwg.PublishedStatus),
                PublishedStatusDate = rpwg.PublishedStatusDate,
                PublicationTags = (rpwg.PublicationTags == null || !rpwg.PublicationTags.Any()) ? null : string.Join(",", rpwg.PublicationTags),
                Payload = JsonConvert.SerializeObject(rpwg, settings)
            });
        }

        private void AddResultPromptRule(DataLib.ResultPromptRule? rpr)
        {
            entitiesRPR.Add(new DataAccessLayer.Models.ResultPromptRule()
            {
                CreatedDate = rpr.CreatedDate,
                CreatedUser = rpr.CreatedUser,
                DeletedDate = rpr.DeletedDate,
                DeletedUser = rpr.DeletedUser,
                LastModifiedDate = rpr.LastModifiedDate ?? rpr.CreatedDate,
                LastModifiedUser = rpr.LastModifiedUser ?? rpr.CreatedUser,
                PromptSequence = rpr.PromptSequence == null ? 0 : rpr.PromptSequence.Value,
                ResultDefinitionUuid = rpr.ResultDefinitionUUID.Value,
                ResultPromptUuid = rpr.ResultPromptUUID.Value,
                Uuid = rpr.UUID.Value,
                MasterUuid = rpr.MasterUUID ?? rpr.UUID.Value,
                PublishedStatus = rpr.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), rpr.PublishedStatus),
                PublishedStatusDate = rpr.PublishedStatusDate,
                PublicationTags = (rpr.PublicationTags == null || !rpr.PublicationTags.Any()) ? null : string.Join(",", rpr.PublicationTags),
                UserGroups = rpr.UserGroups == null || rpr.UserGroups.Count == 0 ? null : string.Join(",", rpr.UserGroups),
                Rule = rpr.Rule == null ? null : Enum.GetName(typeof(DataLib.ResultPromptRuleType), rpr.Rule.Value),
                RuleMags = rpr.RuleMags == null ? null : Enum.GetName(typeof(DataLib.ResultPromptRuleType), rpr.RuleMags.Value),
                RuleCrown = rpr.RuleCrown == null ? null : Enum.GetName(typeof(DataLib.ResultPromptRuleType), rpr.RuleCrown.Value)
            });
        }

        private void AddResultPrompt(DataLib.ResultPrompt? prompt)
        {
            entitiesPrompt.Add(new DataAccessLayer.Models.ResultPrompt()
            {
                CreatedDate = prompt.CreatedDate,
                CreatedUser = prompt.CreatedUser,
                DeletedDate = prompt.DeletedDate,
                DeletedUser = prompt.DeletedUser,
                LastModifiedDate = prompt.LastModifiedDate ?? prompt.CreatedDate,
                LastModifiedUser = prompt.LastModifiedUser ?? prompt.CreatedUser,
                Uuid = prompt.UUID.Value,
                ResultPromptWordGroupIds = prompt.ResultPromptWordGroupsString == null ? null : string.Join(",", prompt.ResultPromptWordGroupsString),
                MasterUuid = prompt.MasterUUID ?? prompt.UUID.Value,
                PublishedStatus = prompt.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), prompt.PublishedStatus),
                PublishedStatusDate = prompt.PublishedStatusDate,
                PublicationTags = (prompt.PublicationTags == null || !prompt.PublicationTags.Any()) ? null : string.Join(",", prompt.PublicationTags),
                Payload = JsonConvert.SerializeObject(prompt, settings)
            });
        }

        private void SetComments(UnitOfWork unitOfWork, IEnumerable<DataAccessLayer.Models.Comment> added, IEnumerable<DataLib.Comment> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.Comments
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.ParentUuid = updatedItem.ParentUUID.Value;
                entity.ParentMasterUuid = updatedItem.ParentMasterUUID.Value;
                entity.Note = updatedItem.Note;
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublicationTags = (updatedItem.PublicationTags == null || !updatedItem.PublicationTags.Any()) ? null : string.Join(",", updatedItem.PublicationTags);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
            }
            unitOfWork.Comments.AddRange(added);
        }
        private void SetFixedLists(UnitOfWork unitOfWork, IEnumerable<DataAccessLayer.Models.FixedList> added, IEnumerable<DataLib.FixedList> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.FixedLists
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
                entity.Payload = JsonConvert.SerializeObject(updatedItem, settings);
            }
            unitOfWork.FixedLists.AddRange(added);
        }

        private void SetNows(UnitOfWork unitOfWork, IEnumerable<DataAccessLayer.Models.Now> added, IEnumerable<DataLib.Now> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.Nows
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
                entity.Payload = JsonConvert.SerializeObject(updatedItem, settings);
            }
            unitOfWork.Nows.AddRange(added);
        }
        private void SetNowRequirements(UnitOfWork unitOfWork, IEnumerable<DataAccessLayer.Models.NowRequirement> added, IEnumerable<DataLib.NowRequirement> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.NowRequirements
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
                entity.Payload = JsonConvert.SerializeObject(updatedItem, settings);
            }
            unitOfWork.NowRequirements.AddRange(added);
        }
        private void SetNowSubscriptions(UnitOfWork unitOfWork, IEnumerable<DataAccessLayer.Models.NowSubscription> added, IEnumerable<DataLib.NowSubscription> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.NowSubscriptions
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
                entity.Payload = JsonConvert.SerializeObject(updatedItem, settings);
            }
            unitOfWork.NowSubscriptions.AddRange(added);
        }
        private void SetResultDefinitions(UnitOfWork unitOfWork, IEnumerable<DataAccessLayer.Models.ResultDefinition> added, IEnumerable<DataLib.ResultDefinition> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.ResultDefinitions
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.ResultDefinitionWordGroupIds = null;
                if (updatedItem.ResultWordGroupsString != null)
                {
                    var rwgList = new List<string>();
                    foreach (var rwg in updatedItem.ResultWordGroupsString)
                    {
                        rwgList.Add(string.Join(",", rwg));
                    }
                    entity.ResultDefinitionWordGroupIds = string.Join(";", rwgList);
                }
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
                entity.PublicationTags = (updatedItem.PublicationTags == null || !updatedItem.PublicationTags.Any()) ? null : string.Join(",", updatedItem.PublicationTags);
                entity.Payload = JsonConvert.SerializeObject(updatedItem, settings);
            }
            unitOfWork.ResultDefinitions.AddRange(added);
        }

        private void SetResultDefinitionRules(UnitOfWork unitOfWork, IEnumerable<DataAccessLayer.Models.ResultDefinitionRule> added, IEnumerable<DataLib.ResultDefinitionRule> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.ResultDefinitionRules
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.ParentUuid = updatedItem.ParentUUID.Value;
                entity.ChildResultDefinitionUuid = updatedItem.ChildResultDefinitionUUID.Value;
                entity.Rule = updatedItem.Rule == null ? null : Enum.GetName(typeof(ResultDefinitionRuleType), updatedItem.Rule);
                entity.RuleMags = updatedItem.RuleMags == null ? null : Enum.GetName(typeof(ResultDefinitionRuleType), updatedItem.RuleMags);
                entity.RuleCrown = updatedItem.RuleCrown == null ? null : Enum.GetName(typeof(ResultDefinitionRuleType), updatedItem.RuleCrown);
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublicationTags = (updatedItem.PublicationTags == null || !updatedItem.PublicationTags.Any()) ? null : string.Join(",", updatedItem.PublicationTags);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
            }
            unitOfWork.ResultDefinitionRules.AddRange(added);
        }

        private void SetResultDefinitionWordGroups(UnitOfWork unitOfWork, List<DataAccessLayer.Models.ResultDefinitionWordGroup> added, IEnumerable<DataLib.ResultDefinitionWordGroup> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.ResultDefinitionWordGroups
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
                entity.PublicationTags = (updatedItem.PublicationTags == null || !updatedItem.PublicationTags.Any()) ? null : string.Join(",", updatedItem.PublicationTags);
                entity.Payload = JsonConvert.SerializeObject(updatedItem, settings);
            }
            unitOfWork.ResultDefinitionWordGroups.AddRange(added);
        }

        private void SetResultPrompts(UnitOfWork unitOfWork, List<DataAccessLayer.Models.ResultPrompt> added, IEnumerable<DataLib.ResultPrompt> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.ResultPrompts
                                .Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.ResultPromptWordGroupIds = updatedItem.ResultPromptWordGroupsString == null ? null : string.Join(",", updatedItem.ResultPromptWordGroupsString);
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
                entity.PublicationTags = (updatedItem.PublicationTags == null || !updatedItem.PublicationTags.Any()) ? null : string.Join(",", updatedItem.PublicationTags);
                entity.Payload = JsonConvert.SerializeObject(updatedItem, settings);
            }
            unitOfWork.ResultPrompts.AddRange(added);
        }

        private void SetResultPromptRules(UnitOfWork unitOfWork, List<DataAccessLayer.Models.ResultPromptRule> added, IEnumerable<DataLib.ResultPromptRule> updated)
        {
            //retrieve the items requiring update
            var entities = unitOfWork.ResultPromptRules.Find(x => updated.Select(y => y.UUID).Contains(x.Uuid)).ToList();

            //update the state for updated and added items
            foreach (var entity in entities)
            {
                var updatedItem = updated.FirstOrDefault(x => x.UUID == entity.Uuid);
                entity.PromptSequence = updatedItem.PromptSequence == null ? 0 : updatedItem.PromptSequence.Value;
                entity.UserGroups = updatedItem.UserGroups == null || updatedItem.UserGroups.Count == 0 ? null : string.Join(",", updatedItem.UserGroups);
                entity.Rule = updatedItem.Rule == null ? null : Enum.GetName(typeof(DataLib.ResultPromptRuleType), updatedItem.Rule.Value);
                entity.RuleMags = updatedItem.RuleMags == null ? null : Enum.GetName(typeof(DataLib.ResultPromptRuleType), updatedItem.RuleMags.Value);
                entity.RuleCrown = updatedItem.RuleCrown == null ? null : Enum.GetName(typeof(DataLib.ResultPromptRuleType), updatedItem.RuleCrown.Value);
                entity.DeletedDate = updatedItem.DeletedDate;
                entity.DeletedUser = updatedItem.DeletedUser;
                entity.LastModifiedDate = updatedItem.LastModifiedDate ?? updatedItem.CreatedDate;
                entity.LastModifiedUser = updatedItem.LastModifiedUser ?? updatedItem.CreatedUser;
                entity.PublishedStatus = updatedItem.PublishedStatus == null ? null : Enum.GetName(typeof(PublishedStatus), updatedItem.PublishedStatus);
                entity.PublishedStatusDate = updatedItem.PublishedStatusDate;
                entity.PublicationTags = (updatedItem.PublicationTags == null || !updatedItem.PublicationTags.Any()) ? null : string.Join(",", updatedItem.PublicationTags);
            }
            unitOfWork.ResultPromptRules.AddRange(added);
        }  
    }
}
