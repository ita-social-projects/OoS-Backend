using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Memento;

/// <summary>
/// Implements the IMementoService{T} interface with CRUD functionality to store an entity of type T in a cache.
/// </summary>
/// <typeparam name="T">T is the entity type that should be stored in the cache.</typeparam>
public class MementoService<T> : IMementoService<T>
{
    private readonly ICrudCacheService crudCacheService;
    private readonly ILogger<MementoService<T>> logger;

    /// <summary>Initializes a new instance of the <see cref="MementoService{T}" /> class.</summary>
    /// <param name="crudCacheService">The CRUD cache service.</param>
    /// <param name="logger">The logger.</param>
    public MementoService(
        ICrudCacheService crudCacheService,
        ILogger<MementoService<T>> logger)
    {
        this.crudCacheService = crudCacheService;
        this.logger = logger;
    }

    /// <summary>Restores the memento.</summary>
    /// <param name="key">The key.</param>
    /// <returns> Representing the asynchronous operation with result of T type.</returns>
    public async Task<T> RestoreMemento(string key)
    {
        var memento = await crudCacheService.GetValueAsync(GetMementoKey(key));

        if (memento is null)
        {
            return default;
        }
        else
        {
            return JsonConvert.DeserializeObject<T>(memento);
        }
    }

    /// <summary>Creates the memento in the cache.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    /// Representing the asynchronous operation - creating memento in the cache.
    /// </returns>
    public async Task CreateMemento(string key, T value)
    {
        await crudCacheService.SetValueAsync(GetMementoKey(key), value);
    }

    /// <summary>Asynchronously removes a memento from the cache.</summary>
    /// <param name="key">The key.</param>
    /// <returns>Representation of an asynchronous operation - removing memento from the cache.</returns>
    public async Task RemoveMementoAsync(string key)
    {
        string logMessage;
        string mementoKey = GetMementoKey(key);
        var valueToRemove = await crudCacheService.GetValueAsync(mementoKey);

        if (valueToRemove == null)
        {
            logMessage = $"Memento with key = {mementoKey} was not found in the cache.";
            logger.LogInformation(logMessage);
            return;
        }

        string logMsg = $"Removing memento with key = {mementoKey} from cache has started.";
        logger.LogInformation(logMsg);
        await crudCacheService.RemoveAsync(mementoKey);
        logMsg = $"Memento with key = {mementoKey} has been removed from the cache.";
        logger.LogInformation(logMsg);
    }

    private static string GetMementoKey(string key)
    {
        return string.Format("{0}_{1}", key, typeof(T).Name);
    }
}