using OutOfSchool.Common.Responces;

namespace OutOfSchool.WebApi.Services.ApiErrors;

public class ApiErrorService : IApiErrorService
{
    public ApiErrorService()
    {
        ApiErrorResponse = new ApiErrorResponse();
    }

    public ApiErrorResponse ApiErrorResponse { get; }

    public void AddApiError(ApiError apiError)
    {
        ApiErrorResponse.AddApiError(apiError);
    }
}
