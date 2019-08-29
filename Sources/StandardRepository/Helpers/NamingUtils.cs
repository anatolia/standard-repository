using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers
{
    public static class NamingUtils
    {
        private static readonly Regex DelimitedNameRegex = new Regex(@"([A-Z]+[a-z]*)|(\d+)", RegexOptions.Compiled);

        public static string GetFieldNameFromPropertyName(this string propertyName, string entityTypeName = null)
        {
            if (string.IsNullOrWhiteSpace(entityTypeName))
            {
                return GetDelimitedName(propertyName);
            }

            if (propertyName == nameof(BaseEntity.Id))
            {
                return GetDelimitedName(entityTypeName) + "_id";
            }

            if (propertyName == nameof(BaseEntity.Uid))
            {
                return GetDelimitedName(entityTypeName) + "_uid";
            }

            if (propertyName == nameof(BaseEntity.Name))
            {
                return GetDelimitedName(entityTypeName) + "_name";
            }

            return GetDelimitedName(propertyName);
        }

        public static string GetDelimitedName(this string name)
        {
            if (name.Length < 2
                || name.ToCharArray().Count(char.IsUpper) < 2)
            {
                return name.ToLowerInvariant();
            }
            
            var matches = DelimitedNameRegex.Matches(name);
            var builder = new StringBuilder();
            foreach (var item in matches)
            {
                builder.AppendFormat("{0}_", item);
            }

            var result = builder.ToString();
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new ArgumentException("Invalid name '{name}'.", nameof(name));
            }

            builder.Remove(builder.Length - 1, 1);
            return builder.ToString().ToLowerInvariant();
        }

        public static string GetPropNameFromFieldName(this string fieldName, string entityTypeName)
        {
            var delimitedTypeName = GetDelimitedName(entityTypeName);
            if (fieldName == delimitedTypeName + "_id")
            {
                return "Id";
            }

            if (fieldName == delimitedTypeName + "_uid")
            {
                return "Uid";
            }

            if (fieldName == delimitedTypeName + "_name")
            {
                return "Name";
            }

            var propName = fieldName[0].ToString().ToUpperInvariant();
            for (var i = 1; i < fieldName.Length; i++)
            {
                if (fieldName[i] == '_')
                {
                    continue;
                }

                if (fieldName[i - 1] == '_')
                {
                    propName += fieldName[i].ToString().ToUpperInvariant();
                }
                else
                {
                    propName += fieldName[i];
                }
            }

            return propName;
        }
    }
}