using System.Collections;

namespace OutOfSchool.BusinessLogic.Services.Logging;

/// <summary>
/// Logs property-level changes in complex (non-collection) nested objects.
/// </summary>
public class NestedObjectChangeLogger : INestedObjectChangeLogger
{
    private const int MaxValueLength = 500;

    /// <inheritdoc />
    public List<ChangesLog> CompareAndLogChanges<T>(
        T oldObj,
        T newObj,
        Guid entityId,
        string entityType,
        string userId,
        IEnumerable<string> trackedProperties,
        IValueProjector valueProjector = null) where T : class
    {
        // Guard: This logger is not designed for collections like List<T>, arrays, etc.
        if (typeof(IEnumerable).IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string))
        {
            throw new ArgumentException(
                $"NestedObjectChangeLogger does not support collection types like '{typeof(T).Name}'. " +
                $"Use CollectionChangeLogger instead.");
        }

        if (trackedProperties == null || !trackedProperties.Any())
        {
            throw new ArgumentException("Tracked properties cannot be null or empty.", nameof(trackedProperties));
        }

        var logs = new List<ChangesLog>();
        if (oldObj == null || newObj == null) return logs;

        var timestamp = DateTime.UtcNow;

        // Iterate over all public instance properties of the type
        foreach (var prop in typeof(T).GetProperties())
        {
            // Skip properties not explicitly marked for tracking
            if (!trackedProperties.Contains(prop.Name))
                continue;

            var oldValue = prop.GetValue(oldObj);
            var newValue = prop.GetValue(newObj);

            if (!Equals(oldValue, newValue))
            {
                var oldValueStr = Truncate(
                    valueProjector?.ProjectValue(prop.PropertyType, oldValue)
                    ?? JsonSerializerHelper.Serialize(oldValue));

                var newValueStr = Truncate(
                    valueProjector?.ProjectValue(prop.PropertyType, newValue)
                    ?? JsonSerializerHelper.Serialize(newValue));

                // Add a new change log entry
                logs.Add(new ChangesLog
                {
                    EntityType = entityType,
                    EntityIdGuid = entityId,
                    PropertyName = prop.Name,
                    OldValue = oldValueStr,
                    NewValue = newValueStr,
                    UserId = userId,
                    UpdatedDate = timestamp,
                });
            }
        }

        return logs;
    }

    /// <summary>
    /// Truncates a string to a maximum allowed length.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <returns>The original string if short enough; otherwise, a truncated version.</returns>
    private static string Truncate(string input) =>
        input?.Length > MaxValueLength ? input.Substring(0, MaxValueLength) : input;
}
