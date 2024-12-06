using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Services.DraftStorage;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>Abstract controller with operations for storing the entity draft in cache.</summary>
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

    /// <summary>Stores the entity draft.</summary>
    /// <param name="draftDto">The entity draft dto for type T.</param>
    /// <returns>
    /// Information about the result of storing an entity of type T in the cache.
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> StoreDraft([FromBody] T draftDto)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequest(ModelState);
        }

        await draftStorageService.CreateAsync(GettingUserProperties.GetUserId(User), draftDto).ConfigureAwait(false);

        return Ok($"{draftDto.GetType().Name} is stored");
    }

    /// <summary>Restores the entity draft.</summary>
    /// <returns> The entity draft dto of type T.</returns>
    [HttpGet]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RestoreDraft()
    {
        var draft = await draftStorageService.RestoreAsync(GettingUserProperties.GetUserId(User)).ConfigureAwait(false);

        return Ok(draft);
    }

    /// <summary>Removes the entity draft from the cache.</summary>
    /// <returns> Information about removing an entity of type T from the cache.</returns>
    [HttpDelete]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RemoveDraft()
    {
        var userId = GettingUserProperties.GetUserId(User);
        await draftStorageService.RemoveAsync(userId).ConfigureAwait(false);

        return NoContent();
    }
}