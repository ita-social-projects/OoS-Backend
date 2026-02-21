using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for Direction entity.
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/directions/")]
[HasPermission(Permissions.SystemManagement)]
public class DirectionController : ControllerBase
{
    private readonly IDirectionService service;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionController"/> class.
    /// </summary>
    /// <param name="service">Service for Direction entity.</param>
    /// <param name="localizer">Localizer.</param>
    public DirectionController(IDirectionService service, IStringLocalizer<SharedResource> localizer)
    {
        this.localizer = localizer;
        this.service = service;
    }

    /// <summary>
    /// To get all Directions from DB.
    /// </summary>
    /// <returns>List of all directions, or no content.</returns>
    /// <response code="200">One or more directions were found.</response>
    /// <response code="204">No direction was found.</response>
    /// <response code="500">If any server error occures.</response>
    [Obsolete("Use paged method")]
    [AllowAnonymous]
    [HttpGet("Get")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DirectionDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        var directions = await service.GetAll().ConfigureAwait(false);

        if (!directions.Any())
        {
            return NoContent();
        }

        return Ok(directions);
    }

    /// <summary>
    /// To get filtered directions from DB.
    /// </summary>
    /// <param name="filter">Filter for directions.</param>
    /// <param name="isAdmins">To filter directions by admin role.</param>
    /// <returns>List of filtered directions, or no content.</returns>
    /// <response code="200">One or more directions were found.</response>
    /// <response code="204">No direction was found.</response>
    /// <response code="500">If any server error occures.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DirectionDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] DirectionFilter filter, bool isAdmins = false) =>
        await service.GetByFilter(filter, isAdmins)
            .ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// To recieve the direction with the defined id.
    /// </summary>
    /// <param name="id">Key of the direction in the table.</param>
    /// <returns><see cref="DirectionDto"/>.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="404">If the entity was not found by given Id.</response>
    /// <response code="500">If any server error occures. For example: Id was wrong.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DirectionDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id)
    {
        this.ValidateId(id, localizer);

        var direction = await service.GetById(id).ConfigureAwait(false);

        if (direction == null)
        {
            return NotFound("Direction with such Id does not exist in the database.");
        }

        return Ok(direction);
    }

    /// <summary>
    /// To create a new direction and add it to the DB.
    /// </summary>
    /// <param name="directionDto">DirectionDto object that will be added.</param>
    /// <returns>Direction that was created.</returns>
    /// <response code="201">Direction was successfully created.</response>
    /// <response code="400">Model is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [FeatureGate(nameof(Feature.DirectionManagement))]
    public async Task<IActionResult> Create([FromBody] DirectionDto directionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            directionDto.Id = default;

            var response = await service.Create(directionDto).ConfigureAwait(false);

            if (response != null)
            {
                if (response.Succeeded)
                {

                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = response.Value.Id, },
                        response.Value);
                }
                else
                {
                    var error = response.OperationResult.Errors.FirstOrDefault();

                    if (error != null)
                    {
                        return error.Code switch
                        {
                            "400" => BadRequest(error.Description),
                            _ => StatusCode(500, "An unexpected error occurred.")
                        };
                    }

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