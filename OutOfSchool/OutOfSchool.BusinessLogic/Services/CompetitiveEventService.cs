using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for CompetitiveEvent entity.
/// </summary>
public class CompetitiveEventService : ICompetitiveEventService
{
    private readonly IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> competitiveEventRepository;
    private readonly IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> accountingTypeOfEventRepository;
    private readonly IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository;
    private readonly IEntityRepository<Guid, Judge> judgeRepository;
    private readonly ILogger<CompetitiveEventService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    public CompetitiveEventService(
        IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> competitiveEventRepository,
        IEntityRepository<Guid, Judge> judgeRepository,
        IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> accountingTypeOfEventRepository,
        IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository,
        ILogger<CompetitiveEventService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.competitiveEventRepository = competitiveEventRepository ?? throw new ArgumentNullException(nameof(competitiveEventRepository));
        this.judgeRepository = judgeRepository ?? throw new ArgumentNullException(nameof(judgeRepository));
        this.accountingTypeOfEventRepository = accountingTypeOfEventRepository ?? throw new ArgumentException(nameof(accountingTypeOfEventRepository));
        this.descriptionItemRepository = descriptionItemRepository ?? throw new ArgumentException(nameof(accountingTypeOfEventRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto?> GetById(Guid id)
    {
        logger.LogTrace($"Getting CompetitiveEvent by Id started. Looking Id = {id}.");

        var competitiveEvent = (await competitiveEventRepository.GetById(id).ConfigureAwait(false));

        string logMessage = competitiveEvent is null
            ? $"CompetitiveEvent with Id = {id} doesn't exist in the system."
            : $"Successfully got a CompetitiveEvent with Id = {id}.";

        logger.LogTrace(logMessage);

        return mapper.Map<CompetitiveEventDto>(competitiveEvent);
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> Create(CompetitiveEventCreateDto dto)
    {
        logger.LogTrace("CompetitiveEvent creating was started.");

        ArgumentNullException.ThrowIfNull(dto);

        return await competitiveEventRepository.RunInTransaction(async () =>
        {
            var competitiveEvent = mapper.Map<CompetitiveEvent>(dto);

            competitiveEvent.Judges = dto.Judges?.Select(dtoJudges => mapper.Map<Judge>(dtoJudges)).ToList();

            var newCompetitiveEvent = await competitiveEventRepository.Create(competitiveEvent).ConfigureAwait(false);

            logger.LogTrace($"CompetitiveEvent with Id = {newCompetitiveEvent?.Id} created successfully.");

            return mapper.Map<CompetitiveEventDto>(newCompetitiveEvent);
        }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> Update(CompetitiveEventUpdateDto dto)
    {
        logger.LogTrace($"Updating CompetitiveEvent with Id = {dto?.Id} started.");

        ArgumentNullException.ThrowIfNull(dto);

        var competitiveEvent = await competitiveEventRepository.GetByIdWithDetails(dto.Id, "Judges,CompetitiveEventDescriptionItems").ConfigureAwait(false);

        if (competitiveEvent is null)
        {
            var message = $"Updating failed. CompetitiveEvent with Id = {dto.Id} doesn't exist in the system.";
            logger.LogError(message);
            throw new DbUpdateConcurrencyException(message);
        }

        async Task<CompetitiveEvent> UpdateCompetitiveEventLocally()
        {
            await ChangeJudges(competitiveEvent, dto.Judges ?? new List<JudgeDto>()).ConfigureAwait(false);
            await ChangeCompetitiveEventDescriptionItems(competitiveEvent,
                dto.CompetitiveEventDescriptionItems ?? new List<CompetitiveEventDescriptionItemDto>()).ConfigureAwait(false);

            mapper.Map(dto, competitiveEvent);
            competitiveEvent = await competitiveEventRepository.Update(competitiveEvent).ConfigureAwait(false);

            logger.LogTrace($"CompetitiveEvent with Id = {competitiveEvent?.Id} updated succesfully.");

            return competitiveEvent;
        }

        var updatedCompetitiveEvent = await competitiveEventRepository
          .RunInTransaction(UpdateCompetitiveEventLocally)
          .ConfigureAwait(false);

        return mapper.Map<CompetitiveEventDto>(updatedCompetitiveEvent);
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogTrace($"Deleting CompetitiveEvent with Id = {id} started.");

        var entity = await competitiveEventRepository.GetById(id);

        try
        {
            await competitiveEventRepository.Delete(entity).ConfigureAwait(false);

            logger.LogTrace($"CompetitiveEvent with Id = {id} succesfully deleted.");
        }
        catch (Exception) // DbUpdateConcurrencyException
        {
            logger.LogError("Deleting failed. CompetitiveEvent with Id = {Id} doesn't exist in the system", id);
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"CompetitiveEvent with Id = {id} doesn't exist in the system"]);
        }
    }

    private async Task ChangeJudges(CompetitiveEvent currentCompetitiveEvent, List<JudgeDto> judgeDtoList)
    {
        try
        {
            var judgesToDelete = currentCompetitiveEvent.Judges
                 .Where(judge => !judgeDtoList.Exists(j => j.Id == judge.Id))
                 .ToList();

            if (judgesToDelete.Count > 0)
            {
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
                            logger.LogError(ex, $"Failed to delete judge with ID: {deletedJudge.Id}");
                            throw;
                        }
                    }
                }
            }

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
                    logger.LogError(ex, $"Failed to process judge DTO with ID: {judgeDto.Id}");
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while changing judges.");
            throw;
        }
    }
    private async Task ChangeCompetitiveEventDescriptionItems(CompetitiveEvent currentCompetitiveEvent, List<CompetitiveEventDescriptionItemDto> descriptionItemsDtoList)
    {
        try {
            var descItemsToDelete1 = currentCompetitiveEvent.CompetitiveEventDescriptionItems
                .Where(descItem => !descriptionItemsDtoList.Exists(item => item.Id == descItem.Id))
                .ToList();
            if (descItemsToDelete1.Count > 0)
            {
                foreach (var descItem in descItemsToDelete1)
                {
                    if (descItem != null)
                    {
                        try
                        {
                            await descriptionItemRepository.Delete(descItem).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Failed to delete description item with ID: {descItem.Id}");
                            throw;
                        }
                    }
                }
            }

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
                    logger.LogError(ex, $"Failed to process description item with ID: {descItemDto.Id}");
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the description items.");
            throw;
        }
    }
}

