using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for statistic service.
/// </summary>
public interface IStatisticService
{
    /// <summary>
    /// Get popular directions using Redis.
    /// </summary>
    /// <param name="limit">Number of entries to return.</param>
    /// <param name="catottgId">Codeficator's id.</param>
    /// <returns>List of popular categories.</returns>
    Task<IEnumerable<DirectionStatistic>> GetPopularDirections(int limit, long catottgId);

    /// <summary>
    /// Get popular directions from DB.
    /// </summary>
    /// <param name="limit">Number of entries to return.</param>
    /// <param name="catottgId">Codeficator's id.</param>
    /// <returns>List of popular categories.</returns>
    Task<IEnumerable<DirectionStatistic>> GetPopularDirectionsFromDatabase(int limit, long catottgId);

    /// <summary>
    /// Get popular workshops using Redis.
    /// </summary>
    /// <param name="limit">Number of entries to return.</param>
    /// <param name="catottgId">Codeficator's id.</param>
    /// <returns>List of popular workshops.</returns>
    Task<IEnumerable<WorkshopCard>> GetPopularWorkshops(int limit, long catottgId);

    /// <summary>
    /// Get popular workshops from DB.
    /// </summary>
    /// <param name="limit">Number of entries to return.</param>
    /// <param name="catottgId">Codeficator's id.</param>
    /// <returns>List of popular workshops.</returns>
    Task<IEnumerable<WorkshopCard>> GetPopularWorkshopsFromDatabase(int limit, long catottgId);
}