using Microsoft.Extensions.Options;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;


/// <inheritdoc/>
public class SportKindSyncService : ISportKindSyncService
{
    private readonly ISportsRegistryProviderService providerService;
    private readonly IInstitutionHierarchyRepository hierarchyRepository;
    private readonly ILogger<SportKindSyncService> logger;
    private readonly IOptions<InstitutionOptions> institutionOptions;

    public SportKindSyncService(
        ISportsRegistryProviderService providerService,
        IInstitutionHierarchyRepository hierarchyRepository,
        ILogger<SportKindSyncService> logger,
        IOptions<InstitutionOptions> institutionOptions)
    {
        this.providerService = providerService;
        this.hierarchyRepository = hierarchyRepository;
        this.logger = logger;
        this.institutionOptions = institutionOptions;
    }

   public async Task<int> SyncSportKindsAsync()
{
    logger.LogInformation("Starting synchronization of sport kinds with Sports Registry...");
    var institutionId = Guid.Parse(institutionOptions.Value.MinistryOfSportId);

    // receive all sport kinds
    var response = await providerService.GetAllSportKindsAsync().ConfigureAwait(false);

    var sportKinds = response.Match(
        error => throw new InvalidOperationException($"Failed to fetch sport kinds: {error.Message}"),
        success => success);

    // Load all existing InstitutionHierarchy entries for these codes
    var codes = sportKinds.Select(sk => sk.IdCode).ToList();
    var existingEntities = await hierarchyRepository
        .GetByFilter(x => x.SportRegistryIdCode.HasValue && codes.Contains(x.SportRegistryIdCode.Value))
        .ConfigureAwait(false);

    var lookup = existingEntities.ToDictionary(x => x.SportRegistryIdCode!.Value);

    var updatedCount = 0;

    foreach (var sportKind in sportKinds)
    {
        if (!lookup.TryGetValue(sportKind.IdCode, out var entity))
        {
            // Create
            entity = new InstitutionHierarchy
            {
                Id = Guid.NewGuid(),
                Title = sportKind.Name,
                HierarchyLevel = 2, // TODO: is it right?
                InstitutionId = institutionId,
                SportRegistryIdCode = sportKind.IdCode,
                SportsSectionNumeral = sportKind.SportKindSectionNumeral,
                UpdatedAt = sportKind.UpdatedAt
            };

            await hierarchyRepository.Create(entity).ConfigureAwait(false);
            updatedCount++;
        }
        else if (sportKind.UpdatedAt > entity.UpdatedAt)
        {
            // Update
            entity.Title = sportKind.Name;
            entity.SportsSectionNumeral = sportKind.SportKindSectionNumeral;
            entity.UpdatedAt = sportKind.UpdatedAt;

            await hierarchyRepository.Update(entity).ConfigureAwait(false);
            updatedCount++;
        }
    }

    logger.LogInformation("Sport kinds sync finished. {Count} records created or updated.", updatedCount);
    return updatedCount;
}


}