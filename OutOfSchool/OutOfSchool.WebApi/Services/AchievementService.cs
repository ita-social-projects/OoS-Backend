using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services;

public class AchievementService: IAchievementService
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
        IMapper mapper,
        OutOfSchoolDbContext context)
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

        var achievement = await achievementRepository.GetById(id).ConfigureAwait(false);

        if (achievement == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a Achievement with Id = {id}.");

        return mapper.Map<AchievementDto>(achievement);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AchievementDto>> GetByWorkshopId(Guid id)
    {
        logger.LogInformation("Getting all Achievements by Workshop Id started.");

        var achievements = await achievementRepository.GetByWorkshopId(id).ConfigureAwait(false);

        logger.LogInformation(!achievements.Any()
            ? "This Workshop has no achievements."
            : $"All {achievements.Count()} records were successfully received");

        return mapper.Map<List<AchievementDto>>(achievements);
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

        var achievement = achievementRepository.Get(where: a => a.Id == id);

        if (!achievement.Any())
        {
            logger.LogInformation($"Operation failed. Achievement with Id = {id} doesn't exist in the system.");
            throw new ArgumentException(localizer[$"Achievement with Id = {id} doesn't exist in the system."]);
        }

        var entity = new Achievement { Id = id };

        try
        {
            await achievementRepository.Delete(entity).ConfigureAwait(false);

            logger.LogInformation($"Achievement with Id = {id} succesfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting failed. Achievement with Id = {id} doesn't exist in the system.");
            throw;
        }
    }        
}