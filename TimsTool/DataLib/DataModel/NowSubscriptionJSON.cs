using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLib
{
    [Serializable]
    public sealed class NowSubscriptionJSON
    {
        private NowSubscription ns;
        private DateTime? patchDate;
        private List<string> featureToggles;

        public NowSubscriptionJSON(NowSubscription ns, DateTime? patchDate, List<string> featureToggles) 
        { 
            this.ns = ns; 
            this.patchDate = patchDate;
            this.featureToggles = featureToggles;
        }

        public Guid? id { get => ns.MasterUUID ?? ns.UUID; }

        public string name { get => ns.Name; }

        public bool isNowSubscription { get => ns.IsNow; }

        public bool isEDTSubscription { get => ns.IsEDT; }

        public bool isInformantRegisterSubscription { get => ns.IsInformantRegister; }

        public bool isCourtRegisterSubscription { get => ns.IsCourtRegister; }

        public bool isPrisonCourtRegisterSubscription { get => ns.IsPrisonCourtRegister; }

        public bool applySubscriptionRules { get => ns.ApplySubscriptionRules; }

        public NowRecipientJSON recipient 
        { 
            get => ns.NowRecipient == null ? null : new NowRecipientJSON(ns.NowRecipient); 
        }

        public SubscriptionVocabularyJSON subscriptionVocabulary
        {
            get => ns.SubscriptionVocabulary == null ? null : new SubscriptionVocabularyJSON(ns.SubscriptionVocabulary, featureToggles);
        }

        public bool firstClassLetterDelivery { get => ns.IsFirstClassLetter; }

        public bool secondClassLetterDelivery { get => ns.IsSecondClassLetter; }

        public bool emailDelivery { get => ns.IsEmail; }

        public bool forDistribution { get => ns.IsForDistribution; }

        public bool? isNotificationApi { get => ns.IsCPSNotificationAPI; }

        public string[] userGroupVariants { get => ns.UserGroupVariantsStringArray; }

        public List<NowSubscriptionJSON> childSubscriptions 
        { 
            get
            {
                if (ns.ChildNowSubscriptions == null || ns.ChildNowSubscriptions.Count == 0) { return null; }

                var children = new List<NowSubscriptionJSON>();
                foreach(var child in ns.ChildNowSubscriptions.Where(x=>x.DeletedDate == null))
                {
                    children.Add(new NowSubscriptionJSON(child, patchDate, featureToggles));
                }

                if (children.Count == 0) { return null; }
                return children;
            }
        }

        public string informantCode { get => ns.InformantCode; }

        public string yotsCode { get => ns.YOTsCode; }

        public string[] selectedCourtHouses { get => ns.SelectedCourtHousesStringArray; }

        public Guid? parentNowSubscriptionId { get => ns.ParentNowSubscription == null ? null : ns.ParentNowSubscription.MasterUUID ?? ns.ParentNowSubscription.UUID; }

        public List<Guid?> includedNOWS { get => ns.IncludedNOWS == null || !ns.IncludedNOWS.Any() ? null : ns.IncludedNOWS.Select(x=>x.MasterUUID ?? x.UUID).ToList(); }

        public List<Guid?> excludedNOWS { get => ns.ExcludedNOWS == null || !ns.ExcludedNOWS.Any() ? null : ns.ExcludedNOWS.Select(x => x.MasterUUID ?? x.UUID).ToList(); }

        public string emailTemplateName { get => ns.EmailTemplateName; }

        public string bilingualEmailTemplateName { get => ns.BilingualEmailTemplateName; }

        public string startDate { get => ns.ParentNowSubscription == null && ns.StartDate.HasValue && patchDate.HasValue && ns.StartDate.Value.Date < patchDate.Value.Date ? null : ns.StartDateJSON; }
    }
}