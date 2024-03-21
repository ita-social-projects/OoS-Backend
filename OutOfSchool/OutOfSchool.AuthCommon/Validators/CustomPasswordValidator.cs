using static OutOfSchool.AuthCommon.Validators.GeneratedRegexes;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomPasswordValidator(string passwordValidationErrorMessage, string passwordRequiredErrorMessage) : IPasswordValidator<User>
{
    public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string? password)
    {
        List<IdentityError> errors = [];
        if (string.IsNullOrEmpty(password))
        {
            errors.Add(new IdentityError()
            {
                Description = passwordRequiredErrorMessage,
            });
        }
        else if (!PasswordGeneratedRegex().IsMatch(password))
        {
            errors.Add(new IdentityError()
            {
                Description = passwordValidationErrorMessage,
            });
        }

        return Task.FromResult(errors.Count == 0 ?
            IdentityResult.Success : IdentityResult.Failed([.. errors]));
    }
}