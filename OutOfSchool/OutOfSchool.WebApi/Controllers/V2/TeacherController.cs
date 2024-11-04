using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Teachers;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Util.ControllersResultsHelpers;

namespace OutOfSchool.WebApi.Controllers.V2;

/// <summary>
/// Controller with CRUD operations for a Teacher entity.
/// </summary>
[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[HasPermission(Permissions.SystemManagement)]
public class TeacherController : ControllerBase // check permissions for workshopIds for public controller
{
    private readonly ITeacherService teacherService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeacherController"/> class.
    /// </summary>
    /// <param name="teacherService">Service for Teacher model.</param>
    /// <param name="localizer">Localizer.</param>
    public TeacherController(ITeacherService teacherService, IStringLocalizer<SharedResource> localizer)
    {
        this.localizer = localizer;
        this.teacherService = teacherService;
    }

    /// <summary>
    /// Get all teachers from the database.
    /// </summary>
    /// <returns>List of teachers.</returns>
    [HasPermission(Permissions.TeacherRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TeacherDTO>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var teachers = await teacherService.GetAll().ConfigureAwait(false);

        if (!teachers.Any())
        {
            return NoContent();
        }

        return Ok(teachers);
    }

    /// <summary>
    /// Get teacher by it's id.
    /// </summary>
    /// <param name="id">Teacher's id.</param>
    /// <returns>Teacher.</returns>
    [HasPermission(Permissions.TeacherRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeacherDTO))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var teacher = await teacherService.GetById(id).ConfigureAwait(false);

        return teacher != null ? (IActionResult)Ok(teacher) : NoContent();
    }

    /// <summary>
    /// Add a new teacher to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.TeacherAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TeacherCreationResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] TeacherDTO dto)
    {
        var creationResult = await teacherService.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = creationResult.Teacher.Id, },
            new TeacherCreationResponse
            {
                Teacher = creationResult.Teacher,
                UploadingAvatarImageResult = creationResult.UploadingAvatarImageResult?.CreateSingleUploadingResult(),
            });
    }

    /// <summary>
    /// Update info about a specific teacher in the database.
    /// </summary>
    /// <param name="dto">Teacher to update.</param>
    /// <returns>Teacher.</returns>
    [HasPermission(Permissions.TeacherEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeacherUpdateResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    [HttpPut]
    public async Task<IActionResult> Update([FromForm] TeacherDTO dto)
    {
        var updateResult = await teacherService.Update(dto).ConfigureAwait(false);

        return Ok(new TeacherUpdateResponse
        {
            Teacher = updateResult.Teacher,
            UploadingAvatarImageResult = updateResult.UploadingAvatarImageResult?.CreateSingleUploadingResult(),
        });
    }

    /// <summary>
    /// Delete a specific Teacher entity from the database.
    /// </summary>
    /// <param name="id">Teacher's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.TeacherRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await teacherService.Delete(id).ConfigureAwait(false);

        return NoContent();
    }
}