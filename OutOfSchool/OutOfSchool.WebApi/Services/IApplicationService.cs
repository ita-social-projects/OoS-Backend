using System;
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
        Task<ModelWithAdditionalData<ApplicationDto, int>> Create(ApplicationDto applicationDto);

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
        Task<ApplicationDto> GetById(Guid id);

        /// <summary>
        /// Get applications by workshop id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <param name="filter">Application filter.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByWorkshop(Guid id, ApplicationFilter filter);

        /// <summary>
        /// Get applications by provider id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <param name="filter">Application filter.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByProvider(Guid id, ApplicationFilter filter);

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
        /// <param name="filter">Application filter.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByParent(Guid id, ApplicationFilter filter);

        /// <summary>
        /// Get applications by child id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of applications.</returns>
        Task<IEnumerable<ApplicationDto>> GetAllByChild(Guid id);

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
        Task Delete(Guid id);

        /// <summary>
        /// Determines ability to create a new application for a child based on previous attempts.
        /// </summary>
        /// <param name="workshopId">Workshop id.</param>
        /// <param name="childId">Child id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains <see langword="true" /> if allowed to create a new application by the child status;
        /// otherwise, <see langword="false" />.</returns>
        Task<bool> AllowedNewApplicationByChildStatus(Guid workshopId, Guid childId);
    }
}
