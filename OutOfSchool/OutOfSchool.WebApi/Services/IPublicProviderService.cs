using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for functionality for Public Provider.
/// </summary>
public interface IPublicProviderService
{
    /// <summary>
    /// Update Provider Status.
    /// </summary>
    /// <param name="dto">Provider to update.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId);
}
