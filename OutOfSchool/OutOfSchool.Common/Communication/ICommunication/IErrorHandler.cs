#nullable enable

using System.Threading.Tasks;

namespace OutOfSchool.Common.Communication.ICommunication;

public interface IErrorHandler<TError>
{
    Task<TError> HandleErrorAsync(CommunicationError errorResponse, string? message = null);
}