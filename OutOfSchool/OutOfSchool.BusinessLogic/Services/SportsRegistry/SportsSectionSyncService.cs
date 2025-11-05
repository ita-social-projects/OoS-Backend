//using System.Linq;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Util.Mappers;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.External;
namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;
public class SportsSectionSyncService(
    ISportsRegistryWorkshopProvider workshopProvider,
    IWorkshopDraftRepository workshopDraftRepository,
    IWorkshopRepository workshopRepository,
    ICodeficatorRepository codeficatorRepository,
    IProviderRepository providerRepository,
    IInstitutionHierarchyRepository hierarchyRepository,
   // IOptions<InstitutionOptions> institutionOptions,
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

        var existingHierarchies = await hierarchyRepository
           .GetByFilter(x => x.SportRegistryIdCode.HasValue)
           .ConfigureAwait(false);
        var lookupHierarchy = existingHierarchies.ToDictionary(x => x.SportRegistryIdCode!.Value);
       
        var toCreate = new List<WorkshopDraft>();
        var toUpdate = new List<WorkshopDraft>();

        var tempSectionList = sportsSections.Where(s => s.OrganizationCode == "45080641").ToList(); // temporary for testing purposes
        foreach (var section in tempSectionList) 
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
                // try to find hierarchy for section's sport kind
                if (!lookupHierarchy.TryGetValue(section.SectionSportKindDictIdCode, out var hierarchy))
                {
                    logger.LogWarning(
                        "Skipping section {SectionId} because no InstitutionHierarchy found for SportKindIdCode {SportKindIdCode}.",
                        section.SectionId,
                        section.SectionSportKindDictIdCode);
                    continue; // skip sections without valid hierarchy
                }

                // try to retrieve CATOTTGId before creating/updating draft
                var catottgId = await TryGetCatottgIdAsync(section).ConfigureAwait(false);
                if (!catottgId.HasValue)
                { 
                    continue; // skip sections without valid CATOTTGId
                }

                WorkshopDraft draft;

                if (existingDraft != null)
                {
                    // update existing draft
                    draft = section.MapToExistingDraft(existingDraft, catottgId.Value);
                    draft.DraftStatus = WorkshopDraftStatus.PendingModeration;
                    draft.WorkshopDraftContent.InstitutionHierarchyId = hierarchy.Id; // ??
                    draft.WorkshopDraftContent.InstitutionId = hierarchy.InstitutionId; // ??

                    toUpdate.Add(draft);
                    logger.LogDebug(
                        "Updating existing draft for section {SectionId}. DraftId={DraftId}, ProviderId={ProviderId}",
                        section.SectionId, draft.Id, draft.ProviderId);
                }
                else if (existingWorkshop != null)
                {
                    if (existingWorkshop.MinsportSectionId != section.SectionId)
                    {
                        logger.LogWarning(
                            "Mismatch between MinsportSectionId and SectionId for workshop {WorkshopId}: expected {Expected}, got {Actual}. Skipping sync for this section.",
                            existingWorkshop.Id,
                            existingWorkshop.MinsportSectionId,
                            section.SectionId);
                        continue; //skip sync section to not create draft for wrong section
                    }
                    // no draft,  only workshop - create draft
                    draft = section.ToWorkshopDraft(existingWorkshop.ProviderId, catottgId.Value);
                    draft.WorkshopId = existingWorkshop.Id;
                    draft.MinsportSectionId = existingWorkshop.MinsportSectionId;

                    toCreate.Add(draft);
                    logger.LogInformation(
                        "Creating new draft for existing workshop {WorkshopId} from section {SectionId}. ProviderId={ProviderId}",
                        existingWorkshop.Id, section.SectionId, draft.ProviderId);
                }
                else
                {
                    // no workshop, no draft - new draft
                    var providerId = await GetProviderIdAsync(section.OrganizationCode).ConfigureAwait(false);
                    if (!providerId.HasValue)
                    {
                        logger.LogWarning("Skipping section {SectionId} because provider was not found.", section.SectionId);
                        continue; // skip sections without valid provider
                    }

                    draft = section.ToWorkshopDraft(providerId.Value, catottgId.Value);
                    draft.WorkshopDraftContent.InstitutionHierarchyId = hierarchy.Id;
                    draft.WorkshopDraftContent.InstitutionId = hierarchy.InstitutionId;

                    toCreate.Add(draft);
                }
               
                break; // temporary for testing purposes
            }
            else
            {
                logger.LogDebug(
                    "Section {SectionId} not updated (UpdatedInRegistryAt={UpdatedAt}). Last known update={LastUpdate}",
                    section.SectionId, section.UpdatedInRegistryAt, lastUpdate);
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
                        await workshopDraftRepository.Update(draft);
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

    private async Task<long?> TryGetCatottgIdAsync(ExternalSportsSectionDto section)
    {
        var catottgId = await codeficatorRepository.GetIdByCodeAsync(section.SectionAddressLocalityDictIdCode)
            .ConfigureAwait(false);

        if (!catottgId.HasValue)
        {
            logger.LogWarning(
                "Skipping section {SectionId} because CATOTTG code '{CatottgCode}' was not found.",
                section.SectionId,
                section.SectionAddressLocalityDictIdCode);
        }

        return catottgId;
    }

    private async Task<Guid?> GetProviderIdAsync(string edrpou)
    {
        return await providerRepository.GetIdByEdrpouAsync(edrpou);
    }
}
