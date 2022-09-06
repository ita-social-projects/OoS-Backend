namespace OutOfSchool.WebApi.Common.Responses;

public class UserApiError : ApiError
{
    public static readonly UserApiError InvalidUserInformation
        = new (nameof(InvalidUserInformation), "Invalid User Information");

    private UserApiError(string code, string message)
        : base(code, message)
    {
    }

    public override string GroupName => nameof(UserApiError);
}