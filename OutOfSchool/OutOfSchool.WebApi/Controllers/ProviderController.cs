using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Services;
using Microsoft.AspNetCore.Http;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProviderController : ControllerBase
    {
        private readonly ILogger<ProviderController> logger;
        private readonly IProviderService providerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderController"/> class.
        /// </summary>
        /// <param name="logger">Logging mechanism</param>
        /// <param name="providerService">Service for Provider model.</param>
        public ProviderController(ILogger<ProviderController> logger, IProviderService providerService)
        {
            this.logger = logger;
            this.providerService = providerService;
        }

        /// <summary>
        /// Get all providers from the database.
        /// </summary>
        /// <returns>List of all providers.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Provider>>> GetProviders()
        {
            return Ok(await providerService.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// Get provider by it's key.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>Provider element with some id.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProviderDto>> GetProviderById(long id)
        {
            if (id == 0)
            { 
                return BadRequest("Id cannot be 0.");
            }

            return Ok(await providerService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Create new provider.
        /// </summary>
        /// <param name="providerDTO">Element which must be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "provider,admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Provider>> Create(ProviderDto providerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                providerDTO.UserId = User.FindFirst("sub")?.Value;
                ProviderDto provider = await providerService.Create(providerDTO).ConfigureAwait(false);
                return CreatedAtAction(
                    nameof(GetProviderById),
                    new
                    {
                        id = provider.Id,
                    }, provider);              
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update info about some provider in database.
        /// </summary>
        /// <param name="providerDTO">Entity.</param>
        /// <returns>Provider's key.</returns>
        [Authorize(Roles = "provider,admin")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Update(ProviderDto providerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await providerService.Update(providerDTO).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Provider entity from the database.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "provider,admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete(long id)
        {
            if (id == 0)
            {
                return BadRequest("Id cannot be 0.");
            }

            await providerService.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}