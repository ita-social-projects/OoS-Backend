using Microsoft.Extensions.Localization;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomPasswordValidator(IStringLocalizer<SharedResource> localizer) : IPasswordValidator<User>
{
    public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string? password)
    {
        if (!CustomPasswordRules.IsValidPassword(password))
        {
            var error = new IdentityError
            {
                Description = localizer[Constants.PasswordValidationErrorMessage],
            };
            return Task.FromResult(IdentityResult.Failed(error));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}