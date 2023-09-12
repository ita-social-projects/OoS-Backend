using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Controllers;
[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class AreaAdminController : Controller
{
    private readonly ILogger<AreaAdminController> logger;
    private readonly ICommonMinistryAdminService<AreaAdminBaseDto> areaAdminService;

    private string currentUserId;

    public AreaAdminController(
        ILogger<AreaAdminController> logger,
        ICommonMinistryAdminService<AreaAdminBaseDto> areaAdminService)
    {
        this.logger = logger;
        this.areaAdminService = areaAdminService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        currentUserId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HasPermission(Permissions.AreaAdminAddNew)]
    [HttpPost]
    public async Task<ResponseDto> Create(AreaAdminBaseDto areaAdminBaseDto)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            currentUserId);

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid for User(id): {CurrentUserId}", currentUserId);

            return new ResponseDto()
            {
                HttpStatusCode = HttpStatusCode.UnprocessableEntity,
                IsSuccess = false,
            };
        }

        return await areaAdminService
            .CreateMinistryAdminAsync(areaAdminBaseDto, Role.AreaAdmin, Url, currentUserId);
    }

    [HttpPut("{areaAdminId}")]
    [HasPermission(Permissions.AreaAdminEdit)]
    public async Task<ResponseDto> Update(string areaAdminId, AreaAdminBaseDto updateAreaAdminDto)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            currentUserId);

        return await areaAdminService
            .UpdateMinistryAdminAsync(updateAreaAdminDto, currentUserId);
    }

    [HttpDelete("{areaAdminId}")]
    [HasPermission(Permissions.AreaAdminRemove)]
    public async Task<ResponseDto> Delete(string areaAdminId)
    {
        _ = areaAdminId ?? throw new ArgumentNullException(nameof(areaAdminId));

        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            currentUserId);

        return await areaAdminService
            .DeleteMinistryAdminAsync(areaAdminId, currentUserId);
    }

    [HttpPut("{areaAdminId}/{isBlocked}")]
    [HasPermission(Permissions.AreaAdminEdit)]
    public async Task<ResponseDto> Block(string areaAdminId, bool isBlocked)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            currentUserId);

        return await areaAdminService
            .BlockMinistryAdminAsync(areaAdminId, currentUserId, isBlocked);
    }

    [HttpPut("{areaAdminId}")]
    [HasPermission(Permissions.AreaAdminEdit)]
    public async Task<ResponseDto> Reinvite(string areaAdminId)
    {
        logger.LogDebug("Operation initiated by User(id): {CurrentUserId}",  currentUserId);
        return await areaAdminService
            .ReinviteMinistryAdminAsync(areaAdminId, currentUserId, Url);
    }
}