using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Services;

public interface IWorkshopServicesCombinerV2 : IWorkshopServicesCombiner
{
    /// <summary>
    /// Add entity to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopResultDto"/>.</returns>
    Task<WorkshopResultDto> Create(WorkshopV2CreateRequestDto dto);

    /// <summary>
    /// Update existing entity in the database.
    /// </summary>
    /// <param name="dto">Entity that will be to updated.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="Result{WorkshopCreateUpdateDto}"/>
    /// that indicates the success or failure of the operation.
    /// If the operation succeeds, the <see cref="Result{WorkshopResultDto}.Value"/> property
    /// contains the updated <see cref="WorkshopResultDto"/>.
    /// If the operation fails, the <see cref="Result{WorkshopResultDto}.OperationResult"/> property
    /// contains error information.</returns>
    Task<Result<WorkshopResultDto>> Update(WorkshopV2Dto dto);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    new Task Delete(Guid id);
}