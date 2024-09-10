using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>Base Controller with operations for storing data in cache.</summary>
/// <typeparam name="T">T is the entity type that should be stored in the cache.</typeparam>
public abstract class MementoController<T> : ControllerBase
{
    private readonly IDraftStorageService<T> mementoService;

    /// <summary>Initializes a new instance of the <see cref="MementoController{T}" /> class.</summary>
    /// <param name="mementoService">The memento service.</param>
    protected MementoController(IDraftStorageService<T> mementoService)
    {
        this.mementoService = mementoService;
    }

    /// <summary>Stores the memento.</summary>
    /// <param name="mementoDto">The memento dto for type T.</param>
    /// <returns>
    /// Information about the stored entity of type T in the cache.
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> StoreMemento([FromBody] T mementoDto)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequest(ModelState);
        }

        await mementoService.CreateAsync(GettingUserProperties.GetUserId(User), mementoDto).ConfigureAwait(false);

        return Ok($"{typeof(T).Name} is stored");
    }

    /// <summary>Restores the memento.</summary>
    /// <returns> The memento dto of type T.</returns>
    [HttpGet]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RestoreMemento()
    {
        var memento = await mementoService.RestoreAsync(GettingUserProperties.GetUserId(User)).ConfigureAwait(false);

        return Ok(memento);
    }

    /// <summary>Removes the memento from the cache.</summary>
    /// <returns> Information about removing an entity of type T from the cache.</returns>
    [HttpDelete]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RemoveMemento()
    {
        var userId = GettingUserProperties.GetUserId(User);
        await mementoService.RemoveAsync(userId).ConfigureAwait(false);

        return Ok($"{typeof(T).Name} for User with Id = {userId} has been removed");
    }
}
