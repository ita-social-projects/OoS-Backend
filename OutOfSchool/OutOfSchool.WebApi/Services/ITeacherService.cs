using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Interface of TeacherService.
    /// </summary>
    public interface ITeacherService
    {
        /// <summary>
        /// Add a new Teacher to the database.
        /// </summary>
        /// <param name="teacherDto">Teacher to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherDTO> Create(TeacherDTO teacherDto);
        
        /// <summary>
        /// Get all teachers from the database.
        /// </summary>
        /// <returns>List of all teachers.</returns>
        Task<IEnumerable<TeacherDTO>> GetAllTeachers();

        /// <summary>
        /// Get teacherDto by it's key.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>Teacher.</returns>
        Task<TeacherDTO> GetById(long id);

        /// <summary>
        /// Update information about a specific teacherDto.
        /// </summary>
        /// <param name="teacherDto">Teacher to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherDTO> Update(TeacherDTO teacherDto);

        /// <summary>
        /// Delete teacherDto from the database by it's key.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}