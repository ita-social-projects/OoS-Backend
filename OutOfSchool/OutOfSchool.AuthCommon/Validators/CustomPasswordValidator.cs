using Microsoft.Extensions.Localization;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomPasswordValidator(IStringLocalizer<SharedResource> localizer, ICustomPasswordRules passwordRules) : IPasswordValidator<User>
{
    private readonly IStringLocalizer<SharedResource> localizer = localizer;
    private readonly ICustomPasswordRules passwordRules = passwordRules;

    public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string? password)
    {
        if (!passwordRules.IsValidPassword(password))
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