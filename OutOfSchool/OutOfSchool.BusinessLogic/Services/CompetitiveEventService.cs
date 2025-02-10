using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for CompetitiveEvent entity.
/// </summary>
public class CompetitiveEventService : ICompetitiveEventService
{
    private readonly string includingPropertiesForCompetitiveEventViewCard = String.Empty;

    private readonly IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> competitiveEventRepository;
    private readonly IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository;
    private readonly ILogger<CompetitiveEventService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>> contactsService;

    public CompetitiveEventService(
        IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> competitiveEventRepository,
        IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository,
        ILogger<CompetitiveEventService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>> contactsService)
    {
        this.competitiveEventRepository = competitiveEventRepository ?? throw new ArgumentNullException(nameof(competitiveEventRepository));
        this.descriptionItemRepository = descriptionItemRepository ?? throw new ArgumentException(nameof(descriptionItemRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.currentUserService = currentUserService;
        this.contactsService = contactsService;
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto?> GetById(Guid id)
    {
        logger.LogDebug("Getting CompetitiveEvent by Id started. Looking Id = {id}.", id);

        var competitiveEvent = (await competitiveEventRepository.GetById(id).ConfigureAwait(false));

        var logMessage = competitiveEvent is null
            ? "CompetitiveEvent with Id = {id} doesn't exist in the system."
            : "Successfully got a CompetitiveEvent with Id = {id}.";

        logger.LogDebug(logMessage, id);

        return mapper.Map<CompetitiveEventDto>(competitiveEvent);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <see cref="CompetitiveEventCreateUpdateDto"/> is null.</exception>
    public async Task<CompetitiveEventDto> Create(CompetitiveEventCreateUpdateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogDebug("CompetitiveEvent creating was started.");

        var competitiveEvent = mapper.Map<CompetitiveEvent>(dto);

        if (!dto.CompetitiveEventDescriptionItems.IsNullOrEmpty()) // test please
        {
            competitiveEvent.CompetitiveEventDescriptionItems =
            dto.CompetitiveEventDescriptionItems.Select(mapper.Map<CompetitiveEventDescriptionItem>).ToList();
        }

        contactsService.PrepareNewContacts(competitiveEvent, dto);
       
        var newCompetitiveEvent = await competitiveEventRepository.RunInTransaction(async () =>
        await competitiveEventRepository.Create(competitiveEvent).ConfigureAwait(false)).ConfigureAwait(false);

        return mapper.Map<CompetitiveEventDto>(newCompetitiveEvent);
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> Update(CompetitiveEventCreateUpdateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogDebug("Updating CompetitiveEvent with Id = {dtoId} started.", dto.Id);

        var competitiveEvent = await competitiveEventRepository.GetByIdWithDetails(dto.Id, "CompetitiveEventDescriptionItems").ConfigureAwait(false);

        if (competitiveEvent is null)
        {
            var message = $"Updating failed. CompetitiveEvent with Id = {dto.Id} doesn't exist in the system.";
            logger.LogError(message);
            throw new DbUpdateConcurrencyException(message);
        }

        await ChangeCompetitiveEventDescriptionItems(competitiveEvent, dto.CompetitiveEventDescriptionItems
            ?? new List<CompetitiveEventDescriptionItemDto>()).ConfigureAwait(false);

        contactsService.PrepareUpdatedContacts(competitiveEvent, dto);

        mapper.Map(dto, competitiveEvent);

        var updatedCompetitiveEvent = await competitiveEventRepository.RunInTransaction(async () =>
        {
            return await competitiveEventRepository.Update(competitiveEvent).ConfigureAwait(false);
        }).ConfigureAwait(false);

        logger.LogDebug("CompetitiveEvent with Id = {competitiveEventId} updated successfully.", updatedCompetitiveEvent.Id);

        return mapper.Map<CompetitiveEventDto>(updatedCompetitiveEvent);
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
            includeProperties: includingPropertiesForCompetitiveEventViewCard,
            whereExpression: predicate)
            .ToListAsync()
            .ConfigureAwait(false);

        var competitiveEventViewCards = mapper.Map<List<CompetitiveEventViewCardDto>>(competitiveEvents);

        logger.LogDebug("From CompetitiveEvents table were successfully received {Count} records.", competitiveEventViewCards.Count);

        var result = new SearchResult<CompetitiveEventViewCardDto>()
        {
            TotalAmount = competitiveEventCardsCount,
            Entities = competitiveEventViewCards,
        };

        return result;
    }

    private static void ValidateExcludedIdFilter(ExcludeIdFilter filter) =>
      ModelValidationHelper.ValidateExcludedIdFilter(filter);

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
                    mapper.Map(descItemDto, foundDescItem);
                    await descriptionItemRepository.Update(foundDescItem).ConfigureAwait(false);
                }
                else
                {
                    var newDescItem = mapper.Map<CompetitiveEventDescriptionItem>(descItemDto);
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
}

