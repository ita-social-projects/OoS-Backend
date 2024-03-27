﻿namespace OutOfSchool.AuthCommon.Validators;
public static class CustomPasswordRules
{
    public static bool IsValidPassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < Constants.PasswordMinLength)
        {
            return false;
        }

        var (hasUpperCase, hasLowerCase, hasDigit, hasSymbol) = (false, false, false, false);

        foreach (char c in password)
        {
            if (char.IsUpper(c))
            {
                hasUpperCase = true;
            }
            else if (char.IsLower(c))
            {
                hasLowerCase = true;
            }
            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }
            else if (Constants.ValidationSymbols.Contains(c))
            {
                hasSymbol = true;
            }
            else
            {
                return false;
            }
        }

        return hasUpperCase && hasLowerCase && hasDigit && hasSymbol;
    }
}
