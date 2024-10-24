using System.Threading.Tasks;
using OutOfSchool.Common.Models;

namespace OutOfSchool.Common.Communication.ICommunication;

public interface ICommunicationService
{
    Task<Either<ErrorResponse, T>> SendRequest<T>(Request request)
        where T : IResponse;
}