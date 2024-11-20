using OutOfSchool.AikomApiClient.Models;

namespace OutOfSchool.AikomApiClient.Extensions;

public static class ResponseMappingExtensions
{
    public static ResponseDto ToResponseDto<TData>(
        this ApiResponse<TData>? response,
        Exception? exception,
        Func<TData, object> mapResult)
        where TData : class
    {
        if (exception != null)
        {
            return new ResponseDto
            {
                IsSuccess = false,
                ErrorMessage = exception.Message,
            };
        }

        if (response?.ResultVariables.Response.Error != null)
        {
            return new ResponseDto
            {
                IsSuccess = false,
                ErrorMessage = response.ResultVariables.Response.Error.Message,
                ErrorCode = response.ResultVariables.Response.Error.Code,
            };
        }

        var data = response?.ResultVariables.Response.Data;

        if (data is null)
        {
            return new ResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Aikom API returned an empty result",
            };
        }

        return new ResponseDto
        {
            IsSuccess = true,
            Result = mapResult.Invoke(data),
        };
    }
}
