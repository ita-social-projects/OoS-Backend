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
    private readonly IEntityRepository<Guid, Judge> judgeRepository;
    private readonly ILogger<CompetitiveEventService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;

    public CompetitiveEventService(
        IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> competitiveEventRepository,
        IEntityRepository<Guid, Judge> judgeRepository,
        IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository,
        ILogger<CompetitiveEventService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        this.competitiveEventRepository = competitiveEventRepository ?? throw new ArgumentNullException(nameof(competitiveEventRepository));
        this.judgeRepository = judgeRepository ?? throw new ArgumentNullException(nameof(judgeRepository));
        this.descriptionItemRepository = descriptionItemRepository ?? throw new ArgumentException(nameof(descriptionItemRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.currentUserService = currentUserService;

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
    public async Task<CompetitiveEventDto> Create(CompetitiveEventCreateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogDebug("CompetitiveEvent creating was started.");

        var competitiveEvent = mapper.Map<CompetitiveEvent>(dto);
        // competitiveEvent.Judges = dto.Judges?.Select(dtoJudges => mapper.Map<Judge>(dtoJudges)).ToList();

        var newCompetitiveEvent = await competitiveEventRepository.RunInTransaction(async () =>
        await competitiveEventRepository.Create(competitiveEvent).ConfigureAwait(false)).ConfigureAwait(false);

        return mapper.Map<CompetitiveEventDto>(newCompetitiveEvent);
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> Update(CompetitiveEventUpdateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogDebug("Updating CompetitiveEvent with Id = {dtoId} started.", dto.Id);

        var competitiveEvent = await competitiveEventRepository.GetByIdWithDetails(dto.Id, "Judges,CompetitiveEventDescriptionItems").ConfigureAwait(false);

        if (competitiveEvent is null)
        {
            var message = $"Updating failed. CompetitiveEvent with Id = {dto.Id} doesn't exist in the system.";
            logger.LogError(message);
            throw new DbUpdateConcurrencyException(message);
        }

        await ChangeJudges(competitiveEvent, dto.Judges ?? new List<JudgeDto>()).ConfigureAwait(false);
        await ChangeCompetitiveEventDescriptionItems(competitiveEvent, dto.CompetitiveEventDescriptionItems
            ?? new List<CompetitiveEventDescriptionItemDto>()).ConfigureAwait(false);

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

    private async Task ChangeJudges(CompetitiveEvent currentCompetitiveEvent, List<JudgeDto> judgeDtoList)
    {
        try
        {
            await DeleteObsoleteJudgesAsync(currentCompetitiveEvent, judgeDtoList).ConfigureAwait(false);
            await UpdateOrAppendJudgesAsync(currentCompetitiveEvent, judgeDtoList).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while changing judges.");
            throw;
        }
    }

    private async Task DeleteObsoleteJudgesAsync(CompetitiveEvent currentCompetitiveEvent, List<JudgeDto> judgeDtoList)
    {
        var judgesToDelete = currentCompetitiveEvent.Judges
             .Where(judge => !judgeDtoList.Exists(j => j.Id == judge.Id))
             .ToList();

        foreach (var deletedJudge in judgesToDelete)
        {
            if (deletedJudge != null)
            {
                try
                {
                    await judgeRepository.Delete(deletedJudge).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to delete judge with ID: {JudgeId}", deletedJudge.Id);
                    throw;
                }
            }
        }
    }

    private async Task UpdateOrAppendJudgesAsync(CompetitiveEvent currentCompetitiveEvent, List<JudgeDto> judgeDtoList)
    {
        foreach (var judgeDto in judgeDtoList)
        {
            try
            {
                var foundJudge = currentCompetitiveEvent.Judges.FirstOrDefault(j => j.Id == judgeDto.Id);
                if (foundJudge != null)
                {
                    mapper.Map(judgeDto, foundJudge);
                    await judgeRepository.Update(foundJudge).ConfigureAwait(false);
                }
                else
                {
                    var newJudge = mapper.Map<Judge>(judgeDto);
                    newJudge.CompetitiveEventId = currentCompetitiveEvent.Id;
                    await judgeRepository.Create(newJudge).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process judge DTO with ID: {JudgeId}", judgeDto.Id);
                throw;
            }
        }
    }

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

