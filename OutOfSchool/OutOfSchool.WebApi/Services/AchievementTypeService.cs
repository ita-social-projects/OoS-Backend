using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public class AchievementTypeService : IAchievementTypeService
{
    private readonly IEntityRepository<long, AchievementType> achievementTypeRepository;
    private readonly ILogger<AchievementTypeService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementTypeService"/> class.
    /// </summary>
    /// <param name="repository">Repository for Achievement Type entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public AchievementTypeService(
        IEntityRepository<long, AchievementType> repository,
        ILogger<AchievementTypeService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.achievementTypeRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

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
            : $"All {achievementTypes.Count()} records were successfully received from the AchievementType table");

        return mapper.Map<List<AchievementTypeDto>>(achievementTypesLocalized);
    }
}