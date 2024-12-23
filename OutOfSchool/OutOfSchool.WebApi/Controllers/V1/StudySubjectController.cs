using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services.ProviderServices;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class StudySubjectController : ControllerBase
{
    private readonly ISubjectService _studySubjectService;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly IProviderService _providerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudySubjectController"/> class.
    /// </summary>
    /// <param name="studySubjectService">Service for StudySubject model.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="providerService">Service for Provider.</param>
    public StudySubjectController(
        ISubjectService studySubjectService,
        IStringLocalizer<SharedResource> localizer,
        IProviderService providerService)
    {
        _providerService = providerService;
        _localizer = localizer;
        _studySubjectService = studySubjectService;
    }

    /// <summary>
    /// Get all StudySubjects from the database.
    /// </summary>
    /// <returns>List of StudySubjects.</returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StudySubjectDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var studySubjects = await _studySubjectService.GetAll().ConfigureAwait(false);

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
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create(StudySubjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var creationResult = await _studySubjectService.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = creationResult.Id, },
            creationResult);
    }

    /// <summary>
    /// Update info about a specific StudySubject in the database.
    /// </summary>
    /// <param name="dto">StudySubject to update.</param>
    /// <returns>StudySubject.</returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudySubjectDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(StudySubjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Ok(await _studySubjectService.Update(dto).ConfigureAwait(false));
    }

    /// <summary>
    /// Delete a specific StudySubject entity from the database.
    /// </summary>
    /// <param name="id">StudySubject's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        this.ValidateId(id, _localizer);

        await _studySubjectService.Delete(id).ConfigureAwait(false);
        return NoContent();
    }
}
