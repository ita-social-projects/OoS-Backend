#nullable enable

using System.Threading.Tasks;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;

namespace OutOfSchool.Common.Communication;

internal class DefaultErrorHandler : IErrorHandler<ErrorResponse>
{
    public Task<ErrorResponse> HandleErrorAsync(CommunicationError response, string? message)
    {
        var errorResponse = new ErrorResponse
        {
            HttpStatusCode = response.HttpStatusCode,
            Message = message ?? $"Request failed with status code {response.HttpStatusCode}",
        };

        return Task.FromResult(errorResponse);
    }
}