using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLib
{
    [Serializable]
    public sealed class SubscriptionVocabularyJSON
    {
        private SubscriptionVocabulary subscriptionVocabulary;
        private List<string> featureToggles;
        public SubscriptionVocabularyJSON(SubscriptionVocabulary subscriptionVocabulary, List<string> featureToggles) 
        { 
            this.subscriptionVocabulary = subscriptionVocabulary;
            this.featureToggles = featureToggles;
        }

        public bool appearedByVideoLink { get => subscriptionVocabulary.AppearedByVideoLink; }

        public bool appearedInPerson { get => subscriptionVocabulary.AppearedInPerson; }

        public bool anyAppearance { get => subscriptionVocabulary.AnyAppearance; }

        public bool inCustody { get => subscriptionVocabulary.InCustody; }

        public bool custodyLocationIsPrison { get => subscriptionVocabulary.CustodyLocationIsPrison; }

        public bool custodyLocationIsPolice { get => subscriptionVocabulary.CustodyLocationIsPolice; }

        public bool ignoreCustody { get => subscriptionVocabulary.IgnoreCustody; }



        public bool? prosecutorMajorCreditor 
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1399")) != null)
                {
                    //only applicable to CCT-1399
                    return subscriptionVocabulary.ProsecutorMajorCreditor;
                }
                return null;
            }
        }

        public bool? nonProsecutorMajorCreditor
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1399")) != null)
                {
                    //only applicable to CCT-1399
                    return subscriptionVocabulary.NonProsecutorMajorCreditor;
                }
                return null;
            }
        }

        public bool? anyMajorCreditor
        {
            get
            {
                if (featureToggles != null && featureToggles.FirstOrDefault(x => x.ToUpperInvariant().Contains("CCT-1399")) != null)
                {
                    //only applicable to CCT-1399
                    return subscriptionVocabulary.AnyMajorCreditor;
                }
                return null;
            }
        }

        public bool allNonCustodialResults { get => subscriptionVocabulary.AllNonCustodialResults; }

        public bool atleastOneNonCustodialResult { get => subscriptionVocabulary.AtleastOneNonCustodialResult; }

        public bool atleastOneCustodialResult { get => subscriptionVocabulary.AtleastOneCustodialResult; }

        public bool ignoreResults { get => subscriptionVocabulary.IgnoreResults; }

        public bool youthDefendant { get => subscriptionVocabulary.YouthDefendant; }

        public bool adultDefendant { get => subscriptionVocabulary.AdultDefendant; }

        public bool adultOrYouthDefendant { get => subscriptionVocabulary.AdultOrYouthDefendant; }

        public bool welshCourtHearing { get => subscriptionVocabulary.WelshCourtHearing; }

        public bool englishCourtHearing { get => subscriptionVocabulary.EnglishCourtHearing; }

        public bool anyCourtHearing { get => subscriptionVocabulary.AnyCourtHearing; }

        public bool? isCpsProsecuted { get => subscriptionVocabulary.IsProsecutedByCPS; }

        public List<Guid?> includedResults 
        {
            get
            {
                if (subscriptionVocabulary.IncludedResults == null || !subscriptionVocabulary.IncludedResults.Where(x=>x.DeletedDate == null).Any())
                {
                    return null;
                }

                return subscriptionVocabulary.IncludedResults.Where(x => x.DeletedDate == null).Select(x => x.MasterUUID ?? x.UUID).ToList();
            }
        }

        public List<Guid?> excludedResults
        {
            get
            {
                if (subscriptionVocabulary.ExcludedResults == null || !subscriptionVocabulary.ExcludedResults.Where(x => x.DeletedDate == null).Any())
                {
                    return null;
                }

                return subscriptionVocabulary.ExcludedResults.Where(x => x.DeletedDate == null).Select(x => x.MasterUUID ?? x.UUID).ToList();
            }
        }

        public List<SubscriptionVocabularyPrompt> includedPrompts
        {
            get
            {
                if (subscriptionVocabulary.IncludedPromptRules == null || !subscriptionVocabulary.IncludedPromptRules.Where(x=>x.DeletedDate == null).Any())
                { 
                    return null; 
                }
                return subscriptionVocabulary.IncludedPromptRules.Where(x => x.DeletedDate == null).Select(x => new SubscriptionVocabularyPrompt(x)).ToList();
            }
        }

        public List<SubscriptionVocabularyPrompt> excludedPrompts
        {
            get
            {
                if (subscriptionVocabulary.ExcludedPromptRules == null || !subscriptionVocabulary.ExcludedPromptRules.Where(x => x.DeletedDate == null).Any())
                {
                    return null;
                }
                return subscriptionVocabulary.ExcludedPromptRules.Where(x => x.DeletedDate == null).Select(x => new SubscriptionVocabularyPrompt(x)).ToList();
            }
        }

        public List<Guid?> includedPromptsIds
        {
            get
            {
                if (subscriptionVocabulary.IncludedPromptRules == null || !subscriptionVocabulary.IncludedPromptRules.Where(x => x.DeletedDate == null).Any())
                {
                    return null;
                }
                return subscriptionVocabulary.IncludedPromptRules.Where(x => x.DeletedDate == null).Select(x => x.ResultPrompt.MasterUUID ?? x.ResultPrompt.UUID).ToList();
            }
        }

        public List<Guid?> excludedPromptsIds
        {
            get
            {
                if (subscriptionVocabulary.ExcludedPromptRules == null || !subscriptionVocabulary.ExcludedPromptRules.Where(x => x.DeletedDate == null).Any())
                {
                    return null;
                }
                return subscriptionVocabulary.ExcludedPromptRules.Where(x => x.DeletedDate == null).Select(x => x.ResultPrompt.MasterUUID ?? x.ResultPrompt.UUID).ToList();
            }
        }

    }
}