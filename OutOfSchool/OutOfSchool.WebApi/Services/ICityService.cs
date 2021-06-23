using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for City entity.
    /// </summary>
    public interface ICityService
    {
        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all Cities.</returns>
        Task<IEnumerable<CityDto>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>City.</returns>
        Task<CityDto> GetById(long id);

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">City entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<CityDto> Create(CityDto dto);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">City entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<CityDto> Update(CityDto dto);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">City key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
