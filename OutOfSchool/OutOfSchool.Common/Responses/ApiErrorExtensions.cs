namespace OutOfSchool.Common.Responses;
public static class ApiErrorExtensions
{
    public static ApiErrorResponse ToResponse(this ApiError apiError) =>
        new ApiErrorResponse().AddApiError(apiError);
}
