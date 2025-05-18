namespace OutOfSchool.BusinessLogic.Services.Logging;

/// <summary>
/// Defines a service that compares two collections and returns a list of change logs
/// describing added, removed, and modified items.
/// </summary>
public interface ICollectionChangeLogger
{
    /// <summary>
    /// Compares two collections of items and detects changes (additions, removals, modifications).
    /// </summary>
    /// <typeparam name="T">The type of objects in the collections.</typeparam>
    /// <param name="oldItems">The original collection (before changes).</param>
    /// <param name="newItems">The updated collection (after changes).</param>
    /// <param name="idSelector">Function to extract a unique identifier from an item.</param>
    /// <param name="entityId">The ID of the parent entity.</param>
    /// <param name="entityType">The type name of the parent entity.</param>
    /// <param name="userId">The ID of the user who performed the changes.</param>
    /// <param name="propertyPrefix">Optional prefix added to property names in the log.</param>
    /// <param name="valueProjector">Optional value formatter to project property values.</param>
    /// <param name="useIndexOnly">If true, items are matched only by their index, not by ID.</param>
    /// <param name="excludedProperties">List of property names to skip during comparison.</param>
    /// <returns>A list of <see cref="ChangesLog"/> entries describing all detected differences.</returns>
    List<ChangesLog> CompareCollections<T>(
        IEnumerable<T> oldItems,
        IEnumerable<T> newItems,
        Func<T, string> idSelector,
        Guid entityId,
        string entityType,
        string userId,
        string propertyPrefix = "",
        IValueProjector valueProjector = null,
        bool useIndexOnly = false,
        string[] excludedProperties = null) where T : class;
}
