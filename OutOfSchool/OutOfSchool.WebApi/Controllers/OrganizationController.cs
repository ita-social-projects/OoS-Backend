using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OrganizationController : ControllerBase
    {
        private readonly ILogger<OrganizationController> logger;
        private IOrganizationService organizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationController"/> class.
        /// </summary>
        /// <param name="organizationService">Service for Organization model.</param>
        public OrganizationController(ILogger<OrganizationController> logger, IOrganizationService organizationService)
        {
            this.logger = logger;
            this.organizationService = organizationService;
        }


        /// <summary>
        /// Get all organization from the database.
        /// </summary>
        /// <returns>List of all organizations.</returns>
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
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDTO>> GetOrganizationById(long id)
        {
            if (id == 0)
            { 
                return BadRequest("Id cannot be 0.");
            }

            return Ok(await organizationService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Method for creating new organization.
        /// </summary>
        /// <param name="organizationDTO">Element which must be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [HttpPost]
        public async Task<ActionResult<Organization>> CreateOrganization(OrganizationDTO organizationDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                organizationDTO.UserId = Convert.ToInt64(User.FindFirst("sub")?.Value);
                OrganizationDTO organization = await organizationService.Create(organizationDTO).ConfigureAwait(false);
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
        /// <param name="organizationDTO">Entity.</param>
        /// <returns>Organization's key.</returns>
        [Authorize(Roles = "organization,admin")]
        [HttpPut]
        public async Task<ActionResult> Update(OrganizationDTO organizationDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await organizationService.Update(organizationDTO).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Organization entity from the database.
        /// </summary>
        /// <param name="id">Organization's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            if (id == 0)
            {
                return BadRequest("Id cannot be 0.");
            }

            await organizationService.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}