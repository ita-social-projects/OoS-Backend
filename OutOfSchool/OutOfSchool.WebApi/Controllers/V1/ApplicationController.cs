using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for a Application entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/applications")]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService applicationService;
    private readonly IProviderService providerService;
    private readonly IProviderAdminService providerAdminService;
    private readonly IWorkshopService workshopService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationController"/> class.
    /// </summary>
    /// <param name="applicationService">Service for Application model.</param>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="providerAdminService">Service for ProviderAdmin model.</param>
    /// <param name="workshopService">Service for Workshop model.</param>
    public ApplicationController(
        IApplicationService applicationService,
        IProviderService providerService,
        IProviderAdminService providerAdminService,
        IWorkshopService workshopService)
    {
        this.applicationService = applicationService;
        this.providerService = providerService;
        this.providerAdminService = providerAdminService;
        this.workshopService = workshopService;
    }

    /// <summary>
    /// Get all applications from the database.
    /// </summary>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of all applications.</returns>
    /// <response code="200">All entities were found.</response>
    /// <response code="204">No entity was found.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get(ApplicationFilter filter)
    {
        var applications = await applicationService.GetAll(filter).ConfigureAwait(false);

        if (!applications.Entities.Any())
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
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var application = await applicationService.GetById(id).ConfigureAwait(false);

            if (application is null)
            {
                return NoContent();
            }

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
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v{version:apiVersion}/parents/{id}/applications")]
    public async Task<IActionResult> GetByParentId(Guid id, [FromQuery] ApplicationFilter filter)
    {
        try
        {
            var applications = await applicationService.GetAllByParent(id, filter).ConfigureAwait(false);

            if (!applications.Entities.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get Applications by Provider Id.
    /// </summary>
    /// <param name="providerId">Provider id.</param>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of applications.</returns>
    /// <response code="200">Entities were found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v{version:apiVersion}/providers/{providerId}/applications")]
    public async Task<IActionResult> GetByProviderId(Guid providerId, [FromQuery] ApplicationFilter filter)
    {
        var provider = await providerService.GetById(providerId).ConfigureAwait(false);

        if (provider is null)
        {
            return BadRequest($"There is no provider with Id = {providerId}");
        }

        var applications = await applicationService.GetAllByProvider(providerId, filter).ConfigureAwait(false);

        if (applications == null || !applications.Entities.Any())
        {
            return NoContent();
        }

        return Ok(applications);
    }

    /// <summary>
    /// Get Applications by Workshop Id.
    /// </summary>
    /// <param name="workshopId">Workshop id.</param>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of applications.</returns>
    /// <response code="200">Entities were found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v{version:apiVersion}/workshops/{workshopId}/applications")]
    public async Task<IActionResult> GetByWorkshopId(Guid workshopId, [FromQuery] ApplicationFilter filter)
    {
        var workshop = await workshopService.GetById(workshopId).ConfigureAwait(false);

        if (workshop is null)
        {
            return BadRequest($"There is no workshop with Id = {workshopId}");
        }

        var applications = await applicationService.GetAllByWorkshop(workshopId, workshop.ProviderId, filter)
            .ConfigureAwait(false);

        if (applications == null || !applications.Entities.Any())
        {
            return NoContent();
        }

        return Ok(applications);
    }

    /// <summary>
    /// Get Applications by ProviderAdmin Id.
    /// </summary>
    /// <param name="providerAdminId">ProviderAdmin id.</param>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of applications.</returns>
    /// <response code="200">Entities were found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v{version:apiVersion}/provideradmins/{providerAdminId}/applications")]
    public async Task<IActionResult> GetByProviderAdminId(Guid providerAdminId, [FromQuery] ApplicationFilter filter)
    {
        var userId = providerAdminId.ToString();
        var providerAdmin = await providerAdminService.GetById(userId).ConfigureAwait(false);

        if (providerAdmin is null)
        {
            return BadRequest($"There is no providerAdmin with userId = {userId}");
        }

        var applications = await applicationService
            .GetAllByProviderAdmin(userId, filter, providerAdmin.ProviderId, providerAdmin.IsDeputy)
            .ConfigureAwait(false);

        if (applications == null || !applications.Entities.Any())
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
    public async Task<IActionResult> Create(ApplicationCreate applicationDto)
    {
        if (applicationDto == null)
        {
            return BadRequest("Application is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var applicationWithAdditionalData = await applicationService.Create(applicationDto).ConfigureAwait(false);

            if (applicationWithAdditionalData.AdditionalData > 0)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "Retry-After");
                Response.Headers.Add(
                    "Retry-After",
                    applicationWithAdditionalData.AdditionalData.ToString(CultureInfo.InvariantCulture));
                return StatusCode(StatusCodes.Status429TooManyRequests);
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = applicationWithAdditionalData.Model.Id, },
                applicationWithAdditionalData.Model);
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
    /// <response code="500">If any server error occurres.</response>
    [HasPermission(Permissions.ApplicationEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(ApplicationUpdate applicationDto)
    {
        if (applicationDto is null)
        {
            return BadRequest("Application data is not provided.");
        }

        var workshop = await workshopService.GetById(applicationDto.WorkshopId).ConfigureAwait(false);

        if (workshop is null)
        {
            return BadRequest("Workshop does not exist.");
        }

        try
        {
            var updatedApplication =
                await applicationService.Update(applicationDto, workshop.ProviderId).ConfigureAwait(false);

            if (updatedApplication is null)
            {
                return BadRequest("Application does not exist.");
            }

            return Ok(updatedApplication);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HasPermission(Permissions.ApplicationAddNew)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [HttpGet("allowed/workshops/{workshopId}/children/{childId}")]
    public async Task<IActionResult> AllowedNewApplicationByChildStatus(Guid workshopId, Guid childId)
    {
        return Ok(await applicationService.AllowedNewApplicationByChildStatus(workshopId, childId)
            .ConfigureAwait(false));
    }

    /// <summary>
    /// Check if exists an any application with approve status in workshop for parent.
    /// </summary>
    /// <param name="parentId">Parent's key.</param>
    /// <param name="workshopId">Workshop's key.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationAddNew)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("reviewable/parents/{parentId}/workshops/{workshopId}")]
    public async Task<IActionResult> AllowedToReview(Guid parentId, Guid workshopId)
    {
        return Ok(await applicationService.AllowedToReview(parentId, workshopId).ConfigureAwait(false));
    }
}