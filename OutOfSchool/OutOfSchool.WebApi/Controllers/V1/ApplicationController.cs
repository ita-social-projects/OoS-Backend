using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
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
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService applicationService;
        private readonly IParentService parentService;
        private readonly IProviderService providerService;
        private readonly IProviderAdminService providerAdminService;
        private readonly IWorkshopService workshopService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationController"/> class.
        /// </summary>
        /// <param name="applicationService">Service for Application model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="providerService">Service for Provider model.</param>
        /// <param name="providerAdminService">Service for ProviderAdmin model.</param>
        /// <param name="parentService">Service for Parent model.</param>
        /// <param name="workshopService">Service for Workshop model.</param>
        public ApplicationController(
            IApplicationService applicationService,
            IStringLocalizer<SharedResource> localizer,
            IProviderService providerService,
            IProviderAdminService providerAdminService,
            IParentService parentService,
            IWorkshopService workshopService)
        {
            this.applicationService = applicationService;
            this.localizer = localizer;
            this.providerService = providerService;
            this.providerAdminService = providerAdminService;
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
        [HasPermission(Permissions.SystemManagement)]
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
        [HasPermission(Permissions.ApplicationRead)]
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
                        providerId: application.Workshop.ProviderId,
                        workshopId: application.Workshop.WorkshopId)
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
        /// <param name="filter">Application filter.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.ApplicationRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByParentId(Guid id, [FromQuery] ApplicationFilter filter)
        {
            try
            {
                await CheckUserRights(parentId: id).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            var applications = await applicationService.GetAllByParent(id, filter).ConfigureAwait(false);

            if (!applications.Entities.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }

        /// <summary>
        /// Get Applications by Provider, Workshop or ProviderAdmin Id.
        /// </summary>
        /// <param name="property">Property to find by (workshop, provider or provideradmin).</param>
        /// <param name="id">Provider, Workshop or ProviderAdmin id.</param>
        /// <param name="filter">Application filter.</param>
        /// <returns>List of applications.</returns>
        /// <response code="200">Entities were found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.ApplicationRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{property:regex(^provider$|^workshop$|^provideradmin$)}/{id}")]
        public async Task<IActionResult> GetByPropertyId(string property, Guid id, [FromQuery] ApplicationFilter filter)
        {
            SearchResult<ApplicationDto> applications = default;

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
                else if (property.Equals("provideradmin", StringComparison.CurrentCultureIgnoreCase))
                {
                    applications = await GetByProviderAdminId(id.ToString(), filter).ConfigureAwait(false);
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            if (!applications.Entities.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }

        /// <summary>
        /// Method for creating a new application.
        /// </summary>
        /// <param name="applicationDto">Application entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        /// <response code="201">Entity was created and returned with Id.</response>
        /// <response code="400">If the model is invalid, some properties are not set etc.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="429">If too many requests have been sent.</response>
        /// <response code="500">If any server error occurs.</response>
        [HasPermission(Permissions.ApplicationAddNew)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApplicationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(ApplicationDto applicationDto)
        {
            if (applicationDto == null)
            {
                return BadRequest("Application is null.");
            }

            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                await CheckUserRights(parentId: applicationDto.ParentId).ConfigureAwait(false);

                applicationDto.Id = Guid.Empty;

                applicationDto.CreationTime = DateTimeOffset.UtcNow;

                applicationDto.Status = ApplicationStatus.Pending;

                var applicationWithAdditionalData = await applicationService.Create(applicationDto).ConfigureAwait(false);

                if (applicationWithAdditionalData.AdditionalData > 0)
                {
                    Response.Headers.Add("Access-Control-Expose-Headers", "Retry-After");
                    Response.Headers.Add("Retry-After", applicationWithAdditionalData.AdditionalData.ToString(CultureInfo.InvariantCulture));
                    return StatusCode(StatusCodes.Status429TooManyRequests);
                }
                else
                {
                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = applicationWithAdditionalData.Model.Id, },
                        applicationWithAdditionalData.Model);
                }
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

            try
            {
                UpdateStatus(applicationDto, application);

                await CheckUserRights(
                    parentId: application.ParentId,
                    providerId: application.Workshop.ProviderId,
                    workshopId: application.Workshop.WorkshopId).ConfigureAwait(false);

                var userId = GettingUserProperties.GetUserId(User);
                var updatedApplication = await applicationService.Update(application, userId).ConfigureAwait(false);

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

        [HasPermission(Permissions.ApplicationAddNew)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [HttpGet]
        public async Task<IActionResult> AllowedNewApplicationByChildStatus(Guid workshopId, Guid childId)
        {
            return Ok(await applicationService.AllowedNewApplicationByChildStatus(workshopId, childId)
                .ConfigureAwait(false));
        }

        private async Task<SearchResult<ApplicationDto>> GetByWorkshopId(Guid id, ApplicationFilter filter)
        {
            var workshop = await workshopService.GetById(id).ConfigureAwait(false);

            if (workshop is null)
            {
                throw new ArgumentException(localizer[$"There is no workshop with Id = {id}"]);
            }

            await CheckUserRights(providerId: workshop.ProviderId, workshopId: workshop.Id).ConfigureAwait(false);

            var applications = await applicationService.GetAllByWorkshop(id, filter).ConfigureAwait(false);

            return applications;
        }

        private async Task<SearchResult<ApplicationDto>> GetByProviderId(Guid id, ApplicationFilter filter)
        {
            var provider = await providerService.GetById(id).ConfigureAwait(false);

            if (provider is null)
            {
                throw new ArgumentException(localizer[$"There is no provider with Id = {id}"]);
            }

            await CheckUserRights(providerId: id).ConfigureAwait(false);

            var applications = await applicationService.GetAllByProvider(id, filter).ConfigureAwait(false);

            return applications;
        }

        private async Task<IEnumerable<ApplicationDto>> GetByProviderAdminId(string userId, ApplicationFilter filter)
        {
            var providerAdmin = await providerAdminService.GetById(userId).ConfigureAwait(false);

            if (providerAdmin is null)
            {
                throw new ArgumentException(localizer[$"There is no providerAdmin with userId = {userId}"]);
            }

            await CheckUserRights(providerAdminId: userId).ConfigureAwait(false);

            var applications = await applicationService.GetAllByProviderAdmin(userId, filter, providerAdmin.ProviderId, providerAdmin.IsDeputy).ConfigureAwait(false);

            return applications;
        }

        private async Task CheckUserRights(Guid parentId = default, Guid providerId = default, Guid workshopId = default, string providerAdminId = default)
        {
            var userId = GettingUserProperties.GetUserId(User);

            bool userHasRights = true;

            if (User.IsInRole("parent"))
            {
                userHasRights = await ParentHasRights(userId, parentId).ConfigureAwait(false);
            }
            else if (User.IsInRole("provider"))
            {
                if (!string.IsNullOrEmpty(providerAdminId))
                {
                    userHasRights = await ProviderHasRights(userId, providerAdminId).ConfigureAwait(false);
                }
                else
                {
                    userHasRights = await ProviderAdminHasRights(userId, providerId, workshopId).ConfigureAwait(false);
                }
            }

            if (!userHasRights)
            {
                throw new ArgumentException(localizer["User has no rights to perform operation"]);
            }
        }

        private void UpdateStatus(ShortApplicationDto applicationDto, ApplicationDto application)
        {
            if (application.Status == ApplicationStatus.Completed || application.Status == ApplicationStatus.Rejected || application.Status == ApplicationStatus.Left)
            {
                if (!User.IsInRole("provider"))
                {
                    throw new ArgumentException("Forbidden to update application.");
                }
            }

            application.Status = applicationDto.Status;
            application.RejectionMessage = applicationDto.RejectionMessage;

            if (application.Status != ApplicationStatus.Rejected)
            {
                application.RejectionMessage = null;
            }

            if (application.Status == ApplicationStatus.Approved)
            {
                application.ApprovedTime = DateTimeOffset.UtcNow;
            }
        }

        private async Task<bool> ParentHasRights(string userId, Guid parentId)
        {
            var parent = await parentService.GetByUserId(userId).ConfigureAwait(false);

            return parent.Id == parentId;
        }

        private async Task<bool> ProviderHasRights(string userId, string providerAdminId)
        {
            var providerAdmin = await providerAdminService.GetById(userId).ConfigureAwait(false);

            return providerAdmin.UserId == providerAdminId;
        }

        private async Task<bool> ProviderAdminHasRights(string userId, Guid providerId, Guid workshopId)
        {
            try
            {
                var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);
                return provider.Id == providerId;
            }
            catch (ArgumentException)
            {
                var isUserRelatedAdmin = await providerAdminService.CheckUserIsRelatedProviderAdmin(userId, providerId, workshopId).ConfigureAwait(false);
                if (!isUserRelatedAdmin)
                {
                    return false;
                }
            }

            return true;
        }
    }
}