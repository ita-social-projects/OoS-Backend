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

    /// <summary>
    /// Updates Provider LicenseStatus.
    /// </summary>
    /// <param name="dto">Provider to update.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ProviderLicenseStatusDto> UpdateLicenseStatus(ProviderLicenseStatusDto dto, string userId);

    /// <summary>
    /// Update Provider Status.
    /// </summary>
    /// <param name="dto">Provider to update.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId);
}