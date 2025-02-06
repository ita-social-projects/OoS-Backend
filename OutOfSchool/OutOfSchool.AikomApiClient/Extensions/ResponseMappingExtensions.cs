using System.Net;
using OutOfSchool.AikomApiClient.Models.Contract;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.AikomApiClient.Extensions;

public static class ResponseMappingExtensions
{
    public static Either<ErrorResponse, TDto> ToResponseDto<TData, TDto>(
        this ApiResponse<TData>? response,
        Func<TData, TDto> mapResult)
        where TData : class
        where TDto : class
    {
        if (response?.ResultVariables.Response.Error != null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = response.ResultVariables.Response.Error.Message,
                ApiErrorResponse = new ApiErrorResponse([
                    new ApiError("Aikom", response.ResultVariables.Response.Error.Code.ToString(),
                        response.ResultVariables.Response.Error.Message)
                ]),
            };
        }

        var data = response?.ResultVariables.Response.Data;

        if (data is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "Aikom API returned an empty result",
            };
        }

        return mapResult(data);
    }
}