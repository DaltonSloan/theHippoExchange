using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace HippoExchange.Api.Utilities
{
    public static class InputSanitizer
    {
        private static readonly Regex HtmlTagRegex = new("<.*?>", RegexOptions.Compiled);

        // Removes excessive whitespace and strips simple markup without mutating safe characters
        public static string Clean(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var cleaned = input.Trim();
            cleaned = HtmlTagRegex.Replace(cleaned, string.Empty);
            cleaned = HttpUtility.HtmlDecode(cleaned);
            cleaned = cleaned.Replace("\0", string.Empty);

            return cleaned;
        }

        // Apply sanitization to all string properties within an object graph
        public static T SanitizeObject<T>(T obj)
        {
            if (obj == null)
            {
                return obj!;
            }

            var visited = new HashSet<object>();
            SanitizeRecursive(obj!, visited);
            return obj;
        }

        private static void SanitizeRecursive(object target, HashSet<object> visited)
        {
            if (target is null || target is string)
            {
                return;
            }

            if (!visited.Add(target))
            {
                return;
            }

            if (target is IEnumerable enumerable && target is not string)
            {
                if (target is IList list)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var value = list[i];
                        if (value is string str)
                        {
                            list[i] = Clean(str);
                        }
                        else if (value is not null)
                        {
                            SanitizeRecursive(value, visited);
                        }
                    }
                }
                else
                {
                    foreach (var item in enumerable)
                    {
                        if (item is not null)
                        {
                            SanitizeRecursive(item, visited);
                        }
                    }
                }
            }

            var properties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (!property.CanRead)
                {
                    continue;
                }

                var propertyType = property.PropertyType;

                if (propertyType == typeof(string) && property.CanWrite)
                {
                    var value = (string?)property.GetValue(target);
                    if (value is not null)
                    {
                        property.SetValue(target, Clean(value));
                    }
                }
                else if (!propertyType.IsValueType && propertyType != typeof(string))
                {
                    var nested = property.GetValue(target);
                    if (nested is null)
                    {
                        continue;
                    }

                    if (nested is string nestedString && property.CanWrite)
                    {
                        property.SetValue(target, Clean(nestedString));
                        continue;
                    }

                    SanitizeRecursive(nested, visited);
                }
            }
        }
    }
}
