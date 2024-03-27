using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class MinistryAdminController : Controller
{
    private readonly ILogger<MinistryAdminController> logger;
    private readonly ICommonMinistryAdminService<MinistryAdminBaseDto> ministryAdminService;

    private string userId;

    public MinistryAdminController(
        ILogger<MinistryAdminController> logger,
        ICommonMinistryAdminService<MinistryAdminBaseDto> ministryAdminService)
    {
        this.logger = logger;
        this.ministryAdminService = ministryAdminService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HasPermission(Permissions.MinistryAdminAddNew)]
    [HttpPost]
    public async Task<ResponseDto> Create(MinistryAdminBaseDto ministryAdminBaseDto)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}", userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid for User(id): {UserId}", userId);

            return new ResponseDto()
            {
                HttpStatusCode = HttpStatusCode.UnprocessableEntity,
                IsSuccess = false,
            };
        }

        return await ministryAdminService
            .CreateMinistryAdminAsync(ministryAdminBaseDto, Role.MinistryAdmin, Url, userId);
    }

    [HttpPut("{ministryAdminId}")]
    [HasPermission(Permissions.MinistryAdminEdit)]
    public async Task<ResponseDto> Update(string ministryAdminId, MinistryAdminBaseDto updateMinistryAdminDto)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await ministryAdminService
            .UpdateMinistryAdminAsync(updateMinistryAdminDto, userId);
    }

    [HttpDelete("{ministryAdminId}")]
    [HasPermission(Permissions.MinistryAdminRemove)]
    public async Task<ResponseDto> Delete(string ministryAdminId)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminId);

        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await ministryAdminService
            .DeleteMinistryAdminAsync(ministryAdminId, userId);
    }

    [HttpPut("{ministryAdminId}/{isBlocked}")]
    [HasPermission(Permissions.MinistryAdminEdit)]
    public async Task<ResponseDto> Block(string ministryAdminId, bool isBlocked)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await ministryAdminService
            .BlockMinistryAdminAsync(ministryAdminId, userId, isBlocked);
    }

    [HttpPut("{ministryAdminId}")]
    [HasPermission(Permissions.MinistryAdminEdit)]
    public async Task<ResponseDto> Reinvite(string ministryAdminId)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await ministryAdminService
            .ReinviteMinistryAdminAsync(ministryAdminId, userId, Url);
    }
}