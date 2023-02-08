using System;

namespace OutOfSchool.Common.Extensions;

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
        => maxLength < 0
            ? throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "Max length cannot be less than 0.")
            : value == null || value.Length <= maxLength || maxLength == 0
                ? value
                : value.Substring(0, maxLength);

    public static bool StartsWithInvariant(this string s, string stringForComparison)
        => s.StartsWith(stringForComparison, StringComparison.InvariantCulture);

    public static bool ContainsInvariant(this string s, string stringForComparison)
        => s.Contains(stringForComparison, StringComparison.InvariantCulture);

    public static string Right(this string value, int length)
    {
        value ??= string.Empty;
        return (value.Length > length)
            ? value[^length..]
            : value;
    }
}