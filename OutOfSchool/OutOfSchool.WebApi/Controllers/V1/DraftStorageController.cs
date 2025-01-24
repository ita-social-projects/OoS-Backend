using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Services.DraftStorage;
using System.Net.Mime;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Abstract controller with operations for storing the entity draft in cache.
/// Warning!!! 
/// This abstract controller does not provide permissions for the User. 
/// So anonymous Users can use its actions.
/// For security you have to restrict access to Actions of controllers in classes inherited from this controller.
/// </summary>
/// <typeparam name="T">T is the entity draft type that should be stored in the cache.</typeparam>
public abstract class DraftStorageController<T> : ControllerBase
{
    private readonly IDraftStorageService<T> draftStorageService;

    /// <summary>Initializes a new instance of the <see cref="DraftStorageController{T}"/> class.</summary>
    /// <param name="draftStorageService">The draft storage service.</param>
    protected DraftStorageController(IDraftStorageService<T> draftStorageService)
    {
        this.draftStorageService = draftStorageService;
    }

    /// <summary>Restores the entity draft.
    /// Warning!!! 
    /// For security you have to restrict access to this Action in class inherited from this abstract controller.
    /// </summary>
    /// <returns> The entity draft dto of type T.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestoreDraft()
    {
        var result = await draftStorageService.RestoreAsync(GettingUserProperties.GetUserId(User)).ConfigureAwait(false);

        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>Returns the time remaining until the end of the draft's life.
    /// Warning!!! 
    /// For security you have to restrict access to this Action in class inherited from this abstract controller.
    /// </summary>
    /// <returns>The time remaining until the end of the draft's life.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTimeToLiveOfDraft()
    {
        var result = await draftStorageService.GetTimeToLiveAsync(GettingUserProperties.GetUserId(User)).ConfigureAwait(false);

        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>Stores the entity draft.
    /// Warning!!! 
    /// For security you have to restrict access to this Action in class inherited from this abstract controller.
    /// </summary>
    /// <param name="draftDto">The entity draft dto for type T.</param>
    /// <returns>
    /// Information about the result of storing an entity of type T in the cache.
    /// </returns>
    [HttpPost]
    [Authorize]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StoreDraft([FromBody] T draftDto)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequest(ModelState);
        }

        await draftStorageService.CreateAsync(GettingUserProperties.GetUserId(User), draftDto).ConfigureAwait(false);

        return Ok($"{draftDto.GetType().Name} is stored");
    }

    /// <summary>Removes the entity draft from the cache.
    /// Warning!!! 
    /// For security you have to restrict access to this Action in class inherited from this abstract controller.
    /// </summary>
    /// <returns> Information about removing an entity of type T from the cache.</returns>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveDraft()
    {
        var userId = GettingUserProperties.GetUserId(User);
        await draftStorageService.RemoveAsync(userId).ConfigureAwait(false);

        return NoContent();
    }
}