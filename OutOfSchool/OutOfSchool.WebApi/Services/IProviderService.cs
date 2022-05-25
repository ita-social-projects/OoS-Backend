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
        /// Add entity to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ProviderDto"/>.</returns>
        Task<ProviderDto> CreateV2(ProviderDto dto);

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
        /// <param name="isDeputyOrAdmin">Is user a deputy or delegated provider admin.</param>
        /// <returns>Provider.</returns>
        Task<ProviderDto> GetByUserId(string id, bool isDeputyOrAdmin = false);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="providerDto">Provider entity to add.</param>
        /// <param name="userId">Id of user that request update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ProviderDto> Update(ProviderDto providerDto, string userId);

        /// <summary>
        /// Update existing entity in the database.
        /// </summary>
        /// <param name="dto">Entity that will be to updated.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ProviderDto"/>.</returns>
        Task<ProviderDto> UpdateV2(ProviderDto dto, string UserId);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(Guid id);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteV2(Guid id);

        /// <summary>
        ///  Gets Id of Provider, which owns a Workshop with specified Id.
        /// </summary>
        /// <param name="workshopId">Workshop's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<Guid> GetProviderIdForWorkshopById(Guid workshopId);
    }
}