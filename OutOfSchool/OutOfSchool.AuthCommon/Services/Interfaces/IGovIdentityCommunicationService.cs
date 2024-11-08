using OutOfSchool.AuthCommon.Models;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Services.Interfaces;

public interface IGovIdentityCommunicationService : ICommunicationService
{
    public Task<Either<IErrorResponse, UserInfoResponse>> GetUserInfo(string remoteUserId, string remoteToken);
}