using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;

namespace OutOfSchool.IdentityServer.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(AuthenticationSchemes = Constants.BearerScheme)]
public class ProviderAdminController : Controller
{
    private readonly ILogger<ProviderAdminController> logger;
    private readonly IProviderAdminService providerAdminService;

    private string path;
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
        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HttpPost]
    [HasPermission(Permissions.ProviderAdmins)]
    public async Task<ResponseDto> Create(CreateProviderAdminDto providerAdminDto)
    {
        logger.LogDebug($"Received request " +
                        $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

        return await providerAdminService
            .CreateProviderAdminAsync(providerAdminDto, Url, userId, Request.Headers["X-Request-ID"]);
    }

    [HttpPut("{providerAdminId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Update(string providerAdminId, UpdateProviderAdminDto providerAdminUpdateDto)
    {
        logger.LogDebug(
            "Received request " +
            "{Headers}. {path} started. User(id): {userId}",
            Request.Headers["X-Request-ID"],
            path,
            userId);

        return await providerAdminService
            .UpdateProviderAdminAsync(providerAdminUpdateDto, userId, Request.Headers["X-Request-ID"]);
    }

    [HttpDelete("{providerAdminId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Delete(string providerAdminId)
    {
        if (providerAdminId is null)
        {
            throw new ArgumentNullException(nameof(providerAdminId));
        }

        logger.LogDebug($"Received request " +
                        $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

        return await providerAdminService
            .DeleteProviderAdminAsync(providerAdminId, userId, Request.Headers["X-Request-ID"]);
    }

    [HttpPut("{providerAdminId}/{isBlocked}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Block(string providerAdminId, bool isBlocked)
    {
        logger.LogDebug($"Received request " +
                        $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

        return await providerAdminService
            .BlockProviderAdminAsync(providerAdminId, userId, Request.Headers["X-Request-ID"], isBlocked);
    }
}