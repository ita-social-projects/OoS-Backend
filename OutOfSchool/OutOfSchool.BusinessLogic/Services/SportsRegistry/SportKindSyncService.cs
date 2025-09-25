using Microsoft.Extensions.Options;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;


/// <inheritdoc/>
public class SportKindSyncService(
    ISportsRegistryProviderService providerService,
    IInstitutionHierarchyRepository hierarchyRepository,
    ILogger<SportKindSyncService> logger,
    IOptions<InstitutionOptions> institutionOptions)
    : ISportKindSyncService
{
    public async Task<int> SyncSportKindsAsync()
    {
        logger.LogInformation("Starting synchronization of sport kinds with Sports Registry...");
        var institutionId = Guid.Parse(institutionOptions.Value.MinistryOfSportId);

        // receive all sport kinds
        var response = await providerService.GetAllSportKindsAsync().ConfigureAwait(false);

        var sportKinds = response.Match(
            error => throw new InvalidOperationException($"Failed to fetch sport kinds: {error.Message}"),
            success => success);
        if (!sportKinds.Any())
        {
            logger.LogWarning("No sport kinds were fetched from Sports Registry");
            return 0;
        }

        // Load all existing InstitutionHierarchy entries for these codes
        var codes = sportKinds.Select(x => x.IdCode).ToList();

        var existingEntities = await hierarchyRepository
            .GetByFilter(x => x.SportRegistryIdCode.HasValue && codes.Contains(x.SportRegistryIdCode.Value))
            .ConfigureAwait(false);

        var lookup = existingEntities.ToDictionary(x => x.SportRegistryIdCode!.Value);
        
        // entities to save changes 
        var toCreateEntities = new List<InstitutionHierarchy>();
        var toUpdateEntities = new List<InstitutionHierarchy>();

        var changedEntitiesCount = 0;
        foreach (var sportKind in sportKinds)
        {
            if (!lookup.TryGetValue(sportKind.IdCode, out var entity))
            {
                // Create
                toCreateEntities.Add(new InstitutionHierarchy
                {
                    Id = Guid.NewGuid(),
                    Title = sportKind.Name,
                    HierarchyLevel = 2, // level sport kinds should belong ( 2 by default for min sport Institution) 
                    InstitutionId = institutionId,
                    SportRegistryIdCode = sportKind.IdCode,
                    SportsSectionNumeral = sportKind.SportKindSectionNumeral,
                    RegistrySyncDate = DateTime.UtcNow
                });
            }
            else if (entity.RegistrySyncDate == null || sportKind.UpdatedAt > entity.RegistrySyncDate)
            {
                // Update
                entity.Title = sportKind.Name;
                entity.SportsSectionNumeral = sportKind.SportKindSectionNumeral;
                entity.RegistrySyncDate = DateTime.UtcNow;
                toUpdateEntities.Add(entity);
            }
        }

        await hierarchyRepository.RunInTransaction(async () =>
        {
            //  Save all created / updated entities in one operation instead of creating every time in loop
            if (toCreateEntities.Any())
            {
                await hierarchyRepository.Create(toCreateEntities).ConfigureAwait(false);
            }
            
            // EF Core already tracks modified entities, SaveChanges will apply updates
            if (toUpdateEntities.Any())
            {
                await hierarchyRepository.SaveChangesAsync().ConfigureAwait(false);
            }

            var updatedEntitiesCount  = toUpdateEntities.Count;
            var createdEntitiesCount = toCreateEntities.Count;
            changedEntitiesCount =  updatedEntitiesCount + createdEntitiesCount;
            logger.LogInformation($"Sport kinds sync finished. {updatedEntitiesCount} updated and {createdEntitiesCount} created");
            
        });
        return changedEntitiesCount;
    }
}
