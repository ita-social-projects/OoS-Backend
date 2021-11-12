using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for statistic service.
    /// </summary>
    public interface IStatisticService
    {
        /// <summary>
        /// Get popular directions.
        /// </summary>
        /// <param name="limit">Number of entries to return.</param>
        /// <param name="city">City to look for.</param>
        /// <returns>List of popular categories.</returns>
        Task<IEnumerable<DirectionStatistic>> GetPopularDirections(int limit, string city);

        /// <summary>
        /// Get popular workshops.
        /// </summary>
        /// <param name="limit">Number of entries to return.</param>
        /// <param name="city">City to look for.</param>
        /// <returns>List of popular workshops.</returns>
        Task<IEnumerable<WorkshopCard>> GetPopularWorkshops(int limit, string city);
    }
}
