using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Provider entity.
    /// </summary>
    public interface IProviderService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">Provider entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ProviderDTO> Create(ProviderDTO dto);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all providers.</returns>
        Task<IEnumerable<ProviderDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Provider.</returns>
        Task<ProviderDTO> GetById(long id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Provider entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ProviderDTO> Update(ProviderDTO dto);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
