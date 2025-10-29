//using System.Linq;
using OutOfSchool.BusinessLogic.Util.Mappers;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;
public class SportsSectionSyncService(
    ISportsRegistryWorkshopProvider workshopProvider,
    IWorkshopDraftRepository workshopDraftRepository,
    IWorkshopRepository workshopRepository,
    ICodeficatorRepository codeficatorRepository,
    IProviderRepository providerRepository,
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

        var existingWorkshopDrafts = await workshopDraftRepository
            .GetByFilter(w => w.MinsportSectionId.HasValue && registrySectionsId.Contains(w.MinsportSectionId.Value))
            .ConfigureAwait(false);

        var existingLookup = existingWorkshops.ToDictionary(w => w.MinsportSectionId!.Value);
        var existingDraftLookup = existingWorkshopDrafts.ToDictionary(w => w.MinsportSectionId!.Value);

        var toCreate = new List<WorkshopDraft>();
        var toUpdate = new List<WorkshopDraft>();

        var tempSectionList = sportsSections.Where(s => s.OrganizationCode == "45080641").ToList(); // temporary for testing purposes
        foreach (var section in tempSectionList ) // check
        //foreach (var section in sportsSections) // check
        {
            existingDraftLookup.TryGetValue(section.SectionId, out var existingDraft);
            existingLookup.TryGetValue(section.SectionId, out var existingWorkshop);

            DateTimeOffset? lastUpdate = existingDraft?.ModifiedAt
                              ?? (existingWorkshop?.UpdatedAt.HasValue == true
                                  ? new DateTimeOffset(existingWorkshop.UpdatedAt.Value)
                                  : null);

            if (!lastUpdate.HasValue || section.UpdatedInRegistryAt > lastUpdate.Value)
            {
                WorkshopDraft draft;

                if (existingDraft != null)
                {
                    // update existing draft
                    draft = section.MapToExistingDraft(existingDraft);
                    draft.DraftStatus = WorkshopDraftStatus.PendingModeration; // to ask Dima
                    toUpdate.Add(draft);
                }
                else if (existingWorkshop != null)
                {
                    // no draft,  only workshop - create draft
                    draft = section.ToWorkshopDraft(existingWorkshop.ProviderId);

                    //draft = existingWorkshop.ToDraft(); // may be this way?
                    //draft = section.MapToExistingDraft(draft);

                    toCreate.Add(draft);
                }
                else
                {
                    // no workshop, no draft - new draft
                    var providerId = await GetProviderIdAsync(section.OrganizationCode).ConfigureAwait(false);
                    if (!providerId.HasValue)
                        continue; // skip sections without valid provider
                    draft = section.ToWorkshopDraft(providerId.Value);
                    toCreate.Add(draft);
                }

                //  update CATOTTGId
                draft.CATOTTGId = await GetIdByCatottgCode(section.SectionAddressLocalityDictIdCode) ?? 0;
                break; // temporary for testing purposes
            }
        }

        if (toCreate.Any() || toUpdate.Any())
        {
            await workshopDraftRepository.RunInTransaction(async () =>
            {
                if (toCreate.Any())
                {
                    foreach (var draft in toCreate)
                    {
                        logger.LogDebug(
                            "Creating WorkshopDraft. SectionId: {SectionId}, Title: {Title}",
                            draft.MinsportSectionId, draft.WorkshopDraftContent.Title);
                    }
                    await workshopDraftRepository.Create(toCreate).ConfigureAwait(false);
                }

                if (toUpdate.Any())
                {
                    foreach (var draft in toUpdate)
                    {
                        logger.LogDebug(
                            "Updating WorkshopDraft. SectionId: {SectionId}, Title: {Title}, DraftStatus: {DraftStatus}",
                            draft.MinsportSectionId,
                            draft.WorkshopDraftContent.Title,
                            draft.DraftStatus);
                    }
                    await workshopDraftRepository.SaveChangesAsync().ConfigureAwait(false);
                }

                logger.LogInformation(
                    "Sports sections sync finished. {CreatedCount} created, {UpdatedCount} updated.",
                    toCreate.Count, toUpdate.Count);
            });
        }

        return toCreate.Count + toUpdate.Count;
    }

    private async Task<long?> GetIdByCatottgCode(string sectionAddressLocalityDictIdCode)
    {
        return await codeficatorRepository.GetIdByCodeAsync(sectionAddressLocalityDictIdCode);
    }
   
    private async Task<Guid?> GetProviderIdAsync(string edrpou)
    {
        return await providerRepository.GetIdByEdrpouAsync(edrpou);
    }
}
