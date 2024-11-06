using System;
using System.Threading.Tasks;
using OutOfSchool.Common.Models;

namespace OutOfSchool.Common.Communication.ICommunication;

public interface ICommunicationService
{
    Task<Either<TError, T>> SendRequest<T, TError>(
        Request request,
        IErrorHandler<TError> errorHandler = null)
        where T : IResponse
        where TError : IErrorResponse;
}