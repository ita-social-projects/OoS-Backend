using Microsoft.IdentityModel.Tokens;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.WebApi.Services;

public class ApiErrorService : IApiErrorService
{
    private readonly IEntityRepositorySoftDeleted<string, User> userRepository;
    private readonly ILogger<ApiErrorService> logger;

    public ApiErrorService(
        IEntityRepositorySoftDeleted<string, User> userRepository,
        ILogger<ApiErrorService> logger)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        this.userRepository = userRepository;
        this.logger = logger;
    }

    public async Task<ApiErrorResponse> AdminsCreatingIsBadRequestDataAttend(AdminBaseDto adminBaseDto, string entityName)
    {
        var badRequestApiErrorResponse = new ApiErrorResponse();
        if (await IsSuchEmailExisted(adminBaseDto.Email))
        {
            logger.LogDebug(
                $"{entityName} creating is not possible. Username {adminBaseDto.Email} is already taken");
            badRequestApiErrorResponse.AddApiError(
                ApiErrorsTypes.Common.EmailAlreadyTaken(entityName, adminBaseDto.Email));
        }

        if (await IsSuchPhoneNumberExisted(adminBaseDto.PhoneNumber))
        {
            logger.LogDebug(
                 $"{entityName} creating is not possible. PhoneNumber {adminBaseDto.PhoneNumber} is already taken");
            badRequestApiErrorResponse.AddApiError(
                ApiErrorsTypes.Common.PhoneNumberAlreadyTaken(entityName, adminBaseDto.PhoneNumber));
        }

        return badRequestApiErrorResponse;
    }

    private async Task<bool> IsSuchEmailExisted(string email)
    {
        var result = await userRepository.GetByFilter(x => x.Email == email);
        return !result.IsNullOrEmpty();
    }

    private async Task<bool> IsSuchPhoneNumberExisted(string phoneNumber)
    {
        var result = await userRepository.GetByFilter(x => x.PhoneNumber == phoneNumber);
        return !result.IsNullOrEmpty();
    }
}
