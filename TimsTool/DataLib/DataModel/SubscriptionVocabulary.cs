using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLib
{
    [Serializable]
    public sealed class SubscriptionVocabulary : Audittable
    {
        public bool AppearedByVideoLink { get; set; }

        public bool AppearedInPerson { get; set; }

        public bool AnyAppearance { get; set; }

        public bool InCustody { get; set; }

        public bool CustodyLocationIsPrison { get; set; }

        public bool CustodyLocationIsPolice { get; set; }

        public bool IgnoreCustody { get; set; }

        public bool ProsecutorMajorCreditor { get; set; }

        public bool NonProsecutorMajorCreditor { get; set; }

        public bool AnyMajorCreditor { get; set; }

        public bool IgnoreMajorCreditor { get; set; }

        public bool AllNonCustodialResults { get; set; }

        public bool AtleastOneNonCustodialResult { get; set; }

        public bool AtleastOneCustodialResult { get; set; }

        public bool IgnoreResults { get; set; }

        public bool YouthDefendant { get; set; }
        
        public bool AdultDefendant { get; set; }

        public bool AdultOrYouthDefendant { get; set; }

        public bool WelshCourtHearing { get; set; }

        public bool EnglishCourtHearing { get; set; }

        public bool AnyCourtHearing { get; set; }

        public bool IsProsecutedByCPS { get; set; }

        [JsonIgnore]
        public List<ResultPromptRule> IncludedPromptRules { get; set; }

        [JsonIgnore]
        public List<ResultPromptRule> ExcludedPromptRules { get; set; }

        [JsonIgnore]
        public List<ResultDefinition> IncludedResults { get; set; }

        [JsonIgnore]
        public List<ResultDefinition> ExcludedResults { get; set; }

        private List<Guid?> includedPromptRuleIds = null;
        [IgnoreChanges]
        public List<Guid?> IncludedPromptRuleIds
        {
            get
            {
                if (IncludedPromptRules == null || IncludedPromptRules.Where(x => x.UUID != null).Count() == 0)
                {
                    return includedPromptRuleIds;
                }

                includedPromptRuleIds = null;

                return IncludedPromptRules.Where(x => x.UUID != null).Select(x => x.UUID).ToList();
            }
            set
            {
                includedPromptRuleIds = value;
            }
        }

        private List<Guid?> eccludedPromptRuleIds = null;

        [IgnoreChanges]
        public List<Guid?> ExcludedPromptRuleIds
        {
            get
            {
                if (ExcludedPromptRules == null || ExcludedPromptRules.Where(x => x.UUID != null).Count() == 0)
                {
                    return eccludedPromptRuleIds;
                }

                eccludedPromptRuleIds = null;

                return ExcludedPromptRules.Where(x => x.UUID != null).Select(x => x.UUID).ToList();
            }
            set
            {
                eccludedPromptRuleIds = value;
            }
        }

        private List<Guid?> includedResultIds = null;

        [IgnoreChanges]
        public List<Guid?> IncludedResultIds
        {
            get
            {
                if (IncludedResults == null || IncludedResults.Where(x => x.UUID != null).Count() == 0)
                {
                    return includedResultIds;
                }

                includedResultIds = null;

                return IncludedResults.Where(x => x.UUID != null).Select(x => x.UUID).ToList();
            }
            set
            {
                includedResultIds = value;
            }
        }

        private List<Guid?> excludedResultIds = null;

        [IgnoreChanges]
        public List<Guid?> ExcludedResultIds
        {
            get
            {
                if (ExcludedResults == null || ExcludedResults.Where(x => x.UUID != null).Count() == 0)
                {
                    return excludedResultIds;
                }

                excludedResultIds = null;

                return ExcludedResults.Where(x => x.UUID != null).Select(x => x.UUID).ToList();
            }
            set
            {
                excludedResultIds = value;
            }
        }
        public string GetRulesReport()
        {
            var sb = new StringBuilder();

            if (AppearedByVideoLink) { sb.AppendLine("AppearedByVideoLink"); }
            if (AppearedInPerson) { sb.AppendLine("AppearedInPerson"); }
            if (AnyAppearance) { sb.AppendLine("AnyAppearance"); }
            if (InCustody) { sb.AppendLine("InCustody"); }
            if (CustodyLocationIsPrison) { sb.AppendLine("CustodyLocationIsPrison"); }
            if (CustodyLocationIsPolice) { sb.AppendLine("CustodyLocationIsPolice"); }
            if (IgnoreCustody) { sb.AppendLine("IgnoreCustody"); }
            if (AllNonCustodialResults) { sb.AppendLine("AllNonCustodialResults"); }
            if (AtleastOneNonCustodialResult) { sb.AppendLine("AtleastOneNonCustodialResult"); }
            if (AtleastOneCustodialResult) { sb.AppendLine("AtleastOneCustodialResult"); }
            if (IgnoreResults) { sb.AppendLine("IgnoreResults"); }
            if (YouthDefendant) { sb.AppendLine("YouthDefendant"); }
            if (AdultDefendant) { sb.AppendLine("AdultDefendant"); }
            if (AdultOrYouthDefendant) { sb.AppendLine("AdultOrYouthDefendant"); }
            if (WelshCourtHearing) { sb.AppendLine("WelshCourtHearing"); }
            if (EnglishCourtHearing) { sb.AppendLine("EnglishCourtHearing"); }
            if (AnyCourtHearing) { sb.AppendLine("AnyCourtHearing"); }
            if (IsProsecutedByCPS) { sb.AppendLine("IsProsecutedByCPS"); }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        public string GetIncludedResultPromptsReport()
        {
            if (IncludedPromptRules == null || IncludedPromptRules.Where(x => x.DeletedDate == null && x.ResultPrompt != null && x.ResultPrompt.DeletedDate == null).Count() == 0) { return null; }

            return string.Join(Environment.NewLine, IncludedPromptRules.Where(x => x.DeletedDate == null && x.ResultPrompt != null && x.ResultPrompt.DeletedDate == null).Select(x => x.ResultPrompt.Label).OrderBy(x => x));
        }

        public string GetExcludedResultPromptsReport()
        {
            if (ExcludedPromptRules == null || ExcludedPromptRules.Where(x => x.DeletedDate == null && x.ResultPrompt != null && x.ResultPrompt.DeletedDate == null).Count() == 0) { return null; }

            return string.Join(Environment.NewLine, ExcludedPromptRules.Where(x => x.DeletedDate == null && x.ResultPrompt != null && x.ResultPrompt.DeletedDate == null).Select(x => x.ResultPrompt.Label).OrderBy(x => x));
        }

        public string GetExcludedResultsReport()
        {
            if (ExcludedResults == null || ExcludedResults.Where(x => x.DeletedDate == null).Count() == 0) { return null; }

            return string.Join(Environment.NewLine, ExcludedResults.Where(x => x.DeletedDate == null).Select(x => x.Label).OrderBy(x => x));
        }

        public string GetIncludedResultsReport()
        {
            if (ExcludedResults == null || ExcludedResults.Where(x => x.DeletedDate == null).Count() == 0) { return null; }

            return string.Join(Environment.NewLine, ExcludedResults.Where(x => x.DeletedDate == null).Select(x => x.Label).OrderBy(x => x));
        }

        private string IncludedResultsChangeReport(List<ResultDefinition> other)
        {
            List<ResultDefinition> currentNotOther = new List<ResultDefinition>();
            List<ResultDefinition> otherNotCurrent = new List<ResultDefinition>();

            if (other == null || other.Count == 0)
            {
                if (IncludedResults == null || IncludedResults.Count == 0)
                {
                    return null;
                }

                currentNotOther.AddRange(IncludedResults);
            }
            else if (IncludedResults == null)
            {
                otherNotCurrent.AddRange(other);
            }
            else
            {
                var currentNotOtherIds = IncludedResults.Select(x => x.MasterUUID ?? x.UUID).Except(other.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                currentNotOther.AddRange(from rd in IncludedResults
                                         join rdId in currentNotOtherIds on (rd.MasterUUID ?? rd.UUID) equals rdId
                                         select rd);

                var otherNotCurrentIds = other.Select(x => x.MasterUUID ?? x.UUID).Except(IncludedResults.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                otherNotCurrent.AddRange(from rd in other
                                         join rdId in otherNotCurrentIds on (rd.MasterUUID ?? rd.UUID) equals rdId
                                         select rd);
            }

            //generate a report
            if (currentNotOther.Any() || otherNotCurrent.Any())
            {
                var sb = new StringBuilder();
                if (currentNotOther.Any())
                {
                    sb.AppendLine("New Included Results...");
                    foreach (var rd in currentNotOther.OrderBy(x=>x.Label))
                    {
                        sb.AppendLine(rd.Label);
                    }
                }
                if (otherNotCurrent.Any())
                {
                    sb.AppendLine("Deleted Included Results...");
                    foreach (var rd in otherNotCurrent.OrderBy(x => x.Label))
                    {
                        sb.AppendLine(rd.Label);
                    }
                }

                return sb.ToString();
            }
            return null;
        }

        private string ExcludedResultsChangeReport(List<ResultDefinition> other)
        {
            List<ResultDefinition> currentNotOther = new List<ResultDefinition>();
            List<ResultDefinition> otherNotCurrent = new List<ResultDefinition>();

            if (other == null || other.Count == 0)
            {
                if (ExcludedResults == null || ExcludedResults.Count == 0)
                {
                    return null;
                }

                currentNotOther.AddRange(ExcludedResults);
            }
            else if (ExcludedResults == null)
            {
                otherNotCurrent.AddRange(other);
            }
            else
            {
                var currentNotOtherIds = ExcludedResults.Select(x => x.MasterUUID ?? x.UUID).Except(other.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                currentNotOther.AddRange(from rd in ExcludedResults
                                         join rdId in currentNotOtherIds on (rd.MasterUUID ?? rd.UUID) equals rdId
                                         select rd);

                var otherNotCurrentIds = other.Select(x => x.MasterUUID ?? x.UUID).Except(ExcludedResults.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                otherNotCurrent.AddRange(from rd in other
                                         join rdId in otherNotCurrentIds on (rd.MasterUUID ?? rd.UUID) equals rdId
                                         select rd);
            }

            //generate a report
            if (currentNotOther.Any() || otherNotCurrent.Any())
            {
                var sb = new StringBuilder();
                if (currentNotOther.Any())
                {
                    sb.AppendLine("New Excluded Results...");
                    foreach (var rd in currentNotOther.OrderBy(x => x.Label))
                    {
                        sb.AppendLine(rd.Label);
                    }
                }
                if (otherNotCurrent.Any())
                {
                    sb.AppendLine("Deleted Excluded Results...");
                    foreach (var rd in otherNotCurrent.OrderBy(x => x.Label))
                    {
                        sb.AppendLine(rd.Label);
                    }
                }

                return sb.ToString();
            }
            return null;
        }

        private string IncludedRPRChangeReport(List<ResultPromptRule> other)
        {
            List<ResultPromptRule> currentNotOther = new List<ResultPromptRule>();
            List<ResultPromptRule> otherNotCurrent = new List<ResultPromptRule>();

            if (other == null || other.Count == 0)
            {
                if (IncludedPromptRules == null || IncludedPromptRules.Count == 0)
                {
                    return null;
                }

                currentNotOther.AddRange(IncludedPromptRules);
            }
            else if (IncludedPromptRules == null)
            {
                otherNotCurrent.AddRange(other);
            }
            else
            {
                var currentNotOtherIds = IncludedPromptRules.Select(x => x.MasterUUID ?? x.UUID).Except(other.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                currentNotOther.AddRange(from rd in IncludedPromptRules
                                         join rdId in currentNotOtherIds on (rd.MasterUUID ?? rd.UUID) equals rdId
                                         select rd);

                var otherNotCurrentIds = other.Select(x => x.MasterUUID ?? x.UUID).Except(IncludedPromptRules.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                otherNotCurrent.AddRange(from rd in other
                                         join rdId in otherNotCurrentIds on (rd.MasterUUID ?? rd.UUID) equals rdId
                                         select rd);
            }

            //generate a report
            if (currentNotOther.Any() || otherNotCurrent.Any())
            {
                var sb = new StringBuilder();
                if (currentNotOther.Any())
                {
                    sb.AppendLine("New Included Result Prompt Rules...");
                    foreach (var rpr in currentNotOther.OrderBy(x => x.ResultPrompt.Label))
                    {
                        sb.AppendLine(rpr.ResultPrompt.Label);
                    }
                }
                if (otherNotCurrent.Any())
                {
                    sb.AppendLine("Deleted Included Result Prompt Rules...");
                    foreach (var rd in otherNotCurrent.OrderBy(x => x.ResultPrompt.Label))
                    {
                        sb.AppendLine(rd.ResultPrompt.Label);
                    }
                }

                return sb.ToString();
            }
            return null;
        }

        private string ExcludedRPRChangeReport(List<ResultPromptRule> other)
        {
            List<ResultPromptRule> currentNotOther = new List<ResultPromptRule>();
            List<ResultPromptRule> otherNotCurrent = new List<ResultPromptRule>();

            if (other == null || other.Count == 0)
            {
                if (ExcludedPromptRules == null || ExcludedPromptRules.Count == 0)
                {
                    return null;
                }

                currentNotOther.AddRange(ExcludedPromptRules);
            }
            else if (ExcludedPromptRules == null)
            {
                otherNotCurrent.AddRange(other);
            }
            else
            {
                var currentNotOtherIds = ExcludedPromptRules.Select(x => x.MasterUUID ?? x.UUID).Except(other.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                currentNotOther.AddRange(from rd in ExcludedPromptRules
                                         join rdId in currentNotOtherIds on (rd.MasterUUID ?? rd.UUID) equals rdId
                                         select rd);

                var otherNotCurrentIds = other.Select(x => x.MasterUUID ?? x.UUID).Except(ExcludedPromptRules.Select(x => x.MasterUUID ?? x.UUID)).ToList();
                otherNotCurrent.AddRange(from rd in other
                                         join rdId in otherNotCurrentIds on (rd.MasterUUID ?? rd.UUID) equals rdId
                                         select rd);
            }

            //generate a report
            if (currentNotOther.Any() || otherNotCurrent.Any())
            {
                var sb = new StringBuilder();
                if (currentNotOther.Any())
                {
                    sb.AppendLine("New Excluded Result Prompt Rules...");
                    foreach (var rpr in currentNotOther.OrderBy(x => x.ResultPrompt.Label))
                    {
                        sb.AppendLine(rpr.ResultPrompt.Label);
                    }
                }
                if (otherNotCurrent.Any())
                {
                    sb.AppendLine("Deleted Excluded Result Prompt Rules...");
                    foreach (var rpr in otherNotCurrent.OrderBy(x => x.ResultPrompt.Label))
                    {
                        sb.AppendLine(rpr.ResultPrompt.Label);
                    }
                }

                return sb.ToString();
            }
            return null;
        }

        public string GetChangedProperties(SubscriptionVocabulary other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = GetChangedProperties<SubscriptionVocabulary>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.Append(simpleTypeChanges);
            }

            //included/excluded results
            var includedResultsReport = IncludedResultsChangeReport(other.IncludedResults);
            if (!string.IsNullOrEmpty(includedResultsReport))
            {
                sb.AppendLine(includedResultsReport);
            }
            var excludedResultsReport = ExcludedResultsChangeReport(other.ExcludedResults);
            if (!string.IsNullOrEmpty(excludedResultsReport))
            {
                sb.AppendLine(excludedResultsReport);
            }

            //included/excluded prompts
            var includedRPRReport = IncludedRPRChangeReport(other.IncludedPromptRules);
            if (!string.IsNullOrEmpty(includedRPRReport))
            {
                sb.AppendLine(includedRPRReport);
            }
            var excludedRPRReport = ExcludedRPRChangeReport(other.ExcludedPromptRules);
            if (!string.IsNullOrEmpty(excludedRPRReport))
            {
                sb.AppendLine(excludedRPRReport);
            }

            return sb.ToString();
        }

        public SubscriptionVocabulary Copy()
        {
            SubscriptionVocabulary res = (SubscriptionVocabulary)MemberwiseClone();

            //make new collections for each of the dependent collections

            res.IncludedPromptRules = new List<ResultPromptRule>();
            if (IncludedPromptRules != null && IncludedPromptRules.Any())
            {
                res.IncludedPromptRules.AddRange(IncludedPromptRules.Where(x=>x.DeletedDate == null));
            }

            res.ExcludedPromptRules = new List<ResultPromptRule>();
            if (ExcludedPromptRules != null && ExcludedPromptRules.Any())
            {
                res.ExcludedPromptRules.AddRange(ExcludedPromptRules.Where(x => x.DeletedDate == null));
            }

            res.IncludedResults = new List<ResultDefinition>();
            if (IncludedResults != null && IncludedResults.Any())
            {
                res.IncludedResults.AddRange(IncludedResults.Where(x => x.DeletedDate == null));
            }

            res.ExcludedResults = new List<ResultDefinition>();
            if (ExcludedResults != null && ExcludedResults.Any())
            {
                res.ExcludedResults.AddRange(ExcludedResults.Where(x => x.DeletedDate == null));
            }

            return res;
        }        
    }
}