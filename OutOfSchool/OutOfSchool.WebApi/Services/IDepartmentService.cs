using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Department entity.
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// Add new Department to the DB.
        /// </summary>
        /// <param name="dto">DepartmentDto element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="DepartmentDto"/> that was created.</returns>
        Task<DepartmentDto> Create(DepartmentDto dto);

        /// <summary>
        /// Get all Department objects from DB.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="DepartmentDto"/> that were found.</returns>
        Task<IEnumerable<DepartmentDto>> GetAll();

        /// <summary>
        /// To recieve the Department object with define id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="DepartmentDto"/> that was found.</returns>
        Task<DepartmentDto> GetById(long id);

        /// <summary>
        /// To recieve the Department list with define Direction id.
        /// </summary>
        /// <param name="id">Key of the Direction in the table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="DepartmentDto"/> that were found.</returns>
        Task<IEnumerable<DepartmentDto>> GetByDirectionId(long id);

        /// <summary>
        /// To Update our object in DB.
        /// </summary>
        /// <param name="dto">Department with new properties.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="DepartmentDto"/> that was updated.</returns>
        Task<DepartmentDto> Update(DepartmentDto dto);

        /// <summary>
        /// To delete the object from DB.
        /// </summary>
        /// <param name="id">Key of the Department in table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Result<DepartmentDto>> Delete(long id);
    }
}
