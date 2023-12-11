﻿using System;

namespace OutOfSchool.Common.Responces;
public class ApiError
{
    public ApiError(string group, string code, string message)
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

        Group = group;
        Code = code;
        Message = message;
    }

    public string Group { get; }

    public string Code { get; }

    public string Message { get; }
}
