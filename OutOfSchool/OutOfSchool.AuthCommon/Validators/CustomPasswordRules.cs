namespace OutOfSchool.AuthCommon.Validators;
public static class CustomPasswordRules
{
    public static bool IsValidPassword(string? password)
    {
        if (string.IsNullOrEmpty(password) ||
            password.Length < Constants.PasswordMinLength ||
            !password.ContainsCharacterType(char.IsUpper) ||
            !password.ContainsCharacterType(char.IsLower) ||
            !password.ContainsCharacterType(char.IsDigit) ||
            !password.ContainsAnySymbol(Constants.PasswordValidationSymbols))
        {
            return false;
        }

        return true;
    }
}
