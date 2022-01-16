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
        public async Task<ResponseDto> Create(CreateProviderAdminDto providerAdminDto)
        {
            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await providerAdminService
                .CreateProviderAdminAsync(providerAdminDto, Request, Url, path, userId);
        }

        [HttpDelete]
        [Authorize(Roles = "provider")]
        public async Task<ResponseDto> Delete(DeleteProviderAdminDto deleteProviderAdminDto)
        {
            if (deleteProviderAdminDto is null)
            {
                throw new ArgumentNullException(nameof(deleteProviderAdminDto));
            }

            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await providerAdminService
                .DeleteProviderAdminAsync(deleteProviderAdminDto, Request, path, userId);
        }

        [HttpPut]
        [Authorize(Roles = "provider")]
        public async Task<ResponseDto> Block(BlockProviderAdminDto blockProviderAdminDto)
        {
            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await providerAdminService
                .BlockProviderAdminAsync(blockProviderAdminDto, Request, path, userId);
        }
    }
}