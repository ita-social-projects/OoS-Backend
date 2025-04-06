using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using System.Net.Mime;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for SubDirection entity.
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/directions/{directionId:long}/subdirections/")]
[HasPermission(Permissions.SystemManagement)]
public class SubDirectionController : ControllerBase
{
    private readonly ISubDirectionService _subDirectionService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubDirectionController"/> class.
    /// </summary>
    /// <param name="subDirectionService">Service for subdirections.</param>
    public SubDirectionController(ISubDirectionService subDirectionService, IStringLocalizer<SharedResource> localizer)
    {
        this._subDirectionService = subDirectionService;
        this._localizer = localizer;
    }

    /// <summary>
    /// To get filtered subdirections from DB.
    /// </summary>
    /// <param name="directionId">Id of the related direction.</param>
    /// <param name="filter">Filter for subdirections.</param>
    /// <returns>List of filtered directions, or no content.</returns>
    /// <response code="200">One or more subdirections were found.</response>
    /// <response code="204">No subdirection was found.</response>
    /// <response code="500">If any server error occures.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SubDirectionDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromRoute] long directionId, [FromQuery] SearchStringFilter filter) =>
        await _subDirectionService.GetByFilter(directionId, filter)
            .ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// To recieve the subdirection with the defined id.
    /// </summary>
    /// <param name="id">Key of the subdirection in the table.</param>
    /// <returns><see cref="SubDirectionDto"/>.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="404">If the entity was not found by given Id.</response>
    /// <response code="500">If any server error occures. For example: Id was wrong.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubDirectionDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id)
    {
        this.ValidateId(id, _localizer);

        var subDirection = await _subDirectionService.GetById(id).ConfigureAwait(false);

        if (subDirection == null)
        {
            return NotFound("SubDirection with such Id does not exist in the database.");
        }

        return Ok(subDirection);
    }

    /// <summary>
    /// To create a new subdirection and add it to the DB.
    /// </summary>
    /// <param name="directionId">Id of the related direction.</param>
    /// <param name="subDirectionDto">SubDirectionDto object that will be added.</param>
    /// <returns>SubDirection that was created.</returns>
    /// <response code="201">SubDirection was successfully created.</response>
    /// <response code="400">Model is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="404">If the direction with such id does not exist.</response>
    /// <response code="500">If any server error occures.</response>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromRoute] long directionId, [FromBody] SubDirectionDto subDirectionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            subDirectionDto.Id = default;

            var response = await _subDirectionService.Create(directionId, subDirectionDto).ConfigureAwait(false);

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
                            "404" => NotFound(error.Description),
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
