namespace OutOfSchool.BusinessLogic.Services.Memento;

public class Storage : IStorage
{
    private readonly ICrudCacheService? crudCacheService;
    private readonly ILogger<Storage> logger;

    public Storage(ICrudCacheService crudCacheService, ILogger<Storage> logger)
    {
        this.crudCacheService = crudCacheService ?? throw new ArgumentNullException(nameof(crudCacheService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SetMementoValueAsync(KeyValuePair<string, string?> keyValue)
    {
        logger.LogInformation("Setting memento to cache has started");
        if (crudCacheService is not null)
        {
            await crudCacheService.SetValueToCacheAsync(keyValue.Key, keyValue.Value ?? string.Empty);
            logger.LogInformation("Memento has been stored in cache");
        }
        else
        {
            logger.LogWarning("Setting memento to cache failed; redisCacheService field is not set.");
        }
    }

    public async Task<KeyValuePair<string, string?>> GetMementoValueAsync(string key)
    {
        logger.LogInformation("Getting memento from cache has started.");
        if (crudCacheService is not null)
        {
            var value = await crudCacheService.GetValueFromCacheAsync(key);
            logger.LogInformation("Memento has been restored from cache.");
            return new KeyValuePair<string, string?>(key, value);
        }

        logger.LogWarning("Getting memento from cache failed; redisCacheService field is not set.");
        return default;
    }

    public async Task RemoveMementoAsync(string key)
    {
        logger.LogInformation("Removing memento from cache has started.");
        if (crudCacheService is not null)
        {
            await crudCacheService.RemoveFromCacheAsync(key);
            logger.LogInformation("Memento has been removed from the cache.");
        }
        else
        {
            logger.LogWarning("Removing memento from cache failed; redisCacheService field is not set.");
        }
    }
}