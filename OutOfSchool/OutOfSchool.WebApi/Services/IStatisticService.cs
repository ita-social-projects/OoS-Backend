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
        /// Get popular categories.
        /// </summary>
        /// <param name="limit">Number of entries.</param>
        /// <returns>List of popular categories.</returns>
        Task<IEnumerable<DirectionStatistic>> GetPopularDirections(int limit);

        /// <summary>
        /// Get popular workshops.
        /// </summary>
        /// <param name="limit">Number of entries.</param>
        /// <returns>List of popular workshops.</returns>
        Task<IEnumerable<WorkshopCard>> GetPopularWorkshops(int limit);
    }
}
