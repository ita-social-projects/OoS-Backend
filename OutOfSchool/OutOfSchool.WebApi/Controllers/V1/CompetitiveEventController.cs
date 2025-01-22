using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class CompetitiveEventController : ControllerBase
{
    private readonly ICompetitiveEventService service;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitiveEventController"/> class.
    /// </summary>
    /// <param name="service">Service for CompetitiveEvent model.</param>
    /// <param name="localizer">Localizer.</param>
    public CompetitiveEventController(ICompetitiveEventService service,/* IUserService userService,*/ IStringLocalizer<SharedResource> localizer)
    {
        this.service = service;
        this.localizer = localizer;
    }

    /// <summary>
    /// Get CompetitiveEvent by it's id.
    /// </summary>
    /// <param name="id">CompetitiveEvent id.</param>
    /// <returns>CompetitiveEvent.</returns>
    [HasPermission(Permissions.CompetitiveEventRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var competitiveEvent = await service.GetById(id).ConfigureAwait(false);

        if (competitiveEvent == null)
            return NoContent(); // Competitive event with ID {id} not found
        
        return Ok(competitiveEvent);
    }

    /// <summary>
    /// Add a new CompetitiveEvent to the database.
    /// </summary>
    /// <param name="dto">CompetitiveEvent entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.CompetitiveEventAddNew)]
    [Authorize]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CompetitiveEventCreateDto dto)
    {
        if (dto == null)
        {
            return BadRequest("The request body is empty.");
        }
        if (!AreJudgesValid(dto.Judges))
        {
            return BadRequest("A competitive event can have no more than one chief judge.");
        }
        var competitiveEvent = await service.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = competitiveEvent.Id, },
            competitiveEvent);
    }

    /// <summary>
    /// Update info about a CompetitiveEvent in the database.
    /// </summary>
    /// <param name="dto">CompetitiveEvent to update.</param>
    /// <returns>Favorite.</returns>
    [HasPermission(Permissions.CompetitiveEventEdit)]
    [Authorize]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CompetitiveEventUpdateDto dto)
    {
        if (dto == null)
        {
            return BadRequest("The request body is empty.");
        }

        dto.Judges ??= new List<JudgeDto>();
        dto.CompetitiveEventDescriptionItems ??= new List<CompetitiveEventDescriptionItemDto>();

        if (!AreJudgesValid(dto.Judges))
        {
            return BadRequest("A competitive event can have no more than one chief judge.");
        }
        try
        {
            return Ok(await service.Update(dto).ConfigureAwait(false));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // For other unexpected exceptions
            return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred. {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a specific CompetitiveEvent from the database.
    /// </summary>
    /// <param name="id">CompetitiveEvent id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.CompetitiveEventRemove)]
    [Authorize] 
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var competitiveEvent = await service.GetById(id).ConfigureAwait(false);

        if (competitiveEvent is null)
        {
            return NoContent();
        }

        await service.Delete(id).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a list of competitive event view cards for a specific provider.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the provider. This value must not be an empty GUID.
    /// </param>
    /// <param name="filter">
    /// Filter to get specified portion of competitive events' view cards for specified provider, Id of the excluded workshop could be specified.
    /// If <see cref="ExcludeIdFilter.ExcludedId"/> is provided, it must not be an empty GUID.
    /// </param>
    /// <returns>
    /// <see cref="SearchResult{CompetitiveEventViewCardDto}"/>, or no content
    /// </returns>
    [HasPermission(Permissions.CompetitiveEventRead)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<CompetitiveEventViewCardDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v{version:apiVersion}/provider/{id}/competitiveevents")]
    public async Task<IActionResult> GetCompetitiveEventViewCardByProviderId(Guid id, [FromQuery] ExcludeIdFilter filter)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Provider id is empty.");
        }
       
        SearchResult<CompetitiveEventViewCardDto> result = await service.GetByProviderId(id, filter).ConfigureAwait(false);
        return this.SearchResultToOkOrNoContent(result);

    }
    private static bool AreJudgesValid(IEnumerable<JudgeDto> judges)
    {
        if (judges != null && judges.Any())
        {
            var chiefJudgeCount = judges.Count(j => j.IsChiefJudge);
            return chiefJudgeCount <= 1;
        }
        return true;
    }
}
