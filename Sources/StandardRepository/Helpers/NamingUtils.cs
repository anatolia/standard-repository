using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers
{
    public static class NamingUtils
    {
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

            var regex = new Regex(@"([A-Z]+[a-z]*)|(\d+)", RegexOptions.Compiled);
            var matches = regex.Matches(name);
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
    }
}