using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(AuthenticationSchemes = Constants.BearerScheme)]
    public class MinistryAdminController : Controller
    {
        private readonly ILogger<MinistryAdminController> logger;
        private readonly IMinistryAdminService ministryAdminService;

        private string path;
        private string userId;

        public MinistryAdminController(
            ILogger<MinistryAdminController> logger,
            IMinistryAdminService ministryAdminService)
        {
            this.logger = logger;
            this.ministryAdminService = ministryAdminService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
            userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
        }

        [HttpPost]
        [HasPermission(Permissions.MinistryAdminAddNew)]
        public async Task<ResponseDto> Create(CreateMinistryAdminDto ministryAdminDto)
        {
            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await ministryAdminService
                .CreateMinistryAdminAsync(ministryAdminDto, Url, userId, Request.Headers["X-Request-ID"]);
        }

        [HttpDelete("{ministryAdminId}")]
        [HasPermission(Permissions.ProviderRemove)]
        public async Task<ResponseDto> Delete(string ministryAdminId)
        {
            if (ministryAdminId is null)
            {
                throw new ArgumentNullException(nameof(ministryAdminId));
            }

            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await ministryAdminService
                .DeleteMinistryAdminAsync(ministryAdminId, userId, Request.Headers["X-Request-ID"]);
        }

        [HttpPut("{ministryAdminId}")]
        [HasPermission(Permissions.ProviderRemove)]
        public async Task<ResponseDto> Block(string ministryAdminId)
        {
            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await ministryAdminService
                .BlockMinistryAdminAsync(ministryAdminId, userId, Request.Headers["X-Request-ID"]);
        }
    }
}