namespace OutOfSchool.Common.Extensions;

/// <summary>
/// Extention methods for the <see cref="char"/> type.
/// </summary>
public static class CharExtensions
{
    /// <summary>
    /// Checks if <paramref name="c"/> is Ukrainian capital char.
    /// </summary>
    /// <param name="c"> <see cref="char"/> to check.</param>
    /// <returns> <see langword="true"/> if <paramref name="c"/> is Ukrainian capital char, otherwise <see langword="false"/>.</returns>
    public static bool IsUkrainianCapitalChar(this char c)
    {
        return c is (not ('Ы' or 'Ъ') and >= 'А' and <= 'Я') or 'Ґ' or 'Є' or 'І' or 'Ї';
    }

    /// <summary>
    /// Checks if <paramref name="c"/> is Ukrainian non-capital char.
    /// </summary>
    /// <param name="c"> <see cref="char"/> to check.</param>
    /// <returns> <see langword="true"/> if <paramref name="c"/> is Ukrainian non-capital char, otherwise <see langword="false"/>.</returns>
    public static bool IsUkrainianNonCapitalChar(this char c)
    {
        return c is (not ('ы' or 'ъ') and >= 'а' and <= 'я') or 'ґ' or 'є' or 'і' or 'ї';
    }

    /// <summary>
    /// Checks if <paramref name="c"/> is Ukrainian char.
    /// </summary>
    /// <param name="c"> <see cref="char"/> to check.</param>
    /// <returns> <see langword="true"/> if <paramref name="c"/> is Ukrainian char, otherwise <see langword="false"/>.</returns>
    public static bool IsUkrainianChar(this char c)
    {
        return c is (not ('Ы' or 'ы' or 'Ъ' or 'ъ') and >= 'а' and <= 'я') or (>= 'А' and <= 'Я') or 'Ґ' or 'ґ' or 'Є' or 'є' or 'І' or 'і' or 'Ї' or 'ї';
    }
}
