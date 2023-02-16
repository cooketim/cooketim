using Microsoft.AspNetCore.StaticFiles;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ResultsDataService
{
    public static class DataExtensionMethods
    {
        private static readonly FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();
        public static string GetDescription(this Enum GenericEnum)
        {
            if (GenericEnum == null) { return null; }
            Type genericEnumType = GenericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var attribs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if ((attribs != null && attribs.Count() > 0))
                {
                    return ((DescriptionAttribute)attribs.ElementAt(0)).Description;
                }
            }
            return GenericEnum.ToString();
        }
        public static T GetValueFromDescription<T>(this string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
            // Or return default(T);
        }
        
        public static string GetContentType(this string filePath)
        {
            const string DefaultContentType = "application/octet-stream";           

            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = DefaultContentType;
            }

            return contentType;
        }

    }
}
