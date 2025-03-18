using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for StudySubject entity
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/providers/{providerId}/studysubjects")]
public class StudySubjectController : ControllerBase
{
    private readonly IStudySubjectService _studySubjectService;
    private readonly IWorkshopService _workshopService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudySubjectController"/> class.
    /// </summary>
    /// <param name="studySubjectService">Service for StudySubject model.</param>
    /// <param name="workshopService">WorkshopService</param>
    public StudySubjectController(
        IStudySubjectService studySubjectService, 
        IWorkshopService workshopService)
    {
        _studySubjectService = studySubjectService;
        _workshopService = workshopService;
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
    public async Task<IActionResult> Get(Guid providerId, [FromQuery] StudySubjectFilter filter = null) =>
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

    /// <summary>
    /// Retrieves a list of workshops (ID and title) associated with a given provider.
    /// </summary>
    /// <param name="providerId">The unique identifier of the provider.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a list of <see cref="ShortEntityDto"/> objects, 
    /// where each object includes the ID and title of a workshop.
    /// </returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ShortEntityDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("workshops")]
    public async Task<IActionResult> GetWorkshopListByProviderId(Guid providerId)
    {
        var workshops = await _workshopService.GetWorkshopListByProviderId(providerId);

        if (workshops == null || !workshops.Any())
        {
            return NoContent();
        }

        return Ok(workshops);
    }

    /// <summary>
    /// Retrieves a list of workshops along with their attachment status for a given provider and study subject.
    /// </summary>
    /// <param name="studySubjectId">The unique identifier of the study subject.</param>
    /// <param name="providerId">The unique identifier of the provider.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of <see cref="ShortEntityDto"/> 
    /// representing the workshops associated with the provider.
    /// </returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopAttachmentStatusDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{studySubjectId}/workshops/status")]
    public async Task<IActionResult> GetWorkshopsWithAttachmentStatusByProviderId(Guid studySubjectId, [FromRoute] Guid providerId)
    {
        var result = await _workshopService.GetWorkshopsWithAttachmentStatusByProviderId(studySubjectId, providerId);

        return HandleServiceRespone(result);
    }

    /// <summary>
    /// Updates attachment of the list of workshops associated with a given study subject.
    /// </summary>
    /// <param name="studySubjectId">The unique identifier of the study subject.</param>
    /// <param name="providerId">The unique identifier of the provider performing the operation.</param>
    /// <param name="request">An object containing collections of workshop IDs to attach and detach.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> indicating 
    /// the success or failure of the operation, along with the updated study subject data if successful.
    /// </returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{studySubjectId}/workshops")]
    public async Task<IActionResult> UpdateWorkshopsForStudySubject(
        Guid studySubjectId, 
        Guid providerId, 
        [FromBody] UpdateWorkshopAttachmentDto request)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        var result = await _studySubjectService.UpdateWorkshopsForStudySubject(
            studySubjectId,
            request.WorkshopIdsToAttach,
            request.WorkshopIdsToDetach,
            providerId
        );

        return HandleServiceRespone(result);
    }

    /// <summary>
    /// Detaches all workshops from a specified study subject.
    /// </summary>
    /// <param name="studySubjectId">The unique identifier of the study subject.</param>
    /// <param name="providerId">The unique identifier of the provider performing the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{StudySubjectDto}"/> 
    /// indicating the success or failure of the operation, along with the updated study subject data.
    /// </returns>
    [Authorize]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{studySubjectId}/workshops")]
    public async Task<IActionResult> DetachAllWorkshops(Guid studySubjectId, Guid providerId)
    {
        var result = await _studySubjectService.DetachAllWorkshops(studySubjectId, providerId);
        return HandleServiceRespone(result);
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

