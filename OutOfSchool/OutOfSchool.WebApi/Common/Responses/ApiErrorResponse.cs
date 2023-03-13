using Ardalis.SmartEnum;

namespace OutOfSchool.WebApi.Common.Responses;

public record ApiErrorResponse
{
    private ApiErrorResponse(string groupName, string code, string message)
    {
        GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message;
    }

    public static ApiErrorResponse Create(string groupName, string code, string message)
        => new (groupName, code, message);

    public string GroupName { get; init; }

    public string Code { get; init; }

    public string Message { get; init; }
}