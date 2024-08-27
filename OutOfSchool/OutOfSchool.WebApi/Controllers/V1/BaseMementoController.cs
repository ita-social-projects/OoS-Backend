using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>Base Controller with operations for storing data in cache.</summary>
/// <typeparam name="T">T is the entity type that should be stored in the cache.</typeparam>
public abstract class BaseMementoController<T> : ControllerBase
{
    private readonly IMementoService<T> mementoService;
    private readonly IStorage storage;

    /// <summary>Initializes a new instance of the <see cref="BaseMementoController{T}" /> class.</summary>
    /// <param name="mementoService">The memento service.</param>
    /// <param name="storage">The storage.</param>
    /// <exception cref="System.ArgumentNullException">crudCacheService
    /// or
    /// mementoService
    /// or
    /// storage.</exception>
    protected BaseMementoController(
        IMementoService<T> mementoService,
        IStorage storage)
    {
        this.mementoService = mementoService ?? throw new ArgumentNullException(nameof(mementoService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <summary>Stores the memento.</summary>
    /// <param name="mementoDto">The memento dto for type T.</param>
    /// <returns>
    /// Information about storing an entity of type T in the cache.
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> StoreMemento([FromBody] T mementoDto)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequest(ModelState);
        }

        var userId = GettingUserProperties.GetUserId(User);
        var memento = mementoService.CreateMemento(userId, mementoDto);
        await storage.SetMementoValueAsync(memento.State);
        return Ok($"{typeof(T).Name} is stored");
    }

    /// <summary>Restores the memento.</summary>
    /// <returns>
    /// The memento dto for type T.
    /// </returns>
    [HttpGet]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RestoreMemento()
    {
        var mementoKey = mementoService.GetMementoKey(GettingUserProperties.GetUserId(User));
        mementoService.RestoreMemento(await storage.GetMementoValueAsync(mementoKey));
        return Ok(mementoService.State);
    }

    /// <summary>Removes the memento.</summary>
    /// <returns>
    /// Information about removing an entity of type T from the cache.
    /// </returns>
    [HttpGet]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RemoveMemento()
    {
        var mementoKey = mementoService.GetMementoKey(GettingUserProperties.GetUserId(User));
        await storage.RemoveMementoAsync(mementoKey);
        return Ok($"{typeof(T).Name} with key = {mementoKey} has been removed");
    }
}
