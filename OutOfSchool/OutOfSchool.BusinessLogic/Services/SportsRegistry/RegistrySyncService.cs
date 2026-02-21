using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;

/// <summary>
/// Provides synchronization logic between the domain (workshops, drafts)
/// and the external Sports Registry system.
/// 
/// Responsibilities:
/// - Builds and normalizes requests before sending them to the registry API.
/// - Determines whether to create or update a section in the registry.
/// - Handles registry responses and updates local entities accordingly.
/// - Encapsulates all error handling and logging related to registry operations.
/// 
/// This service isolates registry-specific integration details from the
/// domain services (<see cref="WorkshopDraftService"/>), promoting single responsibility,
/// testability, and reuse.
/// </summary>
public class RegistrySyncService : IRegistrySyncService
{
    private readonly ISportsRegistrySectionProvider sportsRegistrySectionApi;
    private readonly ICodeficatorService codeficatorService;
    private readonly IInstitutionHierarchyService institutionHierarchyService;
    private readonly ILogger<RegistrySyncService> logger;
    private readonly string baseImageUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrySyncService"/> class.
    /// </summary>
    /// <param name="sportsRegistrySectionApi">Client for interacting with the external Sports Registry API.</param>
    /// <param name="codeficatorRepository">Repository for resolving CATOTTG codes.</param>
    /// <param name="institutionHierarchyService">Service for retrieving institution hierarchy data.</param>
    /// <param name="logger">Logger instance for error and information logging.</param>
    /// <param name="imageOptions">Options that provide the base image URL.</param>
    public RegistrySyncService(
        ISportsRegistrySectionProvider sportsRegistrySectionApi,
        ICodeficatorService codeficatorService,
        IInstitutionHierarchyService institutionHierarchyService,
        ILogger<RegistrySyncService> logger,
        IOptions<ImageStorageOptions> imageOptions)
    {
        this.sportsRegistrySectionApi = sportsRegistrySectionApi;
        this.codeficatorService = codeficatorService;
        this.institutionHierarchyService = institutionHierarchyService;
        this.logger = logger;
        this.baseImageUrl = imageOptions.Value.BaseImageUrl;
    }

    /// <inheritdoc/>
    public async Task SyncDraftAsync(WorkshopDraft draft)
    {
        var institutionHierarchyId = draft.WorkshopDraftContent.InstitutionHierarchyId
            ?? throw new ArgumentException("InstitutionHierarchyId cannot be null.");

        if (draft.MinsportSectionId is null)
        {
            // Create
            var request = draft.ToSportSectionPostRequest(baseImageUrl);
            await NormalizeRequestAsync(request, institutionHierarchyId);
            var response = await sportsRegistrySectionApi.RegisterSectionAsync(request).ConfigureAwait(false);
            HandleDraftResponse(response, draft, isCreate: true);
        }
        else
        {
            // Update
            var request = draft.ToSportSectionUpdateRequest(baseImageUrl);
            await NormalizeRequestAsync(request, institutionHierarchyId);
            var response = await sportsRegistrySectionApi.UpdateSectionAsync(request).ConfigureAwait(false);
            HandleDraftResponse(response, draft, isCreate: false);
        }
    }

    /// <inheritdoc/>
    public async Task SyncWorkshopAsync(WorkshopV2Dto workshopDto)
    {
        var institutionHierarchyId = workshopDto.InstitutionHierarchyId
            ?? throw new ArgumentException("InstitutionHierarchyId cannot be null.");

        var request = workshopDto.ToSportSectionUpdateRequest(baseImageUrl);
        await NormalizeRequestAsync(request, institutionHierarchyId);

        var response = await sportsRegistrySectionApi.UpdateSectionAsync(request).ConfigureAwait(false);
        response.Match(
            error =>
            {
                var details = error.Content ?? error.Message ?? "Unknown";
                logger.LogError(
                    "Failed to sync workshop directly to Sports Registry. Code={Code}, Message={Message}",
                    (int)error.HttpStatusCode, details);

                throw new InvalidOperationException($"Registry sync failed: {details}");
            },
            success =>
            {
                logger.LogInformation(
                    "Workshop was successfully synced with Sports Registry. WorkshopId={WorkshopId}, SectionId={SectionId}",
                    workshopDto.Id, workshopDto.MinsportSectionId);

                return true;
            }
        );
    }

    /// <summary>
    /// Normalizes and validates common request fields before sending them to the registry.
    /// </summary>
    /// <param name="request">The request to normalize.</param>
    /// <param name="institutionHierarchyId">The institution hierarchy identifier associated with the section.</param>
    private async Task NormalizeRequestAsync(SportsSectionBaseDto request, Guid institutionHierarchyId)
    {
        request.SectionSportKindDictIdCode = await GetSportKindCodeAsync(institutionHierarchyId);
        request.SectionAddressLocalityDictIdCode = await NormalizeCatottgCodeAsync(request.SectionAddressLocalityDictIdCode);
    }

    /// <summary>
    /// Retrieves and validates the sport kind code from the institution hierarchy.
    /// </summary>
    /// <param name="institutionHierarchyId">The identifier of the institution hierarchy.</param>
    /// <returns>The sport kind dictionary code.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the hierarchy or code is missing.</exception>
    private async Task<long> GetSportKindCodeAsync(Guid institutionHierarchyId)
    {
        if (institutionHierarchyId == Guid.Empty)
        {
            throw new InvalidOperationException("InstitutionHierarchyId is missing.");
        }
        var hierarchy = await institutionHierarchyService.GetById(institutionHierarchyId)
            ?? throw new InvalidOperationException($"InstitutionHierarchy with Id {institutionHierarchyId} not found.");

        return hierarchy.SportRegistryIdCode
            ?? throw new InvalidOperationException($"SportRegistryIdCode missing for InstitutionHierarchy {institutionHierarchyId}.");
    }

    /// <summary>
    /// Validates and resolves a CATOTTG code string into a normalized registry code.
    /// </summary>
    /// <param name="rawValue">The raw CATOTTG identifier string.</param>
    /// <returns>The normalized registry code.</returns>
    /// <exception cref="InvalidOperationException">Thrown if parsing or resolution fails.</exception>
    private async Task<string> NormalizeCatottgCodeAsync(string? rawValue)
    {
        if (!long.TryParse(rawValue, out var catottgId))
            throw new InvalidOperationException($"Invalid CATOTTG Id: {rawValue}");

        var code = await codeficatorService.GetCodeByIdAsync(catottgId).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException($"Codeficator code not found for CATOTTG Id {catottgId}.");

        return code;
    }

    /// <summary>
    /// Processes the registry response for a draft operation,
    /// updating the draft with the SectionId if successful, or throwing on error.
    /// </summary>
    /// <param name="response">The response from the registry API.</param>
    /// <param name="draft">The draft being synchronized.</param>
    /// <param name="isCreate">True if this is a create operation; false if update.</param>
    private void HandleDraftResponse(
        Either<ErrorResponse, SectionCreateUpdateResponse> response,
        WorkshopDraft draft,
        bool isCreate)
    {
        response.Match(
            error =>
            {
                var details = error.Content ?? error.Message ?? "Unknown";
                logger.LogError(
                    "Failed to sync draft to Sports Registry. Code={Code}, Message={Message}",
                    (int)error.HttpStatusCode, details);

                throw new InvalidOperationException($"Registry sync failed: {details}");
            },
            success =>
            {
                if(success.ResultVariables.SectionId is not null)
                {
                    draft.MinsportSectionId = success.ResultVariables.SectionId;
                }
                var action = isCreate ? "created" : "updated";
                logger.LogInformation(
                    "Draft was successfully {Action} in Sports Registry. DraftId={DraftId}, SectionId={SectionId}",
                    action, draft.Id, success.ResultVariables.SectionId);

                return true;
            });
    }
}
