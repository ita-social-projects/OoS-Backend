using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Extensions;

namespace OutOfSchool.Common.Validators;

/// <summary>
/// Validation attribute for Ukrainian name.
/// </summary>
public class CustomUkrainianNameAttribute : DataTypeAttribute
{
    /// <summary>
    /// Separator for multiple names.
    /// </summary>
    private const char MultipleNameSeparator = '-';

    /// <summary>
    /// Apostrophe symbol.
    /// </summary>
    private const char Apostrophe = '\'';

    /// <summary>
    /// Minimum name length.
    /// </summary>
    private const int MinimumNameLength = 2; // eg: "Ян" "Як" "Лі" etc, if we need to validate names like `0` it can be changed

    /// <summary>
    /// Maximum length of the most common names.
    /// </summary>
    private const int CommonNameLength = 10;

    public CustomUkrainianNameAttribute()
        : base(DataType.Text)
    {
    }

    /// <summary>
    /// Checks that the value is valid Ukrainian name.
    /// </summary>
    /// <remarks>
    /// Rules:
    /// <para>- Null values are valid, unless <see cref="RequiredAttribute"/> is used.</para>
    /// <para>- Empty or whitespace strings are valid, unless <see cref="MinLengthAttribute"/>, <see cref="LengthAttribute"/>, <see cref="StringLengthAttribute"/> or <see cref="RequiredAttribute"/> with <see cref="RequiredAttribute.AllowEmptyStrings"/> set to <c>true</c> is used.</para>
    /// <para>- Name must not start or end with apostrophe or '-'.</para>
    /// <para>- Multiple names are valid and must be separated by `-`.</para>
    /// <para>- Each subname must start with capital letter.</para>
    /// <para>- Each subname must contain at least two characters(eg: 'Ян' is valid name, but 'О' is not).</para>
    /// <para>- Each subname must not contain anything except ukrainian letters and apostrophe.</para>
    /// <para>- Each subname must not start or end with apostrophe.</para>
    /// <para>- Each subname must contain only one apostrophe.</para>
    /// <para>- Each subanem can contain two capital characters only if second capital character placed after apostrophe(eg: 'О'Коннор' is valid).</para>
    /// </remarks>
    /// <param name="value">Value to validate.</param>
    /// <returns> <c>true</c> if valid, otherwise <c>false</c>.</returns>
    public override bool IsValid(object value)
    {
        return value switch
        {
            null => true,
            not string => false,
            string str when string.IsNullOrWhiteSpace(str) => true,
            string { Length: <= CommonNameLength } str when IsValidSubName(str) => true,
            string str => IsValidSubNames(str),
        };

        static bool IsValidSubNames(ReadOnlySpan<char> span)
        {
            var currentSpan = span;
            var isFinished = false;

            while (!isFinished)
            {
                var index = currentSpan.IndexOf(MultipleNameSeparator);

                isFinished = index < 0;

                var toCheck = isFinished ? currentSpan : currentSpan[..index];

                if (!IsValidSubName(toCheck))
                {
                    return false;
                }

                currentSpan = isFinished ? [] : currentSpan[(index + 1)..];
            }

            return true;
        }

        static bool IsValidSubName(ReadOnlySpan<char> subName)
        {
            return subName switch
            {
                { Length: < MinimumNameLength } => false,
                [var firstCh, var lastCh] => firstCh.IsUkrainianCapitalChar() && lastCh.IsUkrainianNonCapitalChar(),
                [var firstCh, .. var otherChars] => firstCh.IsUkrainianCapitalChar() && IsValidOtherCharacters(otherChars),
            };

            static bool IsValidOtherCharacters(ReadOnlySpan<char> otherChars)
            {
                var apostropheIndex = otherChars.IndexOf(Apostrophe);

                if (apostropheIndex < 0)
                {
                    return otherChars.ContainsOnlyNonCapitalUkrainianCharacter();
                }

                if (apostropheIndex >= otherChars.Length - 1)
                {
                    return false;
                }

                var beforeApostrophe = otherChars[..apostropheIndex];

                if (!beforeApostrophe.ContainsOnlyNonCapitalUkrainianCharacter())
                {
                    return false;
                }

                var afterApostrophe = otherChars[(apostropheIndex + 1)..];
                var firstChAfter = afterApostrophe[0];
                var otherChAfter = afterApostrophe[1..];

                if (!firstChAfter.IsUkrainianChar() || !otherChAfter.ContainsOnlyNonCapitalUkrainianCharacter())
                {
                    return false;
                }

                return true;
            }
        }
    }
}
