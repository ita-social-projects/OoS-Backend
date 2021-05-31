using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.ApiModels;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for a Application entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]

    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationController"/> class.
        /// </summary>
        /// <param name="service">Service for Application model.</param>
        /// <param name="localizer">Localizer.</param>
        public ApplicationController(IApplicationService service, IStringLocalizer<SharedResource> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get all applications from the database.
        /// </summary>
        /// <returns>List of all applications.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var applications = await service.GetAll().ConfigureAwait(false);

            if (!applications.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }

        /// <summary>
        /// Get application by it's id.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>Application entity.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            this.ValidateId(id, localizer);

            return Ok(await service.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Get Applications by User Id.
        /// </summary>
        /// <param name="id">User id.</param>
        /// <returns>List of applications.</returns>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByUserId(string id)
        {
            try
            {
                return Ok(await service.GetAllByUser(id).ConfigureAwait(false));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get Applications by Workshop Id.
        /// </summary>
        /// <param name="id">Workshop id.</param>
        /// <returns>List of applications.</returns>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByWorkshopId(long id)
        {
            this.ValidateId(id, localizer);

            try
            {
                return Ok(await service.GetAllByWorkshop(id).ConfigureAwait(false));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Method for creating a new application.
        /// </summary>
        /// <param name="applicationApiModel">Application api model with data to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]ApplicationApiModel applicationApiModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var applications = CreateMultiple(applicationApiModel);

                var newApplications = await service.Create(applications).ConfigureAwait(false);

                var ids = newApplications.Select(a => a.Id);

                return CreatedAtAction(
                     nameof(GetById),
                     new { id = ids, },
                     newApplications);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update info about a specific application in the database.
        /// </summary>
        /// <param name="applicationDto">Application entity.</param>
        /// <returns>Application's key.</returns>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(ApplicationDto applicationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(applicationDto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Application entity from the database.
        /// </summary>
        /// <param name="id">Application's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            this.ValidateId(id, localizer);

            await service.Delete(id).ConfigureAwait(false);

            return NoContent();
        }

        private IEnumerable<ApplicationDto> CreateMultiple(ApplicationApiModel applicationApiModel)
        {
            var applications = applicationApiModel.Children.Select(child => new ApplicationDto
            {
                ChildId = child.Id,
                CreationTime = DateTime.Now,
                UserId = User.FindFirst("sub")?.Value,
                WorkshopId = applicationApiModel.WorkshopId,
            });

            return applications.ToList();
        }
    }
}
