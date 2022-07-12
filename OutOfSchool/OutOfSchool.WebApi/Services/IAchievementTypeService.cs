using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for Achievement Type entity.
/// </summary>
public interface IAchievementTypeService
{
    /// <summary>
    /// To recieve all Achievement Types.
    /// </summary>
    /// <returns>List of all applications.</returns>
    Task<IEnumerable<AchievementTypeDto>> GetAll();
}