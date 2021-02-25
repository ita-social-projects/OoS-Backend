using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Workshop entity.
    /// </summary>
    public interface IWorkshopService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">Workshop to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<WorkshopDTO> Create(WorkshopDTO dto);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all workshops.</returns>
        Task<IEnumerable<WorkshopDTO>> GetAll();
        
        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>Workshop.</returns>
        Task<WorkshopDTO> GetById(long id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Workshop entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<WorkshopDTO> Update(WorkshopDTO dto);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}