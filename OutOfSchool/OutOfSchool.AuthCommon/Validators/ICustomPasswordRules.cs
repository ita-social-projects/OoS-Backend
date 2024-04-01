namespace OutOfSchool.AuthCommon.Validators;
public interface ICustomPasswordRules
{
    bool IsValidPassword(string? password);
}
