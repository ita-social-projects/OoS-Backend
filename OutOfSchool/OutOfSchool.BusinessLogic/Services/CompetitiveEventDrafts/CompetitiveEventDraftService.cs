using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
public class CompetitiveEventDraftService(ILogger<CompetitiveEventDraftService> logger,
    ICurrentUserService currentUserService,
    ICompetitiveEventServiceV2 competitiveEventService,
    ICompetitiveEventDraftRepository competitiveEventDraftRepository,
    IImageDependentEntityImagesInteractionService<CompetitiveEventDraft> competitiveEventDraftImagesService,
    IChangesLogService changesLogService,
    ICodeficatorRepository codeficatorRepository,
    IRegionAdminService regionAdminService,
    IMinistryAdminService ministryAdminService,
    ICodeficatorService codeficatorService,
    ISearchStringService searchStringService,
    IEntityRepositorySoftDeleted<long, SubDirection> subDirectionRepository) : ICompetitiveEventDraftService, ISensitiveCompetitiveEventDraftService
{

    // <inheritdoc/>
    public async Task<CompetitiveEventDraftResultDto> Create(CompetitiveEventV2Dto competitiveEventV2Dto, bool fromCompetitiveEvent = false)
    {
        if (competitiveEventV2Dto == null)
        {
            logger.LogError(
               "ArgumentNullException: While executing the method '{MethodName}'," +
               " the parameter '{ParameterName}' is null.",
               nameof(Create),
               nameof(CompetitiveEventV2Dto));

            throw new ArgumentNullException(nameof(competitiveEventV2Dto));
        }

        logger.LogDebug("Creating competitive event draft with details: {CompetitiveEventDetails}", competitiveEventV2Dto);

        await currentUserService.UserHasRights(new ProviderRights(competitiveEventV2Dto.OrganizerOfTheEventId),
            new EmployeeRights(competitiveEventV2Dto.OrganizerOfTheEventId)).ConfigureAwait(false);

        if (competitiveEventV2Dto.Id != Guid.Empty)
        {
            var existingCompetitiveEvent = await competitiveEventService.GetById(competitiveEventV2Dto.Id);

            if (existingCompetitiveEvent == null)
            {
                competitiveEventV2Dto.Id = Guid.Empty;
            }
            else
            {
                if (existingCompetitiveEvent.State == CompetitiveEventStates.Archived)
                {
                    throw new InvalidOperationException("This CompetitiveEvent is archived. It can not be updated.");
                }
                await currentUserService.UserHasRights(new ProviderRights(existingCompetitiveEvent.OrganizerOfTheEventId),
                    new EmployeeRights(existingCompetitiveEvent.OrganizerOfTheEventId)).ConfigureAwait(false);
            }
        }

        async Task<Result<(CompetitiveEventDraft createdCompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult uploadImagesResult)>>
        CreateCompetitiveEventDraftWithImages()
        {
            NormalizeConditionalFields(competitiveEventV2Dto);

            var createdCompetitiveEventDraft = await CreateCompetitiveEventDraft(competitiveEventV2Dto)
                .ConfigureAwait(false);

            var uploadImagesResult = await UploadImages(createdCompetitiveEventDraft, competitiveEventV2Dto)
                .ConfigureAwait(false);

            if (fromCompetitiveEvent)
            {
                createdCompetitiveEventDraft.Images ??= [];
                createdCompetitiveEventDraft.Images.AddRange((competitiveEventV2Dto.ImageIds ?? [])
                    .Select(id => new Image<CompetitiveEventDraft> { ExternalStorageId = id }));
            }

            await competitiveEventDraftRepository.SaveChangesAsync().ConfigureAwait(false);

            return Result<(CompetitiveEventDraft createdCompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult uploadImagesResult)>
                .Success((createdCompetitiveEventDraft, uploadImagesResult));
        }

        var draftImageUpdateResult = await competitiveEventDraftRepository
            .RunInTransaction(CreateCompetitiveEventDraftWithImages).ConfigureAwait(false);

        if (!draftImageUpdateResult.Succeeded)
        {
            throw new InvalidOperationException(
                draftImageUpdateResult.OperationResult?.Errors?.FirstOrDefault()?.Description
                ?? "Failed to create competitive event draft");
        }

        logger.LogDebug("Competitive event draft created successfully.");

        return new CompetitiveEventDraftResultDto
        {
            CompetitiveEventDraft = await MapCompetitiveEventDraftWithDetails(draftImageUpdateResult.Value.createdCompetitiveEventDraft),
            UploadingCoverImagesCompetitiveEventResult = draftImageUpdateResult.Value.uploadImagesResult?.UploadingCoverImageResult,
            UploadingImagesResults = draftImageUpdateResult.Value.uploadImagesResult?.UploadingImagesResults?.MultipleKeyValueOperationResult
        };
    }

    // <inheritdoc/>
    public async Task<Result<CompetitiveEventDraftResultDto>> Update(Guid id, CompetitiveEventDraftUpdateDto competitiveEventDraftUpdateDto)
    {
        if (competitiveEventDraftUpdateDto == null || competitiveEventDraftUpdateDto.CompetitiveEventV2Dto == null)
        {
            return Result<CompetitiveEventDraftResultDto>.Failed(new OperationError()
            {
                Code = "400",
                Description = "Dto can't be null."
            });
        }

        if (id == Guid.Empty)
        {
            return Result<CompetitiveEventDraftResultDto>.Failed(new OperationError()
            {
                Code = "400",
                Description = "ID in route can't be empty."
            });
        }

        if (competitiveEventDraftUpdateDto.Id == Guid.Empty)
        {
            return Result<CompetitiveEventDraftResultDto>.Failed(new OperationError()
            {
                Code = "400",
                Description = "Dto's id can't be empty."
            });
        }

        if (competitiveEventDraftUpdateDto.Id != id)
        {
            return Result<CompetitiveEventDraftResultDto>.Failed(new OperationError()
            {
                Code = "400",
                Description = "ID in route and DTO do not match."
            });
        }

        logger.LogDebug("Updating competitive event draft with ID: {DraftId}", competitiveEventDraftUpdateDto.Id);

        NormalizeConditionalFields(competitiveEventDraftUpdateDto.CompetitiveEventV2Dto);

        var draftImageUpdateResult = await competitiveEventDraftRepository
            .RunInTransaction(() => UpdateDraftWithImagesAsync(competitiveEventDraftUpdateDto))
            .ConfigureAwait(false);

        if (!draftImageUpdateResult.Succeeded)
        {
            return Result<CompetitiveEventDraftResultDto>.Failed(new OperationError()
            {
                Code = draftImageUpdateResult.OperationResult?.Errors?.FirstOrDefault()?.Code ?? "500",
                Description = draftImageUpdateResult.OperationResult?.Errors?.FirstOrDefault()?.Description ?? "Image uploading gone wrong."
            });
        }

        var (updatedDraft, coverImageResult, imagesResult) = draftImageUpdateResult.Value;

        // load it again to fetch full data
        var updatedDraftWithDetails = await GetByIdWithProviderDetails(updatedDraft.Id);

        return Result<CompetitiveEventDraftResultDto>.Success(new CompetitiveEventDraftResultDto
        {
            CompetitiveEventDraft = await MapCompetitiveEventDraftWithDetails(updatedDraftWithDetails),
            UploadingCoverImagesCompetitiveEventResult = coverImageResult?.UploadingResult?.OperationResult,
            UploadingImagesResults = imagesResult?.UploadedMultipleResult?.MultipleKeyValueOperationResult
        });
    }

    // <inheritdoc/>
    public async Task<OperationResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            return OperationResult.Failed(new OperationError()
            {
                Code = "400",
                Description = "Id cannot be empty."
            });
        }

        logger.LogDebug("Deleting competitive event draft with ID: {DraftId}", id);

        var competitiveEventDraft = await GetDraftById(id).ConfigureAwait(false);

        if (competitiveEventDraft == null)
        {
            return OperationResult.Failed(new OperationError()
            {
                Code = "404",
                Description = "Competitive event draft not found."
            });
        }

        await currentUserService.UserHasRights(
            new ProviderRights(competitiveEventDraft.ProviderId),
            new EmployeeRights(competitiveEventDraft.ProviderId))
            .ConfigureAwait(false);

        if (competitiveEventDraft.DraftStatus == CompetitiveEventDraftStatus.PendingModeration)
        {
            logger.LogWarning("Competitive event draft with ID {DraftId} is in PendingModeration status and cannot be deleted.", id);
            return OperationResult.Failed(new OperationError
            {
                Code = "400",
                Description = "Competitive event draft can only be deleted when it is not in PendingModeration status."
            });
        }

        await competitiveEventDraftRepository.Delete(competitiveEventDraft).ConfigureAwait(false);
        logger.LogDebug("Competitive event draft with ID {DraftId} deleted successfully.", id);

        return OperationResult.Success;
    }

    // <inheritdoc/>
    public async Task<OperationResult> SendForModeration(Guid id)
    {
        if (id == Guid.Empty)
        {
            return OperationResult.Failed(new OperationError()
            {
                Code = "400",
                Description = "Id cannot be empty."
            });
        }

        logger.LogDebug("Sending competitive event draft with ID {DraftId} for moderation.", id);

        var competitiveEventDraft = await GetDraftById(id).ConfigureAwait(false);

        if (competitiveEventDraft == null)
        {
            return OperationResult.Failed(new OperationError
            {
                Code = "404",
                Description = "Competitive event draft not found."
            });
        }

        await currentUserService.UserHasRights(
            new ProviderRights(competitiveEventDraft.ProviderId),
            new EmployeeRights(competitiveEventDraft.ProviderId))
            .ConfigureAwait(false);

        if (competitiveEventDraft.DraftStatus != CompetitiveEventDraftStatus.Draft)
        {
            logger.LogWarning("Competitive event draft with ID {DraftId} is not in Draft status and cannot be sent for moderation.", id);
            return OperationResult.Failed(new OperationError
            {
                Code = "400",
                Description = "Competitive event draft can only be sent for moderation when it is in Draft status."
            });
        }

        competitiveEventDraft.DraftStatus = CompetitiveEventDraftStatus.PendingModeration;
        await competitiveEventDraftRepository.Update(competitiveEventDraft).ConfigureAwait(false);

        logger.LogDebug("Competitive event draft with ID {DraftId} sent for moderation successfully.", id);

        return OperationResult.Success;
    }

    // <inheritdoc/>
    public async Task<SearchResult<CompetitiveEventDraftViewCardDto>> GetByProviderId(Guid id, CompetitiveEventDraftFilterTitle filter)
    {
        logger.LogDebug("Retrieving competitive event drafts for provider with ID: {ProviderId}", id);

        await currentUserService.UserHasRights(new ProviderRights(id), new EmployeeRights(id)).ConfigureAwait(false);

        filter ??= new CompetitiveEventDraftFilterTitle();
        ValidateCompetitiveEventDraftTitleFilter(filter);

        var predicate = BuildPredicate(filter, id);

        var competitiveEventCardsCount = await competitiveEventDraftRepository
            .Count(whereExpression: predicate).ConfigureAwait(false);

        var competitiveEventDrafts = await competitiveEventDraftRepository.Get(
            skip: filter.From,
            take: filter.Size,
            whereExpression: predicate).ToListAsync().ConfigureAwait(false);

        var competitiveEventDraftResponseDtos = new List<CompetitiveEventDraftViewCardDto>();

        foreach (var draft in competitiveEventDrafts)
        {
            var responseDto = draft.ToCardDto();
            competitiveEventDraftResponseDtos.Add(responseDto);
        }

        logger.LogDebug("Retrieved {Count} competitive event drafts for provider with ID: {ProviderId}", competitiveEventDraftResponseDtos.Count, id);

        return new SearchResult<CompetitiveEventDraftViewCardDto>
        {
            TotalAmount = competitiveEventCardsCount,
            Entities = competitiveEventDraftResponseDtos
        };
    }

    // <inheritdoc/>
    public async Task<CompetitiveEventDraftResponseDto> GetCompetitiveEventDraftByIdMapped(Guid id)
    {
        logger.LogDebug("Retrieving competitive event draft with ID: {Id}", id);

        var draft = await GetByIdWithProviderDetails(id).ConfigureAwait(false);

        if (draft == null)
        {
            return null;
        }

        await currentUserService.UserHasRights(
            new ProviderRights(draft.ProviderId),
            new EmployeeRights(draft.ProviderId),
            new ModeratorRights(),
            new TechAdminRights())
            .ConfigureAwait(false);

        return await MapCompetitiveEventDraftWithDetails(draft).ConfigureAwait(false);
    }

    // <inheritdoc/>
    public async Task Approve(Guid id)
    {
        logger.LogDebug("Approving CompetitiveEventDraft started. CompetitiveEventDraft Id = {Id}.", id);

        var competitiveEventDraft = await GetDraftById(id) ?? throw new ArgumentException($"There is no CompetitiveEvent draft with such Id.");

        if (competitiveEventDraft.DraftStatus != CompetitiveEventDraftStatus.PendingModeration && competitiveEventDraft.DraftStatus != CompetitiveEventDraftStatus.EditedByModerator)
        {
            throw new ArgumentException("This Competitive event draft can`t be approved.");
        }

        if (competitiveEventDraft.CompetitiveEventId == null)
        {
            await competitiveEventService.CreateV2(competitiveEventDraft.ToDto());
        }
        else
        {
            await competitiveEventService.UpdateV2(competitiveEventDraft.ToDto(), true);
        }

        await competitiveEventDraftRepository.Delete(competitiveEventDraft);

        logger.LogDebug("Draft was successfully approved and deleted. Draft Id = {DraftId}.", id);
    }

    // <inheritdoc/>
    public async Task Reject(Guid id, string rejectionMessage)
    {
        logger.LogDebug("Rejecting CompetitiveEventDraft started. CompetitiveEventDraft Id = {id}.", id);

        var competitiveEventDraft = await GetDraftById(id) ?? throw new ArgumentException($"There is no CompetitiveEvent draft with such Id.");

        if (competitiveEventDraft.DraftStatus != CompetitiveEventDraftStatus.PendingModeration && competitiveEventDraft.DraftStatus != CompetitiveEventDraftStatus.EditedByModerator)
        {
            throw new ArgumentException("This CompetitiveEventDraft can`t be rejected.");
        }

        competitiveEventDraft.DraftStatus = CompetitiveEventDraftStatus.Rejected;
        competitiveEventDraft.RejectionMessage = rejectionMessage;

        await competitiveEventDraftRepository.Update(competitiveEventDraft);
        logger.LogDebug("CompetitiveEventDraft was successfully rejected. Draft Id = {id}.", id);
    }

    // <inheritdoc/>
    public async Task<CompetitiveEventV2Dto> UpdateCompetitiveEvent(CompetitiveEventV2Dto competitiveEventV2Dto)
    {
        logger.LogDebug("Competitive event updating process started. CompetitiveEvent Id = {Id}.", competitiveEventV2Dto.Id);

        var existingCompetitiveEvent = await competitiveEventService.GetById(competitiveEventV2Dto.Id);

        if (existingCompetitiveEvent == null)
        {
            throw new InvalidOperationException($"There is no CompetitiveEvent with such Id. CompetitiveEvent can`t be updated.");
        }

        if (existingCompetitiveEvent.State == CompetitiveEventStates.Archived)
        {
            throw new InvalidOperationException("This CompetitiveEvent is archived. It can not be updated.");
        }

        var draft = await competitiveEventDraftRepository.Get(whereExpression: ced => ced.CompetitiveEventId == competitiveEventV2Dto.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (draft != null)
        {
            logger.LogDebug("CompetitiveEvent draft for this CompetitiveEvent exists. CompetitiveEvent can`t be updated. CompetitiveEvent Id = {Id}.", competitiveEventV2Dto.Id);

            throw new InvalidOperationException("CompetitiveEvent draft for this CompetitiveEvent exists. CompetitiveEvent can`t be updated.");
        }

        NormalizeConditionalFields(competitiveEventV2Dto);

        if (ShouldBeModerate(competitiveEventV2Dto, existingCompetitiveEvent))
        {
            logger.LogDebug("Moderated fields was changed. CompetitiveEvent draft creation initiated. CompetitiveEvent Id = {Id}.", competitiveEventV2Dto.Id);
            return (await Create(competitiveEventV2Dto, true)).CompetitiveEventDraft.CompetitiveEventDetails;
        }

        logger.LogDebug("Moderated fields was not changed. CompetitiveEvent update initiated. CompetitiveEvent Id = {Id}.", competitiveEventV2Dto.Id);

        var result = await competitiveEventService.UpdateV2(competitiveEventV2Dto).ConfigureAwait(false);

        return result.CompetitiveEventV2;
    }

    /// <summary>
    /// Determines whether any moderated fields differ between an incoming V2 DTO and an existing competitive event.
    /// Returns false when the new value is null or an empty string.
    /// </summary>
    /// <param name="competitiveEventV2Dto">The incoming competitive event data to compare.</param>
    /// <param name="existingCompetitiveEvent">The existing competitive event to compare against.</param>
    /// <returns>
    /// True if any moderated field has changed and the new value is not null or an empty string and therefore requires moderation; false otherwise.
    /// </returns>
    /// <remarks>
    /// The comparison considers:
    /// - Presence of new images (CoverImage or ImageFiles) on the V2 DTO.
    /// - Competitive event description items compared by concatenating SectionName and Description in sequence (order-sensitive).
    /// - The following string fields: ShortTitle, Title, DescriptionOfTheEnrollmentProcedure, CompetitiveSelectionDescription, Benefits, VenueName, and Contacts (contacts are compared by joining each contact's ToString() with " | ").
    /// If any of the above fields are different and the new value is not null or an empty string, the method returns true; otherwise false.    /// </remarks>
    private static bool ShouldBeModerate(CompetitiveEventV2Dto competitiveEventV2Dto, CompetitiveEventDto existingCompetitiveEvent)
    {
        if (competitiveEventV2Dto.CoverImage != null || competitiveEventV2Dto.ImageFiles != null)
        {
            return true;
        }

        if (!competitiveEventV2Dto.CompetitiveEventDescriptionItems.Select(wdi => wdi.SectionName + wdi.Description)
                .SequenceEqual(existingCompetitiveEvent.CompetitiveEventDescriptionItems.Select(wdi => wdi.SectionName + wdi.Description)))
        {
            return true;
        }

        var stringFieldsToCompare = new List<Func<CompetitiveEventDto, string>>
        {
            ce => ce.ShortTitle,
            ce => ce.Title,
            ce => ce.DescriptionOfTheEnrollmentProcedure,
            ce => ce.CompetitiveSelectionDescription,
            ce => ce.Benefits,
            ce => ce.VenueName,
            ce => string.Join(" | ", (ce.Contacts ?? []).Select(c => c?.ToString()))
        };

        return stringFieldsToCompare.Any(field =>
        {
            var newValue = field(competitiveEventV2Dto);
            var oldValue = field(existingCompetitiveEvent);

            return !string.Equals(newValue, oldValue, StringComparison.Ordinal) && !string.IsNullOrEmpty(newValue);
        });
    }

    // <inheritdoc/>
    public async Task<Guid?> GetCompetitiveEventDraftIdByCompetitiveEventId(Guid competitiveEventId)
    {
        logger.LogDebug("Getting CompetitiveEventDraft Id by CompetitiveEvent Id started. Id = {id}.", competitiveEventId);

        var competitiveEventDraft = await competitiveEventDraftRepository.Get(whereExpression: wd => wd.CompetitiveEventId == competitiveEventId).FirstOrDefaultAsync();

        if (competitiveEventDraft == null)
        {
            return null;
        }

        return competitiveEventDraft.Id;
    }

    // <inheritdoc/>
    public async Task<Result<CompetitiveEventDraftResponseDto>> DeleteCoverImageAsModeratorAsync(Guid draftId)
    {
        logger.LogDebug("Deleting cover image as moderator started. CompetitiveEventDraft Id = {Id}.", draftId);

        var validation = await ValidateDraftForModerator(draftId);

        if (!validation.Succeeded)
        {
            return validation.ToFailedResult<CompetitiveEventDraftResponseDto>();
        }

        var competitiveEventDraft = validation.Value;

        if (competitiveEventDraft.CoverImageId == null)
        {
            logger.LogWarning("CompetitiveEventDraft with Id = {Id} doesn't have a cover image to delete.", draftId);

            return Result<CompetitiveEventDraftResponseDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "No cover image exists for this competitive event draft."
            });
        }

        try
        {
            await competitiveEventDraftImagesService.RemoveCoverImageAsync(competitiveEventDraft);
            competitiveEventDraft.DraftStatus = CompetitiveEventDraftStatus.EditedByModerator;

            changesLogService.AddEntityChangesToDbContext(competitiveEventDraft, currentUserService.UserId);

            await competitiveEventDraftRepository.Update(competitiveEventDraft).ConfigureAwait(false);

            logger.LogInformation("Cover image successfully deleted from CompetitiveEventDraft. Id = {Id}.", draftId);
            return Result<CompetitiveEventDraftResponseDto>.Success(competitiveEventDraft.ToResponseDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting cover image for CompetitiveEventDraft with ID {DraftId}.", draftId);
            return Result<CompetitiveEventDraftResponseDto>.Failed(new OperationError
            {
                Code = "500",
                Description = "An error occurred while deleting the cover image."
            });
        }
    }

    // <inheritdoc/>
    public async Task<SearchResult<CompetitiveEventDraftResponseDto>> FetchByFilterForAdmins(CompetitiveEventDraftFilterAdministration filter = null)
    {
        logger.LogDebug("Started retrieving Competitive Event Drafts by filter for admins.");

        filter ??= new CompetitiveEventDraftFilterAdministration();

        var (adminInstitutionId, catottgIdAdmin) = await GetAdminInstitutionAndCatottgIds();

        var allowedSettlementIdsForAdmin = Enumerable.Empty<long>();
        var subSettlementsIdsByFilter = Enumerable.Empty<long>();

        if (catottgIdAdmin > 0)
        {
            allowedSettlementIdsForAdmin = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(catottgIdAdmin)
                .ConfigureAwait(false);
        }

        if (filter.CATOTTGId > 0)
        {
            subSettlementsIdsByFilter = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(filter.CATOTTGId)
                .ConfigureAwait(false);
        }

        var predicate = PredicateBuildForAdmins(
            filter,
            adminInstitutionId,
            allowedSettlementIdsForAdmin.ToList(),
            subSettlementsIdsByFilter.ToList());

        var competitiveEventDrafts = await competitiveEventDraftRepository.Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate)
            .Include(d => d.Provider)
                .ThenInclude(p => p.Positions)
                    .ThenInclude(pos => pos.Officials)
                        .ThenInclude(o => o.Individual)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        var competitiveEventDraftsCount = await competitiveEventDraftRepository
            .Count(predicate)
            .ConfigureAwait(false);

        logger.LogDebug("Retrieved {DraftsCount} matching records by filter for admins", competitiveEventDraftsCount);

        return new SearchResult<CompetitiveEventDraftResponseDto>()
        {
            TotalAmount = competitiveEventDraftsCount,
            Entities = await MapCompetitiveEventDraftsCollectionWithDetails(competitiveEventDrafts),
        };
    }

    // <inheritdoc/>
    public async Task<Result<CompetitiveEventDraftResponseDto>> DeleteImagesAsModeratorAsync(Guid draftId, IEnumerable<string> imageIds)
    {
        logger.LogDebug("Deleting multiple images as moderator started. CompetitiveEventDraft Id = {Id}.", draftId);

        if (imageIds == null || !imageIds.Any())
        {
            return Result<CompetitiveEventDraftResponseDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "At least one ImageId must be provided."
            });
        }

        var validation = await ValidateDraftForModerator(draftId);

        if (!validation.Succeeded)
        {
            return validation.ToFailedResult<CompetitiveEventDraftResponseDto>();
        }

        var competitiveEventDraft = validation.Value;

        var decodedImageIds = imageIds.Select(Uri.UnescapeDataString).ToList();

        var imagesToDelete = competitiveEventDraft.Images
            .Where(i => decodedImageIds.Contains(i.ExternalStorageId))
            .ToList();

        if (imagesToDelete.Count == 0)
        {
            return Result<CompetitiveEventDraftResponseDto>.Failed(new OperationError
            {
                Code = "404",
                Description = "None of the specified images were found in this competitive event draft."
            });
        }

        var oldImageIds = competitiveEventDraft.Images.Select(x => x.ExternalStorageId).ToList();

        try
        {
            await competitiveEventDraftImagesService.RemoveManyImagesAsync(competitiveEventDraft, decodedImageIds);

            var newImageIds = competitiveEventDraft.Images.Select(x => x.ExternalStorageId).ToList();

            competitiveEventDraft.DraftStatus = CompetitiveEventDraftStatus.EditedByModerator;

            changesLogService.LogImageDeletions(
                oldImageIds,
                newImageIds,
                competitiveEventDraft.Id,
                "CompetitiveEventDraft",
                currentUserService.UserId);

            await competitiveEventDraftRepository.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("{Count} images successfully deleted from CompetitiveEventDraft. Draft Id = {DraftId}.",
                imagesToDelete.Count, draftId);

            // load draft with full information
            var draftWithDetails = await GetByIdWithProviderDetails(competitiveEventDraft.Id);

            return Result<CompetitiveEventDraftResponseDto>.Success(draftWithDetails.ToResponseDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting images for CompetitiveEventDraft with ID {DraftId}.", draftId);
            return Result<CompetitiveEventDraftResponseDto>.Failed(new OperationError
            {
                Code = "500",
                Description = "An error occurred while deleting the images."
            });
        }
    }

    // <inheritdoc/>
    public async Task<Result<CompetitiveEventDraftResponseDto>> UpdateDraftAsModeratorAsync(Guid draftId, ModeratorCompetitiveEventDraftEditDto dto)
    {
        if (dto == null)
        {
            logger.LogError("Parameter '{ParameterName}' is null.", nameof(dto));

            return Result<CompetitiveEventDraftResponseDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "Dto must not be null."
            });
        }

        logger.LogDebug("Updating competitive event as moderator started. CompetitiveEventDraft Id = {Id}.", draftId);

        var validation = await ValidateDraftForModerator(draftId);

        if (!validation.Succeeded)
        {
            return validation.ToFailedResult<CompetitiveEventDraftResponseDto>();
        }

        var competitiveEventDraft = validation.Value;

        try
        {
            dto.ToDraft(competitiveEventDraft);
            competitiveEventDraft.DraftStatus = CompetitiveEventDraftStatus.EditedByModerator;

            await competitiveEventDraftRepository.Update(competitiveEventDraft);
            var draftWithDetails = await GetByIdWithProviderDetails(competitiveEventDraft.Id);

            logger.LogInformation("Competitive event was updated by moderator.");

            return Result<CompetitiveEventDraftResponseDto>.Success(draftWithDetails.ToResponseDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating CompetitiveEventDraft with ID {DraftId}.", draftId);
            return Result<CompetitiveEventDraftResponseDto>.Failed(new OperationError
            {
                Code = "500",
                Description = "An error occurred while updating CompetitiveEventDraft."
            });
        }
    }

    private static void ValidateCompetitiveEventDraftTitleFilter(CompetitiveEventDraftFilterTitle filter)
        => ModelValidationHelper.ValidateCompetitiveEventDraftTitleFilter(filter);

    private static Expression<Func<CompetitiveEventDraft, bool>> BuildPredicate(CompetitiveEventDraftFilterTitle filter, Guid providerId)
    {
        var predicate = PredicateBuilder.True<CompetitiveEventDraft>();
        predicate = predicate.And(x => x.ProviderId == providerId);

        if (filter.ExcludedId.HasValue)
        {
            predicate = predicate.And(x => x.Id != filter.ExcludedId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            predicate = predicate.And(x => x.CompetitiveEventDraftContent.Title.Contains(filter.SearchText, StringComparison.InvariantCultureIgnoreCase));
        }

        return predicate;
    }

    private async Task<CompetitiveEventDraftResponseDto> MapCompetitiveEventDraftWithDetails(CompetitiveEventDraft draft)
    {
        var competitiveEventDraftResponseDto = draft.ToResponseDto();

        var catottgIds = competitiveEventDraftResponseDto.CompetitiveEventDetails.Contacts
            .Where(c => c?.Address != null)
            .Select(c => c.Address.CATOTTGId)
            .Distinct()
            .ToList();

        var catottgs = await codeficatorRepository
            .Get(whereExpression: c => catottgIds.Contains(c.Id))
            .ToListAsync();

        competitiveEventDraftResponseDto.CompetitiveEventDetails.Contacts
            .Where(c => c?.Address != null)
            .Select(c => c.Address)
            .ToList()
            .ForEach(address =>
                address.CodeficatorAddress = catottgs
                    .FirstOrDefault(c => c.Id == address.CATOTTGId)
                    ?.ToAllAddressPartsDto()
            );

        competitiveEventDraftResponseDto.CompetitiveEventDetails.DirectionSubDirectionIds = (await subDirectionRepository
            .GetByFilter(whereExpression: sd => competitiveEventDraftResponseDto.CompetitiveEventDetails.SubDirectionIds.Contains(sd.Id) && !sd.IsDeleted)
            .ConfigureAwait(false))
            .Select(
                    s => new DirectionSubDirectionIdsDto
                    {
                        DirectionId = s.DirectionId,
                        SubDirectionId = s.Id
                    })
            .ToList();

        return competitiveEventDraftResponseDto;
    }

    private async Task<CompetitiveEventDraft> CreateCompetitiveEventDraft(CompetitiveEventV2Dto competitiveEventV2Dto)
    {
        var competitiveEventDraft = competitiveEventV2Dto.ToDraft();

        competitiveEventDraft.DraftStatus = CompetitiveEventDraftStatus.Draft;

        var createdDraft = await competitiveEventDraftRepository
            .Create(competitiveEventDraft)
            .ConfigureAwait(false);

        return createdDraft;
    }

    private async Task<UploadCompetitiveEventDraftImagesResult> UploadImages(CompetitiveEventDraft createdDraft, CompetitiveEventV2Dto dto)
    {
        var competitiveEventImagesUploadingTask = Task.FromResult<MultipleImageUploadingResult>(null);

        var competitiveEventUploadingCoverImageTask = Task.FromResult<Result<string>>(null);

        if (dto.ImageFiles?.Count > 0)
        {
            createdDraft.Images = new List<Image<CompetitiveEventDraft>>();
            competitiveEventImagesUploadingTask = competitiveEventDraftImagesService.AddManyImagesAsync(
                createdDraft,
                dto.ImageFiles);
        }

        if (dto.CoverImage != null)
        {
            competitiveEventUploadingCoverImageTask = competitiveEventDraftImagesService.AddCoverImageAsync(
                createdDraft,
                dto.CoverImage);
        }

        try
        {
            await Task.WhenAll(competitiveEventImagesUploadingTask, competitiveEventUploadingCoverImageTask);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while uploading images for draft with ID {DraftId}.", createdDraft.Id);
            throw new InvalidOperationException($"Failed to upload one or more images for draft ID {createdDraft.Id}. See inner exception for details.", ex);
        }

        return new UploadCompetitiveEventDraftImagesResult()
        {
            UploadingCoverImageResult = GetImagesUploadTaskResult(competitiveEventUploadingCoverImageTask, createdDraft.Id)?.OperationResult,
            UploadingImagesResults = GetImagesUploadTaskResult(competitiveEventImagesUploadingTask, createdDraft.Id)
        };
    }

    private T GetImagesUploadTaskResult<T>(Task<T> task, Guid draftId) where T : class
    {
        if (task == null)
        {
            return null;
        }

        if (!task.IsCompletedSuccessfully)
        {
            logger.LogError(
                     task.Exception,
                     "Images upload task for competitive event draft with ID {DraftId} failed due to an exception.",
                     draftId);
            return null;
        }

        return task.Result;
    }

    private async Task<CompetitiveEventDraft> GetDraftById(Guid id)
    {
        var draft = await competitiveEventDraftRepository.GetById(id).ConfigureAwait(false);

        if (draft == null)
        {
            return null;
        }

        return draft;
    }

    private async Task<CompetitiveEventDraft> GetByIdWithProviderDetails(Guid id)
    {
        var draft = await competitiveEventDraftRepository.GetByIdWithDetails(
            id,
            includeExpression: p => p
            .Include(p => p.Provider)
            .ThenInclude(p => p.Positions)
            .ThenInclude(o => o.Officials)
            .ThenInclude(i => i.Individual))
            .ConfigureAwait(false);

        if (draft == null)
        {
            return null;
        }

        return draft;
    }

    private async Task<Result<(CompetitiveEventDraft competitiveEventDraft, ImageChangingResult coverImageResult,
           MultipleImageChangingResult imagesResult)>> UpdateDraftWithImagesAsync(CompetitiveEventDraftUpdateDto competitiveEventDraftUpdateDto)
    {
        var competitiveEventDraft = await GetDraftById(competitiveEventDraftUpdateDto.Id).ConfigureAwait(false);

        if (competitiveEventDraft == null)
        {
            logger.LogWarning("Competitive event draft with ID = {DraftId} doesn't exist in DB, so it cannot be updated.", competitiveEventDraftUpdateDto.Id);

            return Result<(CompetitiveEventDraft competitiveEventDraft, ImageChangingResult coverImageResult, MultipleImageChangingResult imagesResult)>
                .Failed(
                new OperationError
                {
                    Code = "404",
                    Description = $"Competitive event draft with ID = {competitiveEventDraftUpdateDto.Id} doesn't exist in DB, so it cannot be updated."
                });
        }

        await currentUserService.UserHasRights(
            new ProviderRights(competitiveEventDraft.ProviderId),
            new EmployeeRights(competitiveEventDraft.ProviderId),
            new ProviderRights(competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.OrganizerOfTheEventId),
            new EmployeeRights(competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.OrganizerOfTheEventId))
            .ConfigureAwait(false);

        var existingCompetitiveEvent = await competitiveEventService.GetById(competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.Id)
         .ConfigureAwait(false);

        if (existingCompetitiveEvent == null)
        {
            competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.Id = Guid.Empty;
        }
        else
        {
            await currentUserService.UserHasRights(
                new ProviderRights(existingCompetitiveEvent.OrganizerOfTheEventId),
                new EmployeeRights(existingCompetitiveEvent.OrganizerOfTheEventId))
                .ConfigureAwait(false);
        }

        if (competitiveEventDraft.DraftStatus == CompetitiveEventDraftStatus.PendingModeration)
        {
            logger.LogWarning("Competitive event draft with ID {DraftId} can't be updated.", competitiveEventDraftUpdateDto.Id);

            return Result<(CompetitiveEventDraft competitiveEventDraft, ImageChangingResult coverImageResult,
                MultipleImageChangingResult imagesResult)>.Failed(new OperationError
                {
                    Code = "400",
                    Description = "Competitive event draft can't be updated when it is in PendingModeration status."
                });
        }

        competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.SetToDraft(competitiveEventDraft);
        competitiveEventDraft.DraftStatus = CompetitiveEventDraftStatus.Draft;

        var coverImageResult = await competitiveEventDraftImagesService
            .ChangeCoverImageAsync(competitiveEventDraft,
            competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.CoverImageId,
            competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.CoverImage)
            .ConfigureAwait(false);

        var imagesResult = await competitiveEventDraftImagesService
            .ChangeImagesAsync(competitiveEventDraft,
            competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.ImageIds,
            competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.ImageFiles)
            .ConfigureAwait(false);

        await competitiveEventDraftRepository.Update(competitiveEventDraft);
        logger.LogDebug("Competitive event draft with ID {DraftId} updated successfully.", competitiveEventDraftUpdateDto.Id);

        return Result<(CompetitiveEventDraft competitiveEventDraft, ImageChangingResult coverImageResult,
           MultipleImageChangingResult imagesResult)>.Success((competitiveEventDraft, coverImageResult, imagesResult));
    }

    /// <summary>
    /// Validates whether the specified moderator or tech admin is allowed to access and modify the given draft.
    /// Checks the user's permissions, the existence of the draft, and whether it is in an editable status.
    /// </summary>
    /// <param name="draftId">The ID of the competitive event draft to validate.</param>
    /// <returns>
    /// A <see cref="Result{CompetitiveEventDraft}"/> containing the draft if validation is successful,
    /// or a failed result with appropriate error code and description.
    /// </returns>
    private async Task<Result<CompetitiveEventDraft>> ValidateDraftForModerator(Guid draftId)
    {
        await currentUserService.UserHasRights(new ModeratorRights(), new TechAdminRights()).ConfigureAwait(false);

        var competitiveEventDraft = await GetDraftById(draftId);

        if (competitiveEventDraft == null)
        {
            logger.LogWarning("CompetitiveEventDraft with Id = {Id} not found.", draftId);
            return Result<CompetitiveEventDraft>.Failed(new OperationError
            {
                Code = "404",
                Description = "Competitive event draft not found."
            });
        }

        if (competitiveEventDraft.DraftStatus != CompetitiveEventDraftStatus.PendingModeration &&
            competitiveEventDraft.DraftStatus != CompetitiveEventDraftStatus.EditedByModerator)
        {
            logger.LogWarning("CompetitiveEventDraft with Id = {Id} is not editable in current status: {Status}.",
                draftId, competitiveEventDraft.DraftStatus);

            return Result<CompetitiveEventDraft>.Failed(new OperationError
            {
                Code = "409",
                Description = "Competitive event draft is not editable in its current status."
            });
        }

        return Result<CompetitiveEventDraft>.Success(competitiveEventDraft);
    }

    private async Task<(Guid InstitutionId, long CatottgId)> GetAdminInstitutionAndCatottgIds()
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var userId = currentUserService.UserId;
            var ministryAdmin = await ministryAdminService
                .GetByUserId(userId)
                .ConfigureAwait(false);

            return (ministryAdmin.InstitutionId, 0);
        }
        else if (currentUserService.IsRegionAdmin())
        {
            var userId = currentUserService.UserId;
            var regionAdmin = await regionAdminService
                .GetByUserId(userId)
                .ConfigureAwait(false);

            if (regionAdmin == null)
            {
                logger.LogError("Region admin with the specified ID: {UserId} not found", userId);
                throw new InvalidOperationException($"Region admin with the specified ID: {userId} not found");
            }

            return (regionAdmin.InstitutionId, regionAdmin.CATOTTGId);
        }

        return (Guid.Empty, 0);
    }

    private Expression<Func<CompetitiveEventDraft, bool>> PredicateBuildForAdmins(
        CompetitiveEventDraftFilterAdministration filter,
        Guid adminInstitutionId,
        List<long> allowedSettlementIdsForAdmin,
        List<long> subSettlementFilterIds)
    {
        var predicate = PredicateBuilder.True<CompetitiveEventDraft>();

        predicate = predicate.And(x => filter.CompetitiveEventDraftStatuses.Contains(x.DraftStatus));

        if (adminInstitutionId != Guid.Empty)
        {
            predicate = predicate.And(x => x.Provider.InstitutionId == adminInstitutionId);
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(x => x.Provider.InstitutionId == filter.InstitutionId);
        }

        if (allowedSettlementIdsForAdmin != null && allowedSettlementIdsForAdmin.Count != 0)
        {
            predicate = predicate.And(c => allowedSettlementIdsForAdmin.Contains(c.CATOTTGId));
        }

        if (subSettlementFilterIds != null && subSettlementFilterIds.Count != 0)
        {
            predicate = predicate.And(c => subSettlementFilterIds.Contains(c.CATOTTGId));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var searchTerms = searchStringService.SplitSearchString(filter.SearchString);

            if (searchTerms.Length != 0)
            {
                var tempPredicate = PredicateBuilder.False<CompetitiveEventDraft>();
                tempPredicate = searchTerms.Aggregate(tempPredicate,
                    (current, word) => current.Or(x =>
                        x.CompetitiveEventDraftContent.Title.Contains(word,
                            StringComparison.InvariantCultureIgnoreCase) ||
                        x.CompetitiveEventDraftContent.ShortTitle.Contains(word,
                            StringComparison.InvariantCultureIgnoreCase) ||
                        x.Provider.FullTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Provider.FullTitleEn.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Provider.Edrpou.Contains(word, StringComparison.InvariantCultureIgnoreCase)));

                predicate = predicate.And(tempPredicate);
            }
        }

        return predicate;
    }

    private async Task<List<CompetitiveEventDraftResponseDto>> MapCompetitiveEventDraftsCollectionWithDetails(List<CompetitiveEventDraft> competitiveEventDrafts)
    {
        if (!competitiveEventDrafts.Any())
            return new List<CompetitiveEventDraftResponseDto>();

        var catottgIds = competitiveEventDrafts
            .Where(ced => ced.CompetitiveEventDraftContent.Contacts != null)
            .SelectMany(ced => ced.CompetitiveEventDraftContent.Contacts)
            .Where(c => c?.Address != null)
            .Select(c => c.Address.CATOTTGId)
            .Distinct()
            .ToList();

        var catottgs = await codeficatorRepository.Get(
                whereExpression: c => catottgIds.Contains(c.Id))
            .ToListAsync();

        return competitiveEventDrafts.Select(draft =>
        {
            var responseDto = draft.ToResponseDto();

            responseDto.CompetitiveEventDetails.Contacts
                .Where(c => c?.Address != null)
                .Select(c => c.Address)
                .ToList()
                .ForEach(address =>
                    address.CodeficatorAddress = catottgs
                        .FirstOrDefault(c => c.Id == address.CATOTTGId)
                        ?.ToAllAddressPartsDto()
                );

            return responseDto;

        }).ToList();
    }

    /// <summary>
    /// Sets conditional fields in a DTO to null or default values ​​according to their respective flags.
    /// </summary>
    /// <param name="dto">CompetitiveEvent dto.</param>
    private static void NormalizeConditionalFields(CompetitiveEventV2Dto dto)
    {
        dto.CompetitiveSelectionDescription = dto.CompetitiveSelection ?? false ? dto.CompetitiveSelectionDescription : null;
        dto.Benefits = dto.AreThereBenefits ?? false ? dto.Benefits : null;
        dto.Price = dto.IsPaid ? dto.Price : 0;
    }
}
