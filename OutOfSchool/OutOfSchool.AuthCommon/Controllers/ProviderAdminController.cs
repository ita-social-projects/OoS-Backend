using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class ProviderAdminController : Controller
{
    private readonly ILogger<ProviderAdminController> logger;
    private readonly IProviderAdminService providerAdminService;

    private string userId;

    public ProviderAdminController(
        ILogger<ProviderAdminController> logger,
        IProviderAdminService providerAdminService)
    {
        this.logger = logger;
        this.providerAdminService = providerAdminService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HttpPost]
    [HasPermission(Permissions.ProviderAdmins)]
    public async Task<ResponseDto> Create(CreateProviderAdminDto providerAdminDto)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await providerAdminService
            .CreateProviderAdminAsync(providerAdminDto, Url, userId);
    }

    [HttpPut("{providerAdminId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Update(string providerAdminId, UpdateProviderAdminDto providerAdminUpdateDto)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            userId);

        return await providerAdminService
            .UpdateProviderAdminAsync(providerAdminUpdateDto, userId);
    }

    [HttpDelete("{providerAdminId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Delete(string providerAdminId)
    {
        if (providerAdminId is null)
        {
            throw new ArgumentNullException(nameof(providerAdminId));
        }

        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await providerAdminService
            .DeleteProviderAdminAsync(providerAdminId, userId);
    }

    [HttpPut("{providerAdminId}/{isBlocked}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Block(string providerAdminId, bool isBlocked)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await providerAdminService
            .BlockProviderAdminAsync(providerAdminId, userId, isBlocked);
    }

    [HttpPut("{providerId}/{isBlocked}")]
    [HasPermission(Permissions.ProviderBlock)]
    public async Task<ResponseDto> BlockByProvider(Guid providerId, bool isBlocked)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await providerAdminService
            .BlockProviderAdminsAndDeputiesByProviderAsync(providerId, userId, isBlocked);
    }

    [HttpPut("{providerAdminId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Reinvite(string providerAdminId)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await providerAdminService
            .ReinviteProviderAdminAsync(providerAdminId, userId, Url);
    }
}