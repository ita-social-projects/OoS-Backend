using System;

namespace OutOfSchool.Common.Responces;
public class ApiError
{
    public ApiError(string code, string message, string group)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException(@"Code must be non-empty value", nameof(code));
        }

        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException(@"Message must be non-empty value", nameof(message));
        }

        if (string.IsNullOrEmpty(group))
        {
            throw new ArgumentException(@"Group must be non-empty value", nameof(group));
        }

        Code = code;
        Message = message;
        Group = group;
    }

    public string Code { get; }

    public string Message { get; }

    public string Group { get; }
}
