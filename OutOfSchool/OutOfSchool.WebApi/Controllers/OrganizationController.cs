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

        /// <summary>
        /// Get all organization from database.
        /// </summary>
        /// <returns>List of all organizations.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Organization>> GetOrganizations()
        {
            try
            {
                return this.Ok(this.organizationService.GetAll());
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
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
                OrganizationDTO organizationDTO = await this.organizationService.GetById(id).ConfigureAwait(false);
                return this.Ok(organizationDTO);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Method for creating new organization.
        /// </summary>
        /// <param name="organizationDTO">Element which must be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPost]
        public async Task<ActionResult<Organization>> CreateOrganization(OrganizationDTO organizationDTO)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                OrganizationDTO organization = await this.organizationService.Create(organizationDTO).ConfigureAwait(false);
                return this.CreatedAtAction(
                    nameof(this.GetOrganizations),
                    new { id = organization.Id },
                    organization);
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
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
                return this.BadRequest("Entity was null.");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                this.organizationService.Update(organizationDTO);
                return this.Ok(await this.organizationService.GetById(organizationDTO.Id).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
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
                await this.organizationService.Delete(id).ConfigureAwait(false);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
    }
}

