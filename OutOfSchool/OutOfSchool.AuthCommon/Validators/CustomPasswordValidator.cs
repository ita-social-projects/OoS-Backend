using Microsoft.Extensions.Localization;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomPasswordValidator : IPasswordValidator<User>
{
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly ICustomPasswordRules passwordRules;

    public CustomPasswordValidator(IStringLocalizer<SharedResource> localizer)
    {
        this.localizer = localizer;
        passwordRules = new CustomPasswordRules();
    }

    public CustomPasswordValidator(IStringLocalizer<SharedResource> localizer, ICustomPasswordRules passwordRules)
    {
        this.localizer = localizer;
        this.passwordRules = passwordRules;
    }

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