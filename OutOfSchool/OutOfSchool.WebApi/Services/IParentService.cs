using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services;

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
}