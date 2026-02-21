namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;

/// <summary>
/// Service responsible for synchronizing sports sections (workshops) 
/// with the external Ministry of Sports Registry.
/// Pulls section data from the registry and updates or creates corresponding local workshops.
/// </summary>
public interface ISportsSectionSyncService
{
    /// <summary>
    /// Synchronizes sports sections (workshops) with the external Sports Registry.
    /// Creates new draft workshops for records not found locally and updates existing drafts when data has changed.
    /// </summary>
    /// <returns>The total number of workshops created or updated during synchronization.</returns>
    Task<int> SyncSportsSectionsAsync();
}
