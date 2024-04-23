using System.Security.Cryptography;

namespace OutOfSchool.AuthCommon.Services.Password;

public static class PasswordGenerator
{
    private const string Uppercase = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string AllPasswordChars =
        Uppercase + Lowercase + Digits + Constants.ValidationSymbols;

    private static readonly string[] AllowedCharSets =
    [
        Uppercase, Lowercase, Digits, Constants.ValidationSymbols,
    ];

    /// <summary>
    /// Generates a Random Password.
    /// </summary>
    /// <returns>A random password.</returns>
    public static string GenerateRandomPassword()
    {
        Span<char> password = stackalloc char[Constants.PasswordMinLength];
        int index = 8 - AllowedCharSets.Length;
        RandomNumberGenerator.GetItems(AllPasswordChars, password[..index]);
        foreach (var charSet in AllowedCharSets)
        {
            var rndIndex = RandomNumberGenerator.GetInt32(index);
            (password[rndIndex], password[index]) =
                (charSet[RandomNumberGenerator.GetInt32(charSet.Length)], password[rndIndex]);
            index++;
        }

        return new string(password);
    }
}