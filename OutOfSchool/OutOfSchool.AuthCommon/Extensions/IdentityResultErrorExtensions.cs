namespace OutOfSchool.AuthCommon.Extensions;

public static class IdentityResultErrorExtensions
{
    public static string ErrorMessages(this IdentityResult result) =>
        string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
}