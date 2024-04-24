using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomPasswordValidationAttribute : ValidationAttribute
{
    public CustomPasswordValidationAttribute()
    {
        ErrorMessage ??= Constants.PasswordValidationErrorMessage;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        var passwordRules = (ICustomPasswordRules?)validationContext.GetService(typeof(ICustomPasswordRules)) 
                            ?? throw new ArgumentNullException(typeof(ICustomPasswordRules).Name);

        var password = value as string;
        if (!passwordRules.IsValidPassword(password))
        {
            return new ValidationResult(ErrorMessageString);
        }

        return ValidationResult.Success;
    }
}