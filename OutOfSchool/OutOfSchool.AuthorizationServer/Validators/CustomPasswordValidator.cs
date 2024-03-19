using System.Text.RegularExpressions;

namespace OutOfSchool.AuthorizationServer.Validators;

public class CustomPasswordValidator(string regexPattern) : IPasswordValidator<User>
{
    public string RegexPattern { get; set; } = regexPattern;

    public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string? password)
    {
        List<IdentityError> errors = new();
        if (string.IsNullOrEmpty(password))
        {
            errors.Add(new IdentityError()
            {
                Description = $"Password is required",
            });
        }
        else if (!Regex.IsMatch(password, RegexPattern))
        {
            errors.Add(new IdentityError()
            {
                Description = $"Error! The password must be at least 8 characters long, including letters, digits, symbols and special characters (@$!%*?&)",
            });
        }

        return Task.FromResult(errors.Count == 0 ?
            IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
    }
}
