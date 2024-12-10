using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines the interface for CRUD functionality related for CompetitiveEvent AccountingType entities.
/// </summary>
public interface ICompetitiveEventAccountingTypeService
{
    /// <summary>
    /// To recieve all CompetitiveEvent Accounting Types.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of all accounting types for competitive events.</returns>
    Task<IEnumerable<CompetitiveEventAccountingTypeDto>> GetAll(LocalizationType localization = LocalizationType.Ua);

}