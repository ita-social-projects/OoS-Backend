using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Services.TempSave;
using System.Net.Mime;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// The abstract controller with operations for temporarily saving an entity in the cache when it is created.
/// Warning!!! 
/// This abstract controller does not provide permissions for the User. 
/// So anonymous Users can use its actions.
/// For security you have to restrict access to Actions of controllers in classes inherited from this controller.
/// </summary>
/// <typeparam name="T">T is the entity type that should be stored in the cache.</typeparam>
public abstract class TempSaveController<T> : ControllerBase
{
    private readonly ITempSaveService<T> tempSaveService;

    /// <summary>Initializes a new instance of the <see cref="TempSaveController{T}"/> class.</summary>
    /// <param name="tempSaveService">The service for temporarily saving entity in the cache when it is created.</param>
    protected TempSaveController(ITempSaveService<T> tempSaveService)
    {
        this.tempSaveService = tempSaveService;
    }

    /// <summary>Restores the saved entity dto value from the cache.
    /// Warning!!! 
    /// For security you have to restrict access to this Action in class inherited from this abstract controller.
    /// </summary>
    /// <returns> The entity dto of type T.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Restore()
    {
        var result = await tempSaveService.RestoreAsync(GettingUserProperties.GetUserId(User)).ConfigureAwait(false);

        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>Returns the time remaining until the end of the entity dto life.
    /// Warning!!!
    /// For security you have to restrict access to this Action in class inherited from this abstract controller.
    /// </summary>
    /// <returns>The time remaining until the end of the entity dto life.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTimeToLive()
    {
        var result = await tempSaveService.GetTimeToLiveAsync(GettingUserProperties.GetUserId(User)).ConfigureAwait(false);

        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>Stores the entity dto value.
    /// Warning!!! 
    /// For security you have to restrict access to this Action in class inherited from this abstract controller.
    /// </summary>
    /// <param name="dto">The entity dto of type T.</param>
    /// <returns>
    /// Information about the result of storing an entity dto value of type T in the cache.
    /// </returns>
    [HttpPost]
    [Authorize]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Store([FromBody] T dto)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequest(ModelState);
        }

        await tempSaveService.StoreAsync(GettingUserProperties.GetUserId(User), dto).ConfigureAwait(false);

        return Ok($"{dto.GetType().Name} is stored");
    }

    /// <summary>Removes the entity dto value from the cache.
    /// Warning!!! 
    /// For security you have to restrict access to this Action in class inherited from this abstract controller.
    /// </summary>
    /// <returns> Information about removing the entity dto value of type T from the cache.</returns>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remove()
    {
        var userId = GettingUserProperties.GetUserId(User);
        await tempSaveService.RemoveAsync(userId).ConfigureAwait(false);

        return NoContent();
    }
}