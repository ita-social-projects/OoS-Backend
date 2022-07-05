﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Workshop entity.
    /// </summary>
    public interface IWorkshopService
    {
        /// <summary>
        /// Add entity to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopDTO"/>.</returns>
        Task<WorkshopDTO> Create(WorkshopDTO dto);

        /// <summary>
        /// Add entity to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopCreationResultDto"/>.</returns>
        Task<WorkshopCreationResultDto> CreateV2(WorkshopDTO dto);

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task{TEntity}"/> representing the result of the asynchronous operation.
        /// The task result contains the entity that was found, or null.</returns>
        Task<WorkshopDTO> GetById(Guid id);

        /// <summary>
        /// Update existing entity in the database.
        /// </summary>
        /// <param name="dto">Entity that will be to updated.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopDTO"/>.</returns>
        Task<WorkshopDTO> Update(WorkshopDTO dto);

        /// <summary>
        /// Update existing entity in the database.
        /// </summary>
        /// <param name="dto">Entity that will be to updated.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopUpdateResultDto"/>.</returns>
        Task<WorkshopUpdateResultDto> UpdateV2(WorkshopDTO dto);

        /// <summary>
        /// Update status field for existing entity in the database.
        /// </summary>
        /// <param name="dto">Workshop id and status to update.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopStatusDto"/>.</returns>
        Task<WorkshopStatusDto> UpdateStatus(WorkshopStatusDto dto);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(Guid id);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteV2(Guid id);

        /// <summary>
        /// Get all entities from the database.
        /// </summary>
        /// <param name="offsetFilter">Filter to get a certain portion of all entities.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="IEnumerable{TEntity}"/> that contains found elements.</returns>
        Task<SearchResult<WorkshopDTO>> GetAll(OffsetFilter offsetFilter);

        /// <summary>
        /// Get all workshops by provider Id.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopCard}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<WorkshopCard>> GetByProviderId(Guid id);

        /// <summary>
        /// Get entities from the database that match filter's parameters.
        /// </summary>
        /// <param name="filter">Filter with specified searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="SearchResult{WorkshopCard}"/> that contains found elements.</returns>
        Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter = null);

        /// <summary>
        /// Get entities from the database that match filter's parameters and sorts by distance.
        /// </summary>
        /// <param name="filter">Filter with specified searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="SearchResult{WorkshopCard}"/> that contains found elements.</returns>
        Task<SearchResult<WorkshopCard>> GetNearestByFilter(WorkshopFilter filter = null);

        Task<IEnumerable<Workshop>> GetByIds(IEnumerable<Guid> ids);

        /// <summary>
        /// Update provider's properties in all workshops with specified provider.
        /// </summary>
        /// <param name="provider">Provider to be searched by.</param>
        /// <returns>List of Workshops for the specified provider.</returns>
        Task<IEnumerable<Workshop>> PartialUpdateByProvider(Provider provider);

        /// <summary>
        ///  Returns ProviderDto by Id of its own workshop entity.
        /// </summary>
        /// <param name="workshopId">WorkshopId for which we need to get provider owner entity.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<Guid> GetWorkshopProviderOwnerIdAsync(Guid workshopId);
    }
}
