using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;

namespace OutOfSchool.BusinessLogic.Services;

public interface ISensitiveApplicationService
{
    /// <summary>
    /// Get applications for admin.
    /// </summary>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of applications.</returns>
    Task<SearchResult<ApplicationDto>> GetAll(ApplicationFilter filter);
}