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
        /// <param name="number">Number of entries.</param>
        /// <returns>List of popular categories.</returns>
        Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int number);

        /// <summary>
        /// Get popular workshops.
        /// </summary>
        /// <param name="number">Number of entries.</param>
        /// <returns>List of popular workshops.</returns>
        Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int number);
    }
}
