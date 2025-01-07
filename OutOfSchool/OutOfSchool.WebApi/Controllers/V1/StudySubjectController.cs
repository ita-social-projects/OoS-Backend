using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.BusinessLogic.Services.ProviderServices;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for StudySubject entity
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/providers/{providerId}/studysubjects/[action]")]
public class StudySubjectController : ControllerBase
{
    private readonly IStudySubjectService _studySubjectService;
    private readonly IProviderService _providerService;
    private readonly IWorkshopService _workshopService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudySubjectController"/> class.
    /// </summary>
    /// <param name="studySubjectService">Service for StudySubject model.</param>
    /// <param name="providerService">Service for Provider.</param>
    /// <param name="workshopService">Service for Workshop</param>
    public StudySubjectController(
        IStudySubjectService studySubjectService,
        IProviderService providerService,
        IWorkshopService workshopService)
    {
        _providerService = providerService;
        _studySubjectService = studySubjectService;
        _workshopService = workshopService;
    }

    /// <summary>
    /// Get filtered list of StudySubjects from the database.
    /// </summary>
    /// <returns>List of StudySubjects.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StudySubjectDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SearchStringFilter filter = null)
    {
        var studySubjects = await _studySubjectService.GetByFilter(filter).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(studySubjects);
    }

    /// <summary>
    /// Get StudySubject by it's id.
    /// </summary>
    /// <param name="id">StudySubject's id.</param>
    /// <returns>StudySubject.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var studySubjectDto = await _studySubjectService.GetById(id).ConfigureAwait(false);

        if (studySubjectDto == null)
        {
            return NotFound("StudySubject with such Id does not exist in the database.");
        }

        return Ok(studySubjectDto);
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

        try
        {
            dto.Id = Guid.Empty;

            var creationResult = await _studySubjectService.Create(dto).ConfigureAwait(false);

            if (creationResult != null)
            {

                return CreatedAtAction(
                nameof(GetById),
                new { id = creationResult.Id, },
                creationResult);
            }

            return BadRequest("Creating failed, dto is null");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
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
        if (dto is null)
        {
            return BadRequest("StudySubject dto is null.");
        }

        var isWorkshopExists = await _workshopService.Exists(dto.WorkshopId).ConfigureAwait(false);

        if (!isWorkshopExists)
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

        try
        {
            var response = await _studySubjectService.Update(dto).ConfigureAwait(false);

            if (response.Succeeded)
            {
                return Ok(response.Value);
            }

            var operationError = response.OperationResult.Errors.FirstOrDefault();

            if (operationError != null)
            {
                switch (operationError.Code)
                {
                    case "404":
                        return NotFound(operationError.Description);
                    case "400":
                        return BadRequest(operationError.Description);
                    default:
                        return StatusCode(500, "An unexpected error occurred.");
                }
            }

            return StatusCode(500, "An unexpected error occurred.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a specific StudySubject entity from the database.
    /// </summary>
    /// <param name="id">StudySubject's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        StudySubjectDto dto;

        dto = await _studySubjectService.GetById(id).ConfigureAwait(false);

        if (dto == null)
        {
            return BadRequest("StudySubject dto is null.");
        }

        var providerId = await _providerService.GetProviderIdForWorkshopById(dto.WorkshopId).ConfigureAwait(false);

        if (await _providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false)
        {
            return StatusCode(403, "It is forbidden to add study subjects to workshops at blocked providers");
        }

        try
        {
            var response = await _studySubjectService.Delete(id).ConfigureAwait(false);

            if (response.Succeeded)
            {
                return Ok(response.Value);
            }

            var operationError = response.OperationResult.Errors.FirstOrDefault();

            if (operationError != null)
            {
                switch (operationError.Code)
                {
                    case "404":
                        return NotFound(operationError.Description);
                    case "400":
                        return BadRequest(operationError.Description);
                    default:
                        return StatusCode(500, "An unexpected error occurred.");
                }
            }

            return StatusCode(500, "An unexpected error occurred.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

