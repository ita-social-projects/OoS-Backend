using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
public interface ISensitiveWorkshopDraftService
{
    /// <summary>
    /// Get entities from the database that match filter's parameters in admin panel.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{WorkshopDraftViewCardDto}"/> that contains found elements.</returns>    
    Task<SearchResult<WorkshopDraftViewCardDto>> FetchByFilterForAdmins(WorkshopDraftFilterAdministration filter = null);
}