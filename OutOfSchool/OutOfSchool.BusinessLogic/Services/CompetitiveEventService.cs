using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for CompetitiveEvent entity.
/// </summary>
public class CompetitiveEventService : ICompetitiveEventService
{
    private readonly IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> competitiveEventRepository;
    private readonly ILogger<CompetitiveEventService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    public CompetitiveEventService(
        IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> competitiveEventRepository,
        ILogger<CompetitiveEventService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.competitiveEventRepository = competitiveEventRepository ?? throw new ArgumentNullException(nameof(competitiveEventRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> GetById(Guid id)
    {
        logger.LogTrace($"Getting CompetitiveEvent by Id started. Looking Id = {id}.");

        var competitiveEvent = await competitiveEventRepository.GetById(id).ConfigureAwait(false);

        if (competitiveEvent == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogTrace($"Successfully got a CompetitiveEvent with Id = {id}.");

        return mapper.Map<CompetitiveEventDto>(competitiveEvent);
    }

    /// <inheritdoc/>
    public async Task<CompetitiveEventDto> Create(CompetitiveEventDto dto)
    {
        logger.LogTrace("CompetitiveEvent creating was started.");

        var competitiveEvent = mapper.Map<CompetitiveEvent>(dto);

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

        mapper.Map(dto, competitiveEvent);
        competitiveEvent = await competitiveEventRepository.Update(competitiveEvent).ConfigureAwait(false);

        logger.LogTrace($"CompetitiveEvent with Id = {competitiveEvent?.Id} updated succesfully.");

        return mapper.Map<CompetitiveEventDto>(competitiveEvent);
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogTrace($"Deleting CompetitiveEvent with Id = {id} started.");

        var favorite = await competitiveEventRepository.GetById(id).ConfigureAwait(false);

        if (favorite == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"CompetitiveEvent with Id = {id} doesn't exist in the system"]);
        }

        await competitiveEventRepository.Delete(favorite).ConfigureAwait(false);

        logger.LogTrace($"CompetitiveEvent with Id = {id} succesfully deleted.");
    }
}
