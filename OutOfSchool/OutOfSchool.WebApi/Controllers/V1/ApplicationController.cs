using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for a Application entity.
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/applications")]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService applicationService;
    private readonly IProviderService providerService;
    private readonly IEmployeeService employeeService;
    private readonly IWorkshopService workshopService;
    private readonly IUserService userService;
    private readonly IBlockedProviderParentService blockedProviderParentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationController"/> class.
    /// </summary>
    /// <param name="applicationService">Service for Application model.</param>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="employeeService">Service for Employee model.</param>
    /// <param name="workshopService">Service for Workshop model.</param>
    /// <param name="userService">Service for operations with users.</param>
    /// <param name="blockedProviderParentService">Service for blocking parents for providers.</param>
    public ApplicationController(
        IApplicationService applicationService,
        IProviderService providerService,
        IEmployeeService employeeService,
        IWorkshopService workshopService,
        IUserService userService,
        IBlockedProviderParentService blockedProviderParentService)
    {
        this.applicationService = applicationService;
        this.providerService = providerService;
        this.employeeService = employeeService;
        this.workshopService = workshopService;
        this.userService = userService;
        this.blockedProviderParentService = blockedProviderParentService;
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

            return this.SearchResultToOkOrNoContent(applications);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get Applications count by Parent Id.
    /// </summary>
    /// <param name="id">Parent id.</param>
    /// <returns>Count of applications.</returns>
    /// <response code="200">Entities count by given Id.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v{version:apiVersion}/parents/{id}/applicationsCount")]
    public async Task<IActionResult> GetCountByParentId(Guid id)
    {
        try
        {
            var applications = await applicationService.GetCountByParentId(id).ConfigureAwait(false);

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
        var isProviderExists = await providerService.Exists(providerId).ConfigureAwait(false);

        if (!isProviderExists)
        {
            return BadRequest($"There is no provider with Id = {providerId}");
        }

        var applications = await applicationService.GetAllByProvider(providerId, filter).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(applications);
    }

    /// <summary>
    /// Get collection of applications, that have a pending status.
    /// </summary>
    /// <param name="providerId">Provider id.</param>
    /// <returns>List of applications.</returns>
    /// <response code="200">Entities were found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="400">Provider with given id was not found.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v{version:apiVersion}/provider/{providerId}/applications/pending")]
    public async Task<IActionResult> GetPendingApplicationsByProviderId(Guid providerId)
    {
        var providerStandard = await providerService.GetById(providerId).ConfigureAwait(false);

        var filter = new ApplicationFilter()
        {
            Statuses = new List<ApplicationStatus>()
            {
                ApplicationStatus.Pending,
            },
        };

        SearchResult<ApplicationDto> applications;

        // if: providerStandard is not null - get an applications and return it
        // else: find a employee, get an applications and return it
        if (providerStandard is not null)
        {
            applications = await applicationService.GetAllByProvider(providerId, filter).ConfigureAwait(false);
        }
        else
        {
            var employeeIdStringVersion = providerId.ToString();
            var employee = await employeeService.GetById(employeeIdStringVersion).ConfigureAwait(false);

            // employees and providers were not found by given id
            if (employee is null)
            {
                return BadRequest($"There is no any employee or provider with given id - {providerId}.");
            }

            applications = await applicationService
                .GetAllByEmployee(employeeIdStringVersion, filter, employee.ProviderId)
                .ConfigureAwait(false);
        }

        return this.SearchResultToOkOrNoContent(applications);
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

        return this.SearchResultToOkOrNoContent(applications);
    }

    /// <summary>
    /// Get Applications by employee Id.
    /// </summary>
    /// <param name="employeeId">Employee id.</param>
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
    [HttpGet("/api/v{version:apiVersion}/employees/{employeeId}/applications")]
    public async Task<IActionResult> GetByEmployeeId(Guid employeeId, [FromQuery] ApplicationFilter filter)
    {
        var userId = employeeId.ToString();
        var employee = await employeeService.GetById(userId).ConfigureAwait(false);

        if (employee is null)
        {
            return BadRequest($"There is no employee with userId = {userId}");
        }

        var applications = await applicationService
            .GetAllByEmployee(userId, filter, employee.ProviderId)
            .ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(applications);
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
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApplicationDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApplicationCreate applicationDto)
    {
        if (applicationDto == null)
        {
            return BadRequest("Application is null.");
        }

        if (await IsWorkshopBlocked(applicationDto.WorkshopId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to create the application at the blocked workshop.");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to create the application by the blocked user.");
        }

        var workshop = await workshopService.GetById(applicationDto.WorkshopId);

        if (workshop is null)
        {
            return BadRequest("Workshop does not exist.");
        }

        bool isBlockedParent = await blockedProviderParentService.IsBlocked(applicationDto.ParentId, workshop.ProviderId).ConfigureAwait(false);

        if (isBlockedParent)
        {
            return StatusCode(403, "Forbidden to create the application by the blocked parent.");
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
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ApplicationUpdate applicationDto)
    {
        if (applicationDto is null)
        {
            return BadRequest("Application data is not provided.");
        }

        if (await IsWorkshopBlocked(applicationDto.WorkshopId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to update the application at the blocked workshop.");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to update the application by the blocked user.");
        }

        var workshop = await workshopService.GetById(applicationDto.WorkshopId).ConfigureAwait(false);

        if (workshop is null)
        {
            return BadRequest("Workshop does not exist.");
        }

        try
        {
            var result =
                await applicationService.Update(applicationDto, workshop.ProviderId).ConfigureAwait(false);

            return result.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, new { error.Message, error.ApiErrorResponse }),
            result => Ok(result));
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

    private async Task<bool> IsCurrentUserBlocked()
    {
        var userId = GettingUserProperties.GetUserId(User);

        return await userService.IsBlocked(userId);
    }

    private async Task<bool> IsWorkshopBlocked(Guid workshopId) =>
        await workshopService.IsBlocked(workshopId).ConfigureAwait(false);
}