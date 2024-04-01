using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace OutOfSchool.AuthCommon.Validators;
public class CustomPasswordValidationAttribute : ValidationAttribute
{
    private readonly ICustomPasswordRules passwordRules;

    public CustomPasswordValidationAttribute()
    {
        passwordRules = new CustomPasswordRules();
    }

    public CustomPasswordValidationAttribute(ICustomPasswordRules passwordRules)
    {
        this.passwordRules = passwordRules;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        var password = value as string;
        if (!passwordRules.IsValidPassword(password))
        {
            var errorMessage = GetLocalizedErrorMessage(
                Constants.PasswordValidationErrorMessage,
                validationContext);
            return new ValidationResult(errorMessage);
        }

        return ValidationResult.Success;
    }

    private static string GetLocalizedErrorMessage(string errorName, ValidationContext validationContext)
    {
        var localizer = (IStringLocalizer<SharedResource>?)validationContext
            .GetService(typeof(IStringLocalizer<SharedResource>));
        var errorMessage = localizer?.GetString(errorName);
        return errorMessage ?? errorName;
    }
}
