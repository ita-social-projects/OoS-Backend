using System;

namespace OutOfSchool.Common.Extensions;

/// <summary>
/// Extention methods for the <see cref="ReadOnlySpan{T}"/> struct.
/// </summary>
public static class ReadOnlySpanExtensions
{
    /// <summary>
    /// Checks if <paramref name="span"/> contains only characters that allowed in Ukrainian name.
    /// </summary>
    /// <param name="span"> <see cref="ReadOnlySpan{T}"/> to check.</param>
    /// <returns><see langword="true"/> if <paramref name="span"/> contains only characters that allowed in Ukrainian name, otherwise <see langword="false"/>.</returns>
    public static bool IsUkrainianName(this ReadOnlySpan<char> span)
    {
        foreach (var ch in span)
        {
            if (!ch.IsUkrainianNameChar())
            {
                return false;
            }
        }

        return true;
    }
}
