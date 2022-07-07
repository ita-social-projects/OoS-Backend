using System;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services;

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
    /// Get entities from the database that match filter's parameters.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{ProviderDto}"/> that contains found elements.</returns>
    Task<SearchResult<ProviderDto>> GetByFilter(ProviderFilter filter = null);

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
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ProviderDto> Update(ProviderDto providerDto, string userId);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Provider's key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);

    /// <summary>
    ///  Gets Id of Provider, which owns a Workshop with specified Id.
    /// </summary>
    /// <param name="workshopId">Workshop's key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<Guid> GetProviderIdForWorkshopById(Guid workshopId);

    /// <summary>
    /// Update Provider Status.
    /// </summary>
    /// <param name="dto">Provider to update.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId);

    /// <summary>
    /// Updates Provider LicenseStatus.
    /// </summary>
    /// <param name="dto">Provider to update.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ProviderLicenseStatusDto> UpdateLicenseStatus(ProviderLicenseStatusDto dto, string userId);
}