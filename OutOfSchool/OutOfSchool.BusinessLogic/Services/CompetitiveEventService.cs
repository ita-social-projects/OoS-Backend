using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for CompetitiveEvent entity.
/// </summary>
public class CompetitiveEventService(
    ICompetitiveEventRepository competitiveEventRepository,
    IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository,
    ILogger<CompetitiveEventService> logger,
    IStringLocalizer<SharedResource> localizer,
    ICurrentUserService currentUserService,
    IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>> contactsService,
    IImageDependentEntityImagesInteractionService<CompetitiveEvent> competitiveImagesService
) : ICompetitiveEventService, ICompetitiveEventServiceV2
{
    private readonly ICompetitiveEventRepository competitiveEventRepository = competitiveEventRepository ?? throw new ArgumentNullException(nameof(competitiveEventRepository));
    private readonly IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository = descriptionItemRepository ?? throw new ArgumentException(nameof(descriptionItemRepository));
    private readonly ILogger<CompetitiveEventService> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IStringLocalizer<SharedResource> localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

    /// <summary>
    /// Create a delegate to include other entities in CompetitiveEvent entity
    /// </summary>
    private readonly Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>> includeFunc =
    query => query
        .Include(e => e.InstitutionHierarchy)
        .Include(e => e.CompetitiveEventDescriptionItems)
        .Include(e => e.Coverage)
        .IncludeContactsWithCodeficatorHierarchy();

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto?> GetById(Guid id)
    {
        logger.LogDebug("Getting CompetitiveEvent by Id started. Looking Id = {id}.", id);

        var competitiveEvent = (await competitiveEventRepository
            .GetByIdWithDetails(id, String.Empty, includeFunc)
            .ConfigureAwait(false));

        var logMessage = competitiveEvent is null
            ? "CompetitiveEvent with Id = {id} doesn't exist in the system."
            : "Successfully got a CompetitiveEvent with Id = {id}.";

        logger.LogDebug(logMessage, id);

        return competitiveEvent.ToDto();
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <see cref="CompetitiveEventCreateUpdateDto"/> is null.</exception>
    public async Task<CompetitiveEventDto> Create(CompetitiveEventCreateUpdateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogDebug("CompetitiveEvent creating was started.");

        var competitiveEvent = dto.ToModel();

        if (!dto.CompetitiveEventDescriptionItems.IsNullOrEmpty())
        {
            competitiveEvent.CompetitiveEventDescriptionItems = dto.CompetitiveEventDescriptionItems.ToModel();
        }

        contactsService.PrepareNewContacts(competitiveEvent, dto);
       
        var newCompetitiveEvent = await competitiveEventRepository.RunInTransaction(async () =>
        await competitiveEventRepository.Create(competitiveEvent).ConfigureAwait(false)).ConfigureAwait(false);

        return newCompetitiveEvent.ToDto();
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> Update(CompetitiveEventCreateUpdateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogDebug("Updating CompetitiveEvent with Id = {dtoId} started.", dto.Id);

        var competitiveEvent = await competitiveEventRepository.GetByIdWithDetails(
                dto.Id, String.Empty, includeFunc).ConfigureAwait(false);

        if (competitiveEvent is null)
        {
            var message = $"Updating failed. CompetitiveEvent with Id = {dto.Id} doesn't exist in the system.";
            logger.LogError(message);
            throw new DbUpdateConcurrencyException(message);
        }

        await ChangeCompetitiveEventDescriptionItems(competitiveEvent, dto.CompetitiveEventDescriptionItems
            ?? new List<CompetitiveEventDescriptionItemDto>()).ConfigureAwait(false);

        contactsService.PrepareUpdatedContacts(competitiveEvent, dto);

        dto.SetToModel(competitiveEvent);

        var updatedCompetitiveEvent = await competitiveEventRepository.RunInTransaction(async () =>
        {
            return await competitiveEventRepository.Update(competitiveEvent).ConfigureAwait(false);
        }).ConfigureAwait(false);

        logger.LogDebug("CompetitiveEvent with Id = {competitiveEventId} updated successfully.", updatedCompetitiveEvent.Id);

        return updatedCompetitiveEvent.ToDto();
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogDebug("Deleting CompetitiveEvent with Id = {id} started.", id);

        var entity = await competitiveEventRepository.GetById(id);

        try
        {
            await competitiveEventRepository.Delete(entity).ConfigureAwait(false);

            logger.LogDebug("CompetitiveEvent with Id = {id} succesfully deleted.", id);
        }
        catch (Exception ex) // DbUpdateConcurrencyException
        {
            logger.LogError(ex, "Deleting failed. CompetitiveEvent with Id = {Id} doesn't exist in the system", id);
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"CompetitiveEvent with Id = {id} doesn't exist in the system"]);
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResult<CompetitiveEventViewCardDto>> GetByProviderId(Guid id, ExcludeIdFilter filter)
    {
        if (id == Guid.Empty)
        {
            logger.LogWarning("ProviderId is empty. Unable to retrieve competitive events.");
            throw new ArgumentException("ProviderId cannot be empty.", nameof(id));
        }
      
        await currentUserService.UserHasRights(new ProviderRights(id));

        logger.LogDebug("Getting Competitive events by organization started. Looking ProviderId = {Id}.", id);

        filter ??= new ExcludeIdFilter();
        ValidateExcludedIdFilter(filter);

        var predicate = PredicateBuilder.True<CompetitiveEvent>();
        predicate = predicate.And(x => x.OrganizerOfTheEventId == id);

        if (filter.ExcludedId is not null && filter.ExcludedId != Guid.Empty)
        {
            predicate = predicate.And(x => x.Id != filter.ExcludedId);
        }

        var competitiveEventCardsCount = await competitiveEventRepository.Count(
            whereExpression: predicate).ConfigureAwait(false);

        var competitiveEvents = await competitiveEventRepository.Get(
            skip: filter.From,
            take: filter.Size,
            whereExpression: predicate)
            .ToListAsync()
            .ConfigureAwait(false);

        var competitiveEventViewCards = competitiveEvents.ToViewCardDto();

        logger.LogDebug("From CompetitiveEvents table were successfully received {Count} records.", competitiveEventViewCards.Count);

        var result = new SearchResult<CompetitiveEventViewCardDto>()
        {
            TotalAmount = competitiveEventCardsCount,
            Entities = competitiveEventViewCards,
        };

        return result;
    }

    public async Task<IEnumerable<CompetitiveEvent>> GetByIds(IEnumerable<Guid> ids)
    {
        return await competitiveEventRepository.GetByIds(ids).ConfigureAwait(false);
    }

    private static void ValidateExcludedIdFilter(ExcludeIdFilter filter) 
        => ModelValidationHelper.ValidateExcludedIdFilter(filter);

    private async Task ChangeCompetitiveEventDescriptionItems(CompetitiveEvent currentCompetitiveEvent, List<CompetitiveEventDescriptionItemDto> descriptionItemsDtoList)
    {
        try
        {
            await RemoveDescriptionItemsAsync(currentCompetitiveEvent, descriptionItemsDtoList).ConfigureAwait(false);
            await UpsertDescriptionItemsAsync(currentCompetitiveEvent, descriptionItemsDtoList).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the description items.");
            throw;
        }
    }

    private async Task RemoveDescriptionItemsAsync(CompetitiveEvent currentCompetitiveEvent, List<CompetitiveEventDescriptionItemDto> descriptionItemsDtoList)
    {
        var descItemsToDelete = currentCompetitiveEvent.CompetitiveEventDescriptionItems
            .Where(descItem => !descriptionItemsDtoList.Exists(item => item.Id == descItem.Id))
            .ToList();

        foreach (var descItem in descItemsToDelete)
        {
            if (descItem == null) continue;

            try
            {
                await descriptionItemRepository.Delete(descItem).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete description item with ID: {DescItemId}", descItem.Id);
                throw;
            }
        }
    }

    private async Task UpsertDescriptionItemsAsync(CompetitiveEvent currentCompetitiveEvent, List<CompetitiveEventDescriptionItemDto> descriptionItemsDtoList)
    {
        foreach (var descItemDto in descriptionItemsDtoList)
        {
            try
            {
                var foundDescItem = currentCompetitiveEvent.CompetitiveEventDescriptionItems
                    .FirstOrDefault(d => d.Id == descItemDto.Id);

                if (foundDescItem != null)
                {
                    await descriptionItemRepository.Update(descItemDto.SetToModel(foundDescItem)).ConfigureAwait(false);
                }
                else
                {
                    var newDescItem = descItemDto.ToModel();
                    newDescItem.CompetitiveEventId = currentCompetitiveEvent.Id;
                    await descriptionItemRepository.Create(newDescItem).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process description item with ID: {DescItemDtoId}", descItemDto.Id);
                throw;
            }
        }
    }

    #region V2 features with images

    /// <summary>
    /// Checks whether a competitive event with the specified Id exists in the database
    /// </summary>
    /// <param name="id">The identifier of the competitive event</param>
    /// <returns>True if the event exists, otherwise - false</returns>
    private Task<bool> Exists(Guid id)
    {
        logger.LogInformation($"Checking if Competitive event exists by Id started. Looking Id = {id}.");

        return competitiveEventRepository.Any(x => x.Id == id);
    }

    /// <summary>
    /// Validates the incoming DTO and prepares a <see cref="CompetitiveEvent"/> entity for creation
    /// Checks for the existence of the parent event, maps the DTO to the entity, and prepares contacts
    /// </summary>
    /// <param name="dto">The DTO used to create the competitive event</param>
    /// <returns>A prepared <see cref="CompetitiveEvent"/> entity ready to be saved</returns>
    /// <exception cref="ArgumentNullException">Thrown if the DTO is null</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified parent event does not exist</exception>
    private async Task<CompetitiveEvent> CheckDtoAndPrepareCreatedCompetitiveEvent(CompetitiveEventCreateUpdateDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        //if (dto.ParentId.HasValue && !await Exists((Guid)dto.ParentId).ConfigureAwait(false))
        //{
        //    var errorMessage = $"The parent competitive event (ID = {dto.ParentId}) does not exist.";
        //    throw new InvalidOperationException(errorMessage);
        //}

        var createdEvent = dto is CompetitiveEventV2CreateRequestDto v2Dto
            ? v2Dto.ToModel()
            : dto.ToModel();

        //configure additional default value by the logic 

        contactsService.PrepareNewContacts(createdEvent, dto);

        return createdEvent;
    }

    /// <summary>
    /// Persists changes to the competitive event in the database
    /// Logs any exceptions that occurs during the update process
    /// </summary>
    /// <exception cref="DbUpdateException">Thrown if saving changes fails</exception>
    private async Task UpdateCompetitiveEvent()
    {
        try
        {
            await competitiveEventRepository.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, $"Updating a competitive event failed. Exception: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates a new version 2 competitive event, including handling of image and cover uploads
    /// All operations are executed within a transaction
    /// </summary>
    /// <param name="dto">The DTO containing data for the new competitive event with images</param>
    /// <returns>A result DTO containing the created competitive event and image upload results</returns>
    /// <exception cref="ArgumentNullException">Thrown if the DTO is null.</exception>
    public async Task<CompetitiveEventResultDto> CreateV2(CompetitiveEventV2CreateRequestDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        logger.LogDebug("CompetitiveEvent creating was started.");

        var createdEvent = await CheckDtoAndPrepareCreatedCompetitiveEvent(dto);

        async Task<(CompetitiveEvent newEvent, MultipleImageUploadingResult imagesUploadResult, Result<string> coverImageUploadResult)> CreateCompetitiveEventAndDependencies()
        {
            var competitiveEvent = await competitiveEventRepository.Create(createdEvent).ConfigureAwait(false);

            MultipleImageUploadingResult imagesUploadingResult = null;

            if (dto.ImageFiles?.Count > 0)
            {
                competitiveEvent.Images = new List<Image<CompetitiveEvent>>();
                imagesUploadingResult = await competitiveImagesService.AddManyImagesAsync(competitiveEvent, dto.ImageFiles)
                    .ConfigureAwait(false);
            }

            Result<string> uploadingCoverImageResult = null;

            if (dto.CoverImage != null)
            {
                uploadingCoverImageResult = await competitiveImagesService.AddCoverImageAsync(competitiveEvent, dto.CoverImage)
                    .ConfigureAwait(false);
            }

            await UpdateCompetitiveEvent().ConfigureAwait(false);

            return (competitiveEvent, imagesUploadingResult, uploadingCoverImageResult);
        }

        var (lastCompetitiveEvent, imagesUploadResult, coverImageUploadResult) = await competitiveEventRepository
           .RunInTransaction(CreateCompetitiveEventAndDependencies).ConfigureAwait(false);

        logger.LogInformation($"Competitive event with Id = {lastCompetitiveEvent.Id} created successfully.");

        return new CompetitiveEventResultDto
        {
            CompetitiveEventV2 = lastCompetitiveEvent.ToV2Dto(),
            UploadingCoverImageResult = coverImageUploadResult?.OperationResult,
            UploadingImagesResults = imagesUploadResult?.MultipleKeyValueOperationResult,
        };
    }

    /// <summary>
    /// Updates an existing version 2 competitive event, including description changes, image updates, and cover image replacement
    /// All operations are executed within a transaction
    /// </summary>
    /// <param name="dto">The DTO containing updated data for the competitive event</param>
    /// <returns>A result DTO containing the updated competitive event and results of the image updates</returns>
    /// <exception cref="ArgumentNullException">Thrown if the DTO is null.</exception>
    /// <exception cref="DbUpdateConcurrencyException">Thrown if the competitive event with the given Id does not exist</exception>
    public async Task<CompetitiveEventResultDto> UpdateV2(CompetitiveEventV2CreateRequestDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation($"Updating {nameof(CompetitiveEvent)} with Id = {dto.Id} started.");

        async Task<(CompetitiveEvent updatedCompetitiveEvent, MultipleImageChangingResult multipleImageChangingResult,
           ImageChangingResult changingCoverImageResult)> UpdateCompetitiveEventWithDependencies()
        {
            var currentCompetitiveEvent = await competitiveEventRepository.GetByIdWithDetails(
               dto.Id, String.Empty, includeFunc).ConfigureAwait(false);

            if (currentCompetitiveEvent is null)
            {
                var message = $"Updating failed. CompetitiveEvent with Id = {dto.Id} doesn't exist in the system.";
                logger.LogError(message);
                throw new DbUpdateConcurrencyException(message);
            }

            await ChangeCompetitiveEventDescriptionItems(currentCompetitiveEvent, dto.CompetitiveEventDescriptionItems
                ?? new List<CompetitiveEventDescriptionItemDto>()).ConfigureAwait(false);

            dto.ImageIds ??= new List<string>();
            var multipleImageChangingResult = await competitiveImagesService
                .ChangeImagesAsync(currentCompetitiveEvent, dto.ImageIds, dto.ImageFiles)
                .ConfigureAwait(false);

            contactsService.PrepareUpdatedContacts(currentCompetitiveEvent, dto);

            dto.SetToModel(currentCompetitiveEvent);

            var changingCoverImageResult = await competitiveImagesService
                .ChangeCoverImageAsync(currentCompetitiveEvent, dto.CoverImageId, dto.CoverImage).ConfigureAwait(false);

            await UpdateCompetitiveEvent().ConfigureAwait(false);

            return (currentCompetitiveEvent, multipleImageChangingResult, changingCoverImageResult);
        }

        var (updatedCompetitiveEvent, multipleImageChangeResult, changeCoverImageResult) = await competitiveEventRepository
            .RunInTransaction(UpdateCompetitiveEventWithDependencies).ConfigureAwait(false);

        return new CompetitiveEventResultDto
        {
            CompetitiveEventV2 = updatedCompetitiveEvent.ToV2Dto(),
            UploadingCoverImageResult = changeCoverImageResult?.UploadingResult?.OperationResult,
            UploadingImagesResults = multipleImageChangeResult?.UploadedMultipleResult?.MultipleKeyValueOperationResult,
        };
    }

    /// <summary>
    /// Deletes a competitive event and removes any associated images and cover image if present
    /// The operation is executed within a transaction
    /// </summary>
    /// <param name="id">The Id of the competitive event to be deleted</param>
    public async Task DeleteV2(Guid id)
    {
        logger.LogDebug("Deleting CompetitiveEvent with Id = {id} started.", id);

        async Task<Workshop> TransactionOperation()
        {
            var entity = await competitiveEventRepository.GetById(id).ConfigureAwait(false);

            if (entity.Images.Count > 0)
            {
                await competitiveImagesService
                    .RemoveManyImagesAsync(entity, entity.Images.Select(x => x.ExternalStorageId).ToList())
                    .ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(entity.CoverImageId))
            {
                await competitiveImagesService.RemoveCoverImageAsync(entity).ConfigureAwait(false);
            }

            await competitiveEventRepository.Delete(entity).ConfigureAwait(false);

            return null;
        }

        await competitiveEventRepository.RunInTransaction(TransactionOperation).ConfigureAwait(false);

        logger.LogInformation($"{nameof(CompetitiveEvent)} with Id = {id} successfully deleted.");     
    }

    #endregion
}

