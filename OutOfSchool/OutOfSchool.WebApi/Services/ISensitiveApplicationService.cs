using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;

namespace OutOfSchool.WebApi.Services;

public interface ISensitiveApplicationService
{
    /// <summary>
    /// Get applications for admin.
    /// </summary>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of applications.</returns>
    Task<SearchResult<ApplicationDto>> GetAll(ApplicationFilter filter);
}