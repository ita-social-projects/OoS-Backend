using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

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

        public ProviderAdminController(
            IProviderAdminService providerAdminService,
            ILogger<ProviderAdminController> logger)
        {
            this.providerAdminService = providerAdminService;
            this.logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
            userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
        }

        /// <summary>
        /// Method for creating new ProviderAdmin.
        /// </summary>
        /// <param name="providerAdmin">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProviderAdminDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "provider,provideradmin")]
        [HttpPost]
        public async Task<ActionResult> Create(ProviderAdminDto providerAdmin)
        {
            logger.LogDebug($"{path} started. User(id): {userId}.");

            if (!ModelState.IsValid)
            {
                logger.LogError($"Input data was not valid for User(id): {userId}");

                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }

            var response = await providerAdminService.CreateProviderAdminAsync(
                    userId,
                    providerAdmin,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                        .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                ProviderAdminDto providerAdminDto = (ProviderAdminDto)response.Result;

                logger.LogInformation($"Succesfully created ProviderAdmin(id): {providerAdminDto.UserId} by User(id): {userId}.");

                return Ok(providerAdminDto);
            }

            return StatusCode((int)response.HttpStatusCode);
        }

        /// <summary>
        /// Method for deleting ProviderAdmin.
        /// </summary>
        /// <param name="providerAdminId">Entity's id to delete.</param>
        /// <param name="providerId">Provider's id for which operation perform.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "provider")]
        [HttpDelete]
        public async Task<ActionResult> Delete(string providerAdminId, Guid providerId)
        {
            logger.LogDebug($"{path} started. User(id): {userId}.");

            var response = await providerAdminService.DeleteProviderAdminAsync(
                    providerAdminId,
                    userId,
                    providerId,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                        .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                ProviderAdminDto providerAdminDto = (ProviderAdminDto)response.Result;

                logger.LogInformation($"Succesfully deleted ProviderAdmin(id): {providerAdminId} by User(id): {userId}.");

                return Ok();
            }

            return StatusCode((int)response.HttpStatusCode);
        }
    }
}
