using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Teacher entity.
    /// </summary>
    public interface ITeacherService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">Teacher to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherDTO> Create(TeacherDTO dto);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all teachers.</returns>
        Task<IEnumerable<TeacherDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>Teacher.</returns>
        Task<TeacherDTO> GetById(long id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Teacher to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherDTO> Update(TeacherDTO dto);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}