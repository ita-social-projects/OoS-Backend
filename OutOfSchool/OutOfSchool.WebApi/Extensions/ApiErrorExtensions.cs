using Ardalis.SmartEnum;
using OutOfSchool.WebApi.Common.Responses;

namespace OutOfSchool.WebApi.Extensions;

public static class ApiErrorExtensions
{
    public static ApiErrorResponse CreateApiErrorResponse(this ApiError apiError)
    {
        ArgumentNullException.ThrowIfNull(apiError);

        return ApiErrorResponse.Create(apiError.GroupName, apiError.Code, apiError.Message);
    }
}