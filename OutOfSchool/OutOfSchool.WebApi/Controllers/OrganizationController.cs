using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OrganizationController : ControllerBase
    {
        private readonly ILogger<OrganizationController> logger;
        private readonly IOrganizationService organizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationController"/> class.
        /// </summary>
        /// <param name="organizationService">Service for Organization model.</param>
        public OrganizationController(ILogger<OrganizationController> logger, IOrganizationService organizationService)
        {
            this.logger = logger;
            this.organizationService = organizationService;
        }

        public IActionResult TestOk()
        {
            var user = User?.FindFirst("role")?.Value;
            return Ok("Hello to " + user ?? "unknown");
        }

        /// <summary>
        /// Get all organization from the database.
        /// </summary>
        /// <returns>List of all organizations.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations()
        {
            return Ok(await organizationService.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// Get organization by it's key.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>Organization element with some id.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDTO>> GetOrganizationById(long id)
        {
            if (id < 1 || organizationService.GetAll().Result.AsQueryable().Count() < id)
            {
                throw new ArgumentOutOfRangeException(id.ToString(), "The id is less than 1 or greater than number of table entities.");
            }

            return Ok(await organizationService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Method for creating new organization.
        /// </summary>
        /// <param name="dto">Element which must be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ActionResult<Organization>> CreateOrganization(OrganizationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                dto.UserId = Convert.ToInt64(User.FindFirst("sub")?.Value);
                
                var organization = await organizationService.Create(dto).ConfigureAwait(false);
                
                return CreatedAtAction(
                    nameof(GetOrganizationById),
                    new
                    {
                        id = organization.Id,
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update info about some organization in database.
        /// </summary>
        /// <param name="dto">Entity.</param>
        /// <returns>Organization's key.</returns>
        [Authorize(Roles = "organization,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<ActionResult> UpdateOrganization(OrganizationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await organizationService.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Organization entity from the database.
        /// </summary>
        /// <param name="id">Organization's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrganization(long id)
        {
            if (id < 1 || organizationService.GetAll().Result.AsQueryable().Count() < id)
            {
                throw new ArgumentOutOfRangeException(id.ToString(),
                    "The id is less than 1 or greater than number of table entities.");
            }
            
            await organizationService.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}