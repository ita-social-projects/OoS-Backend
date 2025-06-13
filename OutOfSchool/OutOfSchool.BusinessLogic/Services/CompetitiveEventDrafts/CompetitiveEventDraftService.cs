using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
public class CompetitiveEventDraftService(ILogger<CompetitiveEventDraftService> logger,
    ICurrentUserService currentUserService,
    ICompetitiveEventService competitiveEventService,
    IEntityRepository<Guid, CompetitiveEventDraft> competitiveEventDraftRepository,
    IImageDependentEntityImagesInteractionService<CompetitiveEventDraft> competitiveEventDraftImagesService,
    ICodeficatorRepository codeficatorRepository) : ICompetitiveEventDraftService
{

    // <inheritdoc/>
    public async Task<CompetitiveEventDraftResultDto> Create(CompetitiveEventV2Dto competitiveEventV2Dto)
    {
        if (competitiveEventV2Dto == null)
        {
            return null;
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
                await currentUserService.UserHasRights(new ProviderRights(existingCompetitiveEvent.OrganizerOfTheEventId),
                    new EmployeeRights(existingCompetitiveEvent.OrganizerOfTheEventId)).ConfigureAwait(false);
            }
        }

        var createdCompetitiveEventDraft = await competitiveEventDraftRepository
            .RunInTransaction(() => CreateCompetitiveEventDraft(competitiveEventV2Dto))
            .ConfigureAwait(false);

        var uploadImagesResult = await UploadImages(createdCompetitiveEventDraft, competitiveEventV2Dto)
            .ConfigureAwait(false);

        await competitiveEventDraftRepository.SaveChangesAsync().ConfigureAwait(false);

        logger.LogDebug("Competitive event draft created successfully.");

        return new CompetitiveEventDraftResultDto
        {
            CompetitiveEventDraft = createdCompetitiveEventDraft.ToResponseDto(),
            UploadingCoverImagesCompetitiveEventResult = uploadImagesResult.UploadingCoverImageResult,
            UploadingImagesResults = uploadImagesResult.UploadingImagesResults?.MultipleKeyValueOperationResult
        };
    }

    // <inheritdoc/>
    public async Task<CompetitiveEventDraftResultDto> Update(CompetitiveEventDraftUpdateDto competitiveEventDraftUpdateDto)
    {
        if (competitiveEventDraftUpdateDto == null || competitiveEventDraftUpdateDto.CompetitiveEventV2Dto == null)
        {
            return null;
        }

        logger.LogDebug("Updating competitive event draft with ID: {DraftId}", competitiveEventDraftUpdateDto.Id);

        var (updatedDraft, coverImageResult, imagesResult) = await competitiveEventDraftRepository
            .RunInTransaction(() => UpdateDraftWithImagesAsync(competitiveEventDraftUpdateDto))
            .ConfigureAwait(false);

        return new CompetitiveEventDraftResultDto
        {
            CompetitiveEventDraft = updatedDraft.ToResponseDto(),
            UploadingCoverImagesCompetitiveEventResult = coverImageResult?.UploadingResult?.OperationResult,
            UploadingImagesResults = imagesResult?.UploadedMultipleResult?.MultipleKeyValueOperationResult
        };
    }

    // <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogDebug("Deleting competitive event draft with ID: {DraftId}", id);

        var competitiveEventDraft = await GetDraftById(id).ConfigureAwait(false);

        await currentUserService.UserHasRights(
            new ProviderRights(competitiveEventDraft.ProviderId),
            new EmployeeRights(competitiveEventDraft.ProviderId))
            .ConfigureAwait(false);

        if (competitiveEventDraft.DraftStatus == CompetitiveEventDraftStatus.PendingModeration)
        {
            logger.LogWarning("Competitive event draft with ID {DraftId} is in PendingModeration status and cannot be deleted.", id);
            throw new InvalidOperationException("Competitive event draft can only be deleted when it is not in PendingModeration status.");
        }

        await competitiveEventDraftRepository.Delete(competitiveEventDraft).ConfigureAwait(false);
        logger.LogDebug("Competitive event draft with ID {DraftId} deleted successfully.", id);
    }

    // <inheritdoc/>
    public async Task SendForModeration(Guid id)
    {
        logger.LogDebug("Sending competitive event draft with ID {DraftId} for moderation.", id);

        var competitiveEventDraft = await GetDraftById(id).ConfigureAwait(false);

        await currentUserService.UserHasRights(
            new ProviderRights(competitiveEventDraft.ProviderId),
            new EmployeeRights(competitiveEventDraft.ProviderId))
            .ConfigureAwait(false);

        if (competitiveEventDraft.DraftStatus == CompetitiveEventDraftStatus.PendingModeration)
        {
            logger.LogWarning("Competitive event draft with ID {DraftId} is not in Draft status and cannot be sent for moderation.", id);
            throw new InvalidOperationException("Competitive event draft can only be sent for moderation when it is in Draft status.");
        }

        competitiveEventDraft.DraftStatus = CompetitiveEventDraftStatus.PendingModeration;
        await competitiveEventDraftRepository.Update(competitiveEventDraft).ConfigureAwait(false);

        logger.LogDebug("Competitive event draft with ID {DraftId} sent for moderation successfully.", id);
    }

    // <inheritdoc/>
    public async Task<SearchResult<CompetitiveEventDraftViewCardDto>> GetByProviderId(Guid id, ExcludeIdFilter filter)
    {
        logger.LogDebug("Retrieving competitive event drafts for provider with ID: {ProviderId}", id);

        await currentUserService.UserHasRights(new ProviderRights(id), new EmployeeRights(id)).ConfigureAwait(false);

        filter ??= new ExcludeIdFilter();
        ModelValidationHelper.ValidateExcludedIdFilter(filter);

        var competitiveEventCardsCount = await competitiveEventDraftRepository
            .Count(whereExpression: x =>
            filter.ExcludedId == null
            ? (x.ProviderId == id)
            : (x.ProviderId == id && x.Id != filter.ExcludedId)).ConfigureAwait(false);

        var competitiveEventDrafts = await competitiveEventDraftRepository.Get(
            skip: filter.From,
            take: filter.Size,
            whereExpression: x => filter.ExcludedId == null
                ? (x.ProviderId == id)
                : (x.ProviderId == id && x.Id != filter.ExcludedId)).ToListAsync().ConfigureAwait(false);

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

        var draft = await GetDraftById(id).ConfigureAwait(false);

        await currentUserService.UserHasRights(
            new ProviderRights(draft.ProviderId),
            new EmployeeRights(draft.ProviderId),
            new ModeratorRights(),
            new TechAdminRights())
            .ConfigureAwait(false);

        return await MapCompetitiveEventDraftWithDetails(draft).ConfigureAwait(false);
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
                address.CodeficatorAddressDto = catottgs
                    .FirstOrDefault(c => c.Id == address.CATOTTGId)
                    ?.ToAllAddressPartsDto()
            );

        return competitiveEventDraftResponseDto;
    }

    private async Task<CompetitiveEventDraft> CreateCompetitiveEventDraft(CompetitiveEventV2Dto competitiveEventV2Dto)
    {
        var competitiveEventDraft = competitiveEventV2Dto.ToDraft();

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
            logger.LogError(ex,
                "Error occurred while uploading images for draft with ID {DraftId}.",
                createdDraft.Id);
            throw;
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
                     "Images upload task for workshop draft with ID {DraftId} failed due to an exception.",
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

    private async Task<(CompetitiveEventDraft competitiveEventDraft, ImageChangingResult coverImageResult,
           MultipleImageChangingResult imagesResult)> UpdateDraftWithImagesAsync(CompetitiveEventDraftUpdateDto competitiveEventDraftUpdateDto)
    {
        var competitiveEventDraft = await GetDraftById(competitiveEventDraftUpdateDto.Id).ConfigureAwait(false);

        await currentUserService.UserHasRights(
            new ProviderRights(competitiveEventDraft.ProviderId),
            new EmployeeRights(competitiveEventDraft.ProviderId))
            .ConfigureAwait(false);
        await currentUserService.UserHasRights(
            new ProviderRights(competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.OrganizerOfTheEventId),
            new EmployeeRights(competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.OrganizerOfTheEventId))
            .ConfigureAwait(false);

        if (competitiveEventDraftUpdateDto.Id != Guid.Empty)
        {
            var existingCompetitiveEvent = await competitiveEventService.GetById(competitiveEventDraftUpdateDto.Id)
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
        }

        if (competitiveEventDraft.DraftStatus != CompetitiveEventDraftStatus.PendingModeration)
        {
            logger.LogWarning("Competitive event draft with ID {DraftId} is not in Draft status.", competitiveEventDraftUpdateDto.Id);
            throw new InvalidOperationException("Competitive event draft can only be updated when it is in Draft status.");
        }

        competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.SetToDraft(competitiveEventDraft);

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

        return (competitiveEventDraft, coverImageResult, imagesResult);
    }
}
