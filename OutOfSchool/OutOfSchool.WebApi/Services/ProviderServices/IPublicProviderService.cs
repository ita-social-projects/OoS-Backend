using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services.ProviderServices;

/// <summary>
/// Defines interface for functionality for Public Provider.
/// </summary>
public interface IPublicProviderService : IProviderService
{
    /// <summary>
    /// Update Provider Status.
    /// </summary>
    /// <param name="dto">Provider to update.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId);
}
