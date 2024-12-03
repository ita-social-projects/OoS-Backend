using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace OutOfSchool.BusinessLogic.Services.DraftStorage;

/// <summary>
/// Implements the IDraftStorageService{T} interface to store an entity draft of type T in a cache.
/// </summary>
/// <typeparam name="T">T is the entity draft type that should be stored in the cache.</typeparam>
public class DraftStorageService<T> : IDraftStorageService<T>
{
    private readonly IReadWriteCacheService cacheService;
    private readonly ILogger<DraftStorageService<T>> logger;

    /// <summary>Initializes a new instance of the <see cref="DraftStorageService{T}" /> class.</summary>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="logger">The logger.</param>
    public DraftStorageService(
        IReadWriteCacheService cacheService,
        ILogger<DraftStorageService<T>> logger)
    {
        this.cacheService = cacheService;
        this.logger = logger;
    }

    /// <summary>Restores the entity draft of T type entity.</summary>
    /// <param name="key">The key.</param>
    /// <returns> Representing the asynchronous operation with result of T type.</returns>
    public async Task<T?> RestoreAsync([NotNull] string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var draftValue = await cacheService.ReadAsync(GetKey(key)).ConfigureAwait(false);

        if (draftValue.IsNullOrEmpty())
        {
            logger.LogInformation("The {EntityType} draft for User with key = {Key} was not found in the cache.", typeof(T).Name, key);
            return default;
        }

        return JsonSerializerHelper.Deserialize<T>(draftValue);
    }

    /// <summary>Creates the entity draft of T type in the cache.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    /// Representing the asynchronous operation - creating the entity draft of T type in the cache.
    /// </returns>
    public async Task CreateAsync([NotNull] string key, [NotNull] T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        await cacheService.WriteAsync(GetKey(key), JsonSerializerHelper.Serialize(value)).ConfigureAwait(false);
    }

    /// <summary>Asynchronously removes an entity draft from the cache.</summary>
    /// <param name="key">The key.</param>
    /// <returns>Representation of an asynchronous operation - removing an entity draft from the cache.</returns>
    public async Task RemoveAsync([NotNull] string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var draftKey = GetKey(key);
        var valueToRemove = await cacheService.ReadAsync(draftKey);

        if (valueToRemove.IsNullOrEmpty())
        {
            logger.LogInformation("The {EntityType} draft with key = {DraftKey} was not found in the cache.", typeof(T).Name, draftKey);
            return;
        }

        logger.LogInformation("Start removing the {EntityType} draft with key = {DraftKey} from cache.", typeof(T).Name, draftKey);
        await cacheService.RemoveAsync(draftKey).ConfigureAwait(false);
    }

    private static string GetKey(string key)
    {
        return $"{key}_{typeof(T).Name}";
    }
}
