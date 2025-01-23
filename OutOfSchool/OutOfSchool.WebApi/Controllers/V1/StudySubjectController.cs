using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="StudySubjectController"/> class.
    /// </summary>
    /// <param name="studySubjectService">Service for StudySubject model.</param>
    public StudySubjectController(
        IStudySubjectService studySubjectService)
    {
        _studySubjectService = studySubjectService;
    }

    /// <summary>
    /// Get filtered list of StudySubjects from the database.
    /// </summary>
    /// <param name="providerId">Providers' id</param>
    /// <param name="filter">Filter for list of study subjects</param>
    /// <returns>List of StudySubjects.</returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StudySubjectDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get(Guid providerId, [FromQuery] SearchStringFilter filter = null) =>
        await _studySubjectService.GetByFilter(providerId, filter)
            .ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// Get StudySubject by it's id.
    /// </summary>
    /// <param name="id">StudySubject's id.</param>
    /// <param name="providerId">Providers' id</param>
    /// <returns>StudySubject.</returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, Guid providerId)
    {
        var studySubjectDto = await _studySubjectService.GetById(id, providerId).ConfigureAwait(false);

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
    /// <param name="providerId">Providers' id</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StudySubjectCreateUpdateDto dto, Guid providerId)
    {
        if (dto is null)
        {
            return BadRequest("StudySubject dto is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            dto.Id = Guid.Empty;

            var creationResult = await _studySubjectService.Create(dto, providerId).ConfigureAwait(false);

            if (creationResult != null)
            {

                return CreatedAtAction(
                nameof(GetById),
                new { id = creationResult.Id, providerId = providerId },
                creationResult);
            }

            return BadRequest("Creating failed, dto is null");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, "It is forbidden to add study subjects to workshops for other providers");
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
    /// <param name="providerId">Providers' id</param>
    /// <returns>StudySubject.</returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] StudySubjectCreateUpdateDto dto, Guid providerId)
    {
        if (dto is null)
        {
            return BadRequest("StudySubject dto is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _studySubjectService.Update(dto, providerId).ConfigureAwait(false);
            return HandleServiceRespone(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, "It is forbidden to update study subjects to workshops for other providers");
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
    /// <param name="providerId">Providers' id</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, Guid providerId)
    {
        try
        {
            var dto = await _studySubjectService.GetById(id, providerId).ConfigureAwait(false);

            if (dto == null)
            {
                return NotFound("StudySubject with such Id does not exist in the database.");
            }

            var response = await _studySubjectService.Delete(id, providerId).ConfigureAwait(false);
            return HandleServiceRespone(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, "It is forbidden to delete study subjects to workshops for other providers");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private IActionResult HandleServiceRespone<T>(Result<T> response)
    {
        if (response.Succeeded)
        {
            return Ok(response.Value);
        }

        var operationError = response.OperationResult.Errors.FirstOrDefault();

        if (operationError != null)
        {
            return operationError.Code switch
            {
                "404" => NotFound(operationError.Description),
                "400" => BadRequest(operationError.Description),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }

        return StatusCode(500, "An unexpected error occurred.");
    }
}

