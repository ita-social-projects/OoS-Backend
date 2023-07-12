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

    private string path;
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

        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HasPermission(Permissions.MinistryAdminAddNew)]
    [HttpPost]
    public async Task<ResponseDto> Create(MinistryAdminBaseDto ministryAdminBaseDto)
    {
        logger.LogDebug(
            "Received request {RequestHeader}. {Path} started. User(id): {UserId}",
            Request.Headers["X-Request-ID"], path, userId);

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
            .CreateMinistryAdminAsync(ministryAdminBaseDto, Role.MinistryAdmin, Url, userId, Request.Headers["X-Request-ID"]);
    }

    [HttpPut("{ministryAdminId}")]
    [HasPermission(Permissions.MinistryAdminEdit)]
    public async Task<ResponseDto> Update(string ministryAdminId, MinistryAdminBaseDto updateMinistryAdminDto)
    {
        logger.LogDebug(
            "Received request " +
            "{Headers}. {path} started. User(id): {userId}",
            Request.Headers["X-Request-ID"],
            path,
            userId);

        return await ministryAdminService
            .UpdateMinistryAdminAsync(updateMinistryAdminDto, userId, Request.Headers["X-Request-ID"]);
    }

    [HttpDelete("{ministryAdminId}")]
    [HasPermission(Permissions.MinistryAdminRemove)]
    public async Task<ResponseDto> Delete(string ministryAdminId)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminId);

        logger.LogDebug(
            "Received request {RequestHeader}. {Path} started. User(id): {UserId}",
            Request.Headers["X-Request-ID"], path, userId);

        return await ministryAdminService
            .DeleteMinistryAdminAsync(ministryAdminId, userId, Request.Headers["X-Request-ID"]);
    }

    [HttpPut("{ministryAdminId}/{isBlocked}")]
    [HasPermission(Permissions.MinistryAdminEdit)]
    public async Task<ResponseDto> Block(string ministryAdminId, bool isBlocked)
    {
        logger.LogDebug(
            "Received request {RequestHeader}. {Path} started. User(id): {UserId}",
            Request.Headers["X-Request-ID"], path, userId);

        return await ministryAdminService
            .BlockMinistryAdminAsync(ministryAdminId, userId, Request.Headers["X-Request-ID"], isBlocked);
    }

    [HttpPut("{ministryAdminId}")]
    [HasPermission(Permissions.MinistryAdminEdit)]
    public async Task<ResponseDto> Reinvite(string ministryAdminId)
    {
        logger.LogDebug($"Received request " +
                        $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

        return await ministryAdminService
            .ReinviteMinistryAdminAsync(ministryAdminId, userId, Url, Request.Headers["X-Request-ID"]);
    }
}