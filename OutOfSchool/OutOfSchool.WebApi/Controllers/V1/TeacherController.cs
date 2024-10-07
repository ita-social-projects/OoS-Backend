using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for a Teacher entity.
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[HasPermission(Permissions.SystemManagement)]
public class TeacherController : ControllerBase // check permissions for workshopIds for public controller
{
    private readonly ITeacherService teacherService;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IProviderService providerService;
    private readonly IEmployeeService employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeacherController"/> class.
    /// </summary>
    /// <param name="teacherService">Service for Teacher model.</param>
    /// <param name="localizer">Localizer.</param
    /// <param name="providerService">Service for Provider.</param>
    /// <param name="employeeService">Service For Employee.</param>
    public TeacherController(
        ITeacherService teacherService,
        IStringLocalizer<SharedResource> localizer,
        IEmployeeService employeeService,
        IProviderService providerService)
    {
        this.employeeService = employeeService;
        this.providerService = providerService;
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
        try
        {
            var teacherDTO = await teacherService.GetById(id).ConfigureAwait(false);
            return Ok(teacherDTO);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Add a new teacher to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.TeacherAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create(TeacherDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!(await IsUserWorkshopOwnerOrAdmin(dto.WorkshopId).ConfigureAwait(false)))
        {
            return StatusCode(403, $"Forbidden to create teachers related to workshop withId - {dto.WorkshopId}.");
        }

        var creationResult = await teacherService.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = creationResult.Teacher.Id, },
            creationResult);
    }

    /// <summary>
    /// Update info about a specific teacher in the database.
    /// </summary>
    /// <param name="dto">Teacher to update.</param>
    /// <returns>Teacher.</returns>
    [HasPermission(Permissions.TeacherEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeacherDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(TeacherDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var workshopId = dto.WorkshopId;

        if (!(await IsUserWorkshopOwnerOrAdmin(workshopId).ConfigureAwait(false)))
        {
            return StatusCode(403, $"Forbidden to update teachers related to workshop withId- {workshopId}.");
        }

        return Ok(await teacherService.Update(dto).ConfigureAwait(false));
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
        var teachersWorkshopId = await teacherService.GetTeachersWorkshopId(id).ConfigureAwait(false);
        if (!(await IsUserWorkshopOwnerOrAdmin(teachersWorkshopId).ConfigureAwait(false)))
        {
            return StatusCode(403, $"Forbidden to delete teachers related to workshop withId - {teachersWorkshopId}.");
        }

        await teacherService.Delete(id).ConfigureAwait(false);

        return NoContent();
    }

    // method checks whether current user:
    // - is in role provider,
    // - owns workshop with specified id or is related provider admin
    private async Task<bool> IsUserWorkshopOwnerOrAdmin(Guid workshopId)
    {
        if (User.IsInRole(nameof(Role.Provider).ToLower()))
        {
            var providerId = await providerService.GetProviderIdForWorkshopById(workshopId).ConfigureAwait(false);
            var userId = User.FindFirst("sub")?.Value;
            try
            {
                var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);

                if (providerId != provider?.Id)
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                try
                {
                    var isUserRelatedAdmin = await employeeService.CheckUserIsRelatedEmployee(userId, providerId, workshopId).ConfigureAwait(false);
                    if (!isUserRelatedAdmin)
                    {
                        return false;
                    }
                }
                catch (NullReferenceException)
                {
                    return false;
                }

            }
        }

        return true;
    }
}