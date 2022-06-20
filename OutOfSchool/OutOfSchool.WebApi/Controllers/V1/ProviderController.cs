using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService providerService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<ProviderController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderController"/> class.
        /// </summary>
        /// <param name="providerService">Service for Provider model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
        public ProviderController(
            IProviderService providerService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<ProviderController> logger)
        {
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all Provider from the database.
        /// </summary>
        /// <returns>List of all Providers.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProviderDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var providers = await providerService.GetAll().ConfigureAwait(false);

            if (!providers.Any())
            {
                return NoContent();
            }

            return Ok(providers);
        }

        /// <summary>
        /// Get Provider by it's Id.
        /// </summary>
        /// <param name="providerId">Provider's id.</param>
        /// <returns>Provider.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{providerId:Guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid providerId)
        {
            var provider = await providerService.GetById(providerId).ConfigureAwait(false);
            if (provider == null)
            {
                return NotFound($"There is no Provider in DB with {nameof(provider.Id)} - {providerId}");
            }

            return Ok(provider);
        }

        /// <summary>
        /// To Get the Profile of authorized Provider.
        /// </summary>
        /// <returns>Authorized provider's profile.</returns>
        [HasPermission(Permissions.ProviderRead)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile()
        {
            // TODO: localize messages from the conrollers.
            var userId = GettingUserProperties.GetUserId(User);
            var isDeputyOrAdmin = !string.IsNullOrEmpty(GettingUserProperties.GetUserSubrole(User)) &&
                GettingUserProperties.GetUserSubrole(User) != "None";
            if (userId == null)
            {
                BadRequest("Invalid user information.");
            }

            var provider = await providerService.GetByUserId(userId, isDeputyOrAdmin).ConfigureAwait(false);
            if (provider == null)
            {
                return NoContent();
            }

            return Ok(provider);
        }

        /// <summary>
        /// Method for creating new Provider.
        /// </summary>
        /// <param name="providerModel">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HasPermission(Permissions.ProviderAddNew)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost]
        public async Task<IActionResult> Create(ProviderDto providerModel)
        {
            if (providerModel == null)
            {
                throw new ArgumentNullException(nameof(providerModel));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            providerModel.Id = default;
            providerModel.LegalAddress.Id = default;

            if (providerModel.ActualAddress != null)
            {
                providerModel.ActualAddress.Id = default;
            }

            // TODO: find out if we need this field in the model
            providerModel.UserId = GettingUserProperties.GetUserId(User);

            try
            {
                var createdProvider = await providerService.Create(providerModel).ConfigureAwait(false);

                return CreatedAtAction(
                    nameof(GetById),
                    new { providerId = createdProvider.Id, },
                    createdProvider);
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = $"Unable to create a new provider: {ex.Message}";
                logger.LogError(ex, errorMessage);

                // TODO: think about filtering of exception message.
                return BadRequest(errorMessage);
            }
        }

        /// <summary>
        /// Update info about the Provider.
        /// </summary>
        /// <param name="providerModel">Entity to update.</param>
        /// <returns>Updated Provider.</returns>
        [HasPermission(Permissions.ProviderEdit)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(ProviderDto providerModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GettingUserProperties.GetUserId(User);
                var provider = await providerService.Update(providerModel, userId).ConfigureAwait(false);

                if (provider == null)
                {
                    return BadRequest("Can't change Provider with such parameters.\n" +
                        "Please check that information are valid.");
                }

                return Ok(provider);
            }
            catch (DbUpdateConcurrencyException e)
            {
                return BadRequest(e);
            }
        }

        /// <summary>
        /// Delete a specific Provider from the database.
        /// </summary>
        /// <param name="uid">Provider's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HasPermission(Permissions.ProviderRemove)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{uid:guid}")]
        public async Task<IActionResult> Delete(Guid uid)
        {
            try
            {
                await providerService.Delete(uid).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Update Provider status.
        /// </summary>
        /// <param name="request">Provider ID and status to update.</param>
        /// <returns><see cref="ProviderStatusDto"/>.</returns>
        [HttpPut]
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderStatusDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StatusUpdate([FromQuery] ProviderStatusDto request)
        {
            var result = await providerService.UpdateStatus(request, GettingUserProperties.GetUserId(User))
                .ConfigureAwait(false);

            if (result is null)
            {
                return NotFound($"There is no Provider in DB with Id - {request.ProviderId}");
            }

            return Ok(result);
        }

        /// <summary>
        /// Update Provider license status.
        /// </summary>
        /// <param name="request">Provider ID and license status to update.</param>
        /// <returns><see cref="ProviderLicenseStatusDto"/>.</returns>
        [HttpPut]
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderLicenseStatusDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LicenseStatusUpdate([FromQuery] ProviderLicenseStatusDto request)
        {
            try
            {
                var result = await providerService.UpdateLicenseStatus(request, GettingUserProperties.GetUserId(User))
                    .ConfigureAwait(false);

                if (result is null)
                {
                    return NotFound($"There is no Provider in DB with Id - {request.ProviderId}");
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}