using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for StudySubject entity
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class StudySubjectController : ControllerBase
{
    private readonly IStudySubjectService _studySubjectService;
    private readonly IProviderService _providerService;
    private readonly IEmployeeService _employeeService;
    private readonly IWorkshopService _workshopService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudySubjectController"/> class.
    /// </summary>
    /// <param name="studySubjectService">Service for StudySubject model.</param>
    /// <param name="providerService">Service for Provider.</param>
    /// <param name="workshopService"></param>
    /// <param name="employeeService"></param>
    public StudySubjectController(
        IStudySubjectService studySubjectService,
        IProviderService providerService,
        IWorkshopService workshopService,
        IEmployeeService employeeService)
    {
        _providerService = providerService;
        _studySubjectService = studySubjectService;
        _workshopService = workshopService;
        _employeeService = employeeService;
    }

    /// <summary>
    /// Get filtered list of StudySubjects from the database.
    /// </summary>
    /// <returns>List of StudySubjects.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StudySubjectDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SearchStringFilter filter = null)
    {
        var studySubjects = await _studySubjectService.GetByFilter(filter).ConfigureAwait(false);

        if (!studySubjects.Any())
        {
            return NoContent();
        }

        return Ok(studySubjects);
    }

    /// <summary>
    /// Get StudySubject by it's id.
    /// </summary>
    /// <param name="id">StudySubject's id.</param>
    /// <returns>StudySubject.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var studySubjectDto = await _studySubjectService.GetById(id).ConfigureAwait(false);
            return Ok(studySubjectDto);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Add a new StudySubject to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StudySubjectCreateUpdateDto dto)
    {
        if (dto is null)
        {
            return BadRequest("StudySubject dto is null.");
        }

        var isWorkshopExists = await _workshopService.Exists(dto.WorkshopId).ConfigureAwait(false);

        if(!isWorkshopExists)
        {
            return NotFound("There's no such workshop in the database.");
        }

        var providerId = await _providerService.GetProviderIdForWorkshopById(dto.WorkshopId).ConfigureAwait(false);

        if (await _providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false)
        {
            return StatusCode(403, "It is forbidden to add study subjects to workshops at blocked providers");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        //var userHasRights = await this.IsUserProvidersOwnerOrAdmin(dto.WorkshopId).ConfigureAwait(false);

        //if (!userHasRights)
        //{
        //    return StatusCode(403, "Forbidden to create study subjects for another providers");
        //}

        try
        {
            dto.Id = Guid.Empty;

            var creationResult = await _studySubjectService.Create(dto).ConfigureAwait(false);

            return CreatedAtAction(
            nameof(GetById),
            new { id = creationResult.Id, },
            creationResult);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    /// <summary>
    /// Update info about a specific StudySubject in the database.
    /// </summary>
    /// <param name="dto">StudySubject to update.</param>
    /// <returns>StudySubject.</returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] StudySubjectCreateUpdateDto dto)
    {
        var providerId = await _providerService.GetProviderIdForWorkshopById(dto.WorkshopId).ConfigureAwait(false);

        if (await _providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false)
        {
            return StatusCode(403, "It is forbidden to add study subjects to workshops at blocked providers");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        //var userHasRights = await this.IsUserProvidersOwnerOrAdmin(dto.WorkshopId).ConfigureAwait(false);

        //if (!userHasRights)
        //{
        //    return StatusCode(403, "Forbidden to create study subjects for another providers");
        //}

        return Ok(await _studySubjectService.Update(dto).ConfigureAwait(false));
    }

    /// <summary>
    /// Delete a specific StudySubject entity from the database.
    /// </summary>
    /// <param name="id">StudySubject's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        StudySubjectDto dto;

        try
        {
            dto = await _studySubjectService.GetById(id).ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        var providerId = await _providerService.GetProviderIdForWorkshopById(dto.WorkshopId).ConfigureAwait(false);

        if (await _providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false)
        {
            return StatusCode(403, "It is forbidden to add study subjects to workshops at blocked providers");
        }

        //var userHasRights = await this.IsUserProvidersOwnerOrAdmin(dto.WorkshopId).ConfigureAwait(false);

        //if (!userHasRights)
        //{
        //    return StatusCode(403, "Forbidden to create study subjects for another providers");
        //}

        try
        {
            await _studySubjectService.Delete(id).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<bool> IsUserProvidersOwnerOrAdmin(Guid workshopId)
    {
        if (!User.IsInRole(nameof(Role.Provider).ToLower())
            && !User.IsInRole(nameof(Role.Employee).ToLower()))
        {
            return false;
        }

        var userId = GettingUserProperties.GetUserId(User);
        var providerId = await _workshopService.GetWorkshopProviderOwnerIdAsync(workshopId).ConfigureAwait(false);

        if (User.IsInRole(nameof(Role.Employee).ToLower()))
        {
            return await _employeeService.CheckUserIsRelatedEmployee(userId, providerId, workshopId).ConfigureAwait(false);
        }
        else
        {
            var provider = await _providerService.GetByUserId(userId).ConfigureAwait(false);
            return providerId == provider?.Id;
        }
    }
}

