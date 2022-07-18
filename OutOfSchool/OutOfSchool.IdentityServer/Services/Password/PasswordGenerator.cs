namespace OutOfSchool.IdentityServer.Services.Password;

public static class PasswordGenerator
{
    /// <summary>
    /// Generates a Random Password
    /// respecting the given strength requirements.
    /// </summary>
    /// <param name="passwordOptions">A valid PasswordOptions object
    /// containing the password strength requirements.</param>
    /// <returns>A random password</returns>
    public static string GenerateRandomPassword(PasswordOptions passwordOptions)
    {
        string[] randomChars = new[]
        {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-",                       // non-alphanumeric
        };

        CryptoRandom rand = new CryptoRandom();

        List<char> chars = new List<char>();

        if (passwordOptions.RequireUppercase)
        {
            chars.Insert(
                rand.Next(0, chars.Count),
                randomChars[0][rand.Next(0, randomChars[0].Length)]);
        }

        if (passwordOptions.RequireLowercase)
        {
            chars.Insert(
                rand.Next(0, chars.Count),
                randomChars[1][rand.Next(0, randomChars[1].Length)]);
        }

        if (passwordOptions.RequireDigit)
        {
            chars.Insert(
                rand.Next(0, chars.Count),
                randomChars[2][rand.Next(0, randomChars[2].Length)]);
        }

        if (passwordOptions.RequireNonAlphanumeric)
        {
            chars.Insert(
                rand.Next(0, chars.Count),
                randomChars[3][rand.Next(0, randomChars[3].Length)]);
        }

        for (int i = chars.Count; i < passwordOptions.RequiredLength
                                  || chars.Distinct().Count() < passwordOptions.RequiredUniqueChars; i++)
        {
            string rcs = randomChars[rand.Next(0, randomChars.Length)];
            chars.Insert(
                rand.Next(0, chars.Count),
                rcs[rand.Next(0, rcs.Length)]);
        }

        return new string(chars.ToArray());
    }
}