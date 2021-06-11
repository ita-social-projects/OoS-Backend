using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService providerService;    
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderController"/> class.
        /// </summary>
        /// <param name="providerService">Service for Provider model.</param>       
        /// <param name="localizer">Localizer.</param>
        public ProviderController(IProviderService providerService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.providerService = providerService;
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
        /// <param name="id">Provider's id.</param>
        /// <returns>Provider.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(long id)
        {
            this.ValidateId(id, localizer);

            return Ok(await providerService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// To Get the Profile of authorized Provider.
        /// </summary>
        /// <returns>Authorized provider's profile.</returns>
        [Authorize(Roles = "provider,admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile()
        {
            string userId = User.FindFirst("sub")?.Value;

            var providers = await providerService.GetAll().ConfigureAwait(false);

            var providerDTO = providers.FirstOrDefault(x => x.UserId == userId);

            return Ok(providerDTO);
        }

        /// <summary>
        /// Method for creating new Provider.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
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
        }

        /// <summary>
        /// Update info about the Provider.
        /// </summary>
        /// <param name="dto">Entity to update.</param>
        /// <returns>Updated Provider.</returns>
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
        [Authorize(Roles = "provider,admin")]        
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