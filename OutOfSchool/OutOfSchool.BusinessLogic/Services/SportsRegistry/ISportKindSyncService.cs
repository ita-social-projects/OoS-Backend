namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;


/// <summary>
/// Service responsible for synchronizing sport kinds with the external Sports Registry.
/// Pulls the dictionary from the registry and updates local InstitutionHierarchy entities.
/// </summary>
public interface ISportKindSyncService
{
    /// <summary>
    /// Synchronizes the sport kinds dictionary with the external Sports Registry.
    /// Adds new records and updates existing ones.
    /// </summary>
    /// <returns>Total number of created or updated entities.</returns>
    Task<int> SyncSportKindsAsync();
}