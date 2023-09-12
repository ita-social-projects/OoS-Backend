using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;

namespace OutOfSchool.WebApi.Services;

public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository achievementRepository;
    private readonly ILogger<AchievementService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementService"/> class.
    /// </summary>
    /// <param name="repository">Repository for Achievement entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public AchievementService(
        IAchievementRepository repository,
        ILogger<AchievementService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer;
        this.achievementRepository = repository;
        this.logger = logger;
        this.mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<AchievementDto> GetById(Guid id)
    {
        logger.LogInformation($"Getting Achievement by Id started. Looking Id = {id}.");

        var achievements = await achievementRepository.GetByFilter(x => x.Id == id && !x.AchievementType.IsDeleted).ConfigureAwait(false);
        var achievement = achievements.SingleOrDefault();

        if (achievement == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Achievement with Id = {id} doesn't exist in the system."]);
        }

        logger.LogInformation($"Successfully got a Achievement with Id = {id}.");

        return mapper.Map<AchievementDto>(achievement);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<AchievementDto>> GetByFilter(AchievementsFilter filter)
    {
        logger.LogInformation("Getting all Achievements started (by filter)");

        filter ??= new AchievementsFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var predicate = PredicateBuilder.True<Achievement>();

        if (filter.WorkshopId != Guid.Empty)
        {
            predicate = predicate.And(a => a.WorkshopId == filter.WorkshopId);
        }

        predicate = predicate.And(a => !a.AchievementType.IsDeleted);

        int count = await achievementRepository.Count(predicate).ConfigureAwait(false);

        var achievements = await achievementRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: "Children",
                whereExpression: predicate)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(!achievements.Any()
            ? "This Workshop has no achievements."
            : $"All {achievements.Count} records were successfully received");

        var achievementsDto = achievements.Select(achievement => mapper.Map<AchievementDto>(achievement)).ToList();

        var result = new SearchResult<AchievementDto>()
        {
            TotalAmount = count,
            Entities = achievementsDto,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<AchievementDto> Create(AchievementCreateDTO dto)
    {
        logger.LogInformation("Achievement creating was started.");

        if (dto is null)
        {
            logger.LogInformation("Operation failed, dto is null");
            throw new ArgumentException(localizer["dto is null."], nameof(dto));
        }

        var achievement = mapper.Map<Achievement>(dto);

        var newAchievement = await achievementRepository.Create(achievement, dto.ChildrenIDs, dto.Teachers).ConfigureAwait(false);

        logger.LogInformation($"Achievement with Id = {newAchievement?.Id} created successfully.");

        return mapper.Map<AchievementDto>(newAchievement);
    }

    /// <inheritdoc/>
    public async Task<AchievementDto> Update(AchievementCreateDTO dto)
    {
        logger.LogInformation($"Updating Achievement with Id = {dto?.Id} started.");

        if (dto is null)
        {
            logger.LogInformation("Operation failed, dto is null");
            throw new ArgumentException(localizer["dto is null."], nameof(dto));
        }

        try
        {
            var updatedAchievement = await achievementRepository.Update(mapper.Map<Achievement>(dto), dto.ChildrenIDs, dto.Teachers)
                .ConfigureAwait(false);

            logger.LogInformation($"Achievement with Id = {updatedAchievement?.Id} updated succesfully.");

            return mapper.Map<AchievementDto>(updatedAchievement);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. Achievement with Id = {dto?.Id} doesn't exist in the system.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogInformation($"Deleting Achievement with Id = {id} started.");

        var achievement = await achievementRepository.GetById(id).ConfigureAwait(false);

        if (achievement is null)
        {
            logger.LogInformation($"Operation failed. Achievement with Id = {id} doesn't exist in the system.");
            throw new ArgumentException(localizer[$"Achievement with Id = {id} doesn't exist in the system."]);
        }

        try
        {
            await achievementRepository.Delete(achievement).ConfigureAwait(false);

            logger.LogInformation($"Achievement with Id = {id} succesfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting failed. Achievement with Id = {id} doesn't exist in the system.");
            throw;
        }
    }
}