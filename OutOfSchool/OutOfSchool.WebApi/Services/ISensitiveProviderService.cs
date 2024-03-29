using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services;

public interface ISensitiveProviderService
{
    /// <summary>
    /// Get entities from the database that match filter's parameters.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{TEntity}"/> that contains found elements.</returns>
    Task<SearchResult<ProviderDto>> GetByFilter(ProviderFilter filter = null);

    /// <summary>
    /// Set block/unblock state.
    /// </summary>
    /// <param name="providerBlockDto">Provider to block/unblock.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ResponseDto> Block(ProviderBlockDto providerBlockDto, string token = default);
}