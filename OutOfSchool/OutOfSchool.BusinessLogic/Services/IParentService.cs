using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Parent;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Interface of Parent Service.
/// </summary>
public interface IParentService
{
    /// <summary>
    /// Get entity by User id.
    /// </summary>
    /// <param name="id">Key of the User entity in the table.</param>
    /// <returns>Parent.</returns>
    Task<ParentDTO> GetByUserId(string id);

    /// <summary>
    /// Asynchronously gets info about user with specific auxiliary data of parent.
    /// </summary>
    /// <param name="userId">Key of the User entity in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains an instance of <see cref="ShortUserDto"/>.
    /// </returns>
    Task<ShortUserDto> GetPersonalInfoByUserId(string userId);

    /// <summary>
    /// To Update our object in DB.
    /// </summary>
    /// <param name="info">Parent Personal Info with new properties.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<ShortUserDto> Update(ShortUserDto info);

    /// <summary>
    /// To delete the object from DB.
    /// </summary>
    /// <param name="id">Key in table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);

    /// <summary>
    /// Block or unblock Parent entity based on the provided information.
    /// </summary>
    /// <param name="parentBlockUnblock">A DTO containing the necessary information to block or unblock a parent,
    /// including the ParentId, the desired block status, and a reason.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The result contains a boolean indicating the success or failure of the operation.</returns>
    Task<Result<bool>> BlockUnblockParent(BlockUnblockParentDto parentBlockUnblock);

    /// <summary>
    /// Create new Parent in DB.
    /// </summary>
    /// <param name="parentCreateDto">DTO containing necessary information to create new Parent.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ParentDTO> Create(ParentCreateDto parentCreateDto);
}