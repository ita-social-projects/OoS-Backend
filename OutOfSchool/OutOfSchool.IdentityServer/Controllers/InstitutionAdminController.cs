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
    public class InstitutionAdminController : Controller
    {
        private readonly ILogger<InstitutionAdminController> logger;
        private readonly IInstitutionAdminService institutionAdminService;

        private string path;
        private string userId;

        public InstitutionAdminController(
            ILogger<InstitutionAdminController> logger,
            IInstitutionAdminService institutionAdminService)
        {
            this.logger = logger;
            this.institutionAdminService = institutionAdminService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
            userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
        }

        [HttpPost]
        // [HasPermission(Permissions.InstitutionAdmins)]
        public async Task<ResponseDto> Create(CreateInstitutionAdminDto InstitutionAdminDto)
        {
            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await institutionAdminService
                .CreateInstitutionAdminAsync(InstitutionAdminDto, Url, userId, Request.Headers["X-Request-ID"]);
        }

        [HttpDelete("{InstitutionAdminId}")]
        [HasPermission(Permissions.ProviderRemove)]
        public async Task<ResponseDto> Delete(string InstitutionAdminId)
        {
            if (InstitutionAdminId is null)
            {
                throw new ArgumentNullException(nameof(InstitutionAdminId));
            }

            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await institutionAdminService
                .DeleteInstitutionAdminAsync(InstitutionAdminId, userId, Request.Headers["X-Request-ID"]);
        }

        [HttpPut("{InstitutionAdminId}")]
        [HasPermission(Permissions.ProviderRemove)]
        public async Task<ResponseDto> Block(string InstitutionAdminId)
        {
            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

            return await institutionAdminService
                .BlockInstitutionAdminAsync(InstitutionAdminId, userId, Request.Headers["X-Request-ID"]);
        }
    }
}