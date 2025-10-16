using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using System.Linq;

namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;
public class SportsSectionSyncService(
    ISportsRegistryWorkshopProvider workshopProvider,
    IWorkshopRepository workshopRepository,
    ILogger<SportsSectionSyncService> logger)
    : ISportsSectionSyncService
{
    public async Task<int> SyncSportsSectionsAsync()
    {
        logger.LogInformation("Starting synchronization of workshops (sports sections) with Sports Registry...");

        var response = await workshopProvider.GetAllSportsSectionsAsync().ConfigureAwait(false);

        var sportsSections = response.Match(
            error => throw new InvalidOperationException($"Failed to fetch sports sections: {error.Message}"),
            success => success);

        if (!sportsSections.Any())
        {
            logger.LogWarning("No sports sections were fetched from Sports Registry.");
            return 0;
        }

        var registrySectionsId = sportsSections
            .Select(x => x.SectionId)
            .ToList();

        var existingWorkshops = await workshopRepository
            .GetByFilter(w => w.MinsportSectionId.HasValue && registrySectionsId.Contains(w.MinsportSectionId.Value))
            .ConfigureAwait(false);

        var existingLookup = existingWorkshops.ToDictionary(w => w.MinsportSectionId!.Value);

        var toCreate = new List<Workshop>(); // WorkshopDraft?
        var toUpdate = new List<Workshop>();

        foreach (var section in sportsSections)
        {
            if (!existingLookup.TryGetValue(section.SectionId, out var workshop))
            {
                //Create draft
                toCreate.Add(new Workshop() // WorkshopDraft
                {
                    Id = Guid.NewGuid(),
                   
                    Title = section.SectionName,
                  
                    // TODO: map other fields
                });
            }
            //else if (section.UpdatedAt > workshop.RegistrySyncDate)
            //{
            //    // Update draft
            //    workshop.Title = section.Name;
              //}
        }

        // need to review
        if (toCreate.Any() || toUpdate.Any())
        {
            await workshopRepository.RunInTransaction(async () =>
            {
                if (toCreate.Any())
                {
                    await workshopRepository.Create(toCreate).ConfigureAwait(false);
                }

                if (toUpdate.Any())
                {
                    await workshopRepository.SaveChangesAsync().ConfigureAwait(false);
                }

                logger.LogInformation(
                    "Sports sections sync finished. {CreatedCount} created, {UpdatedCount} updated.",
                    toCreate.Count, toUpdate.Count);
            });
        }

        return toCreate.Count + toUpdate.Count;
    }
}
