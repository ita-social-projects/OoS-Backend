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

        CryptoRandom rand = new();

        List<char> password = [];

        foreach (var charSet in allowedCharSets)
        {
            password.Insert(
                rand.Next(0, password.Count),
                charSet[rand.Next(0, charSet.Length)]);
        }

        while (password.Count < Constants.PasswordMinLength)
        {
            var randomCharSetIndex = rand.Next(0, allowedCharSets.Length);
            var randomChar =
                allowedCharSets[randomCharSetIndex][
                rand.Next(0, allowedCharSets[randomCharSetIndex].Length)];

            if (!password.Contains(randomChar))
            {
                password.Add(randomChar);
            }
        }

        return new string(password.ToArray());
    }
}