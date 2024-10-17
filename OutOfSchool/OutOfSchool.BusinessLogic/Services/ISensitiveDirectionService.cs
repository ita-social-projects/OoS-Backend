using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

public interface ISensitiveDirectionService
{
    /// <summary>
    /// To Update our object in DB.
    /// </summary>
    /// <param name="dto">Direction with new properties.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="DirectionDto"/> that was updated.</returns>
    Task<DirectionDto> Update(DirectionDto dto);

    /// <summary>
    /// To delete the object from DB.
    /// </summary>
    /// <param name="id">Key of the Direction in table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Result<DirectionDto>> Delete(long id);
}