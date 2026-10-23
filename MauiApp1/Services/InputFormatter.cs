using System.Globalization;
using System.Text.RegularExpressions;

namespace MauiApp1.Services
{
    public static class InputFormatter
    {
        /// <summary>
        /// Converts text to Title Case (e.g., "john mark" -> "John Mark")
        /// </summary>
        public static string ToTitleCase(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.Trim().ToLower());
        }

        /// <summary>
        /// Converts text to uppercase, removing leading/trailing spaces (e.g., parish/record codes)
        /// </summary>
        public static string ToCleanUpper(string? input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Formats phone numbers into standard international or local format (e.g., "0712345678" -> "+254712345678")
        /// </summary>
        public static string FormatPhoneNumber(string? phone, string defaultCountryCode = "+254")
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Remove all non-numeric characters
            string cleaned = Regex.Replace(phone, @"\D", "");

            if (cleaned.StartsWith("0") && cleaned.Length == 10)
            {
                return $"{defaultCountryCode}{cleaned[1..]}";
            }

            if (cleaned.StartsWith("254") && cleaned.Length == 12)
            {
                return $"+{cleaned}";
            }

            return phone.Trim();
        }

        /// <summary>
        /// Sanitizes text inputs by stripping unwanted characters/extra spaces
        /// </summary>
        public static string SanitizeText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Replaces multiple whitespace characters with a single space
            return Regex.Replace(input.Trim(), @"\s+", " ");
        }
    }
}