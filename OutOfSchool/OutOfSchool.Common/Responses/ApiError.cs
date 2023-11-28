using System;

namespace OutOfSchool.Common.Responces;
public class ApiError
{
    public ApiError(string code, string message)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException(@"Code must be non-empty value", nameof(code));
        }

        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException(@"Message must be non-empty value", nameof(message));
        }

        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }
}
