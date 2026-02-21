using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Initializes a new instance of the <see cref="CompetitiveEventAccountingTypeService"/> class.
/// </summary>
/// <param name="repository">Repository for CompetitiveEvent AccountingType entity.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
/// <param name="mapper">Mapper.</param>
public class CompetitiveEventAccountingTypeService(
    IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> accountingTypeRepository,
    ILogger<CompetitiveEventAccountingType> logger,
    IStringLocalizer<SharedResource> localizer
) : ICompetitiveEventAccountingTypeService
{
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

        return achievementTypesLocalized.ToDto();
    }
}