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
        Task<ChildDto> Create(ChildDto dto);

        /// <summary>
        /// Get all children from the database.
        /// </summary>
        /// <returns>List of all children.</returns>
        Task<IEnumerable<ChildDto>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Child.</returns>
        Task<ChildDto> GetById(long id);

        /// <summary>
        /// Get children with some ParentId.
        /// </summary>
        /// <param name="id">ParentId.</param>
        /// <param name="userId">Key in the User table.</param>
        /// <returns>List of children.</returns>
        Task<IEnumerable<ChildDto>> GetAllByParent(long id, string userId);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Child entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ChildDto> Update(ChildDto dto);

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
        Task<ChildDto> GetByIdWithDetails(long id);
    }
}
