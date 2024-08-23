namespace OutOfSchool.BusinessLogic.Services.Memento;
public class Storage : IStorage
{
    private readonly IRedisCacheService? redisCacheService;
    private readonly ILogger<Storage> logger;

    public Storage(IRedisCacheService redisCacheService, ILogger<Storage> logger)
    {
        this.redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SetMementoValueAsync(KeyValuePair<string, string?> keyValue)
    {
        logger.LogInformation("Setting memento to cache has started");
        if (redisCacheService is not null)
        {
            await redisCacheService.SetValueToRedisCacheAsync(keyValue.Key, keyValue.Value ?? string.Empty);
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
        if (redisCacheService is not null)
        {
            var value = await redisCacheService.GetValueFromRedisCacheAsync(key);
            logger.LogInformation("Memento has been restored from cache.");
            return new KeyValuePair<string, string?>(key, value);
        }

        logger.LogWarning("Getting memento from cache failed; redisCacheService field is not set.");
        return default;
    }

    public async Task RemoveMementoAsync(string key)
    {
        logger.LogInformation("Removing memento from cache has started.");
        if (redisCacheService is not null)
        {
            await redisCacheService.RemoveValueFromRedisCacheAsync(key);
            logger.LogInformation("Memento has been removed from the cache.");
        }
        else
        {
            logger.LogWarning("Removing memento from cache failed; redisCacheService field is not set.");
        }
    }
}