namespace OutOfSchool.BusinessLogic.Services.Logging;

/// <summary>
/// Interface for logging changes in non-collection nested objects.
/// </summary>
public interface INestedObjectChangeLogger
{
    /// <summary>
    /// Compares two objects of the same type and generates a list of logs for changed properties.
    /// Only properties explicitly listed in <paramref name="trackedProperties"/> are included.
    /// </summary>
    /// <typeparam name="T">The type of the objects being compared.</typeparam>
    /// <param name="oldObj">The original object before modification.</param>
    /// <param name="newObj">The updated object after modification.</param>
    /// <param name="entityId">The ID of the parent entity to which this nested object belongs.</param>
    /// <param name="entityType">The name of the parent entity's type (e.g., "WorkshopDraft").</param>
    /// <param name="userId">The ID of the user who made the change.</param>
    /// <param name="trackedProperties">A list of property names to track changes for.</param>
    /// <param name="valueProjector">Optional custom value formatter for serializing property values.</param>
    /// <returns>A list of <see cref="ChangesLog"/> entries describing all detected property changes.</returns>
    List<ChangesLog> CompareAndLogChanges<T>(
        T oldObj,
        T newObj,
        Guid entityId,
        string entityType,
        string userId,
        IEnumerable<string> trackedProperties,
        IValueProjector valueProjector = null) where T : class;
}
