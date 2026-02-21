#nullable enable

using System.Threading.Tasks;
using OutOfSchool.Common.Models;

namespace OutOfSchool.Common.Communication.ICommunication;

public interface IErrorHandler<TError>
where TError : IErrorResponse
{
    Task<TError> HandleErrorAsync(CommunicationError errorResponse, string? message = null);
}