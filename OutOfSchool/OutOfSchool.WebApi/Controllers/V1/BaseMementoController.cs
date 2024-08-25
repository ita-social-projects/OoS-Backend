using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Base Controller with operations for storing data in cache.
/// </summary>
public abstract class BaseMementoController<T> : ControllerBase
{
    private readonly ICrudCacheService crudCacheService;
    private readonly IMementoService<T> mementoService;
    private readonly IStorage storage;

    public BaseMementoController(
        ICrudCacheService crudCacheService,
        IMementoService<T> mementoService,
        IStorage storage)
    {
        this.crudCacheService = crudCacheService ?? throw new ArgumentNullException(nameof(crudCacheService));
        this.mementoService = mementoService ?? throw new ArgumentNullException(nameof(mementoService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [HttpPost]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> StoreMemento([FromBody] T mementoDto)
    {
        var userId = GettingUserProperties.GetUserId(User);
        var memento = mementoService.CreateMemento(userId, mementoDto);
        await storage.SetMementoValueAsync(memento.State);
        return Ok(string.Format("{0} is stored", typeof(T).Name));
    }

    [HttpGet]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RestoreMemento()
    {
        var mementoKey = mementoService.GetMementoKey(GettingUserProperties.GetUserId(User));
        mementoService.RestoreMemento(await storage.GetMementoValueAsync(mementoKey));
        return Ok(mementoService.State);
    }

    [HttpGet]
    [Authorize(Roles = "provider, ministryadmin, areaadmin, regionadmin, techadmin")]
    public async Task<IActionResult> RemoveMemento()
    {
        var mementoKey = mementoService.GetMementoKey(GettingUserProperties.GetUserId(User));
        await storage.RemoveMementoAsync(mementoKey);
        return Ok(string.Format("{0} deletion process is completed", typeof(T).Name));
    }
}
