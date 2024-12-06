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
    private readonly IEntityRepository<Guid, Judge> judgeRepository;
    private readonly ILogger<CompetitiveEventService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    public CompetitiveEventService(
        IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> competitiveEventRepository,
        IEntityRepository<Guid, Judge> judgeRepository,
        IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> accountingTypeOfEventRepository,
        ILogger<CompetitiveEventService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.competitiveEventRepository = competitiveEventRepository ?? throw new ArgumentNullException(nameof(competitiveEventRepository));
        this.judgeRepository = judgeRepository ?? throw new ArgumentNullException(nameof(judgeRepository));
        this.accountingTypeOfEventRepository = accountingTypeOfEventRepository ?? throw new ArgumentException(nameof(accountingTypeOfEventRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto?> GetById(Guid id)
    {
        logger.LogTrace($"Getting CompetitiveEvent by Id started. Looking Id = {id}.");

        var competitiveEvent = (await competitiveEventRepository.GetById(id).ConfigureAwait(false));

        if (competitiveEvent is null)
        {
            logger.LogTrace($"CompetitiveEvent with Id = {id} doesn't exist in the system.");
        }
        else
        {
            logger.LogTrace($"Successfully got a CompetitiveEvent with Id = {id}.");
        }
        return mapper.Map<CompetitiveEventDto>(competitiveEvent);
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> Create(CompetitiveEventCreateDto dto)
    {
        logger.LogTrace("CompetitiveEvent creating was started.");

        ArgumentNullException.ThrowIfNull(dto);

        var competitiveEvent = mapper.Map<CompetitiveEvent>(dto);

        competitiveEvent.Judges = dto.Judges?.Select(dtoJudges => mapper.Map<Judge>(dtoJudges)).ToList();

        var newCompetitiveEvent = await competitiveEventRepository.Create(competitiveEvent).ConfigureAwait(false);

        logger.LogTrace($"CompetitiveEvent with Id = {newCompetitiveEvent?.Id} created successfully.");

        return mapper.Map<CompetitiveEventDto>(newCompetitiveEvent);
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> Update(CompetitiveEventDto dto)
    {
        logger.LogTrace($"Updating CompetitiveEvent with Id = {dto?.Id} started.");

        ArgumentNullException.ThrowIfNull(dto);

        var competitiveEvent = await competitiveEventRepository.GetById(dto.Id).ConfigureAwait(false);

        if (competitiveEvent is null)
        {
            var message = $"Updating failed. CompetitiveEvent with Id = {dto.Id} doesn't exist in the system.";
            logger.LogError(message);
            throw new DbUpdateConcurrencyException(message);
        }

        await ChangeJudges(competitiveEvent, dto.Judges ?? new List<JudgeDto>()).ConfigureAwait(false);

        // await ChangeAccountingTypesOfEvent(competitiveEvent,
        // dto.AccountingTypeOfEvent ?? new List<CompetitiveEventAccountingTypeDto>()).ConfigureAwait(false);

        mapper.Map(dto, competitiveEvent);
        competitiveEvent = await competitiveEventRepository.Update(competitiveEvent).ConfigureAwait(false);

        logger.LogTrace($"CompetitiveEvent with Id = {competitiveEvent?.Id} updated succesfully.");

        return mapper.Map<CompetitiveEventDto>(competitiveEvent);
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogTrace($"Deleting CompetitiveEvent with Id = {id} started.");

        var entity = new CompetitiveEvent() { Id = id };

        try
        {
            await competitiveEventRepository.Delete(entity).ConfigureAwait(false);

            logger.LogTrace($"CompetitiveEvent with Id = {id} succesfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Deleting failed. CompetitiveEvent with Id = {Id} doesn't exist in the system", id);
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"CompetitiveEvent with Id = {id} doesn't exist in the system"]);
        }
    }

    private async Task ChangeJudges(CompetitiveEvent currentCompetitiveEvent, List<JudgeDto> judgeDtoList)
    {
        var deletedIds = currentCompetitiveEvent.Judges
            .Select(x => x.Id)
            .Except(judgeDtoList.Select(x => x.Id))
            .ToList();

        if (deletedIds.Count > 0)
        {
            var judgeToDelete = currentCompetitiveEvent.Judges
                .Where(judge => deletedIds.Contains(judge.Id))
                .ToList();

            var deleteTasks = judgeToDelete
                .Select(deletedJudge => judgeRepository.Delete(deletedJudge));

            await Task.WhenAll(deleteTasks).ConfigureAwait(false);
        }

        foreach (var judgeDto in judgeDtoList)
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
    }
}

