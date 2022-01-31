using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.IdentityServer.Services.Intefaces;

namespace OutOfSchool.IdentityServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
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
                .CreateProviderAdminAsync(providerAdminDto, Request, Url, path, userId);
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
                .DeleteProviderAdminAsync(providerAdminId, Request, path, userId);
        }

        [HttpPut("{providerAdminId}")]
        [HasPermission(Permissions.ProviderRemove)]
        public async Task<ResponseDto> Block(string providerAdminId)
        {
            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await providerAdminService
                .BlockProviderAdminAsync(providerAdminId, Request, path, userId);
        }
    }
}