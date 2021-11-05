using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.ApiModels;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller with CRUD operations for a Application entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService applicationService;
        private readonly IParentService parentService;
        private readonly IProviderService providerService;
        private readonly IWorkshopService workshopService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationController"/> class.
        /// </summary>
        /// <param name="applicationService">Service for Application model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="providerService">Service for Provider model.</param>
        /// <param name="parentService">Service for Parent model.</param>
        /// <param name="workshopService">Service for Workshop model.</param>
        public ApplicationController(
            IApplicationService applicationService,
            IStringLocalizer<SharedResource> localizer,
            IProviderService providerService,
            IParentService parentService,
            IWorkshopService workshopService)
        {
            this.applicationService = applicationService;
            this.localizer = localizer;
            this.providerService = providerService;
            this.parentService = parentService;
            this.workshopService = workshopService;
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
            var applications = await applicationService.GetAll().ConfigureAwait(false);

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
        public async Task<IActionResult> GetById(Guid id)
        {
            var application = await applicationService.GetById(id).ConfigureAwait(false);

            if (application is null)
            {
                return NoContent();
            }

            try
            {
                await CheckUserRights(
                    parentId: application.ParentId,
                    providerId: application.Workshop.ProviderId)
                    .ConfigureAwait(false);

                return Ok(application);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get Applications by Parent Id.
        /// </summary>
        /// <param name="id">Parent id.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.ApplicationReadParent)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByParentId(Guid id)
        {
            try
            {
                await CheckUserRights(parentId: id).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            var applications = await applicationService.GetAllByParent(id).ConfigureAwait(false);

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
        /// <param name="filter">Application filter.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.ApplicationReadManager)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{property:regex(^provider$|^workshop$)}/{id}")]
        public async Task<IActionResult> GetByPropertyId(string property, Guid id, [FromQuery] ApplicationFilter filter)
        {
            IEnumerable<ApplicationDto> applications = default;

            try
            {
                if (property.Equals("provider", StringComparison.CurrentCultureIgnoreCase))
                {
                    applications = await GetByProviderId(id, filter).ConfigureAwait(false);
                }
                else if (property.Equals("workshop", StringComparison.CurrentCultureIgnoreCase))
                {
                    applications = await GetByWorkshopId(id, filter).ConfigureAwait(false);
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
        [HasPermission(Permissions.ApplicationReadManager)]
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

            var applications = await applicationService.GetAllByStatus(status).ConfigureAwait(false);

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
        public async Task<IActionResult> Create([FromBody] ApplicationApiModel applicationApiModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var applications = CreateMultiple(applicationApiModel);

                var newApplications = await applicationService.Create(applications).ConfigureAwait(false);

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
        /// <response code="500">If any server error occurs.</response>
        [HasPermission(Permissions.ApplicationAddNew)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApplicationDto))]
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

            if (applicationDto is null)
            {
                return BadRequest(localizer[$"Application dto should not be null"]);
            }

            try
            {
                await CheckUserRights(parentId: applicationDto.ParentId).ConfigureAwait(false);

                applicationDto.Id = default;

                applicationDto.CreationTime = DateTimeOffset.UtcNow;

                applicationDto.Status = ApplicationStatus.Pending;

                var application = await applicationService.Create(applicationDto).ConfigureAwait(false);

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
        [HasPermission(Permissions.ApplicationEdit)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(ShortApplicationDto applicationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var application = await applicationService.GetById(applicationDto.Id).ConfigureAwait(false);

            if (application is null)
            {
                return BadRequest(localizer[$"There is no application with Id = {applicationDto.Id}."]);
            }

            application.Status = applicationDto.Status;

            try
            {
                await CheckUserRights(
                    parentId: application.ParentId,
                    providerId: application.Workshop.ProviderId).ConfigureAwait(false);

                var updatedApplication = await applicationService.Update(application).ConfigureAwait(false);
                return Ok(updatedApplication);
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
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await applicationService.Delete(id).ConfigureAwait(false);
                return NoContent();
            }
            // TODO: update exception handling
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static IEnumerable<ApplicationDto> CreateMultiple(ApplicationApiModel applicationApiModel)
        {
            var applications = applicationApiModel.Children.Select(child => new ApplicationDto
            {
                ChildId = child.Id,
                CreationTime = DateTimeOffset.UtcNow,
                WorkshopId = applicationApiModel.WorkshopId,
            });

            return applications.ToList();
        }

        // TODO: Ask Polina about status validation
        private void ValidateStatus(int status)
        {
            if (status < 0 || status > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(status), localizer["Status should be from 0 to 2"]);
            }
        }

        private async Task<IEnumerable<ApplicationDto>> GetByWorkshopId(Guid id, ApplicationFilter filter)
        {
            var workshop = await workshopService.GetById(id).ConfigureAwait(false);

            if (workshop is null)
            {
                throw new ArgumentException(localizer[$"There is no workshop with Id = {id}"]);
            }

            await CheckUserRights(providerId: workshop.ProviderId).ConfigureAwait(false);

            var applications = await applicationService.GetAllByWorkshop(id, filter).ConfigureAwait(false);

            return applications;
        }

        private async Task<IEnumerable<ApplicationDto>> GetByProviderId(Guid id, ApplicationFilter filter)
        {
            await CheckUserRights(providerId: id).ConfigureAwait(false);

            var applications = await applicationService.GetAllByProvider(id, filter).ConfigureAwait(false);

            return applications;
        }

        private async Task CheckUserRights(Guid parentId = default, Guid providerId = default)
        {
            var userId = User.FindFirst("sub")?.Value;

            bool userHasRights = true;

            if (User.IsInRole("parent"))
            {
                var parent = await parentService.GetByUserId(userId).ConfigureAwait(false);

                userHasRights = parent.Id == parentId;
            }
            else if (User.IsInRole("provider"))
            {
                var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);

                userHasRights = provider.Id == providerId;
            }

            if (!userHasRights)
            {
                throw new ArgumentException(localizer["User has no rights to perform operation"]);
            }
        }
    }
}