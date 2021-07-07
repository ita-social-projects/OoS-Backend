using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

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
        /// <param name="applicationDto">Application entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ApplicationDto> Create(ApplicationDto applicationDto);

        /// <summary>
        /// Add collection of entities.
        /// </summary>
        /// <param name="applicationDtos">Collection of Application entities to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<ApplicationDto>> Create(IEnumerable<ApplicationDto> applicationDtos);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Application.</returns>
        Task<ApplicationDto> GetById(long id);

        /// <summary>
        /// Get applications by workshop id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByWorkshop(long id);

        /// <summary>
        /// Get applications by provider id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByProvider(long id);

        /// <summary>
        /// Get applications by status.
        /// </summary>
        /// <param name="status">Status of application.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByStatus(int status);

        /// <summary>
        /// Get applications by parent id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByParent(long id);

        /// <summary>
        /// Get applications by child id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByChild(long id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="applicationDto">Application entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ApplicationDto> Update(ApplicationDto applicationDto);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Application's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
