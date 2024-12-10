using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class CompetitiveEventController : ControllerBase
{
    private readonly ICompetitiveEventService service;
    private readonly IUserService userService; // need this?
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitiveEventController"/> class.
    /// </summary>
    /// <param name="service">Service for CompetitiveEvent model.</param>
    /// <param name="localizer">Localizer.</param>
    public CompetitiveEventController(ICompetitiveEventService service, IUserService userService, IStringLocalizer<SharedResource> localizer)
    {
        this.service = service;
        this.userService = userService;
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
            return NoContent(); // $"Competitive event with ID {id} not found."
        
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CompetitiveEventUpdateDto dto)
    {
        return Ok(await service.Update(dto).ConfigureAwait(false));
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
    private bool AreJudgesValid(IEnumerable<JudgeDto> judges)
    {
        if (judges != null && judges.Count() > 0)
        {
            var chiefJudgeCount = judges.Count(j => j.IsChiefJudge);
            return chiefJudgeCount <= 1;
        }
        return true;
    }
    //private async Task<bool> IsCurrentUserBlocked() // need smth like that?
    //{
    //    var userId = GettingUserProperties.GetUserId(User);

    //    return await userService.IsBlocked(userId);
    //}

}
