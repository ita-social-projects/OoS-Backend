using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models.SocialGroup;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for SocialGroup entity.
/// </summary>
public interface ISocialGroupService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <returns>List of all SocialGroups.</returns>
    Task<IEnumerable<SocialGroupDto>> GetAll();

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>SocialGroup.</returns>
    Task<SocialGroupDto> GetById(long id);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">SocialGroup entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<SocialGroupDto> Create(SocialGroupDto dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="dto">SocialGroup entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<SocialGroupDto> Update(SocialGroupDto dto);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">SocialGroup key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(long id);
}