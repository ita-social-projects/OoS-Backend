using OutOfSchool.Common.Responces;

namespace OutOfSchool.WebApi.Services.ApiErrors;

public class ApiErrorService : IApiErrorService
{
    public ApiErrorResponse ApiErrorResponse { get; }

    public void AddApiError(ApiError apiError)
    {
        ApiErrorResponse.AddApiError(apiError);
    }
}
