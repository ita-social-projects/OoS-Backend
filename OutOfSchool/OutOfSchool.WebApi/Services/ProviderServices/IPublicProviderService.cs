using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services.ProviderServices;

/// <summary>
/// Defines interface for functionality for Public Provider.
/// </summary>
public interface IPublicProviderService
{
    /// <summary>
    /// Get entities from the database that match filter's parameters.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{ProviderDto}"/> that contains found elements.</returns>
    Task<SearchResult<ProviderDto>> GetByFilter(BaseProviderFilter filter = null);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Provider.</returns>
    Task<ProviderDto> GetById(Guid id);

    /// <summary>
    /// Update Provider Status.
    /// </summary>
    /// <param name="dto">Provider to update.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId);
}
