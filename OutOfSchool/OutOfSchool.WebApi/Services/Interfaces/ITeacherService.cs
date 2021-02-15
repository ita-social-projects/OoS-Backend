using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models.ModelsDto;

namespace OutOfSchool.WebApi.Services.Interfaces
{
    /// <summary>
    /// Interface of TeacherService.
    /// </summary>
    public interface ITeacherService
    {
        /// <summary>
        /// Add a new Teacher to the database.
        /// </summary>
        /// <param name="teacher">Entity which needs to be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherDTO> Create(TeacherDTO teacher);
        
        /// <summary>
        /// Get all teachers from the database.
        /// </summary>
        /// <returns>List of all teachers.</returns>
        IEnumerable<TeacherDTO> GetAllTeachers();
    }
}