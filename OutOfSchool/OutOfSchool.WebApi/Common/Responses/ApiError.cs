using Ardalis.SmartEnum;

namespace OutOfSchool.WebApi.Common.Responses;

public abstract class ApiError
{
    protected ApiError(string code, string message)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException(@"Code must be non-empty value", nameof(code));
        }

        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }

    public abstract string GroupName { get; }
}