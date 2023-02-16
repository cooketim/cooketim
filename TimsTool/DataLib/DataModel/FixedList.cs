using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Identity;
using Newtonsoft.Json;

namespace DataLib
{
    [Serializable]
    public sealed class FixedList : DataAudit, IEquatable<FixedList>
    {
        public FixedList() 
        { 
            UUID = Guid.NewGuid(); 
            CreatedDate = DateTime.Now; 
            CreatedUser = IdentityHelper.SignedInUser.Email;
            Label = "New Fixed List";
            Values = new List<FixedListValue>();
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
        }

        public string Label { get; set; }

        public List<FixedListValue> Values  { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [JsonIgnore]
        public List<FixedListValueJSON> ValuesJSON
        {
            get
            {
                if (Values == null || Values.Where(x => x.DeletedDate == null).Count() == 0) { return null; }
                var res = new List<FixedListValueJSON>();

                //deal with scenario where there are not any durations or word groups or name address
                foreach (var val in Values.Where(x => x.DeletedDate == null))
                {
                    res.Add(new FixedListValueJSON(val));
                }

                return res;
            }
        }

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

        public FixedList Copy()
        {
            return MakeCopy(false);
        }

        private FixedList MakeCopy(bool asDraft)
        {
            FixedList res = (FixedList)this.MemberwiseClone();
            res.UUID = Guid.NewGuid();
            res.LastModifiedDate = DateTime.Now;
            res.CreatedDate = DateTime.Now;
            res.LastModifiedUser = IdentityHelper.SignedInUser.Email;
            res.CreatedUser = IdentityHelper.SignedInUser.Email;

            //replace the fixed list collection
            res.Values = new List<FixedListValue>();
            if(Values != null)
            {
                foreach(var val in Values.Where(x=>x.DeletedDate == null))
                {
                    //make a new copy of the fixed list value so that it is a separate instance 
                    //and therefore can be maintained separately
                    var newVal = val.Copy();
                    res.Values.Add(newVal);
                }
            }

            //remove the publication tags
            if (res.PublicationTags != null)
            {
                res.PublicationTags.Clear();
            }

            if (asDraft)
            {
                res.PublishedStatus = DataLib.PublishedStatus.Draft;
                if (res.MasterUUID == null)
                {
                    res.MasterUUID = MasterUUID ?? UUID;
                }
            }
            else
            {
                res.MasterUUID = res.UUID;
                res.Label = res.Label + " - Copy";
            }

            return res;
        }

        public FixedList Draft()
        {
            return MakeCopy(true);
        }

        public void SynchroniseChanges(FixedList source)
        {
            Label = source.Label;
            Values = source.Values;
            LastModifiedDate = source.LastModifiedDate;
            LastModifiedUser = source.LastModifiedUser;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FixedList);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;

            hashCode = hashCode * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(UUID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Label);
            hashCode = hashCode * -1521134295 + GetFixedListValueListHashCode();
            return hashCode;
        }

        public bool Equals(FixedList other)
        {
            return other != null &&
                   UUID == other.UUID &&
                   string.Equals(Label, other.Label, StringComparison.InvariantCulture) &&
                   FixedListValueListsAreEqual(other.Values);
        }

        public static bool operator ==(FixedList fl1, FixedList fl2)
        {
            return EqualityComparer<FixedList>.Default.Equals(fl1, fl2);
        }

        public static bool operator !=(FixedList fl1, FixedList fl2)
        {
            return !(fl1 == fl2);
        }

        private bool FixedListValueListsAreEqual(List<FixedListValue> other)
        {
            if (other == null || other.Count == 0)
            {
                if (this.Values == null || this.Values.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.Values == null)
            {
                return false;
            }

            //compare the two lists
            var currentNotOther = Values.Except(other).ToList();
            var otherNotCurrent = other.Except(Values).ToList();
            return !currentNotOther.Any() && !otherNotCurrent.Any();
        }

        public int GetFixedListValueListHashCode()
        {
            int hc = 0;
            if (Values != null)
                foreach (var v in Values)
                    hc ^= v.GetHashCode();
            return hc;
        }

        public string GetChangedProperties(FixedList other)
        {
            var sb = new StringBuilder();
            var simpleTypeChanges = base.GetChangedProperties<FixedList>(this, other);
            if (!string.IsNullOrEmpty(simpleTypeChanges))
            {
                sb.AppendLine(simpleTypeChanges);
            }

            //now add reports for the complex types:
            if (!FixedListValueListsAreEqual(other.Values))
            {
                sb.AppendLine("From Fixed List Values...");
                sb.AppendLine(ReportFixedList());
                sb.AppendLine();
                sb.AppendLine("To Fixed List Values...");
                sb.AppendLine(other.ReportFixedList());
            }

            return sb.ToString();
        }

        private string ReportFixedList()
        {
            if (Values != null && Values.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var flv in Values)
                {
                    sb.AppendLine(string.Format("Code: '{0}', Value: '{1}', Welsh Value: '{2}', CJSQualifier: '{3}'", flv.Code, flv.Value, flv.WelshValue, flv.CJSQualifier));
                }
                return sb.Length == 0 ? "'Empty'" : sb.ToString();
            }
            return "'Empty'";
        }
    }

    [Serializable]
    public sealed class FixedListJSON
    {
        private FixedList fl;

        public FixedListJSON(FixedList fl) { this.fl = fl; }

        public Guid? id { get => fl.MasterUUID ?? fl.UUID; }

        public List<FixedListValueJSON> elements { get { return fl.ValuesJSON; } }

        public string startDate { get => fl.StartDateJSON; }

        public string endDate { get => fl.EndDateJSON; }
    }

    [Serializable]
    public sealed class FixedListValue : DataAudit, IEquatable<FixedListValue>
    {
        public FixedListValue() 
        { 
            UUID = Guid.NewGuid(); 
            CreatedDate = DateTime.Now; 
            CreatedUser = IdentityHelper.SignedInUser.Email;
            Code = "NFLV";
            Value = "New Fixed List Value";
            LastModifiedDate = DateTime.Now;
            LastModifiedUser = IdentityHelper.SignedInUser.Email;
        }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "welshValue")]
        public string WelshValue { get; set; }

        [JsonProperty(PropertyName = "cjsQualifier")]
        public string CJSQualifier { get; set; }

        public void SynchroniseChanges(FixedListValue source)
        {
            Value = source.Value;
            WelshValue = source.WelshValue;
            CJSQualifier = source.CJSQualifier;
            LastModifiedDate = source.LastModifiedDate;
            LastModifiedUser = source.LastModifiedUser;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FixedListValue);
        }

        public override int GetHashCode()
        {
            var hashCode = 1338136996;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WelshValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CJSQualifier);
            return hashCode;
        }

        public bool Equals(FixedListValue other)
        {
            return other != null &&
                   Code == other.Code &&
                   Value == other.Value &&
                   WelshValue == other.WelshValue &&
                   CJSQualifier == other.CJSQualifier;
        }

        internal FixedListValue Copy()
        {
            return (FixedListValue)this.MemberwiseClone();
        }

        public static bool operator ==(FixedListValue flv1, FixedListValue flv2)
        {
            return EqualityComparer<FixedListValue>.Default.Equals(flv1, flv2);
        }

        public static bool operator !=(FixedListValue flv1, FixedListValue flv2)
        {
            return !(flv1 == flv2);
        }
    }

    [Serializable]
    public sealed class FixedListValueJSON
    {
        private FixedListValue flv;
        public FixedListValueJSON(FixedListValue flv) { this.flv = flv; }

        public string code { get { return flv.Code; } }

        public string value { get { return flv.Value; } }

        public string welshValue { get { return flv.WelshValue; } }

        public string cjsQualifier { get { return flv.CJSQualifier; } }
    }

    [Serializable]
    public sealed class FixedList251
    {
        private FixedList fl;
        public FixedList251(FixedList fl) { this.fl = fl; }
        public Guid? id { get => fl.UUID; }
        public List<FixedListValue251> elements
        {
            get
            {
                if (fl.Values == null || fl.Values.Count == 0) { return null; }
                var res = new List<FixedListValue251>();
                fl.Values.ForEach(x => res.Add(new FixedListValue251(x)));
                return res;
            }
        }
    }

    [Serializable]
    public sealed class FixedList251DataPatch
    {
        public Guid? id { get; set; }
        public List<FixedListValue251DataPatch> elements { get; set; }
    }

    [Serializable]
    public sealed class FixedListValue251DataPatch
    {
        public string code { get; set; }

        public string value { get; set; }

        public string welshValue { get; set; }
    }

    [Serializable]
    public sealed class FixedListValue251
    {
        private FixedListValue flv;
        public FixedListValue251(FixedListValue flv) { this.flv = flv; }

        public string code { get => flv.Code; }

        public string value { get => flv.Value; }

        public string welshValue { get => flv.WelshValue; }

        public string cjsQualifier { get => flv.CJSQualifier; }
    }
}