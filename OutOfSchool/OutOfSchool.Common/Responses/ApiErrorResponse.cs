using System.Collections.Generic;

namespace OutOfSchool.Common.Responces;
public class ApiErrorResponse
{
    private List<ApiError> apiErrors;

    public ApiErrorResponse(List<ApiError> apiErrors)
    {
        this.apiErrors = new List<ApiError>(apiErrors);
    }

    public ApiErrorResponse()
    {
        this.apiErrors = new List<ApiError>();
    }

    public List<ApiError> ApiErrors => apiErrors;

    public void AddApiError(ApiError apiError)
    {
        apiErrors.Add(apiError);
    }
}
