using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Application entity.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="application">Application entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ApplicationDTO> Create(ApplicationDTO application);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all applications.</returns>
        Task<IEnumerable<ApplicationDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Application.</returns>
        Task<ApplicationDTO> GetById(long id);

        /// <summary>
        /// Get applications by workshop id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDTO>> GetAllByWorkshop(long id);

        /// <summary>
        /// Get applications by user id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDTO>> GetAllByUser(string id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="application">Application entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ApplicationDTO> Update(ApplicationDTO application);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Application's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
