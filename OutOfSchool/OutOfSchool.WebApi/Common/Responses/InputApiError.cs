namespace OutOfSchool.WebApi.Common.Responses;

public class InputApiError : ApiError
{
    public static readonly InputApiError InputDataIncorrect
        = new (nameof(InputDataIncorrect), "Provided data input is incorrect");

    private InputApiError(string code, string message)
        : base(code, message)
    {
    }

    public override string GroupName => nameof(InputApiError);
}