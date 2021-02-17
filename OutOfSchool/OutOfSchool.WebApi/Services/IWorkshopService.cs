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
        /// Add a new workshop to the database.
        /// </summary>
        /// <param name="workshop">Workshop to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<WorkshopDTO> Create(WorkshopDTO workshop);

        /// <summary>
        /// Get all workshops from the database.
        /// </summary>
        /// <returns>List of all workshops.</returns>
        Task<IEnumerable<WorkshopDTO>> GetAll();
        
        /// <summary>
        /// Get workshop by it'ss key.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>Workshop.</returns>
        Task<WorkshopDTO> GetById(long id);

        /// <summary>
        /// Update information about a specific workshop entity.
        /// </summary>
        /// <param name="workshopDto">Workshop entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<WorkshopDTO> Update(WorkshopDTO workshopDto);

        /// <summary>
        /// Delete workshop from the database by it's key.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}