using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
   
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService providerService;
        private readonly IAddressService addressService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderController"/> class.
        /// </summary>
        /// <param name="providerService">Service for Provider model.</param>
        /// <param name="addressService">Service for Address model.</param>
        /// <param name="localizer">Localizer.</param>
        public ProviderController(IProviderService providerService, IAddressService addressService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.providerService = providerService;
            this.addressService = addressService;
        }

        /// <summary>
        /// Get all Provider from the database.
        /// </summary>
        /// <returns>List of all Providers.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
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
        /// <param name="id">Provider's id.</param>
        /// <returns>Provider.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            return Ok(await providerService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Get Provider by User Id.
        /// </summary>
        /// <param name="id">User id.</param>
        /// <returns>Provider.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]      
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProviderByUserId(string id)
        {
            try
            {
                return Ok(await providerService.GetByUserId(id).ConfigureAwait(false));
            }
            catch (ArgumentException)
            {
                throw;
            }                   
        }

        /// <summary>
        /// Method for creating new Provider.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "provider,admin")]        
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(ProviderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                dto.Id = default;
                dto.LegalAddress.Id = default;

                dto.UserId = User.FindFirst("sub")?.Value;

                var legalAddress = await addressService.Create(dto.LegalAddress).ConfigureAwait(false);
                dto.LegalAddressId = legalAddress.Id;

                if (dto.ActualAddress == null)
                {
                    dto.ActualAddressId = legalAddress.Id;
                }
                else
                {
                    dto.ActualAddress.Id = default;
                    var actualAddress = await addressService.Create(dto.ActualAddress).ConfigureAwait(false);
                    dto.ActualAddressId = actualAddress.Id;
                }

                var provider = await providerService.Create(dto).ConfigureAwait(false);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = provider.Id, },
                    provider);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update info about the Provider.
        /// </summary>
        /// <param name="dto">Entity to update.</param>
        /// <returns>Updated Provider.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "provider,admin")]       
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(ProviderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await providerService.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Provider from the database.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "provider,admin")]        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            await providerService.Delete(id).ConfigureAwait(false);

            return NoContent();
        }
    }
}