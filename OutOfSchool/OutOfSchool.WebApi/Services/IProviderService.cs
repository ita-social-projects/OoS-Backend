using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Provider entity.
    /// </summary>
    public interface IProviderService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="providerDto">Provider entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ProviderDto> Create(ProviderDto providerDto);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all providers.</returns>
        Task<IEnumerable<ProviderDto>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Provider.</returns>
        Task<ProviderDto> GetById(Guid id);

        /// <summary>
        /// Get entity by User id.
        /// </summary>
        /// <param name="id">Key of the User entity in the table.</param>
        /// <returns>Provider.</returns>
        Task<ProviderDto> GetByUserId(string id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="providerDto">Provider entity to add.</param>
        /// <param name="userId">Id of user that request update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ProviderDto> Update(ProviderDto providerDto, string userId);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(Guid id);
    }
}