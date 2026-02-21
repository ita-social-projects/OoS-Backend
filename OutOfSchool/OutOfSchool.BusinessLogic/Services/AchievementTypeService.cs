using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

// No nested entities in use – eager loading not required.
/// <summary>
/// Initializes a new instance of the <see cref="AchievementTypeService"/> class.
/// </summary>
/// <param name="repository">Repository for Achievement Type entity.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
/// <param name="mapper">Mapper.</param>
public class AchievementTypeService(
    IEntityRepositorySoftDeleted<long, AchievementType> achievementTypeRepository,
    ILogger<AchievementTypeService> logger,
    IStringLocalizer<SharedResource> localizer
) : IAchievementTypeService
{
    /// <inheritdoc/>
    public async Task<IEnumerable<AchievementTypeDto>> GetAll(LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation($"Getting all Achievement Types, {localization} localization, started.");

        var achievementTypes = await achievementTypeRepository.GetAll().ConfigureAwait(false);
        var achievementTypesLocalized = achievementTypes.Select(x =>
            new AchievementType
            {
                Id = x.Id,
                Title = localization == LocalizationType.En ? x.TitleEn : x.Title,
            });

        logger.LogInformation(!achievementTypes.Any()
            ? "Achievement Type table is empty."
            : $"All {achievementTypes.Count()} records were successfully received from the Address table");

        return achievementTypesLocalized.ToDto();
    }
}