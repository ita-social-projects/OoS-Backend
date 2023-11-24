using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Controllers.V1.SubordinationStructure;

/// <summary>
/// Controller with CRUD operations for InstitutionHierarchy entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class InstitutionHierarchyController : Controller
{
    private readonly IInstitutionHierarchyService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionHierarchyController"/> class.
    /// </summary>
    /// <param name="service">Service for InstitutionHierarchy entity.</param>
    public InstitutionHierarchyController(IInstitutionHierarchyService service)
    {
        this.service = service;
    }

    /// <summary>
    /// To get all InstitutionHierarchy from DB.
    /// </summary>
    /// <returns>List of all InstitutionHierarchy, or no content.</returns>
    /// <response code="200">One or more InstitutionHierarchy were found.</response>
    /// <response code="204">No InstitutionHierarchy was found.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<InstitutionHierarchyDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var institutionHierarchies = await service.GetAll().ConfigureAwait(false);

        if (!institutionHierarchies.Any())
        {
            return NoContent();
        }

        return Ok(institutionHierarchies);
    }

    /// <summary>
    /// Get InstitutionHierarchy by it's id.
    /// </summary>
    /// <param name="id">InstitutionHierarchy id.</param>
    /// <returns>InstitutionHierarchy.</returns>
    /// <response code="200">Returns InstitutionHierarchy.</response>
    /// <response code="400">Id was wrong.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InstitutionHierarchyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await service.GetById(id).ConfigureAwait(false));
    }

    /// <summary>
    /// Get all children InstitutionHierarchy by it's id.
    /// </summary>
    /// <param name="id">InstitutionHierarchy id.</param>
    /// <returns>List of InstitutionHierarchy.</returns>
    /// <response code="200">Returns InstitutionHierarchies.</response>
    /// <response code="400">Id was wrong.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpGet("{id?}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InstitutionHierarchyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChildren(Guid? id)
    {
        return Ok(await service.GetChildren(id).ConfigureAwait(false));
    }

    /// <summary>
    /// Get all parents InstitutionHierarchy by it's id.
    /// </summary>
    /// <param name="id">InstitutionHierarchy id.</param>
    /// <param name="includeCurrentLevel">Include current child's level to result.</param>
    /// <returns>List<InstitutionHierarchy></InstitutionHierarchy>.</returns>
    /// <response code="200">Returns InstitutionHierarchy.</response>
    /// <response code="400">Id was wrong.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InstitutionHierarchyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetParents(Guid id, bool includeCurrentLevel = false)
    {
        return Ok(await service.GetParents(id, includeCurrentLevel).ConfigureAwait(false));
    }

    /// <summary>
    /// Get all InstitutionHierarchies by institution id and hierarchy level.
    /// </summary>
    /// <param name="institutionId">Institution id.</param>
    /// <param name="hierarchyLevel">Hierarchy level.</param>
    /// <returns>List of InstitutionHierarchy.</returns>
    /// <response code="200">Returns InstitutionHierarchies.</response>
    /// <response code="400">Id was wrong.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InstitutionHierarchyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllByInstitutionAndLevel(Guid institutionId, int hierarchyLevel)
    {
        return Ok(await service.GetAllByInstitutionAndLevel(institutionId, hierarchyLevel).ConfigureAwait(false));
    }

    /// <summary>
    /// Add a new InstitutionHierarchy to the database.
    /// </summary>
    /// <param name="institutionHierarchyDto">InstitutionHierarchy entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    /// <response code="201">InstitutionHierarchy was successfully created.</response>
    /// <response code="400">InstitutionHierarchyDto was wrong.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.SystemManagement)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(InstitutionHierarchyDto institutionHierarchyDto)
    {
        var institutionHierarchy = await service.Create(institutionHierarchyDto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = institutionHierarchy.Id, },
            institutionHierarchy);
    }

    /// <summary>
    /// To update InstitutionHierarchy entity that already exists.
    /// </summary>
    /// <param name="institutionHierarchyDto">InstitutionHierarchyDto object with new properties.</param>
    /// <returns>InstitutionHierarchy that was updated.</returns>
    /// <response code="200">InstitutionHierarchy was successfully updated.</response>
    /// <response code="400">Model is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.SystemManagement)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InstitutionHierarchyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(InstitutionHierarchyDto institutionHierarchyDto)
    {
        return Ok(await service.Update(institutionHierarchyDto).ConfigureAwait(false));
    }

    /// <summary>
    /// Delete the InstitutionHierarchy entity from DB.
    /// </summary>
    /// <param name="id">The key of the InstitutionHierarchy in table.</param>
    /// <returns>Status Code.</returns>
    /// <response code="204">InstitutionHierarchy was successfully deleted.</response>
    /// <response code="400">Id was wrong.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.SystemManagement)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await service.Delete(id).ConfigureAwait(false);

        return NoContent();
    }
}