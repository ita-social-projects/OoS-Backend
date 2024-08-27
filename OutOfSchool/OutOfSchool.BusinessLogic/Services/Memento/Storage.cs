using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Memento;

/// <summary>
/// Implements the IStorage interface to get, set, and remove memento in the cache.
/// </summary>
public class Storage : IStorage
{
    private readonly ICrudCacheService crudCacheService;
    private readonly ILogger<Storage> logger;

    /// <summary>Initializes a new instance of the <see cref="Storage" /> class.</summary>
    /// <param name="crudCacheService">The crud cache service.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">crudCacheService or logger.</exception>
    public Storage(ICrudCacheService crudCacheService, ILogger<Storage> logger)
    {
        this.crudCacheService = crudCacheService ?? throw new ArgumentNullException(nameof(crudCacheService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Asynchronously sets the memento value in the cache .</summary>
    /// <param name="keyValue">The key value.</param>
    /// <returns>Representing the asynchronous operation.</returns>
    public async Task SetMementoValueAsync(KeyValuePair<string, string?> keyValue)
    {
        string logMsg = $"Setting memento with key = {keyValue.Key} to cache has started";
        logger.LogInformation(logMsg);
        await crudCacheService.SetValueToCacheAsync(keyValue.Key, keyValue.Value ?? string.Empty);
        logMsg = $"Memento with key = {keyValue.Key} has been stored in cache";
        logger.LogInformation(logMsg);
    }

    /// <summary>Gets the memento value asynchronous.</summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// Representing the asynchronous operation with result KeyValuePair{string, string?} type.
    /// </returns>
    public async Task<KeyValuePair<string, string?>> GetMementoValueAsync(string key)
    {
        string logMsg = $"Getting memento with key = {key} from cache has started.";
        logger.LogDebug(logMsg);
        var value = await crudCacheService.GetValueFromCacheAsync(key);

        if (value == null)
        {
            logMsg = $"Memento with key = {key} not found in cache.";
            logger.LogWarning(logMsg);
            return new KeyValuePair<string, string?>(key, null);
        }

        logMsg = $"Memento with key = {key} has been restored from cache.";
        logger.LogDebug(logMsg);
        return new KeyValuePair<string, string?>(key, value);
    }

    /// <summary>Removes the memento asynchronous.</summary>
    /// <param name="key">The key.</param>
    /// <returns>Representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Memento with key = {key} was not found in the cache.</exception>
    public async Task RemoveMementoAsync(string key)
    {
        string errMessage;

        var valueToRemove = await crudCacheService.GetValueFromCacheAsync(key);

        if (valueToRemove == null)
        {
            errMessage = $"Removing memento with key = {key} from cache failed. Memento with key = {key} was not found in the cache.";
            logger.LogError(errMessage);
            throw new InvalidOperationException(errMessage);
        }

        string logMsg = $"Removing memento with key = {key} from cache has started.";
        logger.LogDebug(logMsg);
        await crudCacheService.RemoveFromCacheAsync(key);
        logMsg = $"Memento with key = {key} has been removed from the cache.";
        logger.LogDebug(logMsg);
    }
}