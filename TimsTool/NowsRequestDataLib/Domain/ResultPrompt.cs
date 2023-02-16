using DataLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class ResultPrompt
    {
        private bool hidden = false;

        [JsonProperty(PropertyName = "sequence")]
        public int? Sequence { get; }

        [JsonProperty(PropertyName = "label")]
        public string Label 
        {
            get
            {
                if (ResultPromptRule != null) { return ResultPromptRule.ResultPrompt.Label; }
                if (ResultAsAPrompt != null) { return ResultAsAPrompt.ResultDefinitionLabel; }

                return Values != null && Values.Count > 0 ? Values[0].Label : null;
            }
        }

        [JsonProperty(PropertyName = "promptReference")]
        public string PromptReference
        {
            get
            {
                if (ResultPromptRule != null) { return ResultPromptRule.ResultPrompt.PromptReference; }
                if (ResultAsAPrompt != null) { return null; }

                return Values != null && Values.Count > 0 ? Values[0].PromptReference : null;
            }
        }

        [JsonProperty(PropertyName = "hidden")]
        public bool Hidden
        {
            get
            {
                if (hidden) { return true; }
                if (ResultPromptRule != null) { return ResultPromptRule.ResultPrompt.Hidden; }
                if (ResultAsAPrompt != null) { return false; }

                return Values != null && Values.Count > 0 ? Values[0].Hidden : false;
            }
        }

        [JsonProperty(PropertyName = "associateToReferenceData")]
        public bool AssociateToReferenceData
        {
            get
            {
                if (ResultPromptRule != null) { return ResultPromptRule.ResultPrompt.AssociateToReferenceData; }
                if (ResultAsAPrompt != null) { return false; }

                return Values != null && Values.Count > 0 ? Values[0].AssociateToReferenceData : false;
            }
        }

        [JsonProperty(PropertyName = "promptIdentifier")]
        public Guid? PromptIdentifier
        {
            get
            {
                if (ResultPromptRule != null) { return ResultPromptRule.ResultPrompt.UUID; }
                if (ResultAsAPrompt != null) { return ResultAsAPrompt.ResultDefinitionId; }

                return Values != null && Values.Count > 0 ? Values[0].PromptIdentifier : null;
            }
        }

        [JsonProperty(PropertyName = "promptType")]
        public string PromptType
        {
            get
            {
                if (ResultPromptRule != null) { return ResultPromptRule.ResultPrompt.PromptType; }
                if (ResultAsAPrompt != null) { return null; }

                return Values != null && Values.Count > 0 ? Values[0].PromptType : null;
            }
        }

        [JsonProperty(PropertyName = "userGroups")]
        public List<string> UserGroups
        {
            get
            {
                if (ResultPromptRule != null) { return ResultPromptRule.UserGroups; }
                return new List<string>();
            }
        }

        [JsonProperty(PropertyName = "welshLabel")]
        public string WelshLabel
        {
            get
            {
                if (ResultPromptRule != null) { return ResultPromptRule.ResultPrompt.WelshLabel; }
                if (ResultAsAPrompt != null) { return ResultAsAPrompt.ResultDefinitionWelshLabel; }

                return Values != null && Values.Count > 0 ? Values[0].WelshLabel : null;
            }
        }

        [JsonProperty(PropertyName = "value")]
        public string Value
        {
            get
            {
                if (ResultAsAPrompt == null)
                {
                    return (Values != null && Values.Count > 0) ? Values[0].Value : null;
                }

                if (ResultAsAPrompt.SelectedResultPrompts != null)
                {
                    var sb = new StringBuilder();
                    foreach (var item in ResultAsAPrompt.SelectedResultPrompts.Where(x => !x.Hidden).OrderBy(x=>x.Sequence))
                    {
                        if (sb.Length > 0) { sb.Append(", "); }
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            sb.AppendFormat("{0}: {1}", item.Label, item.Value);
                        }
                        else
                        {
                            sb.Append(item.Label);
                        }
                    }
                    return sb.ToString();
                }

                return null;
            }
        }

        [JsonProperty(PropertyName = "welshValue")]
        public string WelshValue
        {
            get
            {
                if (ResultAsAPrompt == null)
                {
                    return (Values != null && Values.Count > 0) ? Values[0].WelshValue : null;
                }

                if (ResultAsAPrompt.SelectedResultPrompts != null)
                {
                    var sb = new StringBuilder();
                    foreach (var item in ResultAsAPrompt.SelectedResultPrompts.Where(x => !x.Hidden).OrderBy(x=>x.Sequence))
                    {
                        if (!string.IsNullOrEmpty(item.WelshValue))
                        {
                            if (sb.Length > 0) { sb.Append(", "); }
                            sb.AppendFormat("{0}: {1}", item.WelshLabel, item.WelshValue);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.WelshLabel))
                            {
                                if (sb.Length > 0) { sb.Append(", "); }
                                sb.Append(item.WelshLabel);
                            }
                        }
                    }
                    return sb.ToString();
                }

                return null;
            }
        }

        [JsonIgnore]
        public Result Result { get; }

        [JsonIgnore]
        public ResultPromptRule ResultPromptRule { get; }

        [JsonProperty(PropertyName = "resultAsAPrompt")]
        public Result ResultAsAPrompt { get; private set; }

        [JsonProperty(PropertyName = "isSelected")]
        public bool IsSelected { get; internal set; }

        [JsonProperty(PropertyName = "isDistinctPromptRequired")]
        public bool IsDistinctPromptRequired 
        {
            get
            {
                if (ResultAsAPrompt != null)
                {
                    return ResultAsAPrompt.DistinctResultTypes;
                }

                return Result.DistinctPromptTypes(ResultPromptRule.ResultPromptUUID);
            }
        }

        [JsonProperty(PropertyName = "isVariantData")]
        public bool IsVariantData
        {
            get
            {
                if (ResultAsAPrompt != null)
                {
                    //mde cannot be set on results as a prompt
                    return false;
                }

                return Result.IsVariantData(ResultPromptRule.ResultPromptUUID);
            }
        }

        [JsonProperty(PropertyName = "level")]
        public string Level
        {
            get => ResultAsAPrompt == null ? Result.Level : ResultAsAPrompt.Level;
        }

        [JsonProperty(PropertyName = "qualifier")]
        public string Qualifier
        {
            get
            {
                if (ResultPromptRule != null) { return ResultPromptRule.ResultPrompt.Qual; }

                return Result.ResultDefinition.Qualifier;
            }
        }

        [JsonProperty(PropertyName = "values")]
        public List<ResultPromptValue> Values { get; private set; }

        internal ResultPrompt(Result result, ResultPromptRule resultPromptRule)
        {
            Result = result;
            ResultPromptRule = resultPromptRule;
            Sequence = resultPromptRule.PromptSequence;
            Values = new List<ResultPromptValue>();
        }

        internal ResultPrompt(Result result, Result resultAsAPrompt)
        {
            Result = result;
            ResultAsAPrompt = resultAsAPrompt;
            Sequence = resultAsAPrompt.ResultDefinitionRank;
            Values = new List<ResultPromptValue>();
            IsSelected = result.IsSelected;
        }

        internal string AppendValue(Defendant defendant, ProsecutionCase prosecutionCase, Offence offence, List<FixedList> fixedLists, string value)
        {
            if ((IsVariantData || Result.Level.ToLowerInvariant() == "d") && Values != null && Values.Count > 0)
            {
                //set the same value, as we do not want any variation
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, Values[0].Value));
                return Values[0].Value;
            }

            if (Result.Level.ToLowerInvariant() == "c" && Values != null && Values.Count > 0)
            {
                //look to see if we already have a value for this case defendant
                var match = Values.FirstOrDefault(x => x.DefendantId == defendant.DefendantId && x.CaseId == prosecutionCase.CaseId);
                if (match != null)
                {
                    Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, match.Value));
                    return match.Value;
                }
            }

            if (!string.IsNullOrEmpty(value))
            {
                //set the same value, as we do not want any variation
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, value));
                return value;
            }


            //BOOLEAN,CURR,DATE,DATES LIST,FIXL,FIXLM,FIXLO,FIXLOM,INT,TIME,TXT,NAMEADDRESS,ADDRESS
            string response = null;

            switch (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant())
            {
                case "boolean":
                    if (Values != null && Values.Count > 0)
                    {
                        //force some variation
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, Values[0].Value == "Yes" ? "No" : "Yes"));
                    }
                    else
                    {
                        var boolVal = HearingResults.Switch();
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, boolVal ? "Yes" : "No"));
                    }
                    response = Values.Last().Value;
                    break;
                case "curr":
                    if (Values != null && Values.Count > 0)
                    {
                        //force some variation
                        var valCurr = Math.Round((HearingResults.Random.Next(100, 999)) / 7.63, 2);
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, string.Format("£{0:0.00}", valCurr)));
                    }
                    else
                    {
                        var valCurr = Math.Round((HearingResults.Random.Next(1000, 99999)) / 7.63, 2);
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, string.Format("£{0:0.00}", valCurr)));
                    }
                    response = Values.Last().Value;
                    break;
                case "date":
                    if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("start")
                        || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("from")
                        || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("conviction"))
                    {
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy")));
                        }
                        else
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, DateTime.Now.ToString("dd/MM/yyyy")));
                        }
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("original"))
                    {
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, DateTime.Now.AddDays(-38).ToString("dd/MM/yyyy")));
                        }
                        else
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, DateTime.Now.AddDays(-42).ToString("dd/MM/yyyy")));
                        }                
                    }
                    else
                    {
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, DateTime.Now.AddDays(70).ToString("dd/MM/yyyy")));
                        }
                        else
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, DateTime.Now.AddDays(73).ToString("dd/MM/yyyy")));
                        }                        
                    }
                    response = Values.Last().Value;
                    break;
                case "int":
                    var valIntMin = string.IsNullOrEmpty(ResultPromptRule.ResultPrompt.Min) ? 1 : int.Parse(ResultPromptRule.ResultPrompt.Min);
                    var valIntMax = string.IsNullOrEmpty(ResultPromptRule.ResultPrompt.Max) ? 40 : int.Parse(ResultPromptRule.ResultPrompt.Max);
                    var intVal = (HearingResults.Random.Next(valIntMin, valIntMax + 1)).ToString();

                    if (ResultPromptRule.ResultPrompt.DurationElements != null && ResultPromptRule.ResultPrompt.DurationElements.Count > 0)
                    {
                        var index = HearingResults.Random.Next(ResultPromptRule.ResultPrompt.DurationElements.Count - 1);
                        intVal = intVal + " " + ResultPromptRule.ResultPrompt.DurationElements[index].DurationElement;
                    }

                    Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, intVal));
                    response = intVal;
                    break;
                case "fixl":
                case "fixlm":
                case "fixlo":
                case "fixlom":
                    List<FixedListValue> fixedListVals = null;
                    if (ResultPromptRule.ResultPrompt.FixedList != null && ResultPromptRule.ResultPrompt.FixedList.Values != null && ResultPromptRule.ResultPrompt.FixedList.Values.Count>0)
                    {
                        fixedListVals = ResultPromptRule.ResultPrompt.FixedList.Values;
                    }
                    else if (ResultPromptRule.ResultPrompt.FixedListUUID != null)
                    {
                        var fixedList = fixedLists.FirstOrDefault(x => x.UUID == ResultPromptRule.ResultPrompt.FixedListUUID);
                        if (fixedList != null && fixedList.Values != null && fixedList.Values.Count>0)
                        {
                            fixedListVals = fixedList.Values;
                        }
                    }
                    if (fixedListVals == null)
                    {
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "FixedList still missing"));
                        }
                        else
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "FixedList missing"));
                        }
                        response = Values.Last().Value;
                        break;
                    }
                    if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "fixlm" || ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "fixlom")
                    {
                        var numberItems = HearingResults.Random.Next(fixedListVals.Count-1)+1;
                        //select random items
                        var indices = new List<int>();
                        var selectedValues = new List<string>();
                        while (indices.Count < numberItems)
                        {
                            var index = HearingResults.Random.Next(fixedListVals.Count - 1);
                            if (!indices.Contains(index))
                            {
                                indices.Add(index);
                                selectedValues.Add(fixedListVals[index].Value);
                            }
                        }
                        var val = string.Join(",", selectedValues);
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            if (val == Values[0].Value)
                            {
                                if (selectedValues.Count>1)
                                {
                                    selectedValues.RemoveAt(0);
                                }
                                else
                                {
                                    bool changed = false;
                                    var index = 0;
                                    while(!changed)
                                    {
                                        if (!selectedValues.Contains(fixedListVals[index].Value))
                                        {
                                            selectedValues.Add(fixedListVals[index].Value);
                                            changed = true;
                                        }
                                        index++;
                                    }
                                }
                                val = string.Join(",", selectedValues);
                            }
                        }
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, val));
                    }
                    else
                    {
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            bool differentValueFound = false;
                            var index = 0;
                            while (!differentValueFound)
                            {
                                if (fixedListVals[index].Value != Values[0].Value)
                                {
                                    Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, fixedListVals[index].Value));
                                    differentValueFound = true;
                                }
                                index++;
                            }
                        }
                        else
                        {
                            var index = HearingResults.Random.Next(fixedListVals.Count - 1);
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, fixedListVals[index].Value));
                        }
                    }
                    response = Values.Last().Value;
                    break;
                case "time":
                    if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("start")
                        || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("from")
                        || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("1"))
                    {
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "9.30 AM"));
                        }
                        else
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "8.30 AM"));
                        }
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("hearing")
                        || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("appeal"))
                    {
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "9.45 AM"));
                        }
                        else
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "10.30 AM"));
                        }
                    }
                    else
                    {
                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "6.45 PM"));
                        }
                        else
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "7.45 PM"));
                        }
                    }
                    response = Values.Last().Value;
                    break;
                case "txt":
                    if (Values != null && Values.Count > 0)
                    {
                        //force some variation
                        response = SetVariableText(defendant, prosecutionCase, offence);
                        break;
                    }
                    if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("conveyor"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Prisoner Escort and Custody Services"));
                    }
                    if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("parent"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Mum & Dad Harrison"));
                    }
                    if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 1"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "14 Somerset Cresent"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 2"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Ashcroft"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 3"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Hertfordshire"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 4")
                        || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 5"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, null));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("post code"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "H34 5TY"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("employer's name"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "H.G. Listers"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "14 Somerset Cresent, Ashcroft, Hertfordshire H34 5TY"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("crown court"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Liverpool Crown Court"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("youth court"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Liverpool Youth Court"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("spoc"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "James Walsh"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("contact"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Liverpool Magistrates' Court / James Walsh"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("magistrate")
                        || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("court"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Liverpool Magistrates' Court"));
                    }                    
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("nationality"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "British"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("animal"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Japanese Tosa"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("probation team"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Central team"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant() == "prison")
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "HMP Bellmarsh"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("sentence consecutive to what"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Case URN 20KK18976GH36"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("consecutive to an offence on another case"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Case URN 20KK18976GH36"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("consecutive to an offence on another case"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Case URN 20KK18976GH36"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("reason for significant risk to public of serious harm"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Repeat offender with a high propoensity to commit crime"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().StartsWith("reason") || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("additional information"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Because the sky is blue and the grass is green"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("excused reason"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Medical emergency"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("under direction of") || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("medical practitioner"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Dr Jekyl"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("treatment place") || 
                         ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("treatment institution") ||
                         ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("care home"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "The Range Consulting Rooms, Hertford"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("review frequency"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "6 months"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("dates of prohibition"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Jan, Feb, March"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("alcohol limit"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "4 bottles of whisky a week"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("place / area"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "The Dog & Duck"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("countries"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "The European Economic Area"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("country allowed"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "France"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("protected person"))
                    {
                        var names = new List<string>() { "Alice Jackie Smith", "Anthony Apachella", "Louise Michelle Williamson", "Simon Ian Donaldson", "Naomi Sophie Jacobs" };
                        var selectedItem = HearingResults.Random.Next(4);
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, names[selectedItem]));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("order details"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Behave yourself"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("vulnerability factors"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "There is an imminent risk of serious harm.  Static factors are drug misuse, impulsivity and poor emotional control, attitudes that support crime"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("name of witness(es)"))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Chief Inspector Clouseau & Miranda Simmons & Caylee Anthony"));
                    }
                    else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("name "))
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Shirley James"));
                    }                    
                    else
                    {
                        Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Lorem ipsum dolor sit amet, consectetur adipiscing elit"));
                    }
                    response = Values.Last().Value;
                    break;
                case "address":
                case "nameaddress":
                    {
                        if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("representative"))
                        {
                            if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("organisationname"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Saul Goodman & Associates"));
                                response = Values.Last().Value;
                            }

                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address1"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "9800 Montgomery Blvd NE"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address2"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Albuquerque"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address3"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "New Mexico"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("postcode"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "NM4 8TY"));
                                response = Values.Last().Value;
                            }
                            break;
                        }

                        if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("local authority"))
                        {
                            if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("organisationname"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Hertfordshire County Council"));
                                response = Values.Last().Value;
                            }

                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("Hertfordshire Bail Support Services"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "228 Hatfield Road"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address2"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "St Albans"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address3"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Hertfordshire"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("postcode"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "AL1 4LW"));
                                response = Values.Last().Value;
                            }
                            break;
                        }                        

                        if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("conveyor"))
                        {
                            if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("organisationname"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Prisoner Escort and Custody Services"));
                                response = Values.Last().Value;
                            }

                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address1"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "East Office For Contracted Prisons"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address2"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Peterborough"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("postcode"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "PE7 8GX"));
                                response = Values.Last().Value;
                            }
                            break;
                        }

                        if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("prison"))
                        {
                            if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("organisationname"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "HMP Bellmarsh"));
                                response = Values.Last().Value;
                            }

                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address1"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Western Way"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address2"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "London"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("postcode"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "SE28 0EB"));
                                response = Values.Last().Value;
                            }
                            break;
                        }

                        if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("parent"))
                        {
                            if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("firstname"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Mum and Dad"));
                                response = Values.Last().Value;
                            }

                            if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("lastname"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Harrison"));
                                response = Values.Last().Value;
                            }

                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address1"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "11 Bishops Close"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address2"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "East Parkway"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address3"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "St Albans"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address4"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Hertfordshire"));
                                response = Values.Last().Value;
                            }
                            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("postcode"))
                            {
                                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "AL4 7YH"));
                                response = Values.Last().Value;
                            }
                            break;
                        }

                        if (Values != null && Values.Count > 0)
                        {
                            //force some variation
                            response = SetVariableAddress(defendant, prosecutionCase, offence);
                            break;
                        }

                        if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("organisationname"))
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "H.G. Listers"));
                            response = Values.Last().Value;
                        }

                        if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address1"))
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "14 Somerset Cresent"));
                            response = Values.Last().Value;
                        }
                        if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address2"))
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Ashcroft"));
                            response = Values.Last().Value;
                        }
                        if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address3"))
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Hertfordshire"));
                            response = Values.Last().Value;
                        }
                        if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("postcode"))
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "H34 5TY"));
                            response = Values.Last().Value;
                        }
                        if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("email1"))
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "info@hglisters.com"));
                            response = Values.Last().Value;
                        }
                        if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("email2"))
                        {
                            Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "admin@hglisters.com"));
                            response = Values.Last().Value;
                        }
                        break;
                    }
            }

            return response;
        }

        internal void Hide()
        {
            hidden = true;
        }

        private string SetVariableAddress(Defendant defendant, ProsecutionCase prosecutionCase, Offence offence)
        {
            if (ResultPromptRule.ResultPrompt.PromptType.ToLowerInvariant() == "nameaddress" && ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("organisationname"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Greggs"));
                return Values.Last().Value;
            }

            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address1"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "22 Acacia Avenue"));
                return Values.Last().Value;
            }
            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address2"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "St Albans"));
                return Values.Last().Value;
            }
            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("address3"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "Hertfordshire"));
                return Values.Last().Value;
            }
            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("postcode"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "AL1 2UH"));
                return Values.Last().Value;
            }
            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("email1"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "info@greggs.com"));
                return Values.Last().Value;
            }
            if (ResultPromptRule.ResultPrompt.PromptReference.ToLowerInvariant().EndsWith("email2"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, ResultPromptRule.ResultPrompt.PromptReference, "admin@greggs.com"));
                return Values.Last().Value;
            }
            return null;
        }

        private string SetVariableText(Defendant defendant, ProsecutionCase prosecutionCase, Offence offence)
        {
            if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 1"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Flat 1"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 2"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "22 Beaumont Avenue"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 3"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "London Colney"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 4"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "St Albans"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address line 5"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Hertfordshire"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("post code"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "AL3 6XK"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("employer's name"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Mr Wippy"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("address"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Flat 1, 22 Beaumont Avenue, London Colney, St Albans, Hertfordshire AL3 6XK"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("crown court"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "St Albans Crown Court"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("youth court"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "St Albans Youth Court"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("spoc"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Adrian Thomas"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("contact"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "St Albans Magistrates' Court / Adrian Thomas"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("magistrate")
                || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("court"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "St Albans Magistrates' Court"));
            }            
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("nationality"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Irish"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("animal"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Rottweiler"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("probation team"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "West Herts team"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant() == "prison")
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "HMP Bedford"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("sentence consecutive to what"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Case URN 19TT68923JD42"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("consecutive to an offence on another case"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Case URN 19TT68923JD42"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("consecutive to an offence on another case"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Case URN 19TT68923JD42"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("reason for significant risk to public of serious harm"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "May cause harm to the public"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().StartsWith("reason") || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("additional information"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Because this is what happens to criminals"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("excused reason"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Sick note from Mum"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("under direction of") || ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("medical practitioner"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Mr Hyde"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("treatment place") ||
                 ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("treatment institution") ||
                 ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("care home"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "The Bridge Rooms, Welwyn Garden City"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("review frequency"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "9 months"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("dates of prohibition"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Jun, Jul, Aug"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("alcohol limit"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "10 bottles of whisky a week"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("place / area"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "The Peahen"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("countries"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "France"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("country allowed"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Germany"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("protected person"))
            {
                var names = new List<string>() { "Helen Emma Wilkins", "John Michael Thomas", "Janet Julie Warwick", "Peter Gerald Scott", "Lesley Theresa Jackson" };
                var selectedItem = HearingResults.Random.Next(4);
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, names[selectedItem]));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("order details"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Stay out of trouble"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("vulnerability factors"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "There is a risk of future reoffending within two years.  Poor emotional control with a propensity to be influenced by others"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("name of witness(es)"))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Alison Green & Hannah Stone & Gill Whitford"));
            }
            else if (ResultPromptRule.ResultPrompt.Label.ToLowerInvariant().Contains("name "))
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Hillary Thomas"));
            }
            else
            {
                Values.Add(new ResultPromptValue(this, defendant, prosecutionCase, offence, null, "Unknown value"));
            }

            return Values.Last().Value;
        }

        internal ResultPrompt CloneDefendantResultPrompt(Guid defendantId)
        {
            var resp = (ResultPrompt)this.MemberwiseClone();

            //filter the values for the passed defendant Id
            resp.Values = new List<ResultPromptValue>();
            resp.Values.AddRange(this.Values.Where(x => x.DefendantId == defendantId));
            return resp.Values.Count == 0 ? null : resp;
        }

        internal ResultPrompt CloneDefendantCaseResultPrompt(Guid? caseId)
        {
            var resp = (ResultPrompt)this.MemberwiseClone();

            //filter the values for the passed caseId
            resp.Values = new List<ResultPromptValue>();
            resp.Values.AddRange(this.Values.Where(x => x.CaseId == caseId));
            return resp.Values.Count == 0 ? null : resp;
        }

        internal ResultPrompt CloneOffenceResultPrompt(Guid? caseId, Guid offenceId)
        {
            var resp = (ResultPrompt)this.MemberwiseClone();

            if (resp.ResultAsAPrompt != null)
            {
                resp.ResultAsAPrompt = resp.ResultAsAPrompt.CloneAsAPrompt(caseId, offenceId);
                return resp.ResultAsAPrompt.ResultPrompts != null && resp.ResultAsAPrompt.ResultPrompts.Count > 0 ? resp : null;
            }

            //filter the values for the passed caseId and offence Id
            resp.Values = new List<ResultPromptValue>();
            resp.Values.AddRange(this.Values.Where(x => x.CaseId == caseId && x.OffenceId == offenceId));
            return resp.Values.Count == 0 ? null : resp;
        }
    }
}
