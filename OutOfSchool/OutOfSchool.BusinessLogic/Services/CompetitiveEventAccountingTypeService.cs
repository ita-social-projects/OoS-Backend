using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class CompetitiveEventAccountingTypeService : ICompetitiveEventAccountingTypeService
{
    private readonly IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> accountingTypeRepository;
    private readonly ILogger<CompetitiveEventAccountingType> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitiveEventAccountingTypeService"/> class.
    /// </summary>
    /// <param name="repository">Repository for CompetitiveEvent AccountingType entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public CompetitiveEventAccountingTypeService(
        IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> accountingTypeRepository,
        ILogger<CompetitiveEventAccountingType> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.accountingTypeRepository = accountingTypeRepository ?? throw new ArgumentNullException(nameof(accountingTypeRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<CompetitiveEventAccountingTypeDto>> GetAll(LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation("Getting all CompetitiveEvent Accounting Types, {Localization} localization, started.", localization);

        var accountingTypes = await accountingTypeRepository.GetAll().ConfigureAwait(false);

        var logMessage = accountingTypes.Any() ?
             "All {Count} records were successfully received from the CompetitiveEvent Accounting Types table."
            : "CompetitiveEvent Accounting Type table is empty.";
        logger.LogDebug(logMessage, accountingTypes.Count());
        
        var achievementTypesLocalized = accountingTypes.Select(x =>
            new CompetitiveEventAccountingType
            {
                Id = x.Id,
                Title = localization == LocalizationType.En ? x.TitleEn : x.Title,
            });
        return mapper.Map<List<CompetitiveEventAccountingTypeDto>>(achievementTypesLocalized);
    }
}