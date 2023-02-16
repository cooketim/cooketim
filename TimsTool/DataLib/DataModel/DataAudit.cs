using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataLib
{
    [Serializable]
    public enum PublishedStatus : byte
    {
        [Description("Draft")]
        Draft = 0,

        [Description("Published")]
        Published = 1,

        [Description("Published Pending")]
        PublishedPending = 2,

        [Description("Revision")]
        Revision = 3,

        [Description("Revision Pending")]
        RevisionPending = 4
    }

    [Serializable]
    public abstract class DataAudit : Audittable
    {
        private DateTime createdDate;
        public DateTime CreatedDate
        {
            get { return createdDate; }
            set 
            { 
                Added = true; 
                createdDate = value;
            }
        }

        private DateTime? lastModifiedDate;
        public DateTime? LastModifiedDate
        {
            get { return lastModifiedDate; }
            set 
            { 
                UpdatedOrDeleted = true; 
                lastModifiedDate = value;
            }
        }

        private DateTime? deletedDate;
        public DateTime? DeletedDate
        {
            get { return deletedDate; }
            set 
            { 
                UpdatedOrDeleted = true; 
                deletedDate = value;
            }
        }

        public virtual Guid? UUID { get; set; }

        public virtual Guid? MasterUUID { get; set; }

        public virtual bool? IsNewItem { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual PublishedStatus? PublishedStatus { get; set; }

        public PublishedStatus CalculatedPublishedStatus
        {
            get
            {
                if (PublishedStatus == null)
                {
                    return DataLib.PublishedStatus.Published;
                }
                return PublishedStatus.Value;
            }
        }

        public virtual DateTime? PublishedStatusDate { get; set; }

        public virtual DateTime? OriginalPublishedDate { get; set; }

        public virtual List<string> PublicationTags { get; set; }

        [JsonIgnore]
        public bool Added { get; set; }

        [JsonIgnore]
        public bool UpdatedOrDeleted { get; set; }

        [JsonIgnore]
        public string CreatedUser { get; set; }

        [JsonIgnore]
        public string LastModifiedUser { get; set; }

        [JsonIgnore]
        public string DeletedUser { get; set; }

        public void ResetTracking()
        {
            Added = false;
            UpdatedOrDeleted = false;
        }
    }

    [Serializable]
    public abstract class Audittable
    {
        public virtual string GetChangedProperties<T>(object from, object to)
        {
            if (from != null && to != null)
            {
                var type = typeof(T);
                var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var allSimpleProperties = allProperties.Where(x => x.PropertyType.IsSimpleType() && x.DeclaringType != typeof(DataAudit) & !IsUnAudittable(x)).ToList();
                var sb = new StringBuilder();

                foreach (var propertyInfo in allSimpleProperties)
                {                   
                    var fromValue = GetValue(propertyInfo, from);
                    var toValue = GetValue(propertyInfo, to);
                    if (fromValue != toValue && (fromValue == null || !fromValue.Equals(toValue)))
                    {
                        sb.AppendLine(string.Format("{0}, from: '{1}' to: '{2}'", propertyInfo.Name, fromValue, toValue));
                    }
                }
                return sb.ToString();
            }
            else
            {
                throw new ArgumentNullException("You need to provide 2 non-null objects");
            }
        }

        private bool IsUnAudittable(PropertyInfo p)
        {
            //check if has a custom attribute.
            return p.GetCustomAttributes(typeof(UnAudittableAttribute)).Any();
        }

        private object GetValue(PropertyInfo propertyInfo, object declaringObject)
        {
            var val = propertyInfo.GetValue(declaringObject, null);

            //enum type
            if (propertyInfo.PropertyType.IsEnum)
            {
                var enumVal = val as Enum;
                return enumVal.GetDescription();
            }

            //nullable enum
            var typeInfo = propertyInfo.PropertyType.GetTypeInfo();
            foreach (var pi in typeInfo.DeclaredProperties)
            {
                if (pi.PropertyType.IsEnum)
                {
                    var enumVal = val as Enum;
                    return enumVal.GetDescription();
                }
            }

            return val;
        }
    }

    // Multiuse attribute.  
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property,
                           AllowMultiple = true)  // Multiuse attribute.  
    ]
    public class UnAudittableAttribute : Attribute
    {
        public UnAudittableAttribute() { }
    }
}