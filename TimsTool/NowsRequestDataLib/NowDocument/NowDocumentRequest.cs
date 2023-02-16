using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NowsRequestDataLib.Domain;

namespace NowsRequestDataLib.NowDocument
{
    [Serializable]
    public class NowDocumentRequest
    {
        public List<Guid?> cases { get; set; }
        public List<Guid?> applications { get; set; }
        public Guid? materialId { get; set; }
        /// <summary>
        /// Identifies the correlation id that will be used to correlate the GOB Account Number to this NOW
        /// </summary>
        public Guid? requestId { get; set; }
        public Guid? hearingId { get; set; }
        public Guid? masterDefendantId { get; set; }
        public Guid? nowTypeId { get; set; }        
        public NowDistribution nowDistribution { get; set; }
        public string[] visibleToUserGroups { get; set; }
        public string[] notVisibleToUserGroups { get; set; }
        public NowDocumentContent nowContent { get; set; }
        public string bilingualTemplateName { get; set; }
        public string templateName { get; set; }
        public string subTemplateName { get; set; }
        public string bilingualEmailTemplateName { get; set; }
        public string subscriberName { get; set; }
        public bool storageRequired { get; set; }

        public NowDocumentRequest(List<Guid?> cases, List<Guid?> applications, Guid? hearingId, Guid? defendantId, 
                                    Guid? nowTypeId, NowDistribution nowDistribution, string[] usergroups, 
                                    NowDocumentContent nowContent, string templateName, 
                                    string subTemplateName, string bilingualTemplateName, 
                                    string subscriberName, bool forStorage)
        {
            this.materialId = Guid.NewGuid();
            this.cases = cases == null || cases.Count==0 ? null : cases;
            this.applications = applications == null || applications.Count == 0 ? null : applications; ;
            this.hearingId = hearingId;
            this.masterDefendantId = defendantId;
            this.nowTypeId = nowTypeId;
            this.visibleToUserGroups = usergroups;
            this.nowContent = nowContent;
            this.nowDistribution = nowDistribution;
            this.templateName = templateName;
            this.subTemplateName = subTemplateName;
            this.bilingualTemplateName = bilingualTemplateName;
            this.bilingualEmailTemplateName = bilingualEmailTemplateName;
            this.subscriberName = subscriberName;
            this.storageRequired = forStorage;
        }
    }
}
