namespace OutOfSchool.Common.Extensions;

/// <summary>
/// Extention methods for the <see cref="char"/> type.
/// </summary>
public static class CharExtensions
{
    /// <summary>
    /// Checks if <paramref name="c"/> is allowed in Ukrainian name.
    /// </summary>
    /// <param name="c"> <see cref="char"/> to check.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is allowed in Ukrainian name, otherwise <see langword="false"/>.</returns>
    public static bool IsUkrainianNameChar(this char c)
    {
        return char.IsDigit(c) || IsUkrainianChar(c) || IsSpecialNameChar(c);

        static bool IsUkrainianLowercaseChar(char c) => c is ((>= 'а' and <= 'я') or 'ґ' or 'є' or 'і' or 'ї') and not ('ы' or 'ъ' or 'э' or 'ё');

        static bool IsUkrainianUppercaseChar(char c) => c is ((>= 'А' and <= 'Я') or 'Ґ' or 'Є' or 'І' or 'Ї') and not ('Ы' or 'Ъ' or 'Э' or 'Ё');

        static bool IsUkrainianChar(char c) => IsUkrainianLowercaseChar(c) || IsUkrainianUppercaseChar(c);

        static bool IsSpecialNameChar(char c) => c is '\'' or '-' or ' ';
    }
}
