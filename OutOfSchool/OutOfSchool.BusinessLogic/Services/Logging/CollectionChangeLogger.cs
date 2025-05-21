using System.Collections.Concurrent;
using System.Reflection;

namespace OutOfSchool.BusinessLogic.Services.Logging;

/// <summary>
/// Provides functionality for comparing two collections and generating detailed logs
/// representing added, removed, or changed items between two states of a collection.
/// </summary>
public class CollectionChangeLogger : ICollectionChangeLogger
{
    private const int MaxValueLength = 500;
    private const int MaxIdLength = 20;
    private const int GuidShortFormLength = 8;

    // Cache for properties
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    /// <inheritdoc />
    public List<ChangesLog> CompareCollections<T>(
        IEnumerable<T> oldItems,
        IEnumerable<T> newItems,
        Func<T, string> idSelector,
        Guid entityId,
        string entityType,
        string userId,
        string propertyPrefix = "",
        IValueProjector valueProjector = null,
        bool useIndexOnly = false,
        string[] excludedProperties = null) where T : class
    {
        var logs = new List<ChangesLog>();
        excludedProperties ??= [];

        // Create a single timestamp for all logs to ensure consistency
        var timestamp = DateTime.UtcNow;

        oldItems ??= [];
        newItems ??= [];

        // If no ID selector is provided or index-only comparison is forced
        if (idSelector == null || useIndexOnly)
        {
            return CompareCollectionsByIndex(
                oldItems.ToList(),
                newItems.ToList(),
                entityId,
                entityType,
                userId,
                propertyPrefix,
                valueProjector,
                excludedProperties,
                timestamp);
        }

        // Build dictionaries using composite keys: Index + ID
        var oldDict = oldItems
           .Select((item, index) => new ItemData<T>
           {
               Item = item,
               Index = index,
               Id = idSelector(item)
           })
           .ToDictionary(
               x => useIndexOnly ? x.Index.ToString() : $"{x.Index}_{x.Id}",
               x => x
           );

        var newDict = newItems
            .Select((item, index) => new ItemData<T>
            {
                Item = item,
                Index = index,
                Id = idSelector(item)
            })
            .ToDictionary(
                x => useIndexOnly ? x.Index.ToString() : $"{x.Index}_{x.Id}",
                x => x
            );

        // Determine which items are added, removed, or modified
        var oldKeys = oldDict.Keys.ToHashSet();
        var newKeys = newDict.Keys.ToHashSet();

        var addedKeys = newKeys.Except(oldKeys).ToHashSet();
        var removedKeys = oldKeys.Except(newKeys).ToHashSet();
        var commonKeys = oldKeys.Intersect(newKeys).ToHashSet();

        // Generate logs for added items
        logs.AddRange(GetAddedItemsLogs<T>(
            addedKeys.Select(key => newDict[key]),
            entityId,
            entityType,
            userId,
            propertyPrefix,
            valueProjector,
            useIndexOnly,
            excludedProperties,
            timestamp));

        // Generate logs for removed items
        logs.AddRange(GetRemovedItemsLogs<T>(
            removedKeys.Select(key => oldDict[key]),
            entityId,
            entityType,
            userId,
            propertyPrefix,
            valueProjector,
            useIndexOnly,
            excludedProperties,
            timestamp));

        // Generate logs for changed items
        logs.AddRange(GetChangedItemsLogs<T>(
            oldDict,
            newDict,
            commonKeys,
            entityId,
            entityType,
            userId,
            propertyPrefix,
            valueProjector,
            useIndexOnly,
            excludedProperties,
            timestamp));

        return logs;
    }

    /// <summary>
    /// Compares collections based solely on index position, without using IDs.
    /// </summary>
    private static List<ChangesLog> CompareCollectionsByIndex<T>(
        List<T> oldItems,
        List<T> newItems,
        Guid entityId,
        string entityType,
        string userId,
        string propertyPrefix,
        IValueProjector valueProjector,
        string[] excludedProperties,
        DateTime timestamp) where T : class
    {
        var logs = new List<ChangesLog>();
        var properties = GetCachedProperties<T>()
            .Where(p => !excludedProperties.Contains(p.Name))
            .ToArray();

        // the minimum number of items in the old and new collections
        // so that only those items that exist in those collections are compared
        var commonCount = Math.Min(oldItems.Count, newItems.Count);

        // Compare items with matching indexes
        for (int i = 0; i < commonCount; i++)
        {
            var oldItem = oldItems[i];
            var newItem = newItems[i];

            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldItem);
                var newValue = prop.GetValue(newItem);

                if (!Equals(oldValue, newValue))
                {
                    var projectedOld = valueProjector?.ProjectValue(prop.PropertyType, oldValue);
                    var projectedNew = valueProjector?.ProjectValue(prop.PropertyType, newValue);

                    logs.Add(new ChangesLog
                    {
                        EntityType = entityType,
                        EntityIdGuid = entityId,
                        PropertyName = $"{propertyPrefix}.Item[{i}].{prop.Name}",
                        OldValue = Truncate(projectedOld),
                        NewValue = Truncate(projectedNew),
                        UserId = userId,
                        UpdatedDate = timestamp
                    });
                }
            }
        }

        // Handle additions
        for (int i = commonCount; i < newItems.Count; i++)
        {
            foreach (var prop in properties)
            {
                var newValue = prop.GetValue(newItems[i]);
                if (newValue == null) continue;

                var projectedNew = valueProjector?.ProjectValue(prop.PropertyType, newValue);

                logs.Add(new ChangesLog
                {
                    EntityType = entityType,
                    EntityIdGuid = entityId,
                    PropertyName = $"{propertyPrefix}.Added[{i}].{prop.Name}",
                    OldValue = null,
                    NewValue = Truncate(projectedNew),
                    UserId = userId,
                    UpdatedDate = timestamp
                });
            }
        }

        // Handle removals
        for (int i = commonCount; i < oldItems.Count; i++)
        {
            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldItems[i]);
                if (oldValue == null) continue;

                var projectedOld = valueProjector?.ProjectValue(prop.PropertyType, oldValue);

                logs.Add(new ChangesLog
                {
                    EntityType = entityType,
                    EntityIdGuid = entityId,
                    PropertyName = $"{propertyPrefix}.Removed[{i}].{prop.Name}",
                    OldValue = Truncate(projectedOld),
                    NewValue = null,
                    UserId = userId,
                    UpdatedDate = timestamp
                });
            }
        }

        return logs;
    }

    /// <summary>
    /// Generates logs for newly added items.
    /// </summary>
    private static List<ChangesLog> GetAddedItemsLogs<T>(
        IEnumerable<ItemData<T>> newItems,
        Guid entityId,
        string entityType,
        string userId,
        string propertyPrefix,
        IValueProjector valueProjector,
        bool useIndexOnly,
        string[] excludedProperties,
        DateTime timestamp) where T : class
    {
        var logs = new List<ChangesLog>();

        var properties = GetCachedProperties<T>()
            .Where(p => !excludedProperties.Contains(p.Name))
            .ToArray();

        foreach (var newData in newItems)
        {
            foreach (var prop in properties)
            {
                var newValue = prop.GetValue(newData.Item);
                if (newValue == null) continue;

                var projectedNew = valueProjector?.ProjectValue(prop.PropertyType, newValue);

                logs.Add(new ChangesLog
                {
                    EntityType = entityType,
                    EntityIdGuid = entityId,
                    PropertyName = $"{propertyPrefix}.Added[{FormatId(newData.Id, newData.Index, useIndexOnly)}].{prop.Name}",
                    OldValue = null,
                    NewValue = Truncate(projectedNew),
                    UserId = userId,
                    UpdatedDate = timestamp
                });
            }
        }

        return logs;
    }

    /// <summary>
    /// Generates logs for removed items.
    /// </summary>
    private static List<ChangesLog> GetRemovedItemsLogs<T>(
        IEnumerable<ItemData<T>> oldItems,
        Guid entityId,
        string entityType,
        string userId,
        string propertyPrefix,
        IValueProjector valueProjector,
        bool useIndexOnly,
        string[] excludedProperties,
        DateTime timestamp) where T : class
    {
        var logs = new List<ChangesLog>();

        var properties = GetCachedProperties<T>()
            .Where(p => !excludedProperties.Contains(p.Name))
            .ToArray();

        foreach (var oldData in oldItems)
        {
            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldData.Item);
                if (oldValue == null) continue;

                var projectedOld = valueProjector?.ProjectValue(prop.PropertyType, oldValue);

                logs.Add(new ChangesLog
                {
                    EntityType = entityType,
                    EntityIdGuid = entityId,
                    PropertyName = $"{propertyPrefix}.Removed[{FormatId(oldData.Id, oldData.Index, useIndexOnly)}].{prop.Name}",
                    OldValue = Truncate(projectedOld),
                    NewValue = null,
                    UserId = userId,
                    UpdatedDate = timestamp
                });
            }
        }

        return logs;
    }

    /// <summary>
    /// Generates logs for changed items.
    /// </summary>
    private static List<ChangesLog> GetChangedItemsLogs<T>(
        Dictionary<string, ItemData<T>> oldDict,
        Dictionary<string, ItemData<T>> newDict,
        ISet<string> commonKeys,
        Guid entityId,
        string entityType,
        string userId,
        string propertyPrefix,
        IValueProjector valueProjector,
        bool useIndexOnly,
        string[] excludedProperties,
        DateTime timestamp) where T : class
    {
        var logs = new List<ChangesLog>();
        var properties = GetCachedProperties<T>()
            .Where(p => !excludedProperties.Contains(p.Name))
            .ToArray();

        foreach (var key in commonKeys)
        {
            var oldData = oldDict[key];
            var newData = newDict[key];

            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldData.Item);
                var newValue = prop.GetValue(newData.Item);

                if (!Equals(oldValue, newValue))
                {
                    var projectedOld = valueProjector?.ProjectValue(prop.PropertyType, oldValue);
                    var projectedNew = valueProjector?.ProjectValue(prop.PropertyType, newValue);

                    logs.Add(new ChangesLog
                    {
                        EntityType = entityType,
                        EntityIdGuid = entityId,
                        PropertyName = $"{propertyPrefix}.Item[{FormatId(oldData.Id, oldData.Index, useIndexOnly)}].{prop.Name}",
                        OldValue = Truncate(projectedOld),
                        NewValue = Truncate(projectedNew),
                        UserId = userId,
                        UpdatedDate = timestamp
                    });
                }
            }
        }

        return logs;
    }

    /// <summary>
    /// Returns cached public properties of type T.
    /// </summary>
    private static PropertyInfo[] GetCachedProperties<T>()
    {
        return PropertyCache.GetOrAdd(typeof(T), t => t.GetProperties());
    }

    /// <summary>
    /// Formats the identifier used in property paths for logs.
    /// Applies trimming or filename extraction logic.
    /// </summary>
    private static string FormatId(string id, int index, bool useIndexOnly = false)
    {
        if (useIndexOnly || string.IsNullOrEmpty(id))
        {
            return index.ToString();
        }

        if (Guid.TryParse(id, out var guid))
        {
            return guid.ToString()[..GuidShortFormLength];
        }

        if (id.Contains('/') || id.Contains('\\'))
        {
            var separators = new[] { '/', '\\' };
            var parts = id.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var fileName = parts[^1];
            return fileName.Length > MaxIdLength ? fileName[..MaxIdLength] : fileName;
        }

        if (id.Length > MaxIdLength)
        {
            return id[..MaxIdLength];
        }

        return id;
    }

    /// <summary>
    /// Truncates long values to avoid exceeding storage/logging limits.
    /// </summary>
    private static string Truncate(string input) =>
        input?.Length > MaxValueLength ? input[..MaxValueLength] : input;

    /// <summary>
    /// Internal helper to wrap an item with its index and ID.
    /// </summary>
    private sealed class ItemData<T>
    {
        public T Item { get; set; }
        public int Index { get; set; }
        public string Id { get; set; }
    }
}