using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Child entity.
    /// </summary>
    public interface IChildService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">Child to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ChildDTO> Create(ChildDTO dto);

        /// <summary>
        /// Get all children from the database.
        /// </summary>
        /// <returns>List of all children.</returns>
        Task<IEnumerable<ChildDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Child.</returns>
        Task<ChildDTO> GetById(long id);

        /// <summary>
        /// Get child with related details by parent Id.
        /// </summary>
        /// <param name="id">Parent's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ChildDTO> GetByIdWithDetails(long id);
        
        /// <summary>
        /// Get all children by parent parentId.
        /// </summary>
        /// <param name="parentId">Parent's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<ChildDTO>> GetAllByParent(long parentId);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Child entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ChildDTO> Update(ChildDTO dto);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Child's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);

        /// <summary>
        /// Get entity by it's key with details.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Child.</returns>
        Task<ChildDTO> GetByIdWithDetails(long id);
    }
}