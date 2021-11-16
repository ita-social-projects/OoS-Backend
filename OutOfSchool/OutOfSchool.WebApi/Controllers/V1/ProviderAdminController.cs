using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProviderAdminController : Controller
    {
        private readonly IProviderAdminService providerAdminService;
        private readonly ILogger<ProviderAdminController> logger;
        private string path;
        private string userId;

        public ProviderAdminController(IProviderAdminService providerAdminService)
        {
            this.providerAdminService = providerAdminService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
            userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
        }

        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProviderAdminDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "provider,provideradmin")]
        [HttpPost]
        public async Task<ActionResult> Create(ProviderAdminDto providerAdmin)
        {
            logger.LogDebug($"{path} started. User(id): {userId}.");

            if (!ModelState.IsValid)
            {
                logger.LogError($"Input data was not valid for User(id): {userId}");

                return BadRequest("Input data was not valid");
            }

            var response = await providerAdminService.CreateProviderAdminAsync(
                    userId,
                    providerAdmin,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                        .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                logger.LogError($"Succesfully created.");

                return Ok((ProviderAdminDto)response.Result);
            }

            return BadRequest(response.Message);
        }
    }
}
