using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using OutOfSchool.Common.Models;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    // [HasPermission(Permissions.InstitutionAdmins)] FIXME
    public class InstitutionAdminController : Controller
    {
        private readonly IInstitutionAdminService institutionAdminService;
        private readonly ILogger<InstitutionAdminController> logger;
        private string path;
        private string userId;

        public InstitutionAdminController(
            IInstitutionAdminService institutionAdminService,
            ILogger<InstitutionAdminController> logger)
        {
            this.institutionAdminService = institutionAdminService;
            this.logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
            userId = GettingUserProperties.GetUserId(User);
        }

        /// <summary>
        /// Method for creating new InstitutionAdmin.
        /// </summary>
        /// <param name="institutionAdmin">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateInstitutionAdminDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ActionResult> Create(CreateInstitutionAdminDto institutionAdmin)
        {
            logger.LogDebug($"{path} started. User(id): {userId}.");

            if (!ModelState.IsValid)
            {
                logger.LogError($"Input data was not valid for User(id): {userId}");

                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }

            var response = await institutionAdminService.CreateInstitutionAdminAsync(
                    userId,
                    institutionAdmin,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                        .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                CreateInstitutionAdminDto institutionAdminDto = (CreateInstitutionAdminDto)response.Result;

                logger.LogInformation($"Succesfully created institutionAdmin(id): {institutionAdminDto.UserId} by User(id): {userId}.");

                return Ok(institutionAdminDto);
            }

            return StatusCode((int)response.HttpStatusCode);
        }

        /// <summary>
        /// Method for deleting institutionAdmin.
        /// </summary>
        /// <param name="institutionAdminId">Entity's id to delete.</param>
        /// <param name="providerId">Provider's id for which operation perform.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete]
        public async Task<ActionResult> Delete(string institutionAdminId, Guid providerId)
        {
            logger.LogDebug($"{path} started. User(id): {userId}.");

            var response = await institutionAdminService.DeleteInstitutionAdminAsync(
                    institutionAdminId,
                    userId,
                    providerId,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                        .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                logger.LogInformation($"Succesfully deleted institutionAdmin(id): {institutionAdminId} by User(id): {userId}.");

                return Ok();
            }

            return StatusCode((int)response.HttpStatusCode);
        }

        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut]
        public async Task<ActionResult> Block(string institutionAdminId, Guid providerId)
        {
            logger.LogDebug($"{path} started. User(id): {userId}.");

            var response = await institutionAdminService.BlockInstitutionAdminAsync(
                    institutionAdminId,
                    userId,
                    providerId,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                        .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                logger.LogInformation($"Succesfully blocked institutionAdmin(id): {institutionAdminId} by User(id): {userId}.");

                return Ok();
            }

            return StatusCode((int)response.HttpStatusCode);
        }

        /// <summary>
        /// Method to Get filtered data about related institutionAdmins.
        /// </summary>
        /// <param name="deputyOnly">Returns only deputy provider admins.</param>
        /// <param name="assistantsOnly">Returns only assistants (workshop) provider admins.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InstitutionAdminDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HasPermission(Permissions.ProviderRead)]
        [HttpGet]
        public async Task<IActionResult> GetFilteredinstitutionAdminsAsync(bool deputyOnly, bool assistantsOnly)
        {
            var relatedAdmins = await institutionAdminService.GetRelatedInstitutionAdmins(userId).ConfigureAwait(false);

            IActionResult result = Ok(relatedAdmins);

            return result;
        }

        /// <summary>
        /// Method to Get data about related institutionAdmins.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InstitutionAdminDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetRelatedinstitutionAdmins()
        {
            var relatedAdmins = await institutionAdminService.GetRelatedInstitutionAdmins(userId).ConfigureAwait(false);

            if (!relatedAdmins.Any())
            {
                return NoContent();
            }

            IActionResult result = Ok(relatedAdmins);

            return result;
        }
    }
}
