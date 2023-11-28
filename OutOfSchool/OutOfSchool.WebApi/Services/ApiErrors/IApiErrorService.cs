using OutOfSchool.Common.Responces;

namespace OutOfSchool.WebApi.Services.ApiErrors;

public interface IApiErrorService
{
    ApiErrorResponse ApiErrorResponse { get; }

    void AddApiError(ApiError apiError);
}
