using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Achievement;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Initializes a new instance of the <see cref="AchievementService"/> class.
/// </summary>
/// <param name="repository">Repository for Achievement entity.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
public class AchievementService(
    IAchievementRepository repository,
    ILogger<AchievementService> logger,
    IStringLocalizer<SharedResource> localizer) : IAchievementService
{
    /// <summary>
    /// Create a delegate to include other entities in Achievement entity
    /// </summary>
    private readonly Func<IQueryable<Achievement>, IQueryable<Achievement>> includeFunc =
        a => a.Include(a => a.Children)
              .Include(a => a.Teachers)
              .Include(a => a.AchievementType);

    /// <inheritdoc/>
    public async Task<AchievementDto> GetById(Guid id)
    {
        logger.LogInformation($"Getting Achievement by Id started. Looking Id = {id}.");

        var achievements = await repository
            .GetByFilter(
                x => x.Id == id && 
                !x.AchievementType.IsDeleted,
                includeExpression: includeFunc)
            .ConfigureAwait(false);

        var achievement = achievements.SingleOrDefault();

        if (achievement == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Achievement with Id = {id} doesn't exist in the system."]);
        }

        logger.LogInformation($"Successfully got a Achievement with Id = {id}.");

        return achievement.ToDto();
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

        var count = await repository.Count(predicate).ConfigureAwait(false);

        var achievements = await repository
            .Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate)
            .IncludeProperties(includeFunc)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(!achievements.Any()
            ? "This Workshop has no achievements."
            : $"All {achievements.Count} records were successfully received");

        var result = new SearchResult<AchievementDto>()
        {
            TotalAmount = count,
            Entities = achievements.ToDto(), 
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

        var newAchievement = await repository.Create(dto.ToModel(), dto.ChildrenIDs, dto.Teachers).ConfigureAwait(false);

        logger.LogInformation($"Achievement with Id = {newAchievement?.Id} created successfully.");

        return newAchievement.ToDto();
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
            var updatedAchievement = await repository.Update(dto.ToModel(), dto.ChildrenIDs, dto.Teachers)
                .ConfigureAwait(false);

            logger.LogInformation($"Achievement with Id = {updatedAchievement?.Id} updated succesfully.");

            return updatedAchievement.ToDto();
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

        var achievement = await repository.GetById(id).ConfigureAwait(false);

        if (achievement is null)
        {
            logger.LogInformation($"Operation failed. Achievement with Id = {id} doesn't exist in the system.");
            throw new ArgumentException(localizer[$"Achievement with Id = {id} doesn't exist in the system."]);
        }

        try
        {
            await repository.Delete(achievement).ConfigureAwait(false);

            logger.LogInformation($"Achievement with Id = {id} succesfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting failed. Achievement with Id = {id} doesn't exist in the system.");
            throw;
        }
    }
}