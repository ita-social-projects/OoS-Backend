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
    ILogger<SportsSectionSyncService> logger)
    : ISportsSectionSyncService
{
    private const int SyncIntervalMinutes = 30;

    public async Task<int> SyncSportsSectionsAsync()
    {
        try
        {
            var sportsSections = await FetchSportsSectionsAsync().ConfigureAwait(false);

            if (!sportsSections.Any())
            {
                logger.LogWarning("No sports sections were fetched from Sports Registry.");
                return 0;
            }

            var lookup = await CreateLookupsAsync(sportsSections).ConfigureAwait(false);

            var toCreate = new List<WorkshopDraft>();
            var toUpdate = new List<WorkshopDraft>();

            foreach (var section in sportsSections)
            {
                await this.ProcessSectionAsync(section, lookup, toCreate, toUpdate).ConfigureAwait(false);
            }

            await SaveDraftChangesAsync(toCreate, toUpdate);
            return toCreate.Count + toUpdate.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during sports sections sync.");
            return 0;
        }
    }

    private async Task ProcessSectionAsync(
        ExternalSportsSectionDto section,
        LookupContext lookup,
        List<WorkshopDraft> toCreate,
        List<WorkshopDraft> toUpdate
        )
    {
        lookup.ExistingDraftLookup.TryGetValue(section.SectionId, out var existingDraft);
        lookup.ExistingLookup.TryGetValue(section.SectionId, out var existingWorkshop);

        if (!IsUpdated(section, existingDraft, existingWorkshop, out var lastUpdate))
        {
            logger.LogDebug(
               "Section {SectionId} not updated (UpdatedInRegistryAt={UpdatedAt}). Last known update={LastUpdate}",
               section.SectionId, section.UpdatedInRegistryAt, lastUpdate);
            return;
        }
        // try to find hierarchy for section's sport kind
        if (!lookup.LookupHierarchy.TryGetValue(section.SectionSportKindDictIdCode, out var hierarchy))
        {
            logger.LogWarning(
                "Skipping section {SectionId} because no InstitutionHierarchy found for SportKindIdCode {SportKindIdCode}.",
                section.SectionId,
                section.SectionSportKindDictIdCode);
            return; // skip sections without valid hierarchy
        }

        // try to retrieve CATOTTGId before creating/updating draft
        var catottgId = await TryGetCatottgIdAsync(section).ConfigureAwait(false);
        if (!catottgId.HasValue)
        {
            return; // skip sections without valid CATOTTGId
        }

        if (existingDraft != null)
        {
            var updatedDraft = ProcessExistingDraft(section, existingDraft, hierarchy!, catottgId.Value);
            toUpdate.Add(updatedDraft);
            return;
        }
        if (existingWorkshop != null)
        {
            var draft = ProcessExistingWorkshop(section, existingWorkshop, catottgId.Value);
            if (draft != null)
                toCreate.Add(draft);
            
            return;
        }
       
        // no workshop, no draft - new draft
        var newDraft = await CreateNewDraftAsync(section, hierarchy!, catottgId.Value).ConfigureAwait(false);
        if (newDraft != null)
        {
            toCreate.Add(newDraft);
        }
    }

    private WorkshopDraft ProcessExistingDraft(
        ExternalSportsSectionDto section,
        WorkshopDraft existingDraft,
        InstitutionHierarchy hierarchy,
        long catottgId)
    {
        var draft = section.MapToExistingDraft(existingDraft, catottgId);
        draft.DraftStatus = WorkshopDraftStatus.PendingModeration;

        draft.WorkshopDraftContent.InstitutionHierarchyId = hierarchy.Id;
        draft.WorkshopDraftContent.InstitutionId = hierarchy.InstitutionId;

        logger.LogDebug(
            "Updating existing draft for section {SectionId}. DraftId={DraftId}, ProviderId={ProviderId}",
            section.SectionId, draft.Id, draft.ProviderId);

        return draft;
    }

    private WorkshopDraft? ProcessExistingWorkshop(
    ExternalSportsSectionDto section,
    Workshop existingWorkshop,
    long catottgId)
    {
        if (existingWorkshop.MinsportSectionId != section.SectionId)
        {
            logger.LogWarning(
                "Mismatch between MinsportSectionId and SectionId for workshop {WorkshopId}: expected {Expected}, got {Actual}.",
                existingWorkshop.Id,
                existingWorkshop.MinsportSectionId,
                section.SectionId);

            return null;
        }

        var draft = section.ToWorkshopDraft(existingWorkshop.ProviderId, catottgId);
        draft.WorkshopId = existingWorkshop.Id;
        draft.MinsportSectionId = existingWorkshop.MinsportSectionId;

        logger.LogInformation(
            "Creating new draft for existing workshop {WorkshopId} from section {SectionId}.",
            existingWorkshop.Id, section.SectionId);

        return draft;
    }
    private async Task<WorkshopDraft?> CreateNewDraftAsync(
        ExternalSportsSectionDto section,
        InstitutionHierarchy hierarchy,
        long catottgId)
    {
        var providerId = await GetProviderIdAsync(section.OrganizationCode).ConfigureAwait(false);
        if (!providerId.HasValue)
        {
            logger.LogWarning("Skipping section {SectionId} because provider not found.", section.SectionId);
            return null;
        }

        var draft = section.ToWorkshopDraft(providerId.Value, catottgId);
        draft.WorkshopDraftContent.InstitutionHierarchyId = hierarchy.Id;
        draft.WorkshopDraftContent.InstitutionId = hierarchy.InstitutionId;

        return draft;
    }

    private async Task<LookupContext> CreateLookupsAsync(List<ExternalSportsSectionDto> sportsSections)
    {
        var registrySectionsId = sportsSections
            .Select(x => x.SectionId)
            .ToHashSet();

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

        var hierarchyLookup = existingHierarchies.ToDictionary(x => x.SportRegistryIdCode!.Value);

        return new LookupContext(existingLookup, existingDraftLookup, hierarchyLookup);
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
    private async Task<List<ExternalSportsSectionDto>> FetchSportsSectionsAsync()
    {
        var updatedAtTo = DateTimeOffset.UtcNow;
        var updatedAtFrom = updatedAtTo.AddMinutes(-SyncIntervalMinutes);

        logger.LogInformation("Fetching updated sections from {From} to {To}", updatedAtFrom, updatedAtTo);

        var response = await workshopProvider.GetAllSportsSectionsAsync(updatedAtFrom, updatedAtTo).ConfigureAwait(false);

        if (response.TryGetLeft(out var error))
        {
            logger.LogWarning("Filtered fetch failed with: {Error}. Falling back to full fetch.", error.Message);
            response = await workshopProvider.GetAllSportsSectionsAsync().ConfigureAwait(false);
        }

        return response.Match(
            error => throw new InvalidOperationException($"Failed to fetch sports sections: {error.Message}"),
            success => success);
    }

    private async Task SaveDraftChangesAsync(List<WorkshopDraft> toCreate, List<WorkshopDraft> toUpdate)
    {
        if (!toCreate.Any() && !toUpdate.Any())
            return;

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

    private bool IsUpdated(ExternalSportsSectionDto section, WorkshopDraft? draft, Workshop? workshop, out DateTimeOffset? lastUpdate)
    {
        lastUpdate = draft?.ModifiedAt
                         ?? (workshop?.UpdatedAt.HasValue == true
                             ? new DateTimeOffset(workshop.UpdatedAt.Value)
                             : null);

        return !lastUpdate.HasValue || section.UpdatedInRegistryAt > lastUpdate.Value;
    }
}
public record LookupContext
(
    Dictionary<Guid, Workshop> ExistingLookup,
    Dictionary<Guid, WorkshopDraft> ExistingDraftLookup,
    Dictionary<long, InstitutionHierarchy> LookupHierarchy
);