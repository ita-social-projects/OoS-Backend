using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Interface of WorkshopService.
    /// </summary>
    public interface IWorkshopService
    {
        /// <summary>
        /// Add a new Workshop to the database.
        /// </summary>
        /// <param name="workshop">Entity which needs to be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<WorkshopDTO> Create(WorkshopDTO workshop);

        /// <summary>
        /// Get all sections from the database.
        /// </summary>
        /// <returns>List of all sections.</returns>
        Task<IEnumerable<WorkshopDTO>> GetAllWorkshops();
    }
}