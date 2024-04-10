using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services.Communication.ICommunication;

public interface ICommunicationService
{
    Task<Either<ErrorResponse, T>> SendRequest<T>(Request request)
        where T : IResponse;
}