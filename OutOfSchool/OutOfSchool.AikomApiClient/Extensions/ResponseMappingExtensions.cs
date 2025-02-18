using System.Net;
using System.Text.Json;
using OutOfSchool.AikomApiClient.Models.Contract;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.AikomApiClient.Extensions;

internal static class ResponseMappingExtensions
{
    internal static Either<ErrorResponse, TData> ToResponseData<TData>(
        this ApiResponse? response)
        where TData : class
    {
        if (response?.ResultVariables.Response is null or "")
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "Aikom API returned an empty result"
            };
        }

        AikomResponse<TData>? innerResponse;
        try
        {
            innerResponse = JsonSerializerHelper.Deserialize<AikomResponse<TData>>(
                response.ResultVariables.Response);
        }
        catch (JsonException ex)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = $"Failed to parse Aikom API response: {ex.Message}"
            };
        }

        if (innerResponse?.Error is not null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = innerResponse.Error.Message,
                ApiErrorResponse = new ApiErrorResponse([
                    new ApiError("Aikom", innerResponse.Error.Code.ToString(), innerResponse.Error.Message)
                ])
            };
        }

        if (innerResponse?.Data is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "Aikom API returned an empty result"
            };
        }

        return innerResponse.Data;
    }
}