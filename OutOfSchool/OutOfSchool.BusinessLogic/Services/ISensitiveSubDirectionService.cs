using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;
public interface ISensitiveSubDirectionService
{
    /// <summary>
    /// To Update our object in DB.
    /// </summary>
    /// <param name="dto">SubDirection with new properties.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="SubDirectionDto"/> that was updated.</returns>
    Task<Result<SubDirectionDto>> Update(SubDirectionDto dto);

    /// <summary>
    /// To delete the object from DB.
    /// </summary>
    /// <param name="id">Key of the SubDirection in table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    /// The task result contains a <see cref="SubDirectionDto"/> that was updated.</returns>
    Task<Result<SubDirectionDto>> Delete(long id);
}
