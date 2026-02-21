using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace OutOfSchool.BusinessLogic.Services.TempSave;

/// <summary>
/// Implements the ITempSaveService{T} interface to store an entity dto of type T in the cache.
/// </summary>
/// <typeparam name="T">T is the entity dto type that should be stored in the cache.</typeparam>
/// <remarks>Initializes a new instance of the <see cref="TempSaveService{T}" /> class.</remarks>
/// <param name="cacheService">The cache service.</param>
/// <param name="logger">The logger.</param>
public class TempSaveService<T>(
    IReadWriteCacheService cacheService,
    ILogger<TempSaveService<T>> logger,
    IOptions<RedisForTempSaveConfig> redisConfig) : ITempSaveService<T>
{
    private readonly RedisForTempSaveConfig redisConfig = redisConfig.Value;

    /// <summary>Restores the entity dto of T type.</summary>
    /// <param name="key">The key.</param>
    /// <returns> Representing the asynchronous operation with result of T type.</returns>
    public async Task<T?> RestoreAsync([NotNull] string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var draftValue = await cacheService.ReadAsync(GetKey(key)).ConfigureAwait(false);

        if (draftValue.IsNullOrEmpty())
        {
            logger.LogInformation("The {EntityType} value for User with key = {Key} was not found in the cache.", typeof(T).Name, key);
            return default;
        }

        return JsonSerializerHelper.Deserialize<T>(draftValue);
    }

    /// <summary>Restores the entity dto of T type in the cache.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    /// Representing the asynchronous operation - storing the entity dto of T type in the cache.
    /// </returns>
    public async Task StoreAsync([NotNull] string key, [NotNull] T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        await cacheService.WriteAsync(
                                      GetKey(key), 
                                      JsonSerializerHelper.Serialize(value), 
                                      redisConfig.AbsoluteExpirationRelativeToNowInterval,
                                      TimeSpan.Zero
                                      )
                                      .ConfigureAwait(false);
    }

    /// <summary>Asynchronously removes an entity dto value from the cache.</summary>
    /// <param name="key">The key.</param>
    /// <returns>Representation of an asynchronous operation - removing an entity dto value from the cache.</returns>
    public async Task RemoveAsync([NotNull] string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var draftKey = GetKey(key);
        var valueToRemove = await cacheService.ReadAsync(draftKey);

        if (valueToRemove.IsNullOrEmpty())
        {
            logger.LogInformation("The {EntityType} value with key = {DraftKey} was not found in the cache.", typeof(T).Name, draftKey);
            return;
        }

        logger.LogInformation("Start removing the {EntityType} value with key = {DraftKey} from cache.", typeof(T).Name, draftKey);
        await cacheService.RemoveAsync(draftKey).ConfigureAwait(false);
    }

    /// <summary>Returns the time remaining until the end of the entity dto life.</summary>
    /// <param name="key">The key.</param>
    /// <returns> Representing the asynchronous operation with result of TimeSpan? type.</returns>
    public async Task<TimeSpan?> GetTimeToLiveAsync([NotNull] string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        return await cacheService.GetTimeToLiveAsync(GetKey(key)).ConfigureAwait(false);
    }

    private static string GetKey(string key)
    {
        return $"{key}_{typeof(T).Name}";
    }
}
