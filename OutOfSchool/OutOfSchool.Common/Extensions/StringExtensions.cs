using System;

namespace OutOfSchool.Common.Extensions
{
    public static class StringExtensions
    {
        public static TEnum ToEnum<TEnum>(this string value, TEnum defaultValue)
            where TEnum : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return Enum.TryParse<TEnum>(value, true, out var result) ? result : defaultValue;
        }

        public static string Limit(this string value, int maxLength)
            => value.Length <= maxLength || maxLength <= 0
            ? value
            : value.Substring(0, maxLength);
    }
}