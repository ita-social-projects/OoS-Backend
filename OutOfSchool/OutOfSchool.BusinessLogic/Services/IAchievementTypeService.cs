using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for Achievement Type entity.
/// </summary>
public interface IAchievementTypeService
{
    /// <summary>
    /// To recieve all Achievement Types.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all applications.</returns>
    Task<IEnumerable<AchievementTypeDto>> GetAll(LocalizationType localization = LocalizationType.Ua);
}