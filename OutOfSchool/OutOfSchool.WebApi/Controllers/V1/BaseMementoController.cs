using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>Base Controller with operations for storing data in cache.</summary>
/// <typeparam name="T">T is the entity type that should be stored in the cache.</typeparam>
public abstract class BaseMementoController<T> : ControllerBase
{
    private readonly IMementoService<T> mementoService;

    /// <summary>Initializes a new instance of the <see cref="BaseMementoController{T}" /> class.</summary>
    /// <param name="mementoService">The memento service.</param>
    protected BaseMementoController(IMementoService<T> mementoService)
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
        await mementoService.CreateMemento(GettingUserProperties.GetUserId(User), mementoDto);
        return Ok($"{typeof(T).Name} is stored");
    }

    /// <summary>Restores the memento.</summary>
    /// <returns> The memento dto of type T.</returns>
    [HttpGet]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RestoreMemento()
    {
        var memento = await mementoService.RestoreMemento(GettingUserProperties.GetUserId(User));
        return Ok(memento);
    }

    /// <summary>Removes the memento fron the cache.</summary>
    /// <returns> Information about removing an entity of type T from the cache.</returns>
    [HttpGet]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RemoveMemento()
    {
        var userId = GettingUserProperties.GetUserId(User);
        await mementoService.RemoveMementoAsync(userId);
        return Ok($"{typeof(T).Name} for User with Id = {userId} has been removed");
    }
}
