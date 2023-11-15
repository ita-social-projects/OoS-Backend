using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Controllers.V1.SubordinationStructure;

/// <summary>
/// Controller with CRUD operations for Institution entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class InstitutionFieldDescriptionController : Controller
{
    private readonly IInstitutionFieldDescriptionService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionFieldDescriptionController"/> class.
    /// </summary>
    /// <param name="service">Service for InstitutionFieldDescription entity.</param>
    public InstitutionFieldDescriptionController(IInstitutionFieldDescriptionService service)
    {
        this.service = service;
    }

    /// <summary>
    /// Get all InstitutionFieldDescriptions by institution id.
    /// </summary>
    /// <param name="id">Institution id.</param>
    /// <returns>List of InstitutionFieldDescriptionDto.</returns>
    /// <response code="200">Returns InstitutionFieldDescriptions.</response>
    /// <response code="400">Id was wrong.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InstitutionFieldDescriptionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByInstitutionId(Guid id)
    {
        return Ok(await service.GetByInstitutionId(id).ConfigureAwait(false));
    }
}