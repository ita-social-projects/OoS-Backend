using OutOfSchool.BusinessLogic.Common;
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
    /// <exception cref="InvalidOperationException">If the region admin is not found in the database.</exception>
    Task<SearchResult<WorkshopDto>> FetchByFilterForAdmins(WorkshopFilterAdministration filter = null);

    /// <summary>
    /// Delete entity from the database by it's key.
    /// </summary>
    /// <param name="id">Key.</param>
    /// <returns></returns>
    Task<OperationResult> Delete(Guid id);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task DeleteV2(Guid id);
}
