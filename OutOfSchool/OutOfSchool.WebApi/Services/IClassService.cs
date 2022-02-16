using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Class entity.
    /// </summary>
    public interface IClassService
    {
        /// <summary>
        /// Add new Class to the DB.
        /// </summary>
        /// <param name="dto">ClassDto entity.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="ClassDto"/> that was created.</returns>
        Task<ClassDto> Create(ClassDto dto);

        /// <summary>
        /// Get all Class objects from DB.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="ClassDto"/> that were found.</returns>
        Task<IEnumerable<ClassDto>> GetAll();

        /// <summary>
        /// Get Class objects from DB by filter.
        /// </summary>
        /// <param name="filter">Filter for Class dto.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="ClassDto"/> that were found.</returns>
        Task<IEnumerable<ClassDto>> GetByFilter(OffsetFilter filter);

        /// <summary>
        /// To recieve the Class object with define id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="ClassDto"/> that was found.</returns>
        Task<ClassDto> GetById(long id);

        /// <summary>
        /// To recieve the Class list with defined DepartmentId.
        /// </summary>
        /// <param name="id">Key of the Department in the table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="ClassDto"/> that were found.</returns>
        Task<IEnumerable<ClassDto>> GetByDepartmentId(long id);

        /// <summary>
        /// To Update our object in DB.
        /// </summary>
        /// <param name="dto">Class with new properties.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="ClassDto"/> that was updated.</returns>
        Task<ClassDto> Update(ClassDto dto);

        /// <summary>
        /// To delete the object from DB.
        /// </summary>
        /// <param name="id">Key of the Class in table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Result<ClassDto>> Delete(long id);
    }
}
