using System.Threading.Tasks;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Services.Communication.ICommunication;

public interface ICommunicationService
{
    Task<Either<ErrorResponse, T>> SendRequest<T>(Request request)
        where T : IResponse;
}