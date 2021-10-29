using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{providerId:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(long providerId)
        {
            try
            {
                this.ValidateId(providerId, localizer);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.LogError(ex, $"Validation failed for provider ID: {providerId}");

                return BadRequest("Provider data is missing or invalid.");
            }

            var provider = await providerService.GetById(providerId).ConfigureAwait(false);

            if (provider == null)
            {
                return NoContent();
            }

            return Ok(provider);
        }

        /// <summary>
        /// To Get the Profile of authorized Provider.
        /// </summary>
        /// <returns>Authorized provider's profile.</returns>
        // [Authorize(Roles = "provider,admin")]
        [HasPermission(Permissions.ProviderRead)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile()
        {
            // TODO: localize messages from the conrollers.
            var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
            if (userId == null)
            {
                BadRequest("Invalid user information.");
            }

            var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);
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
        // [Authorize(Roles = "provider,admin")]
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
            providerModel.UserId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

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
        // [Authorize(Roles = "provider,admin")]
        [HasPermission(Permissions.ProviderEdit)]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

            var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var userRole = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role);

            var provider = await providerService.Update(providerModel, userId, userRole).ConfigureAwait(false);

            if (provider == null)
            {
                return BadRequest("Can't change Provider with such parameters.\n" +
                    "Please check that information are valid.");
            }

            return Ok(provider);
        }

        /// <summary>
        /// Delete a specific Provider from the database.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        // [Authorize(Roles = "provider,admin")]
        [HasPermission(Permissions.ProviderRemove)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            this.ValidateId(id, localizer);

            await providerService.Delete(id).ConfigureAwait(false);

            return NoContent();
        }
    }
}