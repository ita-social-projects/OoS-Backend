using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

public interface IUserService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <returns>List of all Users.</returns>
    Task<IEnumerable<ShortUserDto>> GetAll();

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>User.</returns>
    Task<ShortUserDto> GetById(string id);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="dto">User entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ShortUserDto> Update(BaseUpdateUserDto dto);

    /// <summary>
    /// Check if entity is blocked.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns><see cref="Task{TResult}"/>.</returns>
    Task<bool> IsBlocked(string id);

    /// <summary>
    /// Delete entity by id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <summary>
/// Asynchronously deletes the user identified by the specified ID.
/// </summary>
    Task Delete(string id);

    /// <summary>
    /// Get AccountStatus for the User.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <summary>
/// Retrieves the account status of the user with the specified ID.
/// </summary>
/// <param name="id">The unique identifier of the user.</param>
/// <returns>A task that resolves to the user's account status.</returns>
    Task<AccountStatus> GetAccountStatus(string id);
}