using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// <response code="200">All entities were found.</response>
        /// <response code="204">No entity was found.</response>
        /// <response code="500">If any server error occures.</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApplicationDto>))]
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
        /// <returns><see cref="ApplicationDto"/>.</returns>
        /// <response code="200">The entity was found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                this.ValidateId(id, localizer);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }

            var application = await service.GetById(id).ConfigureAwait(false);

            if (application is null)
            {
                return NoContent();
            }

            return Ok(application);
        }

        /// <summary>
        /// Get Applications by User Id.
        /// </summary>
        /// <param name="id">User id.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByParentId(long id)
        {
            var applications = await service.GetAllByParent(id).ConfigureAwait(false);

            if (!applications.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }

        /// <summary>
        /// Get Applications by Workshop Id.
        /// </summary>
        /// <param name="id">Workshop id.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByWorkshopId(long id)
        {
            try
            {
                this.ValidateId(id, localizer);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }

            var applications = await service.GetAllByWorkshop(id).ConfigureAwait(false);

            if (!applications.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }

        /// <summary>
        /// Get Applications by Provider Id.
        /// </summary>
        /// <param name="id">Provider id.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByProviderId(long id)
        {
            try
            {
                this.ValidateId(id, localizer);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }

            var applications = await service.GetAllByProvider(id).ConfigureAwait(false);

            if (!applications.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }

        /// <summary>
        /// Get Applications by Provider or Workshop Id.
        /// </summary>
        /// <param name="property">Property to find by (workshop or provider).</param>
        /// <param name="id">Provider or Workshop id.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{property:regex(^provider$|^workshop$)}/{id}")]
        public async Task<IActionResult> GetByPropertyId(string property, long id)
        {
            try
            {
                this.ValidateId(id, localizer);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }

            IEnumerable<ApplicationDto> applications = default;

            switch (property.ToLower(CultureInfo.CurrentCulture))
            {
                case "workshop":
                    applications = await service.GetAllByWorkshop(id).ConfigureAwait(false);
                    break;
                case "provider":
                    applications = await service.GetAllByProvider(id).ConfigureAwait(false);
                    break;
            }

            if (!applications.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }

        /// <summary>
        /// Get Applications by Status.
        /// </summary>
        /// <param name="status">Application status.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given status.</response>
        /// <response code="204">No entity with given status was found.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetByStatus(int status)
        {
            try
            {
                ValidateStatus(status);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }

            var applications = await service.GetAllByStatus(status).ConfigureAwait(false);

            if (!applications.Any())
            {
                return NoContent();
            }

            return Ok(applications);
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
        [HttpPost("multiple")]
        [Obsolete("This method is obsolete. Call another Create instead", false)]
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
        /// Method for creating a new application.
        /// </summary>
        /// <param name="applicationDto">Application entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        /// <response code="201">Entity was created and returned with Id.</response>
        /// <response code="400">If the model is invalid, some properties are not set etc.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(ApplicationDto applicationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                applicationDto.Id = default;

                applicationDto.CreationTime = DateTime.Now;

                applicationDto.Status = 0;

                var application = await service.Create(applicationDto).ConfigureAwait(false);

                return CreatedAtAction(
                     nameof(GetById),
                     new { id = application.Id, },
                     application);
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
        /// <returns><see cref="ApplicationDto"/>.</returns>
        /// <response code="200">Entity was updated and returned.</response>
        /// <response code="400">If the model is invalid, some properties are not set etc.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationDto))]
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

            try
            {
                var application = await service.Update(applicationDto).ConfigureAwait(false);
                return Ok(application);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a specific Application entity from the database.
        /// </summary>
        /// <param name="id">Application's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        /// <response code="204">If the entity was successfully deleted.</response>
        /// <responce code="400">If entity with given Id does not exist.</responce>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                this.ValidateId(id, localizer);

                await service.Delete(id).ConfigureAwait(false);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private IEnumerable<ApplicationDto> CreateMultiple(ApplicationApiModel applicationApiModel)
        {
            var applications = applicationApiModel.Children.Select(child => new ApplicationDto
            {
                ChildId = child.Id,
                CreationTime = DateTime.Now,
                WorkshopId = applicationApiModel.WorkshopId,
            });

            return applications.ToList();
        }

        private void ValidateStatus(int status)
        {
            if (status < 0 || status > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(status), localizer["Status should be from 0 to 2"]);
            }
        }
    }
}
