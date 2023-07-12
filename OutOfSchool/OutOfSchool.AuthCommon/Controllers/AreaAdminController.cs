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

    private string path;
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

        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        currentUserId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HasPermission(Permissions.AreaAdminAddNew)]
    [HttpPost]
    public async Task<ResponseDto> Create(AreaAdminBaseDto areaAdminBaseDto)
    {
        logger.LogDebug(
            "Received request {RequestHeader}. {Path} started. User(id): {UserId}",
            Request.Headers["X-Request-ID"],
            path,
            currentUserId);

        if (!ModelState.IsValid)
        {
            logger.LogError($"Input data was not valid for User(id): {currentUserId}");

            return new ResponseDto()
            {
                HttpStatusCode = HttpStatusCode.UnprocessableEntity,
                IsSuccess = false,
            };
        }

        return await areaAdminService
            .CreateMinistryAdminAsync(areaAdminBaseDto, Role.AreaAdmin, Url, currentUserId, Request.Headers["X-Request-ID"]);
    }

    [HttpPut("{areaAdminId}")]
    [HasPermission(Permissions.AreaAdminEdit)]
    public async Task<ResponseDto> Update(string areaAdminId, AreaAdminBaseDto updateAreaAdminDto)
    {
        logger.LogDebug(
            "Received request {Headers}. {path} started. User(id): {userId}",
            Request.Headers["X-Request-ID"],
            path,
            currentUserId);

        return await areaAdminService
            .UpdateMinistryAdminAsync(updateAreaAdminDto, currentUserId, Request.Headers["X-Request-ID"]);
    }

    [HttpDelete("{areaAdminId}")]
    [HasPermission(Permissions.AreaAdminRemove)]
    public async Task<ResponseDto> Delete(string areaAdminId)
    {
        _ = areaAdminId ?? throw new ArgumentNullException(nameof(areaAdminId));

        logger.LogDebug(
            "Received request {RequestHeader}. {Path} started. User(id): {UserId}",
            Request.Headers["X-Request-ID"],
            path,
            currentUserId);

        return await areaAdminService
            .DeleteMinistryAdminAsync(areaAdminId, currentUserId, Request.Headers["X-Request-ID"]);
    }

    [HttpPut("{areaAdminId}/{isBlocked}")]
    [HasPermission(Permissions.AreaAdminEdit)]
    public async Task<ResponseDto> Block(string areaAdminId, bool isBlocked)
    {
        logger.LogDebug(
            "Received request {RequestHeader}. {Path} started. User(id): {UserId}",
            Request.Headers["X-Request-ID"],
            path,
            currentUserId);

        return await areaAdminService
            .BlockMinistryAdminAsync(areaAdminId, currentUserId, Request.Headers["X-Request-ID"], isBlocked);
    }

    [HttpPut("{areaAdminId}")]
    [HasPermission(Permissions.AreaAdminEdit)]
    public async Task<ResponseDto> Reinvite(string areaAdminId)
    {
        logger.LogDebug($"Received request " +
                        $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {currentUserId}");
        return await areaAdminService
            .ReinviteMinistryAdminAsync(areaAdminId, currentUserId, Url, Request.Headers["X-Request-ID"]);
    }
}