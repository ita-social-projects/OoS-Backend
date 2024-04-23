using System.Security.Cryptography;

namespace OutOfSchool.AuthCommon.Services.Password;

public static class PasswordGenerator
{
    /// <summary>
    /// Generates a Random Password.
    /// </summary>
    /// <returns>A random password.</returns>
    public static string GenerateRandomPassword()
    {
        return string.Create(Constants.PasswordMinLength, 0, static (span, count) =>
        {
            ReadOnlySpan<char> allAllowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789" + Constants.ValidationSymbols;
            ReadOnlySpan<char> separators = "Zz9&";
            int charSetLowerIndex = 0;
            foreach (char separator in separators)
            {
                int separatorIndex = allAllowedChars.IndexOf(separator);
                int charSetUpperIndex = separatorIndex == allAllowedChars.Length - 1
                    ? allAllowedChars.Length
                    : separatorIndex + 1;
                ReadOnlySpan<char> charSet = allAllowedChars[charSetLowerIndex..charSetUpperIndex];
                span[count++] = charSet[RandomNumberGenerator.GetInt32(charSet.Length)];
                charSetLowerIndex += charSetUpperIndex - charSetLowerIndex;
            }

            RandomNumberGenerator.GetItems(allAllowedChars, span[count..]);
            RandomNumberGenerator.Shuffle(span);
        });
    }
}