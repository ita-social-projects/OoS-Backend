using System;

namespace OutOfSchool.Common.Extensions;

/// <summary>
/// Extention methods for the <see cref="ReadOnlySpan{T}"/> class.
/// </summary>
public static class ReadOnlySpanExtensions
{
    /// <summary>
    /// Checks if <paramref name="span"/> contains only Ukrainian non-capital characters.
    /// </summary>
    /// <param name="span"> <see cref="ReadOnlySpan{T}"/> to check.</param>
    /// <returns> <see langword="true"/> if <paramref name="span"/> contains only Ukrainian non-capital characters, otherwise <see langword="false"/>.</returns>
    public static bool ContainsOnlyNonCapitalUkrainianCharacter(this ReadOnlySpan<char> span)
    {
        foreach (var ch in span)
        {
            if (!ch.IsUkrainianNonCapitalChar())
            {
                return false;
            }
        }

        return true;
    }
}
