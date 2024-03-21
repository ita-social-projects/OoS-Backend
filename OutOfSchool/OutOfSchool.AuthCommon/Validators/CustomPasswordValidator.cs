using System.Text.RegularExpressions;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomPasswordValidator(string regexPattern, string passwordValidationErrorMessage, string passwordRequiredErrorMessage) : IPasswordValidator<User>
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
        else if (!Regex.IsMatch(password, regexPattern))
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
