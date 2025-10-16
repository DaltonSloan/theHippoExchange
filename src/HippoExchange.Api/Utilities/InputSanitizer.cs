using System.Text.RegularExpressions;
using System.Web;

namespace HippoExchange.Api.Utilities
{
    public static class InputSanitizer
    {
        // Removes whitespace and dangerous tags from input
        public static string Clean(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1. Trim leading/trailing whitespace
            string cleaned = input.Trim();

            // 2. Remove HTML or JavaScript tags
            cleaned = Regex.Replace(cleaned, "<.*?>", string.Empty);

            // 3. Decode any encoded HTML entities (&lt;, &gt;)
            cleaned = HttpUtility.HtmlDecode(cleaned);

            // 4. Optionally re-encode dangerous characters
            cleaned = HttpUtility.HtmlEncode(cleaned);

            return cleaned;
        }

        // Apply to every string property in an object
        public static T SanitizeObject<T>(T obj)
        {
            //if an object is null just return it
            if (obj == null) return obj!;

            //Checks if the object is a string and only continues with the string objects 
            var stringProperties = typeof(T)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanWrite);

            //recursivly goes through all the string attributes in the object and passes them through the clean func
            foreach (var prop in stringProperties)
            {
                var value = (string?)prop.GetValue(obj);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    prop.SetValue(obj, Clean(value));
                }
            }

            return obj;
        }
    }
}
// This is the function call to this method just replace the "newAsset" with the variable name
//newAsset = InputSanitizer.SanitizeObject(newAsset);