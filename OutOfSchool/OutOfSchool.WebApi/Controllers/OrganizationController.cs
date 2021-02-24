using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
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
        
        
        public IActionResult TestOk()
        {
            var user = User?.FindFirst("role")?.Value;
            return Ok("Hello to "+user ?? "unknown");
        }
        
        /// <summary>
        /// Get all organization from database.
        /// </summary>
        /// <returns>List of all organizations.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations()
        {
            try
            {
                return Ok(await organizationService.GetAll());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get organization by id.
        /// </summary>
        /// <param name="id">Key in database.</param>
        /// <returns>Organization element with some id.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDTO>> GetOrganizationById(long id)
        {
            try
            {
                return Ok(await organizationService.GetById(id).ConfigureAwait(false));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
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
                    nameof(GetOrganizations),
                    new { id = organization.Id },
                    organization);
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
            if (organizationDTO == null)
            {
                return BadRequest("Entity was null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {              
                return Ok(await organizationService.Update(organizationDTO));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete some element from database.
        /// </summary>
        /// <param name="id">Element's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            try
            {
                await organizationService.Delete(id).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
