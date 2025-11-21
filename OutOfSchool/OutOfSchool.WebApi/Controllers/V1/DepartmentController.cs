using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Department;
using OutOfSchool.Common.PermissionsModule;

namespace OutOfSchool.WebApi.Controllers.V1;

[Route("api/v{version:apiVersion}/departments/[action]")]
[Authorize(Roles = "provider")]
[ApiController]
public class DepartmentController : Controller
{
    private readonly ILogger<DepartmentController> logger;
    private readonly IDepartmentService departmentService;

    public DepartmentController(
        IDepartmentService departmentService,
        ILogger<DepartmentController> logger
        )
    {
        this.departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new department for the current provider
    /// </summary>
    /// <param name="providerId">The ID of the provider.</param>
    /// <param name="createDto">The department data to create</param>
    /// <returns>The created department.</returns>
    /// <response code="201">If the department was successfully created.</response>
    /// <response code="400">If the request data is invalid or validation fails.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occurs.</response>
    [HttpPost]
    [HasPermission(Permissions.DepartmentAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DepartmentDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(Guid providerId, [FromBody] DepartmentCreateUpdateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (createDto == null)
        {
            return BadRequest("Department data cannot be null.");
        }

        try
        {
            var createdDepartment = await departmentService.CreateAsync(createDto, providerId).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { providerId, departmentId = createdDepartment.Id },
                createdDepartment);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Validation error occurred while creating department.");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating department.");
            return StatusCode(500, "An error occurred while creating the department.");
        }
    }

    /// <summary>
    /// Retrieves all departments for the provider with filters.
    /// </summary>
    /// <param name="providerId">The ID of the provider.</param>
    /// <param name="filter">Parameters to filter the departments.</param>
    /// <returns><see cref="SearchResult{DepartmentDto}"/>.</returns>
    /// <response code="200">If departments were found and returned successfully.</response>
    /// <response code="204">If no departments were found.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occurs.</response>
    [HttpGet]
    [HasPermission(Permissions.DepartmentRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<DepartmentDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter(Guid providerId, [FromQuery] DepartmentFilter filter) =>
        await departmentService.GetByFilter(providerId, filter).ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// Retrieves a specific department by its ID.
    /// </summary>
    /// <param name="providerId">The ID of the provider.</param>
    /// <param name="departmentId">The ID of the department to get.</param>
    /// <returns>The department details.</returns>
    /// <response code="200">If the department was found and returned successfully.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="404">If the department was not found.</response>
    /// <response code="500">If any server error occurs.</response>
    [HttpGet("{departmentId}")]
    [HasPermission(Permissions.DepartmentRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid providerId, Guid departmentId)
    {
        try
        {
            var department = await departmentService.GetByIdAsync(departmentId, providerId).ConfigureAwait(false);
            if (department == null)
            {
                return NotFound($"Department with id {departmentId} was not found.");
            }

            return Ok(department);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving department {DepartmentId}", departmentId);
            return StatusCode(500, "An error occurred while retrieving the department.");
        }
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    /// <param name="updateDto">The updated department data.</param>
    /// <param name="providerId">The ID of the provider.</param>
    /// <param name="departmentId">The ID of the department to update.</param>
    /// <returns>The updated department.</returns>
    /// <response code="200">If the department was successfully updated.</response>
    /// <response code="400">If the request data is invalid or validation fails.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="404">If the department was not found.</response>
    /// <response code="500">If any server error occurs.</response>
    [HttpPut("{departmentId}")]
    [HasPermission(Permissions.DepartmentEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody] DepartmentCreateUpdateDto updateDto, Guid providerId, Guid departmentId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (updateDto == null)
        {
            return BadRequest("Department data cannot be null.");
        }

        try
        {
            var updatedDepartment = await departmentService.UpdateAsync(departmentId, updateDto, providerId).ConfigureAwait(false);
            return Ok(updatedDepartment);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Department {DepartmentId} not found for update", departmentId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Validation error occurred while updating department {DepartmentId}", departmentId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating department {DepartmentId}", departmentId);
            return StatusCode(500, "An error occurred while updating the department.");
        }
    }

    /// <summary>
    /// Deletes a specific department.
    /// </summary>
    /// <param name="providerId">The ID of the provider.</param>
    /// <param name="departmentId">The ID of the department to delete.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the department was successfully deleted.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="409">If the department has active dependencies (child departments) that block deletion.</response>
    /// <response code="500">If any server error occurs.</response>
    [HttpDelete("{departmentId}")]
    [HasPermission(Permissions.DepartmentRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid providerId, Guid departmentId)
    {
        try
        {
            await departmentService.DeleteAsync(departmentId, providerId).ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Cannot delete department {DepartmentId} due to active dependencies", departmentId);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting department {DepartmentId}", departmentId);
            return StatusCode(500, "An error occurred while deleting the department.");
        }
    }
}
