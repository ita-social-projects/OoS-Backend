using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.Services.MinistryAdmin;

public interface ISensitiveMinistryAdminService
{
    /// <summary>
    /// Get Ministry Admins from the database that match filter's parameters.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{TEntity}"/> that contains found elements.</returns>
    Task<SearchResult<MinistryAdminDto>> GetByFilter(MinistryAdminFilter filter);
}
