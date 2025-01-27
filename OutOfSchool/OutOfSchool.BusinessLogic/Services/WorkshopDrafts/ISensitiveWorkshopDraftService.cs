using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
public interface ISensitiveWorkshopDraftService
{
    /// <summary>
    /// Get entities from the database that match filter's parameters in admin panel.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{WorkshopV2Dto}"/> that contains found elements.</returns>
    /// <exception cref="InvalidOperationException">If the region admin is not found in the database.</exception>
    Task<SearchResult<WorkshopV2Dto>> FetchByFilterForAdmins(WorkshopDraftFilterAdministration filter = null);
}