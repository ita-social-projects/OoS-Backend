using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.IdentityServer.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(AuthenticationSchemes = Constants.BearerScheme)]
public class RegionAdminController : Controller
{
    private readonly ILogger<RegionAdminController> logger;
    private readonly ICommonMinistryAdminService<RegionAdminBaseDto> regionAdminService;

    private string path;
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

        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        currentUserId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HasPermission(Permissions.RegionAdminAddNew)]
    [HttpPost]
    public async Task<ResponseDto> Create(RegionAdminBaseDto regionAdminBaseDto)
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

        return await regionAdminService
            .CreateMinistryAdminAsync(regionAdminBaseDto, Role.RegionAdmin, Url, currentUserId, Request.Headers["X-Request-ID"]);
    }

    [HttpPut("{regionAdminId}")]
    [HasPermission(Permissions.RegionAdminEdit)]
    public async Task<ResponseDto> Update(string regionAdminId, RegionAdminBaseDto updateRegionAdminDto)
    {
        logger.LogDebug(
            "Received request {Headers}. {path} started. User(id): {userId}",
            Request.Headers["X-Request-ID"],
            path,
            currentUserId);

        return await regionAdminService
            .UpdateMinistryAdminAsync(updateRegionAdminDto, currentUserId, Request.Headers["X-Request-ID"]);
    }

    [HttpDelete("{regionAdminId}")]
    [HasPermission(Permissions.RegionAdminRemove)]
    public async Task<ResponseDto> Delete(string regionAdminId)
    {
        _ = regionAdminId ?? throw new ArgumentNullException(nameof(regionAdminId));

        logger.LogDebug(
            "Received request {RequestHeader}. {Path} started. User(id): {UserId}",
            Request.Headers["X-Request-ID"],
            path,
            currentUserId);

        return await regionAdminService
            .DeleteMinistryAdminAsync(regionAdminId, currentUserId, Request.Headers["X-Request-ID"]);
    }

    [HttpPut("{regionAdminId}/{isBlocked}")]
    [HasPermission(Permissions.RegionAdminEdit)]
    public async Task<ResponseDto> Block(string regionAdminId, bool isBlocked)
    {
        logger.LogDebug(
            "Received request {RequestHeader}. {Path} started. User(id): {UserId}",
            Request.Headers["X-Request-ID"],
            path,
            currentUserId);

        return await regionAdminService
            .BlockMinistryAdminAsync(regionAdminId, currentUserId, Request.Headers["X-Request-ID"], isBlocked);
    }
}