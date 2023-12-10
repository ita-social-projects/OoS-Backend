﻿using System.Collections.Generic;

namespace OutOfSchool.Common.Responces;
public class ApiErrorResponse
{
    private readonly List<ApiError> apiErrors;

    public ApiErrorResponse(List<ApiError> apiErrors)
    {
        this.apiErrors = new List<ApiError>(apiErrors);
    }

    public ApiErrorResponse()
    {
        this.apiErrors = new List<ApiError>();
    }

    public IReadOnlyList<ApiError> ApiErrors => apiErrors.AsReadOnly();

    public void AddApiError(ApiError apiError)
    {
        apiErrors.Add(apiError);
    }
}
