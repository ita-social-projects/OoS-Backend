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
        string[] allowedCharSets =
        [
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",
            "abcdefghijkmnopqrstuvwxyz",
            "0123456789",
            Constants.ValidationSymbols,
        ];

        List<char> password = [];

        while (password.Count < Constants.PasswordMinLength - allowedCharSets.Length)
        {
            var randomCharSetIndex = GetRandomInt32(allowedCharSets.Length);
            var randomChar =
                allowedCharSets[randomCharSetIndex][
                GetRandomInt32(allowedCharSets[randomCharSetIndex].Length)];
            password.Add(randomChar);
        }

        foreach (var charSet in allowedCharSets)
        {
            password.Insert(
                GetRandomInt32(password.Count),
                charSet[GetRandomInt32(charSet.Length)]);
        }

        return new string(password.ToArray());
    }

    private static int GetRandomInt32(int toExclusive)
    {
        return toExclusive == 0 ? 0 : RandomNumberGenerator.GetInt32(toExclusive);
    }
}