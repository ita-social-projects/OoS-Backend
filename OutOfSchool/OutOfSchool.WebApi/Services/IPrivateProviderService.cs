using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for functionality for Private Provider.
/// </summary>
public interface IPrivateProviderService
{
    /// <summary>
    /// Updates Provider LicenseStatus.
    /// </summary>
    /// <param name="dto">Provider to update.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<ProviderLicenseStatusDto> UpdateLicenseStatus(ProviderLicenseStatusDto dto, string userId);
}
