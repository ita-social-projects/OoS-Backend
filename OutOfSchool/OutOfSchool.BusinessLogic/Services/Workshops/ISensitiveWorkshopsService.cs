using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Services.Workshops;
public interface ISensitiveWorkshopsService
{
    /// <summary>
    /// Get entities from the database that match filter's parameters in admin panel.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{WorkshopDto}"/> that contains found elements.</returns>
    Task<SearchResult<WorkshopDto>> FetchByFilterForAdmins(WorkshopFilterAdministration filter = null);
}
