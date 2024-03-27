using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class RegionAdminController : Controller
{
    private readonly ILogger<RegionAdminController> logger;
    private readonly ICommonMinistryAdminService<RegionAdminBaseDto> regionAdminService;

    private string currentUserId;

    public RegionAdminController(
        ILogger<RegionAdminController> logger,
        ICommonMinistryAdminService<RegionAdminBaseDto> regionAdminService)
    {
        this.logger = logger;
        this.regionAdminService = regionAdminService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        currentUserId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HasPermission(Permissions.RegionAdminAddNew)]
    [HttpPost]
    public async Task<ResponseDto> Create(RegionAdminBaseDto regionAdminBaseDto)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", currentUserId);

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid for User(id): {UserId}", currentUserId);

            return new ResponseDto()
            {
                HttpStatusCode = HttpStatusCode.UnprocessableEntity,
                IsSuccess = false,
            };
        }

        return await regionAdminService
            .CreateMinistryAdminAsync(regionAdminBaseDto, Role.RegionAdmin, Url, currentUserId);
    }

    [HttpPut("{regionAdminId}")]
    [HasPermission(Permissions.RegionAdminEdit)]
    public async Task<ResponseDto> Update(string regionAdminId, RegionAdminBaseDto updateRegionAdminDto)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            currentUserId);

        return await regionAdminService
            .UpdateMinistryAdminAsync(updateRegionAdminDto, currentUserId);
    }

    [HttpDelete("{regionAdminId}")]
    [HasPermission(Permissions.RegionAdminRemove)]
    public async Task<ResponseDto> Delete(string regionAdminId)
    {
        _ = regionAdminId ?? throw new ArgumentNullException(nameof(regionAdminId));

        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            currentUserId);

        return await regionAdminService
            .DeleteMinistryAdminAsync(regionAdminId, currentUserId);
    }

    [HttpPut("{regionAdminId}/{isBlocked}")]
    [HasPermission(Permissions.RegionAdminEdit)]
    public async Task<ResponseDto> Block(string regionAdminId, bool isBlocked)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            currentUserId);

        return await regionAdminService
            .BlockMinistryAdminAsync(regionAdminId, currentUserId, isBlocked);
    }

    [HttpPut("{regionAdminId}")]
    [HasPermission(Permissions.RegionAdminEdit)]
    public async Task<ResponseDto> Reinvite(string regionAdminId)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", currentUserId);
        return await regionAdminService
            .ReinviteMinistryAdminAsync(regionAdminId, currentUserId, Url);
    }
}