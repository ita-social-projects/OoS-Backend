using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Interface of ChildService.
    /// </summary>
    public interface IChildService
    {
        /// <summary>
        /// Add a new Child to the database.
        /// </summary>
        /// <param name="child">Child to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ChildDTO> Create(ChildDTO child);

        /// <summary>
        /// Get all children from the database.
        /// </summary>
        /// <returns>List of all children.</returns>
        Task<IEnumerable<ChildDTO>> GetAll();

        /// <summary>
        /// Get child by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Child.</returns>
        Task<ChildDTO> GetById(long id);


        /// <summary>
        /// Update information about child entity.
        /// </summary>
        /// <param name="childDTO">Child entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ChildDTO> Update(ChildDTO childDTO);

        /// <summary>
        /// Delete some element in database.
        /// </summary>
        /// <param name="id">Child's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
