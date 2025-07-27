using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Assignment.Utilities
{
    public static class StringConverter
    {
        public static string ConvertUrl(string str)
        {
            string result = str;

            result = result.Replace("Đ", "d").Replace("đ", "d").ToLowerInvariant();

            result = new string(result.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            result = Regex.Replace(result, @"\s+", "-");

            result = Regex.Replace(result, @"[^a-z0-9-]", "");

            return result;
        }
    }
}
