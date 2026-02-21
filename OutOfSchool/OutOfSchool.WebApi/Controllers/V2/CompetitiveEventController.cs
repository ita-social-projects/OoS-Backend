using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Util.ControllersResultsHelpers;

namespace OutOfSchool.WebApi.Controllers.V2;
/// <summary>
/// Controller with CRUD operations for CompetitiveEvent entity.
/// </summary>
[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class CompetitiveEventController : ControllerBase
{
    private readonly ILogger<CompetitiveEventController> logger;
    private readonly ICompetitiveEventServiceV2 competitiveEventService;
    private readonly IUserService userService;
    private readonly ICompetitiveEventDraftService competitiveEventDraftService;

    public CompetitiveEventController(
        ICompetitiveEventServiceV2 competitiveEventService,
        IUserService userService,
        ILogger<CompetitiveEventController> logger,
        ICompetitiveEventDraftService competitiveEventDraftService)
    {
        this.competitiveEventService = competitiveEventService ?? throw new ArgumentNullException(nameof(competitiveEventService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.competitiveEventDraftService = competitiveEventDraftService ?? throw new ArgumentNullException(nameof(competitiveEventDraftService));
    }

    /// <summary>
    /// Get competitive event by it's id.
    /// </summary>
    /// <param name="id">CompetitiveEvent's id.</param>
    /// <returns><see cref="CompetitiveEventV2Dto"/>, or no content.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventV2Dto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var competitiveEvent = await competitiveEventService.GetById(id).ConfigureAwait(false);

        if (competitiveEvent is null)
        {
            return NotFound();
        }

        return Ok(competitiveEvent);
    }

    /// <summary>
    /// Get CompetitiveEvents cards by Provider's Id.
    /// </summary>
    /// <param name="id">CompetitiveEvent's id.</param>
    /// <param name="filter">Filter to get specified portion of competitive events for specified provider</param>
    /// <response code="200">The list of found entities by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<CompetitiveEventViewCardDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByProviderId(Guid id, [FromQuery] CompetitiveEventFilterTitle filter) =>
        await competitiveEventService.GetByProviderId(id, filter).ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// Get CompetitiveEvents cards by list of Ids.
    /// </summary>
    /// <param name="ids">CompetitiveEvent's ids list.</param>
    /// <response code="200">The list of found entities by given Ids.</response>
    /// <response code="204">No entity with any of given Ids was found.</response>
    /// <response code="500">If any server error occures. For example: Id was incorrect format.</response>
    [AllowAnonymous]
    [HttpGet("multipleIds")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<CompetitiveEvent>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIds(IEnumerable<Guid> ids)
    {
        var competitiveEvents = await competitiveEventService.GetByIds(ids).ConfigureAwait(false);

        if (!competitiveEvents.Any())
        {
            return NoContent();
        }
        return this.Ok(competitiveEvents);
    }

    /// <summary>
    /// Add new competitive image with image to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>Created <see cref="CompetitiveEventV2Dto"/>.</returns>
    /// <response code="201">Entity was created and returned with Id.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CompetitiveEventResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CompetitiveEventV2Dto dto)
    {
        var error = dto switch
        {
            null => BadRequest("CompetitiveEvent is null"),
            _ when !ModelState.IsValid => BadRequest(ModelState),
            _ when await IsCurrentUserBlocked() => StatusCode(403, "User is blocked"),
            _ when (dto.ImageFiles ?? []).Count == 0 || (dto.ImageIds ?? []).Count > 0 
            => BadRequest("When creating CompetitiveEvent, the ImageFiles field must contain a non-empty array of images, and the ImageIds field must be null or an empty array."),
            _ => null
        };

        if (error != null)
        {
            return error;
        }

        try
        {
            var creationResult = await competitiveEventService.CreateV2(dto).ConfigureAwait(false);

            return CreatedAtAction(
            nameof(GetById),
            new { id = creationResult.CompetitiveEventV2.Id, },
            new CompetitiveEventResponseDto
            {
                CompetitiveEventV2 = creationResult.CompetitiveEventV2,
                UploadingCoverImageResult = creationResult.UploadingCoverImageResult?.CreateSingleUploadingResult(),
                UploadingImagesResults = creationResult.UploadingImagesResults?.CreateMultipleUploadingResult(),
            });
        }
        catch (InvalidOperationException ex)
        {
            var errorMessage = $"Unable to create a new competitive event: {ex.Message}";
            logger.LogError(ex, errorMessage);

            return BadRequest(errorMessage);
        }
    }

    /// <summary>
    /// Update info about competitive event entity.
    /// </summary>
    /// <param name="dto">CompetitiveEvent to update.</param>
    /// <returns>Updated <see cref="CompetitiveEventV2Dto"/>.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventV2Dto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] CompetitiveEventV2Dto dto)
    {
        try
        {
            var result = await competitiveEventDraftService.UpdateCompetitiveEvent(dto).ConfigureAwait(false);

            if (result is null)
            {
                return BadRequest();
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Unable to update competitive event: {Message}", ex.Message);

            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a specific competitive event from the database
    /// </summary>
    /// <param name="id">CompetitiveEvent's id</param>
    /// <returns>StatusCode representing the task completion</returns>
    /// <response code="204">If the entity was successfully deleted, or if the entity was not found by given Id</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or deletes not own competitive event</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var competitiveEventToDelete = await competitiveEventService.GetById(id).ConfigureAwait(false);

            if (competitiveEventToDelete is null)
            {
                return NoContent();
            }

            await competitiveEventService.DeleteV2(id).ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            var errorMessage = $"Unable to delete a competitive event: {ex.Message}";
            logger.LogError(ex, errorMessage);

            return BadRequest(errorMessage);
        }
    }

    private async Task<bool> IsCurrentUserBlocked()
    {
        var userId = GettingUserProperties.GetUserId(User);

        return await userService.IsBlocked(userId);
    }
}
