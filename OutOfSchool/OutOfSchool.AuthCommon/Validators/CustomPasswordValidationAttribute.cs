using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace OutOfSchool.AuthCommon.Validators;
public class CustomPasswordValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;
        if (!CustomPasswordRules.IsValidPassword(password))
        {
            var localizer = (IStringLocalizer<SharedResource>?)validationContext
                .GetService(typeof(IStringLocalizer<SharedResource>));
            var errorMessage = localizer?.GetString(Constants.PasswordValidationErrorMessage)
                ?? Constants.PasswordValidationErrorMessage;
            return new ValidationResult(errorMessage);
        }

        return ValidationResult.Success;
    }
}
