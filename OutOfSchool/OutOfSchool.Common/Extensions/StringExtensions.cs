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
    }
}